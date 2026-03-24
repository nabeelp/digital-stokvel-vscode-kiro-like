using Microsoft.Extensions.Logging;

namespace DigitalStokvel.Common.Logging;

/// <summary>
/// Standardized log messages for Digital Stokvel operations
/// </summary>
public static class LogMessages
{
    // Group Service Messages
    public static class GroupService
    {
        public static void GroupCreated(ILogger logger, Guid groupId, string groupName, Guid createdBy)
        {
            logger.LogInformation(
                "Group created: {GroupId} '{GroupName}' by user {UserId}",
                groupId, groupName, createdBy);
        }

        public static void MemberAdded(ILogger logger, Guid groupId, Guid userId, string role)
        {
            logger.LogInformation(
                "Member added to group {GroupId}: User {UserId} as {Role}",
                groupId, userId, role);
        }

        public static void GroupAtCapacity(ILogger logger, Guid groupId, int currentMembers, int maxMembers)
        {
            logger.LogWarning(
                "Group {GroupId} is at capacity: {CurrentMembers}/{MaxMembers}",
                groupId, currentMembers, maxMembers);
        }

        public static void GroupNotFound(ILogger logger, Guid groupId)
        {
            logger.LogWarning("Group not found: {GroupId}", groupId);
        }
    }

    // Contribution Service Messages
    public static class ContributionService
    {
        public static void ContributionReceived(ILogger logger, Guid contributionId, Guid groupId, Guid memberId, decimal amount)
        {
            logger.LogInformation(
                "Contribution received: {ContributionId} for group {GroupId} from member {MemberId}, amount: R{Amount:N2}",
                contributionId, groupId, memberId, amount);
        }

        public static void PaymentProcessing(ILogger logger, Guid contributionId, string transactionRef)
        {
            logger.LogInformation(
                "Processing payment for contribution {ContributionId}, transaction: {TransactionRef}",
                contributionId, transactionRef);
        }

        public static void PaymentFailed(ILogger logger, Guid contributionId, string reason)
        {
            logger.LogError(
                "Payment failed for contribution {ContributionId}: {Reason}",
                contributionId, reason);
        }

        public static void LedgerEntryCreated(ILogger logger, Guid ledgerId, Guid contributionId, decimal amount)
        {
            logger.LogInformation(
                "Ledger entry created: {LedgerId} for contribution {ContributionId}, amount: R{Amount:N2}",
                ledgerId, contributionId, amount);
        }

        public static void ContributionOverdue(ILogger logger, Guid contributionId, Guid memberId, DateTime dueDate)
        {
            logger.LogWarning(
                "Contribution {ContributionId} overdue for member {MemberId}, due date: {DueDate}",
                contributionId, memberId, dueDate);
        }
    }

    // Payout Service Messages
    public static class PayoutService
    {
        public static void PayoutInitiated(ILogger logger, Guid payoutId, Guid groupId, decimal totalAmount, Guid initiatedBy)
        {
            logger.LogInformation(
                "Payout initiated: {PayoutId} for group {GroupId}, amount: R{TotalAmount:N2}, initiated by {UserId}",
                payoutId, groupId, totalAmount, initiatedBy);
        }

        public static void PayoutApproved(ILogger logger, Guid payoutId, Guid approvedBy)
        {
            logger.LogInformation(
                "Payout approved: {PayoutId} by {UserId}",
                payoutId, approvedBy);
        }

        public static void DualApprovalComplete(ILogger logger, Guid payoutId, Guid initiator, Guid approver)
        {
            logger.LogInformation(
                "Dual approval complete for payout {PayoutId}: Initiator {InitiatorId}, Approver {ApproverId}",
                payoutId, initiator, approver);
        }

        public static void DisbursementProcessed(ILogger logger, Guid disbursementId, Guid memberId, decimal amount, string transactionRef)
        {
            logger.LogInformation(
                "Disbursement processed: {DisbursementId} to member {MemberId}, amount: R{Amount:N2}, transaction: {TransactionRef}",
                disbursementId, memberId, amount, transactionRef);
        }

        public static void PayoutFailed(ILogger logger, Guid payoutId, string reason)
        {
            logger.LogError(
                "Payout failed: {PayoutId}, reason: {Reason}",
                payoutId, reason);
        }
    }

    // Governance Service Messages
    public static class GovernanceService
    {
        public static void VoteCreated(ILogger logger, Guid voteId, Guid groupId, string voteType, Guid initiatedBy)
        {
            logger.LogInformation(
                "Vote created: {VoteId} in group {GroupId}, type: {VoteType}, initiated by {UserId}",
                voteId, groupId, voteType, initiatedBy);
        }

        public static void VoteCast(ILogger logger, Guid voteId, Guid userId, string response)
        {
            logger.LogInformation(
                "Vote cast: {VoteId} by user {UserId}, response: {Response}",
                voteId, userId, response);
        }

        public static void QuorumReached(ILogger logger, Guid voteId, int votesReceived, int requiredQuorum)
        {
            logger.LogInformation(
                "Quorum reached for vote {VoteId}: {VotesReceived}/{RequiredQuorum}",
                voteId, votesReceived, requiredQuorum);
        }

        public static void DisputeRaised(ILogger logger, Guid disputeId, Guid groupId, Guid raisedBy)
        {
            logger.LogWarning(
                "Dispute raised: {DisputeId} in group {GroupId} by user {UserId}",
                disputeId, groupId, raisedBy);
        }

        public static void LateFeeApplied(ILogger logger, Guid contributionId, decimal lateFee)
        {
            logger.LogInformation(
                "Late fee applied to contribution {ContributionId}: R{LateFee:N2}",
                contributionId, lateFee);
        }
    }

    // Security Messages
    public static class Security
    {
        public static void AuthenticationSuccessful(ILogger logger, Guid userId, string phoneNumber)
        {
            logger.LogInformation(
                "User authenticated successfully: {UserId}, phone: {PhoneNumber}",
                userId, phoneNumber);
        }

        public static void AuthenticationFailed(ILogger logger, string phoneNumber, string reason)
        {
            logger.LogWarning(
                "Authentication failed for phone: {PhoneNumber}, reason: {Reason}",
                phoneNumber, reason);
        }

        public static void UnauthorizedAccess(ILogger logger, Guid userId, string resource, string action)
        {
            logger.LogWarning(
                "Unauthorized access attempt: User {UserId} tried to {Action} {Resource}",
                userId, action, resource);
        }

        public static void SensitiveDataAccessed(ILogger logger, Guid userId, string dataType)
        {
            logger.LogInformation(
                "Sensitive data accessed: {DataType} by user {UserId}",
                dataType, userId);
        }
    }

    // Performance Messages
    public static class Performance
    {
        public static void SlowQuery(ILogger logger, string queryName, long durationMs)
        {
            logger.LogWarning(
                "Slow query detected: {QueryName} took {Duration}ms",
                queryName, durationMs);
        }

        public static void CacheHit(ILogger logger, string cacheKey)
        {
            logger.LogDebug("Cache hit: {CacheKey}", cacheKey);
        }

        public static void CacheMiss(ILogger logger, string cacheKey)
        {
            logger.LogDebug("Cache miss: {CacheKey}", cacheKey);
        }

        public static void ExternalServiceCall(ILogger logger, string serviceName, string operation, long durationMs)
        {
            logger.LogInformation(
                "External service call: {ServiceName}.{Operation} completed in {Duration}ms",
                serviceName, operation, durationMs);
        }
    }

    // Integration Messages
    public static class Integration
    {
        public static void CbsAccountCreated(ILogger logger, Guid groupId, string accountNumber)
        {
            logger.LogInformation(
                "CBS account created for group {GroupId}: {AccountNumber}",
                groupId, accountNumber);
        }

        public static void PaymentGatewayError(ILogger logger, string transactionRef, string errorMessage)
        {
            logger.LogError(
                "Payment gateway error for transaction {TransactionRef}: {Error}",
                transactionRef, errorMessage);
        }

        public static void SmsSent(ILogger logger, string phoneNumber, string messageType)
        {
            logger.LogInformation(
                "SMS sent to {PhoneNumber}, type: {MessageType}",
                phoneNumber, messageType);
        }

        public static void UssdSessionStarted(ILogger logger, string sessionId, string phoneNumber)
        {
            logger.LogInformation(
                "USSD session started: {SessionId} for {PhoneNumber}",
                sessionId, phoneNumber);
        }
    }
}
