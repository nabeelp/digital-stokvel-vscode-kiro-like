# Local Redis 7+ Setup Guide

**Version:** 1.0  
**Date:** March 2026  
**Applies To:** Digital Stokvel Banking - Development Environment

---

## Table of Contents

1. [Overview](#overview)
2. [Quick Start (Docker Compose - Recommended)](#quick-start-docker-compose---recommended)
3. [Manual Redis Installation](#manual-redis-installation)
4. [Redis Configuration](#redis-configuration)
5. [Connection Configuration](#connection-configuration)
6. [Redis Insight Setup](#redis-insight-setup)
7. [Common Operations](#common-operations)
8. [.NET Integration](#net-integration)
9. [Performance Optimization](#performance-optimization)
10. [Troubleshooting](#troubleshooting)

---

## Overview

### Purpose

Redis serves as the high-performance caching layer for the Digital Stokvel Banking platform, providing:

- **Session Management**: USSD session state storage (120-second timeout)
- **API Response Caching**: Frequently accessed data with sub-millisecond latency
- **Rate Limiting**: Request throttling (100 req/min per user)
- **Distributed Locking**: Coordination across microservices
- **Pub/Sub Messaging**: Real-time notifications and events

### Architecture Role

```
┌─────────────────┐
│  API Gateway    │
│   & Services    │
└────────┬────────┘
         │
    ┌────▼─────┐
    │  Redis   │ ◄─── L2 Cache (sub-millisecond latency)
    │  Cache   │      Session state (USSD)
    └────┬─────┘      Rate limiting counters
         │            Distributed locks
    ┌────▼─────┐
    │PostgreSQL│ ◄─── Primary data store
    │ Primary  │
    └──────────┘
```

### Requirements

- **Version**: Redis 7.0 or higher
- **Memory**: 512 MB minimum (2 GB recommended for development)
- **Persistence**: AOF (Append-Only File) enabled for durability
- **Security**: Password authentication mandatory
- **Port**: 6379 (default)

---

## Quick Start (Docker Compose - Recommended)

### Prerequisites

- Docker Desktop installed and running
- `.env` file configured in project root

### Step 1: Configure Environment Variables

Create or update `.env` file in the project root:

```bash
# Redis Configuration
REDIS_PASSWORD=your_secure_redis_password_here
```

**Security Notes:**
- Use a strong password (16+ characters, alphanumeric + special characters)
- Never commit `.env` to version control
- See `.env.example` for template

### Step 2: Start Redis Container

Start all development services including Redis:

```bash
docker-compose up -d redis
```

Or start all services:

```bash
docker-compose up -d
```

### Step 3: Verify Redis is Running

Check container status:

```bash
docker-compose ps redis
```

Expected output:
```
NAME                        STATUS      PORTS
digitalstokvel-redis        Up          0.0.0.0:6379->6379/tcp
```

Test connection with redis-cli:

```bash
docker exec -it digitalstokvel-redis redis-cli -a your_password PING
```

Expected response: `PONG`

### Step 4: Test Basic Operations

```bash
# Set a test key
docker exec -it digitalstokvel-redis redis-cli -a your_password SET test:key "Hello Redis"

# Get the test key
docker exec -it digitalstokvel-redis redis-cli -a your_password GET test:key

# Expected output: "Hello Redis"

# List all keys (should show test:key)
docker exec -it digitalstokvel-redis redis-cli -a your_password KEYS "*"
```

### Docker Compose Configuration Details

The `docker-compose.yml` configures Redis as follows:

```yaml
redis:
  image: redis:7-alpine
  container_name: digitalstokvel-redis
  ports:
    - "6379:6379"
  command: redis-server --appendonly yes --requirepass ${REDIS_PASSWORD}
  volumes:
    - redis_data:/data
  healthcheck:
    test: ["CMD", "redis-cli", "--raw", "incr", "ping"]
    interval: 10s
    timeout: 5s
    retries: 5
```

**Configuration Features:**
- **Image**: `redis:7-alpine` (lightweight, production-grade)
- **Persistence**: AOF enabled (`--appendonly yes`)
- **Authentication**: Password-protected (`--requirepass`)
- **Volume**: `redis_data` for persistent storage
- **Health Check**: Automatic monitoring every 10 seconds

### Docker Compose Commands

```bash
# Start Redis only
docker-compose up -d redis

# Stop Redis
docker-compose stop redis

# View Redis logs
docker-compose logs -f redis

# Restart Redis
docker-compose restart redis

# Remove Redis container and volume (WARNING: deletes all data)
docker-compose down -v redis
```

---

## Manual Redis Installation

If you prefer not to use Docker or need Redis installed directly on your machine:

### Windows Installation

#### Option 1: Windows Subsystem for Linux (WSL) - Recommended

1. **Install WSL 2** (if not already installed):
   ```powershell
   wsl --install
   ```

2. **Install Redis in Ubuntu (WSL)**:
   ```bash
   sudo apt update
   sudo apt install redis-server
   ```

3. **Configure Redis**:
   ```bash
   sudo nano /etc/redis/redis.conf
   ```
   
   Update the following settings:
   ```
   bind 127.0.0.1
   port 6379
   requirepass your_secure_password
   appendonly yes
   appendfilename "appendonly.aof"
   ```

4. **Start Redis**:
   ```bash
   sudo service redis-server start
   ```

5. **Test Connection**:
   ```bash
   redis-cli -a your_secure_password PING
   ```

#### Option 2: Memurai (Native Windows Redis Alternative)

1. **Download Memurai**:
   - Visit: https://www.memurai.com/
   - Download Memurai Developer Edition (free)

2. **Install Memurai**:
   - Run the installer
   - Follow the installation wizard

3. **Configure Memurai**:
   - Configuration file: `C:\Program Files\Memurai\memurai.conf`
   - Set password: `requirepass your_secure_password`

4. **Start Memurai Service**:
   ```powershell
   net start Memurai
   ```

5. **Test Connection**:
   ```powershell
   memurai-cli -a your_secure_password PING
   ```

#### Option 3: Chocolatey (Redis for Windows)

```powershell
# Install Chocolatey (if not already installed)
Set-ExecutionPolicy Bypass -Scope Process -Force
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

# Install Redis
choco install redis-64 -y

# Start Redis service
redis-server --service-start

# Test connection
redis-cli ping
```

### macOS Installation

#### Option 1: Homebrew (Recommended)

1. **Install Redis**:
   ```bash
   brew install redis
   ```

2. **Configure Redis**:
   ```bash
   nano /opt/homebrew/etc/redis.conf
   ```
   
   Update settings:
   ```
   requirepass your_secure_password
   appendonly yes
   ```

3. **Start Redis as a Service**:
   ```bash
   brew services start redis
   ```

4. **Test Connection**:
   ```bash
   redis-cli -a your_secure_password PING
   ```

#### Option 2: Build from Source

1. **Download Redis**:
   ```bash
   curl -O https://download.redis.io/redis-stable.tar.gz
   tar xzvf redis-stable.tar.gz
   cd redis-stable
   ```

2. **Compile Redis**:
   ```bash
   make
   make test
   sudo make install
   ```

3. **Configure Redis**:
   ```bash
   sudo mkdir /etc/redis
   sudo cp redis.conf /etc/redis/redis.conf
   sudo nano /etc/redis/redis.conf
   ```

4. **Start Redis**:
   ```bash
   redis-server /etc/redis/redis.conf
   ```

### Linux Installation

#### Ubuntu/Debian

1. **Update Package Index**:
   ```bash
   sudo apt update
   ```

2. **Install Redis**:
   ```bash
   sudo apt install redis-server -y
   ```

3. **Configure Redis**:
   ```bash
   sudo nano /etc/redis/redis.conf
   ```
   
   Update settings:
   ```
   supervised systemd
   requirepass your_secure_password
   appendonly yes
   maxmemory 2gb
   maxmemory-policy allkeys-lru
   ```

4. **Enable and Start Redis**:
   ```bash
   sudo systemctl enable redis-server
   sudo systemctl start redis-server
   ```

5. **Verify Status**:
   ```bash
   sudo systemctl status redis-server
   ```

6. **Test Connection**:
   ```bash
   redis-cli -a your_secure_password PING
   ```

#### Fedora/RHEL/CentOS

1. **Enable EPEL Repository** (RHEL/CentOS only):
   ```bash
   sudo yum install epel-release -y
   ```

2. **Install Redis**:
   ```bash
   sudo yum install redis -y
   ```

3. **Configure Redis**:
   ```bash
   sudo nano /etc/redis.conf
   ```

4. **Enable and Start Redis**:
   ```bash
   sudo systemctl enable redis
   sudo systemctl start redis
   ```

5. **Test Connection**:
   ```bash
   redis-cli -a your_secure_password PING
   ```

---

## Redis Configuration

### Essential Configuration Settings

**Location**:
- Docker: Set via `docker-compose.yml` command arguments
- Linux: `/etc/redis/redis.conf`
- macOS (Homebrew): `/opt/homebrew/etc/redis.conf`
- Windows (WSL): `/etc/redis/redis.conf`

**Key Settings for Development**:

```conf
# Network
bind 127.0.0.1                    # Only accept local connections
port 6379                          # Default Redis port

# Security
requirepass your_secure_password   # Mandatory authentication

# Persistence (AOF - Append-Only File)
appendonly yes                     # Enable AOF for durability
appendfilename "appendonly.aof"    # AOF file name
appendfsync everysec               # Sync every second (good balance)

# Memory Management
maxmemory 2gb                      # Maximum memory limit
maxmemory-policy allkeys-lru       # Eviction policy (LRU = Least Recently Used)

# Logging
loglevel notice                    # Log verbosity
logfile /var/log/redis/redis-server.log

# Performance
timeout 0                          # Client idle timeout (0 = disabled)
tcp-keepalive 300                  # TCP keepalive
```

### Configuration for USSD Session Management

Redis is critical for USSD session state. Ensure these settings:

```conf
# Session timeout handling (120 seconds as per design)
# Sessions are managed at application level with TTL

# Increase max clients for concurrent USSD sessions
maxclients 10000

# Enable keyspace notifications for session expiration events
notify-keyspace-events Ex
```

### Applying Configuration Changes

**Docker Compose**:
```bash
docker-compose restart redis
```

**Linux (systemd)**:
```bash
sudo systemctl restart redis-server
```

**macOS (Homebrew)**:
```bash
brew services restart redis
```

---

## Connection Configuration

### Connection String Format

For .NET applications, use the StackExchange.Redis connection string format:

```
localhost:6379,password=your_secure_password,ssl=false,abortConnect=false,connectTimeout=5000,syncTimeout=5000
```

### Environment Variables

Set in `.env` file:

```bash
# Redis Connection
REDIS_CONNECTION_STRING=localhost:6379,password=your_secure_password,ssl=false,abortConnect=false
REDIS_PASSWORD=your_secure_password
REDIS_HOST=localhost
REDIS_PORT=6379
```

### Docker Compose Service-to-Service Connection

When connecting from other Docker Compose services:

```yaml
# Example: API service connecting to Redis
environment:
  REDIS_CONNECTION_STRING: "redis:6379,password=${REDIS_PASSWORD},ssl=false,abortConnect=false"
```

**Note**: Use service name `redis` as the hostname within Docker Compose network.

### Connection String Options Explained

| Option | Value | Purpose |
|--------|-------|---------|
| `password` | Your Redis password | Authentication |
| `ssl` | `false` (dev), `true` (prod) | Encrypt connection |
| `abortConnect` | `false` | Don't fail fast if Redis unavailable |
| `connectTimeout` | `5000` ms | Connection timeout |
| `syncTimeout` | `5000` ms | Synchronous operation timeout |
| `connectRetry` | `3` | Number of retry attempts |
| `defaultDatabase` | `0` | Default database index (0-15) |

### Testing Connection from .NET

Create a simple test in `Program.cs`:

```csharp
using StackExchange.Redis;

var connectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") 
    ?? "localhost:6379,password=your_password";

var redis = ConnectionMultiplexer.Connect(connectionString);
var db = redis.GetDatabase();

// Test SET
await db.StringSetAsync("test:connection", "Redis is working!");

// Test GET
var value = await db.StringGetAsync("test:connection");
Console.WriteLine($"Redis value: {value}");

// Expected output: "Redis is working!"
```

---

## Redis Insight Setup

**Redis Insight** is the official GUI for Redis, providing visualization, monitoring, and query tools.

### Installation

#### Option 1: Desktop Application

1. **Download Redis Insight**:
   - Visit: https://redis.com/redis-enterprise/redis-insight/
   - Download for your platform (Windows, macOS, Linux)

2. **Install**:
   - Run the installer
   - Follow the installation wizard

3. **Launch Redis Insight**

#### Option 2: Docker Container

```bash
docker run -d \
  --name redis-insight \
  -p 5540:5540 \
  -v redisinsight:/db \
  redis/redisinsight:latest
```

Access at: http://localhost:5540

### Connecting to Local Redis

1. **Open Redis Insight**

2. **Add Database**:
   - Click **"Add Redis Database"**
   - Select **"Connect to a Redis Database"**

3. **Configure Connection**:
   ```
   Host: localhost (or 127.0.0.1)
   Port: 6379
   Database Alias: Digital Stokvel - Local
   Username: (leave blank)
   Password: your_secure_password
   ```

4. **Test Connection**: Click **"Test Connection"**

5. **Save**: Click **"Add Redis Database"**

### Redis Insight Features

- **Browser**: Explore keys, view values, edit data
- **Workbench**: Execute Redis commands with autocomplete
- **Analysis**: Memory analysis, slow log, key patterns
- **Pub/Sub**: Monitor real-time events
- **Profiler**: Profile commands and identify bottlenecks

### Common Operations in Redis Insight

1. **View All Keys**:
   - Navigate to **Browser** tab
   - Search with pattern: `*`

2. **Inspect USSD Sessions**:
   - Search pattern: `ussd:session:*`
   - View session data and TTL

3. **Monitor Cache Hit Rate**:
   - Go to **Analysis** tab
   - View **INFO** statistics
   - Check `keyspace_hits` vs `keyspace_misses`

4. **Profiler**:
   - Enable profiler to see real-time commands
   - Identify slow operations

---

## Common Operations

### Using redis-cli

#### Connect to Redis

**Docker**:
```bash
docker exec -it digitalstokvel-redis redis-cli -a your_password
```

**Local Installation**:
```bash
redis-cli -a your_password
```

#### Basic Commands

```bash
# Server Info
INFO
INFO stats
INFO memory

# Key Operations
SET mykey "Hello World"              # Set a key
GET mykey                            # Get a key
DEL mykey                            # Delete a key
EXISTS mykey                         # Check if key exists
KEYS *                               # List all keys (use with caution in production)
SCAN 0 MATCH ussd:* COUNT 100        # Iterate through keys safely

# Key Expiration
SET session:123 "data" EX 120        # Set with 120-second TTL
TTL session:123                      # Get remaining TTL
EXPIRE mykey 300                     # Set expiration on existing key
PERSIST mykey                        # Remove expiration

# String Operations
APPEND mykey " more text"            # Append to string
STRLEN mykey                         # Get string length
INCR counter                         # Increment counter
DECR counter                         # Decrement counter

# Hash Operations (for structured data)
HSET user:1 name "John" age 30       # Set hash fields
HGET user:1 name                     # Get hash field
HGETALL user:1                       # Get all hash fields
HDEL user:1 age                      # Delete hash field

# List Operations
LPUSH mylist "item1"                 # Push to list head
RPUSH mylist "item2"                 # Push to list tail
LRANGE mylist 0 -1                   # Get all list items
LPOP mylist                          # Pop from list head

# Set Operations
SADD myset "member1"                 # Add to set
SMEMBERS myset                       # Get all set members
SISMEMBER myset "member1"            # Check set membership

# Sorted Set Operations (for leaderboards)
ZADD leaderboard 100 "player1"       # Add to sorted set
ZRANGE leaderboard 0 -1 WITHSCORES   # Get sorted set range
ZRANK leaderboard "player1"          # Get rank

# Pub/Sub (Real-time messaging)
SUBSCRIBE notifications              # Subscribe to channel
PUBLISH notifications "Hello"        # Publish message
```

### Cache Invalidation Patterns

#### Time-Based Expiration (TTL)

```bash
# Set cache with 5-minute expiration
SET cache:groups:123 "{...json...}" EX 300
```

#### Manual Invalidation

```bash
# Delete specific cache entry
DEL cache:groups:123

# Delete all cache entries for a pattern
EVAL "return redis.call('del', unpack(redis.call('keys', ARGV[1])))" 0 "cache:groups:*"
```

#### Cache-Aside Pattern (Application Level)

```csharp
// Pseudo-code for cache-aside pattern
var cacheKey = $"cache:group:{groupId}";
var cachedData = await redis.StringGetAsync(cacheKey);

if (cachedData.IsNull)
{
    // Cache miss - fetch from database
    var data = await database.GetGroupAsync(groupId);
    
    // Store in cache with 5-minute TTL
    await redis.StringSetAsync(cacheKey, JsonSerializer.Serialize(data), TimeSpan.FromMinutes(5));
    
    return data;
}

// Cache hit - return cached data
return JsonSerializer.Deserialize<Group>(cachedData);
```

### Monitoring Redis Performance

```bash
# Monitor real-time commands
redis-cli -a your_password MONITOR

# Get server statistics
redis-cli -a your_password INFO stats

# Get memory usage
redis-cli -a your_password INFO memory

# Get keyspace statistics
redis-cli -a your_password INFO keyspace

# Identify slow commands (>10ms)
redis-cli -a your_password SLOWLOG GET 10
```

### Backup and Restore

#### Backup (AOF Enabled)

**Automatic**: AOF file is continuously updated at `/data/appendonly.aof`

**Manual Snapshot**:
```bash
# Trigger background save
redis-cli -a your_password BGSAVE

# Check save status
redis-cli -a your_password LASTSAVE
```

**Docker Volume Backup**:
```bash
# Backup Redis volume to tar file
docker run --rm \
  -v digitalstokvel-redis-data:/data \
  -v $(pwd):/backup \
  alpine tar czf /backup/redis-backup-$(date +%Y%m%d).tar.gz /data
```

#### Restore

**Docker Volume Restore**:
```bash
# Stop Redis
docker-compose stop redis

# Restore from tar file
docker run --rm \
  -v digitalstokvel-redis-data:/data \
  -v $(pwd):/backup \
  alpine tar xzf /backup/redis-backup-20260324.tar.gz -C /

# Start Redis
docker-compose start redis
```

---

## .NET Integration

### NuGet Packages

Install the StackExchange.Redis package:

```bash
dotnet add package StackExchange.Redis
```

### Recommended Packages

```xml
<PackageReference Include="StackExchange.Redis" Version="2.7.10" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="10.0.0" />
```

### Dependency Injection Setup

**Program.cs**:

```csharp
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Redis Connection (Singleton)
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") 
    ?? throw new InvalidOperationException("Redis connection string not configured");

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(redisConnectionString);
    configuration.AbortOnConnectFail = false; // Resilient to Redis downtime
    configuration.ConnectTimeout = 5000;
    configuration.SyncTimeout = 5000;
    configuration.ConnectRetry = 3;
    
    return ConnectionMultiplexer.Connect(configuration);
});

// Distributed Cache (IDistributedCache)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "DigitalStokvel:";
});

var app = builder.Build();

// Test Redis connection on startup
var redis = app.Services.GetRequiredService<IConnectionMultiplexer>();
if (!redis.IsConnected)
{
    app.Logger.LogWarning("Redis is not connected. Cache operations will fail gracefully.");
}

app.Run();
```

**appsettings.json**:

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,password=your_password,ssl=false,abortConnect=false"
  }
}
```

### Cache Service Implementation

**ICacheService.cs**:

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
```

**CacheService.cs**:

```csharp
using StackExchange.Redis;
using System.Text.Json;

public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<CacheService> _logger;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    public CacheService(IConnectionMultiplexer redis, ILogger<CacheService> logger)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNull)
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading from cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            var ttl = expiration ?? DefaultExpiration;
            await _db.StringSetAsync(key, json, ttl);
            _logger.LogDebug("Cache set for key: {Key} with TTL: {TTL}", key, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing to cache for key: {Key}", key);
            // Fail gracefully - don't throw, let the application continue without cache
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _db.KeyDeleteAsync(key);
            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache for key: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
            return false;
        }
    }
}
```

**Register in DI**:

```csharp
builder.Services.AddSingleton<ICacheService, CacheService>();
```

### USSD Session Management Example

```csharp
public class UssdSessionService
{
    private readonly ICacheService _cache;
    private static readonly TimeSpan SessionTimeout = TimeSpan.FromSeconds(120);

    public UssdSessionService(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<UssdSession?> GetSessionAsync(string sessionId)
    {
        var key = $"ussd:session:{sessionId}";
        return await _cache.GetAsync<UssdSession>(key);
    }

    public async Task SaveSessionAsync(string sessionId, UssdSession session)
    {
        var key = $"ussd:session:{sessionId}";
        await _cache.SetAsync(key, session, SessionTimeout);
    }

    public async Task DeleteSessionAsync(string sessionId)
    {
        var key = $"ussd:session:{sessionId}";
        await _cache.RemoveAsync(key);
    }
}
```

### Rate Limiting Example

```csharp
public class RateLimiter
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private const int MaxRequestsPerMinute = 100;

    public RateLimiter(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task<bool> IsAllowedAsync(string userId)
    {
        var key = $"ratelimit:{userId}:{DateTime.UtcNow:yyyyMMddHHmm}";
        
        var count = await _db.StringIncrementAsync(key);
        
        if (count == 1)
        {
            // First request in this minute - set expiration
            await _db.KeyExpireAsync(key, TimeSpan.FromMinutes(1));
        }

        return count <= MaxRequestsPerMinute;
    }
}
```

### Cache-Aside Pattern with Fallback

```csharp
public class GroupService
{
    private readonly ICacheService _cache;
    private readonly IGroupRepository _repository;
    private const int CacheTtlMinutes = 5;

    public async Task<Group> GetGroupByIdAsync(Guid groupId)
    {
        var cacheKey = $"cache:group:{groupId}";
        
        // Try cache first
        var cachedGroup = await _cache.GetAsync<Group>(cacheKey);
        if (cachedGroup != null)
        {
            return cachedGroup;
        }

        // Cache miss - fetch from database
        var group = await _repository.GetByIdAsync(groupId);
        if (group == null)
        {
            throw new NotFoundException($"Group {groupId} not found");
        }

        // Store in cache for next time
        await _cache.SetAsync(cacheKey, group, TimeSpan.FromMinutes(CacheTtlMinutes));

        return group;
    }

    public async Task UpdateGroupAsync(Group group)
    {
        // Update database
        await _repository.UpdateAsync(group);

        // Invalidate cache
        var cacheKey = $"cache:group:{group.Id}";
        await _cache.RemoveAsync(cacheKey);
    }
}
```

---

## Performance Optimization

### Caching Strategy

#### L1 Cache (In-Memory) + L2 Cache (Redis)

```csharp
public class TwoLevelCacheService
{
    private readonly IMemoryCache _l1Cache; // In-memory cache
    private readonly ICacheService _l2Cache; // Redis cache
    private static readonly TimeSpan L1Ttl = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan L2Ttl = TimeSpan.FromMinutes(5);

    public async Task<T?> GetAsync<T>(string key)
    {
        // Try L1 (in-memory) first
        if (_l1Cache.TryGetValue(key, out T l1Value))
        {
            return l1Value;
        }

        // Try L2 (Redis)
        var l2Value = await _l2Cache.GetAsync<T>(key);
        if (l2Value != null)
        {
            // Populate L1 cache
            _l1Cache.Set(key, l2Value, L1Ttl);
            return l2Value;
        }

        return default;
    }
}
```

### Key Naming Conventions

Use consistent, hierarchical key patterns:

```
{prefix}:{entity}:{id}:{attribute}

Examples:
  cache:group:123e4567-e89b-12d3-a456-426614174000
  cache:user:profile:123e4567
  ussd:session:MTN-20260324-12345
  ratelimit:user:123e4567:202603241430
  lock:payout:123e4567
```

**Benefits**:
- Easy pattern-based searches (`KEYS cache:group:*`)
- Clear organization
- Prevents key collisions

### Pipelining for Bulk Operations

```csharp
public async Task<Dictionary<string, Group>> GetMultipleGroupsAsync(IEnumerable<Guid> groupIds)
{
    var batch = _db.CreateBatch();
    var tasks = new Dictionary<Guid, Task<RedisValue>>();

    foreach (var id in groupIds)
    {
        var key = $"cache:group:{id}";
        tasks[id] = batch.StringGetAsync(key);
    }

    batch.Execute();

    var results = new Dictionary<string, Group>();
    foreach (var (id, task) in tasks)
    {
        var value = await task;
        if (!value.IsNull)
        {
            results[id.ToString()] = JsonSerializer.Deserialize<Group>(value);
        }
    }

    return results;
}
```

### Connection Pool Management

Redis connections are expensive. Always use a singleton `IConnectionMultiplexer`:

```csharp
// ✅ CORRECT: Singleton connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    return ConnectionMultiplexer.Connect(connectionString);
});

// ❌ WRONG: New connection per request
builder.Services.AddScoped<IConnectionMultiplexer>(sp =>
{
    return ConnectionMultiplexer.Connect(connectionString); // DON'T DO THIS
});
```

### Memory Optimization

**Set maxmemory and eviction policy**:

```conf
maxmemory 2gb
maxmemory-policy allkeys-lru
```

**Eviction Policies**:
- `allkeys-lru`: Evict least recently used keys (recommended for cache)
- `volatile-lru`: Evict LRU keys with expiration set
- `allkeys-lfu`: Evict least frequently used keys
- `volatile-ttl`: Evict keys with shortest TTL first

### Monitoring Performance

```bash
# Get current memory usage
redis-cli -a your_password INFO memory | grep used_memory_human

# Get hit/miss ratio
redis-cli -a your_password INFO stats | grep keyspace

# Calculate hit rate:
# hit_rate = keyspace_hits / (keyspace_hits + keyspace_misses)
```

**Target metrics**:
- Cache hit rate: > 80%
- Average latency: < 1ms (P95)
- Memory usage: < 80% of maxmemory

---

## Troubleshooting

### Common Issues

#### 1. Connection Refused

**Symptoms**:
```
StackExchange.Redis.RedisConnectionException: It was not possible to connect to the redis server(s).
```

**Solutions**:
```bash
# Check if Redis is running (Docker)
docker ps | grep redis

# Check if Redis is running (Linux)
sudo systemctl status redis-server

# Check if port 6379 is listening
netstat -an | grep 6379  # Linux/macOS
netstat -an | findstr 6379  # Windows

# Test connection with redis-cli
redis-cli -h localhost -p 6379 -a your_password PING
```

#### 2. Authentication Failed (NOAUTH)

**Symptoms**:
```
NOAUTH Authentication required
```

**Solutions**:
- Verify password in connection string matches `requirepass` in redis.conf
- Check `.env` file has correct `REDIS_PASSWORD`
- Restart Redis after changing password

#### 3. Out of Memory (OOM)

**Symptoms**:
```
OOM command not allowed when used memory > 'maxmemory'
```

**Solutions**:
```bash
# Check current memory usage
redis-cli -a your_password INFO memory

# Increase maxmemory in redis.conf
maxmemory 4gb

# Set eviction policy
maxmemory-policy allkeys-lru

# Manually flush if needed (WARNING: deletes all data)
redis-cli -a your_password FLUSHDB
```

#### 4. Slow Performance

**Diagnosis**:
```bash
# Check slow log
redis-cli -a your_password SLOWLOG GET 10

# Monitor commands in real-time
redis-cli -a your_password MONITOR
```

**Solutions**:
- Avoid `KEYS *` in production (use `SCAN` instead)
- Use pipelining for bulk operations
- Implement L1 in-memory cache
- Review and optimize key expiration strategies

#### 5. Docker Volume Permissions

**Symptoms**:
```
Fatal error: can't open the append-only file: Permission denied
```

**Solutions**:
```bash
# Fix permissions on Docker volume
docker exec -it digitalstokvel-redis chown -R redis:redis /data

# Or recreate volume
docker-compose down -v redis
docker-compose up -d redis
```

#### 6. Connection Timeout

**Symptoms**:
```
Timeout performing GET
```

**Solutions**:
```csharp
// Increase timeouts in connection string
var config = ConfigurationOptions.Parse(connectionString);
config.ConnectTimeout = 10000;  // 10 seconds
config.SyncTimeout = 10000;     // 10 seconds
config.AsyncTimeout = 10000;    // 10 seconds
```

#### 7. Cannot Execute Command (Redis Protocol Error)

**Symptoms**:
```
ERR unknown command 'SERT'
```

**Cause**: Typo or unsupported command

**Solutions**:
- Verify command spelling (`SET`, not `SERT`)
- Check Redis version supports command (e.g., `ACL` requires Redis 6+)

### Health Check Endpoint

Implement a health check in your API:

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration.GetConnectionString("Redis"), name: "redis");

app.MapHealthChecks("/health");
```

Test health check:
```bash
curl http://localhost:5000/health
```

Expected response:
```json
{
  "status": "Healthy",
  "results": {
    "redis": {
      "status": "Healthy",
      "description": null,
      "data": {}
    }
  }
}
```

### Logging Configuration

Enable detailed Redis logging in .NET:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "StackExchange.Redis": "Debug"
    }
  }
}
```

### Testing Redis Availability

Use `AbortOnConnectFail = false` to gracefully handle Redis unavailability:

```csharp
var config = ConfigurationOptions.Parse(connectionString);
config.AbortOnConnectFail = false; // Continue even if Redis is down

try
{
    var redis = ConnectionMultiplexer.Connect(config);
    if (!redis.IsConnected)
    {
        logger.LogWarning("Redis is unavailable. Continuing without cache.");
    }
}
catch (RedisConnectionException ex)
{
    logger.LogError(ex, "Failed to connect to Redis. Application will run without cache.");
}
```

---

## Best Practices

### 1. Always Set Expiration

Avoid memory leaks by always setting TTL:

```csharp
// ✅ GOOD: Set expiration
await db.StringSetAsync("cache:group:123", data, TimeSpan.FromMinutes(5));

// ❌ BAD: No expiration (memory leak risk)
await db.StringSetAsync("cache:group:123", data);
```

### 2. Use Connection Multiplexer as Singleton

```csharp
// ✅ GOOD: Singleton
builder.Services.AddSingleton<IConnectionMultiplexer>(...);

// ❌ BAD: Per-request connection
builder.Services.AddScoped<IConnectionMultiplexer>(...);
```

### 3. Handle Redis Failures Gracefully

```csharp
public async Task<Group> GetGroupAsync(Guid id)
{
    try
    {
        var cached = await _cache.GetAsync<Group>($"cache:group:{id}");
        if (cached != null) return cached;
    }
    catch (RedisException ex)
    {
        _logger.LogWarning(ex, "Redis unavailable, falling back to database");
    }

    // Always have a fallback to database
    return await _repository.GetByIdAsync(id);
}
```

### 4. Use Structured Key Names

```csharp
// ✅ GOOD: Structured and consistent
$"cache:group:{groupId}"
$"ussd:session:{sessionId}"
$"ratelimit:user:{userId}:{DateTime.UtcNow:yyyyMMddHHmm}"

// ❌ BAD: Flat and inconsistent
$"group{groupId}"
$"session-{sessionId}"
$"{userId}_ratelimit"
```

### 5. Implement Health Checks

Always add Redis to health checks:

```csharp
builder.Services.AddHealthChecks()
    .AddRedis(redisConnectionString);
```

### 6. Monitor Cache Hit Rate

Track cache effectiveness:

```csharp
public class CacheMetrics
{
    public long Hits { get; set; }
    public long Misses { get; set; }
    public double HitRate => Hits + Misses > 0 ? (double)Hits / (Hits + Misses) : 0;
}
```

Target: > 80% hit rate for frequently accessed data

### 7. Use Pipelining for Bulk Operations

```csharp
// ✅ GOOD: Pipelining (1 network round-trip)
var batch = db.CreateBatch();
var tasks = new List<Task>();
foreach (var key in keys)
{
    tasks.Add(batch.StringGetAsync(key));
}
batch.Execute();
await Task.WhenAll(tasks);

// ❌ BAD: Sequential (N network round-trips)
foreach (var key in keys)
{
    await db.StringGetAsync(key);
}
```

### 8. Secure with Password

Never run Redis without authentication:

```conf
# ✅ GOOD: Password-protected
requirepass your_secure_password

# ❌ BAD: No authentication (security risk)
# requirepass (commented out)
```

---

## Next Steps

### Post-Setup Checklist

- [ ] Redis container is running (`docker-compose ps`)
- [ ] Connection test successful (`redis-cli PING` returns `PONG`)
- [ ] `.env` file configured with `REDIS_PASSWORD`
- [ ] Redis Insight connected and browsing keys
- [ ] .NET application connects successfully
- [ ] Health check endpoint returns healthy status
- [ ] Cache hit rate being monitored

### Integration with Services

1. **USSD Gateway Service**: Implement session management using Redis
2. **Group Service**: Implement group data caching
3. **Contribution Service**: Implement contribution history caching
4. **API Gateway**: Implement rate limiting using Redis

### Further Reading

- **Official Redis Documentation**: https://redis.io/documentation
- **StackExchange.Redis Documentation**: https://stackexchange.github.io/StackExchange.Redis/
- **Redis Best Practices**: https://redis.io/docs/manual/patterns/
- **Caching Strategies**: https://docs.microsoft.com/en-us/azure/architecture/best-practices/caching

---

## Summary

This guide covered:

✅ **Quick Start** with Docker Compose (recommended)  
✅ **Manual Installation** for Windows, macOS, Linux  
✅ **Configuration** for development and USSD sessions  
✅ **Connection Setup** with .NET and StackExchange.Redis  
✅ **Redis Insight** for GUI management  
✅ **Common Operations** with redis-cli and C#  
✅ **.NET Integration** with dependency injection and caching patterns  
✅ **Performance Optimization** strategies  
✅ **Troubleshooting** common issues  

**Redis is now configured for local development!** 🚀

For questions or issues, refer to the troubleshooting section or consult the team.
