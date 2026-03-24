using DigitalStokvel.PayoutService.Data;
using DigitalStokvel.PayoutService.DTOs;
using DigitalStokvel.PayoutService.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.PayoutService.Services;

/// <summary>
/// Service implementation for payout operations with dual approval workflow.
/// </summary>
public class PayoutService : IPayoutService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PayoutService> _logger;

    public PayoutService(ApplicationDbContext context, ILogger<PayoutService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Initiates a new payout for a group (typically by Chairperson).
    /// Creates a payout request with status "pending_approval".
    /// </summary>
    public async Task<InitiatePayoutResponse> InitiatePayoutAsync(InitiatePayoutRequest request, Guid userId)
    {
        _logger.LogInformation("Initiating payout for group {GroupId} by user {UserId}", request.GroupId, userId);

        // Validate payout type
        if (!Enum.TryParse<PayoutType>(request.PayoutType, true, out var payoutType))
        {
            throw new ArgumentException($"Invalid payout type: {request.PayoutType}");
        }

        // TODO: Validate that userId is a member of the group with appropriate role (Chairperson)
        // TODO: Validate that group has sufficient balance for the payout amount
        // TODO: Validate that recipient member exists in the group (for rotating payouts)

        // Create payout entity
        var payout = new Payout
        {
            GroupId = request.GroupId,
            PayoutType = payoutType,
            TotalAmount = request.Amount,
            Status = PayoutStatus.PendingApproval,
            InitiatedBy = userId,
            InitiatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Payouts.Add(payout);

        // Create disbursement(s) based on payout type
        if (payoutType == PayoutType.Rotating && request.RecipientMemberId.HasValue)
        {
            // Single disbursement for rotating payout
            var disbursement = new PayoutDisbursement
            {
                PayoutId = payout.Id,
                MemberId = request.RecipientMemberId.Value,
                Amount = request.Amount,
                Status = PayoutStatus.PendingApproval,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PayoutDisbursements.Add(disbursement);
        }
        else if (payoutType == PayoutType.YearEndPot)
        {
            // TODO: Create disbursements for all eligible group members
            // Calculate equal shares or proportional shares based on contributions
            _logger.LogWarning("Year-end pot disbursement calculation not yet implemented");
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Payout {PayoutId} initiated successfully for group {GroupId}", payout.Id, request.GroupId);

        // Build response (with placeholder initiator data)
        return new InitiatePayoutResponse
        {
            Id = payout.Id,
            GroupId = payout.GroupId,
            PayoutType = payout.PayoutType.ToString(),
            TotalAmount = payout.TotalAmount,
            Status = payout.Status.ToString(),
            InitiatedBy = new InitiatorDto
            {
                Id = userId,
                Name = "Placeholder Name", // TODO: Fetch actual user/member name
                Role = "chairperson" // TODO: Fetch actual role from group membership
            },
            InitiatedAt = payout.InitiatedAt,
            RequiresApprovalFrom = "treasurer"
        };
    }

    /// <summary>
    /// Approves a payout (typically by Treasurer).
    /// Updates status to "approved" and triggers execution.
    /// </summary>
    public async Task<ApprovePayoutResponse> ApprovePayoutAsync(Guid payoutId, ApprovePayoutRequest request, Guid userId)
    {
        _logger.LogInformation("Approving payout {PayoutId} by user {UserId}", payoutId, userId);

        // Retrieve payout
        var payout = await _context.Payouts
            .Include(p => p.Disbursements)
            .FirstOrDefaultAsync(p => p.Id == payoutId);

        if (payout == null)
        {
            throw new InvalidOperationException($"Payout {payoutId} not found");
        }

        // Validate payout is in pending_approval status
        if (payout.Status != PayoutStatus.PendingApproval)
        {
            throw new InvalidOperationException($"Payout {payoutId} is not pending approval (current status: {payout.Status})");
        }

        // TODO: Validate that userId is a member of the group with Treasurer role
        // TODO: Validate PIN for authorization
        // TODO: Validate that approver is not the same as initiator (dual approval constraint)
        // TODO: Validate that group has sufficient balance for the payout amount

        // Update payout status to approved
        payout.Status = PayoutStatus.Approved;
        payout.ApprovedBy = userId;
        payout.ApprovedAt = DateTime.UtcNow;
        payout.UpdatedAt = DateTime.UtcNow;

        // Update disbursement statuses to approved
        foreach (var disbursement in payout.Disbursements)
        {
            disbursement.Status = PayoutStatus.Approved;
            disbursement.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Payout {PayoutId} approved by user {UserId}, executing disbursements...", payoutId, userId);

        // Execute disbursements asynchronously
        await ExecuteDisbursementsAsync(payout);

        _logger.LogInformation("Payout {PayoutId} executed successfully", payoutId);

        // Build response (with placeholder approver data)
        return new ApprovePayoutResponse
        {
            Id = payout.Id,
            Status = payout.Status.ToString(),
            ApprovedBy = new ApproverDto
            {
                Id = userId,
                Name = "Placeholder Name", // TODO: Fetch actual user/member name
                Role = "treasurer" // TODO: Fetch actual role from group membership
            },
            ApprovedAt = payout.ApprovedAt.Value,
            ExecutionStatus = payout.Status == PayoutStatus.Executed ? "completed" : "processing"
        };
    }

    /// <summary>
    /// Retrieves the status of a payout including disbursements.
    /// </summary>
    public async Task<PayoutStatusResponse> GetPayoutStatusAsync(Guid payoutId)
    {
        _logger.LogInformation("Retrieving payout status for {PayoutId}", payoutId);

        var payout = await _context.Payouts
            .Include(p => p.Disbursements)
            .FirstOrDefaultAsync(p => p.Id == payoutId);

        if (payout == null)
        {
            throw new InvalidOperationException($"Payout {payoutId} not found");
        }

        // Build response with placeholder member data
        return new PayoutStatusResponse
        {
            Id = payout.Id,
            GroupId = payout.GroupId,
            PayoutType = payout.PayoutType.ToString(),
            TotalAmount = payout.TotalAmount,
            Status = payout.Status.ToString(),
            Disbursements = payout.Disbursements.Select(d => new DisbursementDto
            {
                Member = new MemberDto
                {
                    Id = d.MemberId,
                    Name = "Placeholder Member", // TODO: Fetch actual member name
                    Phone = "+27821234567" // TODO: Fetch actual member phone
                },
                Amount = d.Amount,
                TransactionRef = d.TransactionRef,
                Status = d.Status.ToString(),
                ExecutedAt = d.ExecutedAt
            }).ToList(),
            InitiatedAt = payout.InitiatedAt,
            ApprovedAt = payout.ApprovedAt,
            ExecutedAt = payout.ExecutedAt,
            InitiatedBy = new InitiatorDto
            {
                Id = payout.InitiatedBy,
                Name = "Placeholder Initiator", // TODO: Fetch actual initiator name
                Role = "chairperson" // TODO: Fetch actual role
            },
            ApprovedBy = payout.ApprovedBy.HasValue ? new ApproverDto
            {
                Id = payout.ApprovedBy.Value,
                Name = "Placeholder Approver", // TODO: Fetch actual approver name
                Role = "treasurer" // TODO: Fetch actual role
            } : null
        };
    }

    /// <summary>
    /// Executes disbursements for an approved payout.
    /// Simulates EFT disbursement via payment gateway.
    /// </summary>
    private async Task ExecuteDisbursementsAsync(Payout payout)
    {
        _logger.LogInformation("Executing disbursements for payout {PayoutId}", payout.Id);

        var allSucceeded = true;

        foreach (var disbursement in payout.Disbursements)
        {
            try
            {
                // TODO: Integrate with actual payment gateway/bank API for EFT disbursement
                // Simulate payment gateway call
                await Task.Delay(100); // Simulate network latency

                // Generate transaction reference
                var transactionRef = GenerateTransactionRef();

                // Update disbursement
                disbursement.Status = PayoutStatus.Executed;
                disbursement.TransactionRef = transactionRef;
                disbursement.ExecutedAt = DateTime.UtcNow;
                disbursement.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Disbursement {DisbursementId} executed successfully with transaction ref {TransactionRef}",
                    disbursement.Id, transactionRef);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute disbursement {DisbursementId}", disbursement.Id);
                disbursement.Status = PayoutStatus.Failed;
                disbursement.UpdatedAt = DateTime.UtcNow;
                allSucceeded = false;
            }
        }

        // Update payout status based on disbursement results
        if (allSucceeded)
        {
            payout.Status = PayoutStatus.Executed;
            payout.ExecutedAt = DateTime.UtcNow;
        }
        else
        {
            payout.Status = PayoutStatus.Failed;
            _logger.LogWarning("Payout {PayoutId} partially or fully failed", payout.Id);
        }

        payout.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Generates a unique transaction reference for a disbursement.
    /// Format: PAY-YYYY-MM-DD-XXXX
    /// </summary>
    private string GenerateTransactionRef()
    {
        var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var random = new Random().Next(1000, 9999);
        return $"PAY-{date}-{random}";
    }
}
