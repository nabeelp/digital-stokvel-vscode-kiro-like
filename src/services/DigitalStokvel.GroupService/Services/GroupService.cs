using DigitalStokvel.GroupService.Data;
using DigitalStokvel.GroupService.DTOs;
using DigitalStokvel.GroupService.Entities;
using DigitalStokvel.Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DigitalStokvel.GroupService.Services;

/// <summary>
/// Service implementation for group management operations
/// </summary>
public class GroupService : IGroupService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GroupService> _logger;
    private readonly ICbsClient _cbsClient;

    public GroupService(
        ApplicationDbContext context, 
        ILogger<GroupService> logger,
        ICbsClient cbsClient)
    {
        _context = context;
        _logger = logger;
        _cbsClient = cbsClient;
    }

    /// <inheritdoc/>
    public async Task<CreateGroupResponse> CreateGroupAsync(CreateGroupRequest request, Guid creatorUserId)
    {
        _logger.LogInformation("Creating new group '{GroupName}' by user {UserId}", request.Name, creatorUserId);

        // Parse group type
        if (!Enum.TryParse<GroupType>(request.GroupType, true, out var groupType))
        {
            throw new ArgumentException($"Invalid group type: {request.GroupType}");
        }

        // Parse contribution frequency
        if (!Enum.TryParse<ContributionFrequency>(request.ContributionFrequency, true, out var contributionFrequency))
        {
            throw new ArgumentException($"Invalid contribution frequency: {request.ContributionFrequency}");
        }

        // Serialize payout schedule and constitution to JSON
        var payoutScheduleJson = JsonSerializer.Serialize(request.PayoutSchedule);
        var constitutionJson = request.Constitution != null 
            ? JsonSerializer.Serialize(request.Constitution)
            : JsonSerializer.Serialize(new ConstitutionDto());

        // Create the group
        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            GroupType = groupType,
            ContributionAmount = request.ContributionAmount,
            ContributionFrequency = contributionFrequency,
            PayoutSchedule = payoutScheduleJson,
            Constitution = constitutionJson,
            Status = GroupStatus.Active,
            CreatedBy = creatorUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Groups.Add(group);

        // Add the creator as chairperson
        var chairperson = new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            UserId = creatorUserId,
            Role = MemberRole.Chairperson,
            Status = MemberStatus.Active,
            JoinedAt = DateTime.UtcNow,
            InvitedBy = null,
            InviteAcceptedAt = DateTime.UtcNow
        };

        _context.GroupMembers.Add(chairperson);

        // Add invited members
        var invitationLinks = new List<InvitationLinkDto>();
        if (request.InvitedMembers != null && request.InvitedMembers.Any())
        {
            foreach (var invitedMember in request.InvitedMembers)
            {
                // Parse role
                if (!Enum.TryParse<MemberRole>(invitedMember.Role, true, out var memberRole))
                {
                    memberRole = MemberRole.Member;
                }

                // Generate invitation token
                var inviteToken = GenerateInviteToken();

                var member = new GroupMember
                {
                    Id = Guid.NewGuid(),
                    GroupId = group.Id,
                    UserId = Guid.Empty, // Will be set when invitation is accepted
                    Role = memberRole,
                    Status = MemberStatus.Pending,
                    JoinedAt = DateTime.UtcNow,
                    InvitedBy = creatorUserId,
                    InviteToken = inviteToken
                };

                _context.GroupMembers.Add(member);

                // Create invitation link
                invitationLinks.Add(new InvitationLinkDto
                {
                    MemberId = member.Id,
                    InviteToken = inviteToken,
                    ShareLink = $"https://stokvel.bank.co.za/join/{inviteToken}"
                });
            }
        }

        // Create savings account in Core Banking System
        string cbsAccountNumber;
        try
        {
            _logger.LogInformation("Creating CBS account for group '{GroupName}' (ID: {GroupId})", request.Name, group.Id);
            
            cbsAccountNumber = await _cbsClient.CreateGroupAccountAsync(
                group.Id, 
                $"STOKVEL-{request.Name}");
            
            _logger.LogInformation("CBS account created successfully: {AccountNumber} for group {GroupId}", 
                cbsAccountNumber, group.Id);
        }
        catch (CbsClientException ex)
        {
            _logger.LogError(ex, "Failed to create CBS account for group {GroupId}: {ErrorMessage}", 
                group.Id, ex.Message);
            throw new InvalidOperationException(
                "Unable to create banking account for the group. Please try again later.", ex);
        }

        var savingsAccount = new GroupSavingsAccount
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            AccountNumber = cbsAccountNumber,
            Balance = 0,
            TotalContributions = 0,
            TotalInterestEarned = 0,
            TotalPayouts = 0,
            InterestRate = 4.5m, // Default interest rate
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.GroupSavingsAccounts.Add(savingsAccount);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Group '{GroupName}' created successfully with ID {GroupId}", request.Name, group.Id);

        // Get member count
        var memberCount = await _context.GroupMembers
            .Where(m => m.GroupId == group.Id)
            .CountAsync();

        return new CreateGroupResponse
        {
            Id = group.Id,
            Name = group.Name,
            GroupType = group.GroupType.ToString().ToLowerInvariant(),
            ContributionAmount = group.ContributionAmount,
            ContributionFrequency = group.ContributionFrequency.ToString().ToLowerInvariant(),
            Status = group.Status.ToString().ToLowerInvariant(),
            AccountNumber = savingsAccount.AccountNumber,
            MemberCount = memberCount,
            CreatedAt = group.CreatedAt,
            InvitationLinks = invitationLinks.Any() ? invitationLinks : null
        };
    }

    /// <inheritdoc/>
    public async Task<GroupDetailsResponse?> GetGroupByIdAsync(Guid groupId)
    {
        _logger.LogInformation("Retrieving group details for group ID {GroupId}", groupId);

        var group = await _context.Groups
            .Include(g => g.SavingsAccount)
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
        {
            _logger.LogWarning("Group with ID {GroupId} not found", groupId);
            return null;
        }

        // Parse constitution
        var constitution = JsonSerializer.Deserialize<ConstitutionDto>(group.Constitution);

        // Parse payout schedule
        var payoutSchedule = JsonSerializer.Deserialize<PayoutScheduleDto>(group.PayoutSchedule);

        // Calculate next contribution due (simplified logic)
        DateTime? nextContributionDue = CalculateNextContributionDue(group.ContributionFrequency);

        // Calculate next payout (simplified logic)
        NextPayoutDto? nextPayout = null;
        if (payoutSchedule != null && payoutSchedule.Date.HasValue)
        {
            nextPayout = new NextPayoutDto
            {
                Type = payoutSchedule.Type,
                Date = payoutSchedule.Date.Value,
                EstimatedAmount = group.SavingsAccount?.Balance ?? 0
            };
        }

        var response = new GroupDetailsResponse
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            GroupType = group.GroupType.ToString().ToLowerInvariant(),
            ContributionAmount = group.ContributionAmount,
            ContributionFrequency = group.ContributionFrequency.ToString().ToLowerInvariant(),
            Status = group.Status.ToString().ToLowerInvariant(),
            Account = group.SavingsAccount != null ? new AccountDetailsDto
            {
                AccountNumber = group.SavingsAccount.AccountNumber,
                Balance = group.SavingsAccount.Balance,
                TotalContributions = group.SavingsAccount.TotalContributions,
                TotalInterestEarned = group.SavingsAccount.TotalInterestEarned,
                InterestRate = group.SavingsAccount.InterestRate
            } : null,
            MemberCount = group.Members.Count,
            NextContributionDue = nextContributionDue,
            NextPayout = nextPayout,
            Constitution = constitution,
            CreatedAt = group.CreatedAt
        };

        return response;
    }

    /// <inheritdoc/>
    public async Task<UserGroupsResponse> GetUserGroupsAsync(Guid userId)
    {
        _logger.LogInformation("Retrieving groups for user ID {UserId}", userId);

        var groupSummaries = await _context.GroupMembers
            .Where(m => m.UserId == userId && m.Status == MemberStatus.Active)
            .Include(m => m.Group)
                .ThenInclude(g => g!.SavingsAccount)
            .Include(m => m.Group)
                .ThenInclude(g => g!.Members)
            .Select(m => new GroupSummaryDto
            {
                Id = m.Group!.Id,
                Name = m.Group.Name,
                GroupType = m.Group.GroupType.ToString().ToLowerInvariant(),
                ContributionAmount = m.Group.ContributionAmount,
                MemberCount = m.Group.Members.Count,
                UserRole = m.Role.ToString().ToLowerInvariant(),
                Balance = m.Group.SavingsAccount!.Balance
            })
            .ToListAsync();

        return new UserGroupsResponse
        {
            Groups = groupSummaries,
            TotalCount = groupSummaries.Count
        };
    }

    private string GenerateInviteToken()
    {
        // Generate a random 16-character alphanumeric token
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 16)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private string GenerateAccountNumber()
    {
        // Generate a simulated 13-digit account number
        // In production, this would be obtained from the Core Banking System
        var random = new Random();
        var accountNumber = "400" + string.Join("", Enumerable.Range(0, 10).Select(_ => random.Next(0, 10)));
        return accountNumber;
    }

    private DateTime? CalculateNextContributionDue(ContributionFrequency frequency)
    {
        var today = DateTime.UtcNow.Date;
        
        return frequency switch
        {
            ContributionFrequency.Weekly => today.AddDays(7),
            ContributionFrequency.BiWeekly => today.AddDays(14),
            ContributionFrequency.Monthly => today.AddMonths(1),
            _ => null
        };
    }
}
