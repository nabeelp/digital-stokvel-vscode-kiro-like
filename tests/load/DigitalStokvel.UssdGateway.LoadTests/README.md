# Digital Stokvel USSD Gateway - Load Tests

This project contains load tests for the USSD Gateway Service using [NBomber](https://nbomber.com/), a modern .NET load testing framework.

## Test Scenarios

### 1. USSD Session Initiation
**Purpose:** Tests concurrent USSD session creation and Redis session storage performance

**Profile:**
- Ramp up to 100 concurrent users over 30 seconds
- Maintain 100 concurrent users for 2 minutes
- Ramp down over 10 seconds

**Success Criteria:**
- 95%+ of requests succeed (HTTP 200)
- P95 latency < 500ms
- P99 latency < 1000ms
- No database connection errors
- No Redis connection errors

### 2. Menu Navigation
**Purpose:** Tests menu state management and multi-step USSD flows

**Profile:**
- Ramp up to 50 concurrent users over 20 seconds
- Maintain 50 concurrent users for 1 minute
- Simulates 2-second user think time between requests

**Success Criteria:**
- 95%+ of requests succeed
- Session state persists correctly between requests
- P95 latency < 800ms (includes API Gateway call)
- No memory leaks

### 3. Session Timeout
**Purpose:** Tests 120-second session timeout handling under load

**Profile:**
- 10 concurrent users
- Create session → Wait 125 seconds → Attempt to use expired session

**Success Criteria:**
- Expired sessions are properly cleaned up from Redis
- New sessions created when expired sessions are accessed
- No zombie sessions in Redis

## Performance Targets

Based on design specifications (Section 10: Performance Considerations):

| Metric | MVP (3 months) | Year 1 | Year 3 |
|--------|----------------|--------|--------|
| **Concurrent Users** | 10,000 | 50,000 | 250,000 |
| **API Requests/sec** | 1,000 | 5,000 | 25,000 |
| **USSD Sessions/sec** | 100 | 500 | 2,500 |

### Current Test Configuration
- **Moderate Load:** 100 concurrent users (Session Init scenario)
- **Light Load:** 50 concurrent users (Menu Navigation scenario)
- **Stress Test:** To be implemented (1000+ concurrent users)

## Prerequisites

1. **Running Services:**
   ```bash
   # PostgreSQL
   docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres:15

   # Redis
   docker run -d -p 6379:6379 redis:7-alpine

   # USSD Gateway Service (default port: 5003)
   cd src/services/DigitalStokvel.UssdGateway
   dotnet run
   ```

2. **Environment Configuration:**
   ```bash
   # Set target URL (default: http://localhost:5003)
   export USSD_BASE_URL=http://localhost:5003
   ```

## Running Load Tests

```bash
cd tests/load/DigitalStokvel.UssdGateway.LoadTests
dotnet run
```

### Test Reports
Reports are generated in `load-test-results/` folder:
- **HTML Report:** Interactive charts and graphs
- **TXT Report:** Console-friendly summary
- **MD Report:** Markdown summary for documentation

## Performance Optimization Recommendations

### 1. Redis Configuration
```ini
# redis.conf
maxmemory 2gb
maxmemory-policy allkeys-lru
timeout 0
tcp-keepalive 300
```

### 2. PostgreSQL Configuration
```ini
# postgresql.conf
max_connections = 200
shared_buffers = 256MB
effective_cache_size = 1GB
work_mem = 4MB
maintenance_work_mem = 64MB
```

### 3. USSD Gateway Service
```json
// appsettings.json - Production
{
  "UssdSettings": {
    "SessionTimeoutSeconds": 120,
    "MaxMenuDepth": 3
  },
  "Kestrel": {
    "Limits": {
      "MaxConcurrentConnections": 1000,
      "MaxConcurrentUpgradedConnections": 100,
      "RequestHeadersTimeout": "00:00:30"
    }
  }
}
```

### 4. Horizontal Scaling
```yaml
# kubernetes/ussd-gateway-deployment.yaml
replicas: 3  # Start with 3 pods
resources:
  requests:
    memory: "256Mi"
    cpu: "250m"
  limits:
    memory: "512Mi"
    cpu: "500m"
```

## Monitoring Metrics

Track these metrics during load tests:

1. **Application Metrics:**
   - Request rate (req/sec)
   - Response time (P50, P95, P99)
   - Error rate (%)
   - Active sessions count

2. **Redis Metrics:**
   - Memory usage
   - Keyspace hits/misses ratio
   - Evicted keys
   - Connected clients

3. **Database Metrics:**
   - Connection pool usage
   - Query execution time
   - Active connections
   - Transaction rate

## Load Testing Roadmap

- [x] **Task 3.3.10:** Basic load test framework
- [ ] **Phase 6.2:** Extended load testing (1000+ concurrent users)
- [ ] **Phase 7.1:** Performance profiling and bottleneck identification
- [ ] **Phase 7.2:** Stress testing (10,000+ concurrent users)
- [ ] **Phase 7.3:** Chaos engineering (service failure scenarios)

## Troubleshooting

### Issue: High Latency (> 1000ms P99)
**Causes:**
- Database query performance
- Redis connection pool exhaustion
- API Gateway latency

**Solutions:**
- Add database query indexes
- Increase Redis connection pool size
- Cache frequently accessed data

### Issue: Connection Refused Errors
**Causes:**
- Service not running
- Port conflicts
- Firewall blocking

**Solutions:**
- Verify services are running: `docker ps`, `dotnet ps`
- Check port availability: `netstat -an | findstr 5003`
- Review firewall rules

### Issue: Memory Leaks
**Causes:**
- Unclosed database connections
- Large in-memory session caches
- Event handler subscriptions

**Solutions:**
- Use `using` statements for disposable resources
- Implement Redis-based session storage (already done)
- Profile with dotMemory or PerfView

## References

- [NBomber Documentation](https://nbomber.com/docs/overview)
- [Design Document - Section 10: Performance Considerations](.kiro-like/specs/design.md#performance-considerations)
- [USSD Architecture - Section 2.3](.kiro-like/specs/design.md#ussd-architecture)
