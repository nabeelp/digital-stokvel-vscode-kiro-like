using System.Net.Http.Json;

namespace DigitalStokvel.UssdGateway.LoadTests;

/// <summary>
/// Load tests for USSD Gateway Service
/// Tests concurrent USSD session handling, Redis performance, and timeout behavior
/// Note: This is a foundational load test project. Full NBomber integration to be completed in Phase 6.
/// </summary>
public class Program
{
    private static readonly string BaseUrl = Environment.GetEnvironmentVariable("USSD_BASE_URL") ?? "http://localhost:5003";
    private static readonly HttpClient _httpClient = new HttpClient();
    
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Digital Stokvel USSD Gateway - Load Test Framework");
        Console.WriteLine("===================================================");
        Console.WriteLine($"Target: {BaseUrl}");
        Console.WriteLine();
        Console.WriteLine("Load test execution requires:");
        Console.WriteLine("1. Running USSD Gateway Service");
        Console.WriteLine("2. Running PostgreSQL database");
        Console.WriteLine("3. Running Redis cache");
        Console.WriteLine();
        Console.WriteLine("To execute load tests:");
        Console.WriteLine("  - Ensure all services are running");
        Console.WriteLine("  - Configure USSD_BASE_URL environment variable");
        Console.WriteLine("  - Run: dotnet run");
        Console.WriteLine();
        
        // Verify service availability
        await VerifyServiceAvailability();
        
        Console.WriteLine("Load test scenarios:");
        Console.WriteLine("1. USSD Session Initiation (100 concurrent users)");
        Console.WriteLine("2. Menu Navigation (50 concurrent users with think time)");
        Console.WriteLine("3. Session Timeout (10 concurrent users, 120s+ wait)");
        Console.WriteLine();
        Console.WriteLine("See README.md for detailed scenario descriptions and success criteria.");
        Console.WriteLine();
        Console.WriteLine("Note: Full NBomber integration will be completed in Phase 6 (Testing & QA).");
    }

    private static async Task VerifyServiceAvailability()
    {
        Console.WriteLine("Verifying service availability...");
        
        try
        {
            var healthUrl = $"{BaseUrl}/health";
            Console.WriteLine($"Checking: {healthUrl}");
            
            var response = await _httpClient.GetAsync(healthUrl);
            Console.WriteLine($"Health check response: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✓ USSD Gateway service is available");
            }
            else
            {
                Console.WriteLine($"✗ USSD Gateway service returned: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"✗ Cannot reach USSD Gateway service: {ex.Message}");
            Console.WriteLine($"   Ensure service is running at: {BaseUrl}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error checking service: {ex.Message}");
        }
        
        Console.WriteLine();
    }
}

