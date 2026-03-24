using DigitalStokvel.GroupService.Data;
using DigitalStokvel.GroupService.DTOs;
using DigitalStokvel.GroupService.Entities;
using DigitalStokvel.GroupService.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DigitalStokvel.GroupService.Tests;

/// <summary>
/// Unit tests for GroupService business logic
/// </summary>
public class GroupServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<Services.GroupService>> _loggerMock;
    private readonly Services.GroupService _service;

    public GroupServiceTests()
    {
        // Set up in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<Services.GroupService>>();
        _service = new Services.GroupService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region CreateGroupAsync Tests

    [Fact]
    public async Task CreateGroupAsync_ValidRequest_ReturnsGroupWithInvitations()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var request = new CreateGroupRequest
        {
            Name = "Test Stokvel",
            Description = "Test Description",
            GroupType = "RotatingPayout",
            ContributionAmount = 500.00m,
            ContributionFrequency = "Monthly",
            PayoutSchedule = new PayoutScheduleDto { Type = "RotatingBasis" },
            Constitution = new ConstitutionDto
            {
                GracePeriodDays = 3,
                LateFee = 50.00m,
                MissedPaymentsThreshold = 3,
                QuorumPercentage = 51
            },
            InvitedMembers = new List<InviteMemberDto>
            {
                new() { PhoneNumber = "+27123456789", Role = "Member" },
                new() { PhoneNumber = "+27987654321", Role = "Treasurer" }
            }
        };

        // Act
        var result = await _service.CreateGroupAsync(request, creatorUserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Test Stokvel");
        result.GroupType.Should().Be("rotatingpayout"); // Enum is serialized to lowercase
        result.ContributionAmount.Should().Be(500.00m);
        result.MemberCount.Should().Be(3); // Creator + 2 invited
        result.AccountNumber.Should().NotBeNullOrEmpty();
        result.AccountNumber.Should().StartWith("400");
        result.InvitationLinks.Should().HaveCount(2);
        result.InvitationLinks.All(i => !string.IsNullOrEmpty(i.InviteToken)).Should().BeTrue();

        // Verify database state
        var group = await _context.Groups.FirstOrDefaultAsync();
        group.Should().NotBeNull();
        group!.Name.Should().Be("Test Stokvel");

        var members = await _context.GroupMembers.ToListAsync();
        members.Should().HaveCount(3);
        members.Should().Contain(m => m.UserId == creatorUserId && m.Role == MemberRole.Chairperson);

        var account = await _context.GroupSavingsAccounts.FirstOrDefaultAsync();
        account.Should().NotBeNull();
        account!.Balance.Should().Be(0);
        account.InterestRate.Should().Be(4.5m);
    }

    [Fact]
    public async Task CreateGroupAsync_InvalidGroupType_ThrowsArgumentException()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var request = new CreateGroupRequest
        {
            Name = "Test Stokvel",
            GroupType = "InvalidType",
            ContributionAmount = 500.00m,
            ContributionFrequency = "Monthly",
            PayoutSchedule = new PayoutScheduleDto { Type = "RotatingBasis" },
            InvitedMembers = new List<InviteMemberDto>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.CreateGroupAsync(request, creatorUserId));
    }

    [Fact]
    public async Task CreateGroupAsync_InvalidFrequency_ThrowsArgumentException()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var request = new CreateGroupRequest
        {
            Name = "Test Stokvel",
            GroupType = "SavingsPot",
            ContributionAmount = 500.00m,
            ContributionFrequency = "InvalidFrequency",
            PayoutSchedule = new PayoutScheduleDto { Type = "RotatingBasis" },
            InvitedMembers = new List<InviteMemberDto>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.CreateGroupAsync(request, creatorUserId));
    }

    [Fact]
    public async Task CreateGroupAsync_CreatorIsChairperson_WithActiveStatus()
    {
        // Arrange
        var creatorUserId = Guid.NewGuid();
        var request = new CreateGroupRequest
        {
            Name = "Leadership Test",
            GroupType = "SavingsPot",
            ContributionAmount = 1000.00m,
            ContributionFrequency = "Weekly",
            PayoutSchedule = new PayoutScheduleDto { Type = "RotatingBasis" },
            InvitedMembers = new List<InviteMemberDto>()
        };

        // Act
        await _service.CreateGroupAsync(request, creatorUserId);

        // Assert
        var chairperson = await _context.GroupMembers
            .FirstOrDefaultAsync(m => m.UserId == creatorUserId);
        
        chairperson.Should().NotBeNull();
        chairperson!.Role.Should().Be(MemberRole.Chairperson);
        chairperson.Status.Should().Be(MemberStatus.Active);
        chairperson.InviteAcceptedAt.Should().NotBeNull();
    }

    #endregion

    #region GetGroupByIdAsync Tests

    [Fact]
    public async Task GetGroupByIdAsync_ExistingGroup_ReturnsDetails()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var group = new Group
        {
            Id = groupId,
            Name = "Existing Group",
            GroupType = GroupType.SavingsPot,
            ContributionAmount = 750.00m,
            ContributionFrequency = ContributionFrequency.BiWeekly,
            Status = GroupStatus.Active,
            CreatedBy = Guid.NewGuid(),
            Constitution = "{\"GracePeriodDays\":3,\"LateFee\":50.0,\"MissedPaymentsThreshold\":3,\"QuorumPercentage\":51}",
            PayoutSchedule = "{\"Type\":\"FixedDate\",\"Date\":\"2026-04-01\"}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var account = new GroupSavingsAccount
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            AccountNumber = "4001234567890",
            Balance = 5000.00m,
            TotalContributions = 5000.00m,
            TotalInterestEarned = 0,
            TotalPayouts = 0,
            InterestRate = 4.5m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var member = new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            UserId = group.CreatedBy,
            Role = MemberRole.Chairperson,
            Status = MemberStatus.Active,
            JoinedAt = DateTime.UtcNow
        };

        _context.Groups.Add(group);
        _context.GroupSavingsAccounts.Add(account);
        _context.GroupMembers.Add(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetGroupByIdAsync(groupId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(groupId);
        result.Name.Should().Be("Existing Group");
        result.ContributionAmount.Should().Be(750.00m);
        result.MemberCount.Should().Be(1);
        result.Account.Should().NotBeNull();
        result.Account!.Balance.Should().Be(5000.00m);
        result.Constitution.Should().NotBeNull();
        result.Constitution!.GracePeriodDays.Should().Be(3);
    }

    [Fact]
    public async Task GetGroupByIdAsync_NonExistingGroup_ReturnsNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _service.GetGroupByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetUserGroupsAsync Tests

    [Fact]
    public async Task GetUserGroupsAsync_UserWithGroups_ReturnsGroupList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        var group1 = new Group
        {
            Id = Guid.NewGuid(),
            Name = "Group 1",
            GroupType = GroupType.RotatingPayout,
            ContributionAmount = 500.00m,
            ContributionFrequency = ContributionFrequency.Monthly,
            Status = GroupStatus.Active,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var group2 = new Group
        {
            Id = Guid.NewGuid(),
            Name = "Group 2",
            GroupType = GroupType.InvestmentClub,
            ContributionAmount = 1000.00m,
            ContributionFrequency = ContributionFrequency.Weekly,
            Status = GroupStatus.Active,
            CreatedBy = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var account1 = new GroupSavingsAccount
        {
            Id = Guid.NewGuid(),
            GroupId = group1.Id,
            AccountNumber = "4001111111111",
            Balance = 2500.00m,
            InterestRate = 4.5m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var account2 = new GroupSavingsAccount
        {
            Id = Guid.NewGuid(),
            GroupId = group2.Id,
            AccountNumber = "4002222222222",
            Balance = 10000.00m,
            InterestRate = 4.5m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var member1 = new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = group1.Id,
            UserId = userId,
            Role = MemberRole.Chairperson,
            Status = MemberStatus.Active,
            JoinedAt = DateTime.UtcNow
        };

        var member2 = new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = group2.Id,
            UserId = userId,
            Role = MemberRole.Treasurer,
            Status = MemberStatus.Active,
            JoinedAt = DateTime.UtcNow
        };

        _context.Groups.AddRange(group1, group2);
        _context.GroupSavingsAccounts.AddRange(account1, account2);
        _context.GroupMembers.AddRange(member1, member2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetUserGroupsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Groups.Should().HaveCount(2);
        result.Groups.Should().Contain(g => g.Name == "Group 1" && g.UserRole == "chairperson"); // Enum serialized to lowercase
        result.Groups.Should().Contain(g => g.Name == "Group 2" && g.UserRole == "treasurer"); // Enum serialized to lowercase
        result.Groups.First(g => g.Name == "Group 1").Balance.Should().Be(2500.00m);
    }

    [Fact]
    public async Task GetUserGroupsAsync_UserWithNoGroups_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _service.GetUserGroupsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Groups.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserGroupsAsync_OnlyActiveMembers_ExcludesPending()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = "Test Group",
            GroupType = GroupType.SavingsPot,
            ContributionAmount = 500.00m,
            ContributionFrequency = ContributionFrequency.Monthly,
            Status = GroupStatus.Active,
            CreatedBy = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var account = new GroupSavingsAccount
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            AccountNumber = "4001234567890",
            Balance = 0,
            InterestRate = 4.5m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var pendingMember = new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            UserId = userId,
            Role = MemberRole.Member,
            Status = MemberStatus.Pending,
            JoinedAt = DateTime.UtcNow
        };

        _context.Groups.Add(group);
        _context.GroupSavingsAccounts.Add(account);
        _context.GroupMembers.Add(pendingMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetUserGroupsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Groups.Should().BeEmpty();
    }

    #endregion
}
