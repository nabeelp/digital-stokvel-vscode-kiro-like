using DigitalStokvel.GovernanceService.Data;
using DigitalStokvel.GovernanceService.DTOs;
using DigitalStokvel.GovernanceService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace DigitalStokvel.GovernanceService.Tests;

public class GovernanceServiceTests
{
    private readonly Mock<ILogger<DigitalStokvel.GovernanceService.Services.GovernanceService>> _mockLogger;

    public GovernanceServiceTests()
    {
        _mockLogger = new Mock<ILogger<DigitalStokvel.GovernanceService.Services.GovernanceService>>();
    }

    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateVoteAsync_CreatesVoteWithActiveStatus()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new DigitalStokvel.GovernanceService.Services.GovernanceService(context, _mockLogger.Object);

        var request = new CreateVoteRequest
        {
            GroupId = Guid.NewGuid(),
            Subject = "Should we increase monthly contribution to R600?",
            Options = new List<string> { "yes", "no", "abstain" },
            VotingDurationHours = 48
        };
        var userId = Guid.NewGuid();

        // Act
        var response = await service.CreateVoteAsync(request, userId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(request.GroupId, response.GroupId);
        Assert.Equal(request.Subject, response.Subject);
        Assert.Equal("active", response.Status);
        Assert.Equal(0, response.CurrentVotes);
        Assert.True(response.QuorumRequired > 0);

        // Verify vote was saved to database
        var vote = await context.Votes.FirstOrDefaultAsync(v => v.Id == response.Id);
        Assert.NotNull(vote);
        Assert.Equal(VoteStatus.Active, vote.Status);
    }

    [Fact]
    public async Task CastVoteAsync_CastsVoteAndUpdatesResults()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new DigitalStokvel.GovernanceService.Services.GovernanceService(context, _mockLogger.Object);

        var groupId = Guid.NewGuid();
        var options = new List<string> { "yes", "no", "abstain" };
        var vote = new Vote
        {
            GroupId = groupId,
            InitiatedBy = Guid.NewGuid(),
            Subject = "Test vote",
            Options = JsonSerializer.Serialize(options),
            VotingStartsAt = DateTime.UtcNow.AddHours(-1),
            VotingEndsAt = DateTime.UtcNow.AddHours(47),
            Status = VoteStatus.Active,
            Results = "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Votes.Add(vote);
        await context.SaveChangesAsync();

        var request = new CastVoteRequest
        {
            SelectedOption = "yes"
        };
        var userId = Guid.NewGuid();

        // Act
        var response = await service.CastVoteAsync(vote.Id, request, userId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(vote.Id, response.VoteId);
        Assert.Equal("yes", response.SelectedOption);

        // Verify vote response was saved
        var voteResponse = await context.VoteResponses.FirstOrDefaultAsync(vr => vr.Id == response.Id);
        Assert.NotNull(voteResponse);
        Assert.Equal(userId, voteResponse.MemberId);

        // Verify vote results were updated
        var updatedVote = await context.Votes.FirstOrDefaultAsync(v => v.Id == vote.Id);
        Assert.NotNull(updatedVote);
        var results = JsonSerializer.Deserialize<Dictionary<string, int>>(updatedVote.Results);
        Assert.NotNull(results);
        Assert.Equal(1, results["yes"]);
    }

    [Fact]
    public async Task CastVoteAsync_ThrowsExceptionForDuplicateVote()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new DigitalStokvel.GovernanceService.Services.GovernanceService(context, _mockLogger.Object);

        var memberId = Guid.NewGuid();
        var vote = new Vote
        {
            GroupId = Guid.NewGuid(),
            InitiatedBy = Guid.NewGuid(),
            Subject = "Test vote",
            Options = JsonSerializer.Serialize(new List<string> { "yes", "no" }),
            VotingStartsAt = DateTime.UtcNow.AddHours(-1),
            VotingEndsAt = DateTime.UtcNow.AddHours(47),
            Status = VoteStatus.Active,
            Results = "{\"yes\": 1}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Votes.Add(vote);

        var existingResponse = new VoteResponse
        {
            VoteId = vote.Id,
            MemberId = memberId,
            SelectedOption = "yes",
            VotedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.VoteResponses.Add(existingResponse);
        await context.SaveChangesAsync();

        var request = new CastVoteRequest
        {
            SelectedOption = "no"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CastVoteAsync(vote.Id, request, memberId));
    }

    [Fact]
    public async Task GetVoteStatusAsync_ReturnsVoteWithResponses()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new DigitalStokvel.GovernanceService.Services.GovernanceService(context, _mockLogger.Object);

        var groupId = Guid.NewGuid();
        var vote = new Vote
        {
            GroupId = groupId,
            InitiatedBy = Guid.NewGuid(),
            Subject = "Test vote",
            Options = JsonSerializer.Serialize(new List<string> { "yes", "no", "abstain" }),
            VotingStartsAt = DateTime.UtcNow.AddHours(-1),
            VotingEndsAt = DateTime.UtcNow.AddHours(47),
            Status = VoteStatus.Active,
            Results = "{\"yes\": 2, \"no\": 1}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Votes.Add(vote);

        // Add vote responses
        for (int i = 0; i < 3; i++)
        {
            var voteResponse = new VoteResponse
            {
                VoteId = vote.Id,
                MemberId = Guid.NewGuid(),
                SelectedOption = i < 2 ? "yes" : "no",
                VotedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.VoteResponses.Add(voteResponse);
        }
        await context.SaveChangesAsync();

        // Act
        var response = await service.GetVoteStatusAsync(vote.Id);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(vote.Id, response.Id);
        Assert.Equal(groupId, response.GroupId);
        Assert.Equal("Test vote", response.Subject);
        Assert.Equal(3, response.CurrentVotes);
        Assert.Equal("active", response.Status);
    }

    [Fact]
    public async Task RaiseDisputeAsync_CreatesDispute()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new DigitalStokvel.GovernanceService.Services.GovernanceService(context, _mockLogger.Object);

        var request = new RaiseDisputeRequest
        {
            GroupId = Guid.NewGuid(),
            DisputeType = "MissedPayment",
            Description = "Member has not paid contribution for 3 months",
            Evidence = new List<EvidenceItem>
            {
                new EvidenceItem
                {
                    Type = "document",
                    Url = "https://storage.example.com/payment-records.pdf"
                }
            }
        };
        var userId = Guid.NewGuid();

        // Act
        var disputeResponse = await service.RaiseDisputeAsync(request, userId);

        // Assert
        Assert.NotNull(disputeResponse);
        Assert.Equal(request.GroupId, disputeResponse.GroupId);
        Assert.Equal(request.DisputeType.ToLowerInvariant(), disputeResponse.DisputeType.ToLowerInvariant());
        Assert.Equal("open", disputeResponse.Status);

        // Verify dispute was saved
        var dispute = await context.Disputes.FirstOrDefaultAsync(d => d.Id == disputeResponse.Id);
        Assert.NotNull(dispute);
        Assert.Equal(DisputeStatus.Open, dispute.Status);
        Assert.Equal(userId, dispute.RaisedBy);
    }

    [Fact]
    public async Task GetDisputeDetailsAsync_ReturnsDisputeDetails()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new DigitalStokvel.GovernanceService.Services.GovernanceService(context, _mockLogger.Object);

        var groupId = Guid.NewGuid();
        var raisedBy = Guid.NewGuid();
        var dispute = new Dispute
        {
            GroupId = groupId,
            RaisedBy = raisedBy,
            DisputeType = DisputeType.MissedPayment,
            Description = "Test dispute",
            Evidence = "[]",
            Status = DisputeStatus.Open,
            RaisedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Disputes.Add(dispute);
        await context.SaveChangesAsync();

        // Act
        var detailsResponse = await service.GetDisputeDetailsAsync(dispute.Id);

        // Assert
        Assert.NotNull(detailsResponse);
        Assert.Equal(dispute.Id, detailsResponse.Id);
        Assert.Equal(groupId, detailsResponse.GroupId);
        Assert.Contains("missed", detailsResponse.DisputeType.ToLowerInvariant());
        Assert.Equal("Test dispute", detailsResponse.Description);
        Assert.Equal("open", detailsResponse.Status);
        Assert.NotNull(detailsResponse.RaisedBy);
        Assert.Equal(raisedBy, detailsResponse.RaisedBy.Id);
    }
}
