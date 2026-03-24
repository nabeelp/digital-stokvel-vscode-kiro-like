using DigitalStokvel.UssdGateway.Data;
using DigitalStokvel.UssdGateway.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace DigitalStokvel.UssdGateway.Services;

/// <summary>
/// Implementation of USSD session service with Redis and database persistence
/// Sessions have 120-second timeout per design requirements
/// </summary>
public class UssdSessionService : IUssdSessionService
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<UssdSessionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly int _sessionTimeoutSeconds;

    public UssdSessionService(
        ApplicationDbContext context,
        IDistributedCache cache,
        ILogger<UssdSessionService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _configuration = configuration;
        _sessionTimeoutSeconds = configuration.GetValue<int>("UssdSettings:SessionTimeoutSeconds", 120);
    }

    public async Task<UssdSession> GetOrCreateSessionAsync(string sessionId, string phoneNumber, string language)
    {
        _logger.LogInformation("Getting or creating USSD session {SessionId} for {PhoneNumber}", sessionId, phoneNumber);

        // Try to get from Redis cache first
        var cacheKey = $"ussd:session:{sessionId}";
        var cachedSession = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedSession))
        {
            _logger.LogDebug("Session {SessionId} found in cache", sessionId);
            var session = JsonSerializer.Deserialize<UssdSession>(cachedSession);
            if (session != null && !session.IsTerminated && session.ExpiresAt > DateTime.UtcNow)
            {
                // Update last activity
                session.LastActivityAt = DateTime.UtcNow;
                await UpdateSessionAsync(session);
                return session;
            }
        }

        // Try to get from database
        var dbSession = await _context.UssdSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId && !s.IsTerminated);

        if (dbSession != null && dbSession.ExpiresAt > DateTime.UtcNow)
        {
            _logger.LogDebug("Session {SessionId} found in database", sessionId);
            dbSession.LastActivityAt = DateTime.UtcNow;
            await UpdateSessionAsync(dbSession);
            return dbSession;
        }

        // Create new session
        _logger.LogInformation("Creating new USSD session {SessionId}", sessionId);
        var newSession = new UssdSession
        {
            SessionId = sessionId,
            PhoneNumber = phoneNumber,
            Language = language,
            MenuLevel = 1,
            Context = "{}",
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddSeconds(_sessionTimeoutSeconds),
            IsTerminated = false
        };

        _context.UssdSessions.Add(newSession);
        await _context.SaveChangesAsync();

        // Store in Redis
        await UpdateSessionAsync(newSession);

        return newSession;
    }

    public async Task UpdateSessionAsync(UssdSession session)
    {
        _logger.LogDebug("Updating USSD session {SessionId}", session.SessionId);

        // Update database
        _context.Entry(session).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        // Update Redis cache
        var cacheKey = $"ussd:session:{session.SessionId}";
        var serialized = JsonSerializer.Serialize(session);
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = session.ExpiresAt
        };

        await _cache.SetStringAsync(cacheKey, serialized, cacheOptions);
    }

    public async Task TerminateSessionAsync(string sessionId)
    {
        _logger.LogInformation("Terminating USSD session {SessionId}", sessionId);

        var session = await _context.UssdSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session != null)
        {
            session.IsTerminated = true;
            await _context.SaveChangesAsync();
        }

        // Remove from Redis
        var cacheKey = $"ussd:session:{sessionId}";
        await _cache.RemoveAsync(cacheKey);
    }

    public async Task CleanupExpiredSessionsAsync()
    {
        _logger.LogInformation("Cleaning up expired USSD sessions");

        var expiredSessions = await _context.UssdSessions
            .Where(s => !s.IsTerminated && s.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        foreach (var session in expiredSessions)
        {
            session.IsTerminated = true;
            var cacheKey = $"ussd:session:{session.SessionId}";
            await _cache.RemoveAsync(cacheKey);
        }

        if (expiredSessions.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cleaned up {Count} expired sessions", expiredSessions.Count);
        }
    }
}
