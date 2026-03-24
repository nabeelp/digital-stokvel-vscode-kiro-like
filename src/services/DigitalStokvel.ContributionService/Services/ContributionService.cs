using DigitalStokvel.ContributionService.Data;
using DigitalStokvel.ContributionService.DTOs;
using DigitalStokvel.ContributionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.ContributionService.Services;

/// <summary>
/// Implementation of contribution service business logic
/// </summary>
public class ContributionService : IContributionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ContributionService> _logger;

    public ContributionService(ApplicationDbContext context, ILogger<ContributionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CreateContributionResponse> CreateContributionAsync(CreateContributionRequest request, Guid userId)
    {
        _logger.LogInformation("Creating contribution for group {GroupId}, amount {Amount}", request.GroupId, request.Amount);

        // Create contribution record
        var contribution = new Contribution
        {
            GroupId = request.GroupId,
            MemberId = userId, // Assuming userId maps to member_id
            Amount = request.Amount,
            Status = ContributionStatus.Pending,
            DueDate = DateTime.UtcNow, // Set to current date for immediate payment
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Contributions.Add(contribution);
        await _context.SaveChangesAsync();

        // In a real implementation, this would call payment gateway
        // For now, simulate successful payment
        var transactionRef = GenerateTransactionRef();
        contribution.TransactionRef = transactionRef;
        contribution.Status = ContributionStatus.Paid;
        contribution.PaidAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Create immutable ledger entry
        var ledgerEntry = new ContributionLedger
        {
            ContributionId = contribution.Id,
            GroupId = contribution.GroupId,
            MemberId = contribution.MemberId,
            Amount = contribution.Amount,
            TransactionRef = transactionRef,
            Metadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                payment_method = request.PaymentMethod,
                gateway_response = "success",
                processed_at = DateTime.UtcNow
            }),
            RecordedAt = DateTime.UtcNow
        };

        _context.ContributionLedger.Add(ledgerEntry);
        await _context.SaveChangesAsync();

        // Generate receipt (simulated)
        var receipt = new ReceiptDto
        {
            ReceiptNumber = GenerateReceiptNumber(),
            Date = DateTime.UtcNow,
            GroupName = "Sample Group", // TODO: Fetch actual group name
            Amount = contribution.Amount,
            BalanceAfter = 0, // TODO: Calculate actual balance
            PdfUrl = null // TODO: Generate PDF receipt
        };

        var response = new CreateContributionResponse
        {
            Id = contribution.Id,
            GroupId = contribution.GroupId,
            Amount = contribution.Amount,
            TransactionRef = transactionRef,
            Status = contribution.Status.ToString().ToLower(),
            Receipt = receipt,
            PaidAt = contribution.PaidAt
        };

        _logger.LogInformation("Contribution {ContributionId} created successfully", contribution.Id);

        return response;
    }

    public async Task<ContributionHistoryResponse> GetGroupContributionsAsync(
        Guid groupId,
        Guid? memberId,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int limit)
    {
        _logger.LogInformation("Fetching contribution history for group {GroupId}", groupId);

        var query = _context.Contributions
            .Where(c => c.GroupId == groupId);

        if (memberId.HasValue)
        {
            query = query.Where(c => c.MemberId == memberId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= toDate.Value);
        }

        var totalCount = await query.CountAsync();

        var contributions = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(c => new ContributionDto
            {
                Id = c.Id,
                Member = new MemberDto
                {
                    Id = c.MemberId,
                    Name = "Member Name", // TODO: Join with user/member table
                    Phone = "+27XXXXXXXXX" // TODO: Fetch actual phone
                },
                Amount = c.Amount,
                TransactionRef = c.TransactionRef ?? "",
                Status = c.Status.ToString().ToLower(),
                DueDate = c.DueDate,
                PaidAt = c.PaidAt
            })
            .ToListAsync();

        // Calculate summary statistics
        var allContributions = await _context.Contributions
            .Where(c => c.GroupId == groupId)
            .ToListAsync();

        var summary = new ContributionSummaryDto
        {
            TotalContributions = allContributions.Where(c => c.Status == ContributionStatus.Paid).Sum(c => c.Amount),
            TotalMembersPaid = allContributions.Where(c => c.Status == ContributionStatus.Paid)
                .Select(c => c.MemberId)
                .Distinct()
                .Count(),
            TotalMembersPending = allContributions.Where(c => c.Status == ContributionStatus.Pending)
                .Select(c => c.MemberId)
                .Distinct()
                .Count(),
            TotalMembersOverdue = allContributions.Where(c => c.Status == ContributionStatus.Pending && c.DueDate < DateTime.UtcNow)
                .Select(c => c.MemberId)
                .Distinct()
                .Count()
        };

        var pagination = new PaginationDto
        {
            Page = page,
            Limit = limit,
            TotalItems = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)limit)
        };

        return new ContributionHistoryResponse
        {
            Contributions = contributions,
            Summary = summary,
            Pagination = pagination
        };
    }

    public async Task<CreateRecurringPaymentResponse> CreateRecurringPaymentAsync(CreateRecurringPaymentRequest request, Guid userId)
    {
        _logger.LogInformation("Creating recurring payment for group {GroupId}, user {UserId}", request.GroupId, userId);

        // Parse frequency enum
        if (!Enum.TryParse<ContributionFrequency>(request.Frequency, true, out var frequency))
        {
            throw new ArgumentException($"Invalid frequency: {request.Frequency}");
        }

        // Check if user already has an active recurring payment for this group
        var existingActive = await _context.RecurringPayments
            .Where(r => r.MemberId == userId && r.GroupId == request.GroupId && r.Status == RecurringPaymentStatus.Active)
            .FirstOrDefaultAsync();

        if (existingActive != null)
        {
            throw new InvalidOperationException("Active recurring payment already exists for this group");
        }

        var recurringPayment = new RecurringPayment
        {
            MemberId = userId,
            GroupId = request.GroupId,
            Amount = request.Amount,
            Frequency = frequency,
            Status = RecurringPaymentStatus.Active,
            NextPaymentDate = request.StartDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.RecurringPayments.Add(recurringPayment);
        await _context.SaveChangesAsync();

        var response = new CreateRecurringPaymentResponse
        {
            Id = recurringPayment.Id,
            GroupId = recurringPayment.GroupId,
            Amount = recurringPayment.Amount,
            Frequency = recurringPayment.Frequency.ToString().ToLower(),
            Status = recurringPayment.Status.ToString().ToLower(),
            NextPaymentDate = recurringPayment.NextPaymentDate,
            CreatedAt = recurringPayment.CreatedAt
        };

        _logger.LogInformation("Recurring payment {RecurringPaymentId} created successfully", recurringPayment.Id);

        return response;
    }

    public async Task<RecurringPaymentsResponse> GetUserRecurringPaymentsAsync(Guid userId)
    {
        _logger.LogInformation("Fetching recurring payments for user {UserId}", userId);

        var recurringPayments = await _context.RecurringPayments
            .Where(r => r.MemberId == userId && r.Status == RecurringPaymentStatus.Active)
            .Select(r => new RecurringPaymentDto
            {
                Id = r.Id,
                GroupName = "Group Name", // TODO: Join with groups table
                Amount = r.Amount,
                Frequency = r.Frequency.ToString().ToLower(),
                Status = r.Status.ToString().ToLower(),
                NextPaymentDate = r.NextPaymentDate
            })
            .ToListAsync();

        return new RecurringPaymentsResponse
        {
            RecurringPayments = recurringPayments,
            TotalCount = recurringPayments.Count
        };
    }

    /// <summary>
    /// Generate unique transaction reference
    /// </summary>
    private string GenerateTransactionRef()
    {
        var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var random = new Random().Next(1000, 9999);
        return $"TXN-{date}-{random}";
    }

    /// <summary>
    /// Generate unique receipt number
    /// </summary>
    private string GenerateReceiptNumber()
    {
        var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var random = new Random().Next(1000, 9999);
        return $"RCP-{date}-{random}";
    }
}
