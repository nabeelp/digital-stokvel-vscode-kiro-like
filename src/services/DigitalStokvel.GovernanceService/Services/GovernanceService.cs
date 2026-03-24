using DigitalStokvel.GovernanceService.Data;
using DigitalStokvel.GovernanceService.DTOs;
using DigitalStokvel.GovernanceService.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DigitalStokvel.GovernanceService.Services;

/// <summary>
/// Service implementation for governance operations with quorum calculation.
/// </summary>
public class GovernanceService : IGovernanceService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GovernanceService> _logger;

    public GovernanceService(ApplicationDbContext context, ILogger<GovernanceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new vote for a group.
    /// </summary>
    public async Task<CreateVoteResponse> CreateVoteAsync(CreateVoteRequest request, Guid userId)
    {
        _logger.LogInformation("Creating vote for group {GroupId} by user {UserId}", request.GroupId, userId);

        // TODO: Validate that userId is a member of the group
        // TODO: Validate that user has permission to create votes (e.g., Chairperson)

        // Calculate voting period
        var votingStartsAt = DateTime.UtcNow;
        var votingEndsAt = votingStartsAt.AddHours(request.VotingDurationHours);

        // Serialize options to JSON
        var optionsJson = JsonSerializer.Serialize(request.Options);

        // Create vote entity
        var vote = new Vote
        {
            GroupId = request.GroupId,
            InitiatedBy = userId,
            Subject = request.Subject,
            Options = optionsJson,
            VotingStartsAt = votingStartsAt,
            VotingEndsAt = votingEndsAt,
            Status = VoteStatus.Active, // Immediately active
            Results = "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Votes.Add(vote);
        await _context.SaveChangesAsync();

        // Calculate quorum (51% of group members)
        var quorumRequired = await CalculateQuorumAsync(request.GroupId);

        _logger.LogInformation("Vote {VoteId} created successfully for group {GroupId}", vote.Id, request.GroupId);

        return new CreateVoteResponse
        {
            Id = vote.Id,
            GroupId = vote.GroupId,
            Subject = vote.Subject,
            Options = request.Options,
            VotingStartsAt = vote.VotingStartsAt,
            VotingEndsAt = vote.VotingEndsAt,
            Status = vote.Status.ToString().ToLowerInvariant(),
            QuorumRequired = quorumRequired,
            CurrentVotes = 0
        };
    }

    /// <summary>
    /// Casts a vote for a member.
    /// </summary>
    public async Task<CastVoteResponse> CastVoteAsync(Guid voteId, CastVoteRequest request, Guid userId)
    {
        _logger.LogInformation("User {UserId} casting vote for vote {VoteId}", userId, voteId);

        // Retrieve vote
        var vote = await _context.Votes
            .Include(v => v.Responses)
            .FirstOrDefaultAsync(v => v.Id == voteId);

        if (vote == null)
        {
            throw new InvalidOperationException($"Vote {voteId} not found");
        }

        // Validate vote is active
        if (vote.Status != VoteStatus.Active)
        {
            throw new InvalidOperationException($"Vote {voteId} is not active (current status: {vote.Status})");
        }

        // Validate voting period
        var now = DateTime.UtcNow;
        if (now < vote.VotingStartsAt || now > vote.VotingEndsAt)
        {
            throw new InvalidOperationException($"Voting period has ended");
        }

        // Validate selected option
        var options = JsonSerializer.Deserialize<List<string>>(vote.Options) ?? new List<string>();
        if (!options.Contains(request.SelectedOption))
        {
            throw new ArgumentException($"Invalid option: {request.SelectedOption}");
        }

        // TODO: Get actual member_id from user_id via Group Service
        var memberId = userId; // Placeholder: using user_id as member_id for now

        // Check if member has already voted
        var existingResponse = vote.Responses.FirstOrDefault(r => r.MemberId == memberId);
        if (existingResponse != null)
        {
            throw new InvalidOperationException($"Member has already voted");
        }

        // Create vote response
        var voteResponse = new VoteResponse
        {
            VoteId = voteId,
            MemberId = memberId,
            SelectedOption = request.SelectedOption,
            VotedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.VoteResponses.Add(voteResponse);

        // Update vote results
        var results = JsonSerializer.Deserialize<Dictionary<string, int>>(vote.Results ?? "{}") ?? new Dictionary<string, int>();
        results[request.SelectedOption] = results.GetValueOrDefault(request.SelectedOption, 0) + 1;
        vote.Results = JsonSerializer.Serialize(results);
        vote.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Vote cast successfully for vote {VoteId} by user {UserId}", voteId, userId);

        return new CastVoteResponse
        {
            Id = voteResponse.Id,
            VoteId = voteResponse.VoteId,
            SelectedOption = voteResponse.SelectedOption,
            VotedAt = voteResponse.VotedAt
        };
    }

    /// <summary>
    /// Retrieves vote status including results and quorum information.
    /// </summary>
    public async Task<VoteStatusResponse> GetVoteStatusAsync(Guid voteId)
    {
        _logger.LogInformation("Retrieving vote status for {VoteId}", voteId);

        var vote = await _context.Votes
            .Include(v => v.Responses)
            .FirstOrDefaultAsync(v => v.Id == voteId);

        if (vote == null)
        {
            throw new InvalidOperationException($"Vote {voteId} not found");
        }

        // Deserialize options and results
        var options = JsonSerializer.Deserialize<List<string>>(vote.Options) ?? new List<string>();
        var results = JsonSerializer.Deserialize<Dictionary<string, int>>(vote.Results ?? "{}") ?? new Dictionary<string, int>();

        // Calculate quorum
        var quorumRequired = await CalculateQuorumAsync(vote.GroupId);
        var currentVotes = vote.Responses.Count;
        var quorumReached = currentVotes >= quorumRequired;

        return new VoteStatusResponse
        {
            Id = vote.Id,
            GroupId = vote.GroupId,
            Subject = vote.Subject,
            Options = options,
            Status = vote.Status.ToString().ToLowerInvariant(),
            VotingStartsAt = vote.VotingStartsAt,
            VotingEndsAt = vote.VotingEndsAt,
            Results = results,
            QuorumRequired = quorumRequired,
            CurrentVotes = currentVotes,
            QuorumReached = quorumReached
        };
    }

    /// <summary>
    /// Raises a dispute for a group.
    /// </summary>
    public async Task<RaiseDisputeResponse> RaiseDisputeAsync(RaiseDisputeRequest request, Guid userId)
    {
        _logger.LogInformation("Raising dispute for group {GroupId} by user {UserId}", request.GroupId, userId);

        // Validate dispute type
        if (!Enum.TryParse<DisputeType>(request.DisputeType, true, out var disputeType))
        {
            throw new ArgumentException($"Invalid dispute type: {request.DisputeType}");
        }

        // TODO: Get actual member_id from user_id via Group Service
        var memberId = userId; // Placeholder: using user_id as member_id for now

        // Serialize evidence to JSON
        var evidenceJson = JsonSerializer.Serialize(request.Evidence);

        // Create dispute entity
        var dispute = new Dispute
        {
            GroupId = request.GroupId,
            RaisedBy = memberId,
            DisputeType = disputeType,
            Description = request.Description,
            Evidence = evidenceJson,
            Status = DisputeStatus.Open,
            RaisedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Disputes.Add(dispute);
        await _context.SaveChangesAsync();

        // Calculate resolution deadline (14 days from now)
        var resolutionDeadline = dispute.RaisedAt.AddDays(14);

        _logger.LogInformation("Dispute {DisputeId} raised successfully for group {GroupId}", dispute.Id, request.GroupId);

        return new RaiseDisputeResponse
        {
            Id = dispute.Id,
            GroupId = dispute.GroupId,
            DisputeType = dispute.DisputeType.ToString().ToLowerInvariant(),
            Status = dispute.Status.ToString().ToLowerInvariant(),
            RaisedAt = dispute.RaisedAt,
            ResolutionDeadline = resolutionDeadline
        };
    }

    /// <summary>
    /// Retrieves dispute details.
    /// </summary>
    public async Task<DisputeDetailsResponse> GetDisputeDetailsAsync(Guid disputeId)
    {
        _logger.LogInformation("Retrieving dispute details for {DisputeId}", disputeId);

        var dispute = await _context.Disputes
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
        {
            throw new InvalidOperationException($"Dispute {disputeId} not found");
        }

        // Deserialize evidence
        var evidence = JsonSerializer.Deserialize<List<EvidenceItem>>(dispute.Evidence) ?? new List<EvidenceItem>();

        return new DisputeDetailsResponse
        {
            Id = dispute.Id,
            GroupId = dispute.GroupId,
            DisputeType = dispute.DisputeType.ToString().ToLowerInvariant(),
            Description = dispute.Description,
            Evidence = evidence,
            Status = dispute.Status.ToString().ToLowerInvariant(),
            ResolutionNotes = dispute.ResolutionNotes,
            RaisedAt = dispute.RaisedAt,
            ResolvedAt = dispute.ResolvedAt,
            RaisedBy = new MemberDto
            {
                Id = dispute.RaisedBy,
                Name = "Placeholder Member", // TODO: Fetch actual member name from Group Service
                Phone = "+27821234567" // TODO: Fetch actual member phone from Group Service
            }
        };
    }

    /// <summary>
    /// Calculates the quorum required for a group vote (51% of members).
    /// </summary>
    private async Task<int> CalculateQuorumAsync(Guid groupId)
    {
        // TODO: Fetch actual member count from Group Service
        // For now, using a placeholder calculation
        var memberCount = 24; // Placeholder
        var quorum = (int)Math.Ceiling(memberCount * 0.51);
        
        _logger.LogInformation("Calculated quorum for group {GroupId}: {Quorum} out of {MemberCount}", groupId, quorum, memberCount);
        
        return quorum;
    }
}
