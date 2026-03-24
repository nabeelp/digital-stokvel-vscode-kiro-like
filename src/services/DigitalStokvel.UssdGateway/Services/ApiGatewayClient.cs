using DigitalStokvel.UssdGateway.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DigitalStokvel.UssdGateway.Services;

/// <summary>
/// Implementation of API Gateway client for USSD service
/// Communicates with backend services through API Gateway
/// </summary>
public class ApiGatewayClient : IApiGatewayClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiGatewayClient> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _apiGatewayBaseUrl;

    public ApiGatewayClient(
        HttpClient httpClient,
        ILogger<ApiGatewayClient> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        _apiGatewayBaseUrl = configuration.GetValue<string>("ApiGateway:BaseUrl") ?? "http://localhost:5000/api";
        
        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_apiGatewayBaseUrl);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<UserGroupsResponseDto?> GetUserGroupsAsync(string phoneNumber)
    {
        try
        {
            _logger.LogInformation("Fetching groups for phone number {PhoneNumber}", phoneNumber);

            // For now, we'll use phone number for identification
            // In production, this would use proper authentication tokens
            var requestUri = $"/groups/user/{phoneNumber}";
            
            var response = await _httpClient.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch groups for {PhoneNumber}. Status: {StatusCode}",
                    phoneNumber, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UserGroupsResponseDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Retrieved {Count} groups for {PhoneNumber}", 
                result?.Groups.Count ?? 0, phoneNumber);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching groups for {PhoneNumber}", phoneNumber);
            return null;
        }
    }

    public async Task<ContributionDueResponseDto?> GetContributionDueAsync(Guid groupId, string phoneNumber)
    {
        try
        {
            _logger.LogInformation("Fetching contribution due for group {GroupId}, phone {PhoneNumber}",
                groupId, phoneNumber);

            var requestUri = $"/groups/{groupId}/members/{phoneNumber}/contribution-due";
            
            var response = await _httpClient.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch contribution due for group {GroupId}. Status: {StatusCode}",
                    groupId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ContributionDueResponseDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Contribution due for group {GroupId}: R{Amount}", 
                groupId, result?.Amount ?? 0);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching contribution due for group {GroupId}", groupId);
            return null;
        }
    }

    public async Task<ContributionPaymentResponseDto?> ProcessContributionAsync(
        ContributionPaymentRequestDto request, string phoneNumber)
    {
        try
        {
            _logger.LogInformation("Processing contribution payment for group {GroupId}, phone {PhoneNumber}, amount R{Amount}",
                request.GroupId, phoneNumber, request.Amount);

            var requestUri = $"/contributions";
            
            var jsonContent = JsonSerializer.Serialize(request);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            // Add phone number as header for identification
            _httpClient.DefaultRequestHeaders.Add("X-User-Phone", phoneNumber);
            
            var response = await _httpClient.PostAsync(requestUri, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to process contribution for group {GroupId}. Status: {StatusCode}",
                    request.GroupId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ContributionPaymentResponseDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Contribution processed successfully. Transaction: {TransactionRef}, Receipt: {ReceiptNumber}",
                result?.TransactionRef, result?.Receipt?.ReceiptNumber);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing contribution for group {GroupId}", request.GroupId);
            return null;
        }
    }

    public async Task<GroupBalanceResponseDto?> GetGroupBalanceAsync(Guid groupId, string phoneNumber)
    {
        try
        {
            _logger.LogInformation("Fetching balance for group {GroupId}, phone {PhoneNumber}",
                groupId, phoneNumber);

            var requestUri = $"/groups/{groupId}/balance";
            
            var response = await _httpClient.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch balance for group {GroupId}. Status: {StatusCode}",
                    groupId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GroupBalanceResponseDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Retrieved balance for group {GroupId}: R{Balance}",
                groupId, result?.Balance ?? 0);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching balance for group {GroupId}", groupId);
            return null;
        }
    }
}
