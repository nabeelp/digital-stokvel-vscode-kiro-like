using DigitalStokvel.UssdGateway.DTOs;
using DigitalStokvel.UssdGateway.Entities;
using System.Text;
using System.Text.Json;

namespace DigitalStokvel.UssdGateway.Services;

/// <summary>
/// Helper class for storing group information in session context
/// </summary>
internal class GroupInfo
{
    public int Index { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Implementation of USSD menu service
/// Handles 3-level menu navigation per design constraints
/// </summary>
public class UssdMenuService : IUssdMenuService
{
    private readonly ILogger<UssdMenuService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IApiGatewayClient _apiGatewayClient;
    private readonly Dictionary<string, Dictionary<string, string>> _menuTexts;

    public UssdMenuService(
        ILogger<UssdMenuService> logger,
        IConfiguration configuration,
        IApiGatewayClient apiGatewayClient)
    {
        _logger = logger;
        _configuration = configuration;
        _apiGatewayClient = apiGatewayClient;
        
        // Initialize menu texts for supported languages
        _menuTexts = InitializeMenuTexts();
    }

    public async Task<UssdSessionResponseDto> ProcessRequestAsync(UssdSessionRequestDto request, UssdSession session)
    {
        _logger.LogInformation("Processing USSD request for session {SessionId}, MenuLevel: {MenuLevel}, Input: {UserInput}",
            session.SessionId, session.MenuLevel, request.UserInput);

        // Parse session context
        var context = string.IsNullOrEmpty(session.Context) 
            ? new Dictionary<string, object>() 
            : JsonSerializer.Deserialize<Dictionary<string, object>>(session.Context) ?? new Dictionary<string, object>();

        // If no user input (initial request), show main menu or authentication menu
        if (string.IsNullOrEmpty(request.UserInput))
        {
            if (session.UserId.HasValue)
            {
                return await ShowMainMenuAsync(session);
            }
            else
            {
                return await ShowAuthenticationMenuAsync(session);
            }
        }

        // Process user input based on current menu level and context
        var currentAction = context.ContainsKey("action") ? context["action"]?.ToString() : null;

        return currentAction switch
        {
            "authentication" => await HandleAuthenticationAsync(request, session, context),
            "main_menu" => await HandleMainMenuAsync(request, session, context),
            "select_group" => await HandleGroupSelectionAsync(request, session, context),
            "contribution" => await HandleContributionAsync(request, session, context),
            "balance" => await HandleBalanceCheckAsync(request, session, context),
            _ => await ShowMainMenuAsync(session)
        };
    }

    public async Task<UssdSessionResponseDto> ShowMainMenuAsync(UssdSession session)
    {
        _logger.LogInformation("Showing main menu for session {SessionId}", session.SessionId);

        var context = new Dictionary<string, object>
        {
            { "action", "main_menu" }
        };

        session.Context = JsonSerializer.Serialize(context);
        session.MenuLevel = 1;

        var menuText = GetMenuText(session.Language, "main_menu");

        return await Task.FromResult(new UssdSessionResponseDto
        {
            SessionId = session.SessionId,
            ResponseType = "CON",
            Message = menuText,
            SessionState = new UssdSessionStateDto
            {
                MenuLevel = session.MenuLevel,
                Context = context
            }
        });
    }

    public async Task<UssdSessionResponseDto> ShowAuthenticationMenuAsync(UssdSession session)
    {
        _logger.LogInformation("Showing authentication menu for session {SessionId}", session.SessionId);

        var context = new Dictionary<string, object>
        {
            { "action", "authentication" }
        };

        session.Context = JsonSerializer.Serialize(context);
        session.MenuLevel = 1;

        var menuText = GetMenuText(session.Language, "authentication");

        return await Task.FromResult(new UssdSessionResponseDto
        {
            SessionId = session.SessionId,
            ResponseType = "CON",
            Message = menuText,
            SessionState = new UssdSessionStateDto
            {
                MenuLevel = session.MenuLevel,
                Context = context
            }
        });
    }

    private async Task<UssdSessionResponseDto> HandleAuthenticationAsync(
        UssdSessionRequestDto request, UssdSession session, Dictionary<string, object> context)
    {
        _logger.LogInformation("Handling authentication for session {SessionId}", session.SessionId);

        // For now, return a placeholder message
        // Full authentication will be implemented in later tasks
        return await Task.FromResult(new UssdSessionResponseDto
        {
            SessionId = session.SessionId,
            ResponseType = "END",
            Message = GetMenuText(session.Language, "authentication_pending"),
            SessionState = new UssdSessionStateDto
            {
                MenuLevel = session.MenuLevel,
                Context = context
            }
        });
    }

    private async Task<UssdSessionResponseDto> HandleMainMenuAsync(
        UssdSessionRequestDto request, UssdSession session, Dictionary<string, object> context)
    {
        _logger.LogInformation("Handling main menu selection for session {SessionId}, Input: {Input}",
            session.SessionId, request.UserInput);

        var input = request.UserInput?.Trim();

        return input switch
        {
            "1" => await ShowGroupSelectionAsync(session, context),
            "2" => await ShowBalanceCheckAsync(session, context),
            "0" => await ShowExitMessageAsync(session),
            _ => await ShowMainMenuAsync(session) // Invalid input, show menu again
        };
    }

    private async Task<UssdSessionResponseDto> HandleGroupSelectionAsync(
        UssdSessionRequestDto request, UssdSession session, Dictionary<string, object> context)
    {
        _logger.LogInformation("Handling group selection for session {SessionId}, Input: {Input}",
            session.SessionId, request.UserInput);

        var input = request.UserInput?.Trim();

        // Handle back navigation
        if (input == "0")
        {
            return await ShowMainMenuAsync(session);
        }

        // Parse group selection
        if (!int.TryParse(input, out var groupIndex) || groupIndex < 1)
        {
            _logger.LogWarning("Invalid group selection: {Input}", input);
            return await ShowGroupSelectionAsync(session, context);
        }

        // Get groups from context
        if (!context.ContainsKey("groups"))
        {
            _logger.LogWarning("Groups not found in context");
            return await ShowGroupSelectionAsync(session, context);
        }

        var groupsJson = context["groups"]?.ToString() ?? "";
        var groups = JsonSerializer.Deserialize<List<GroupInfo>>(groupsJson);

        if (groups == null || !groups.Any())
        {
            _logger.LogWarning("Failed to deserialize groups from context");
            return await ShowGroupSelectionAsync(session, context);
        }

        // Find selected group
        var selectedGroup = groups.FirstOrDefault(g => g.Index == groupIndex);
        if (selectedGroup == null)
        {
            _logger.LogWarning("Group index {Index} not found", groupIndex);
            return await ShowGroupSelectionAsync(session, context);
        }

        // Fetch contribution due for selected group
        var contributionDue = await _apiGatewayClient.GetContributionDueAsync(selectedGroup.Id, session.PhoneNumber);

        if (contributionDue == null)
        {
            _logger.LogWarning("Failed to fetch contribution due for group {GroupId}", selectedGroup.Id);
            return new UssdSessionResponseDto
            {
                SessionId = session.SessionId,
                ResponseType = "END",
                Message = GetMenuText(session.Language, "error_fetching_amount"),
                SessionState = null
            };
        }

        // Update context with selected group and amount
        context["action"] = "contribution";
        context["selected_group_id"] = selectedGroup.Id.ToString();
        context["selected_group_name"] = selectedGroup.Name;
        context["contribution_amount"] = contributionDue.Amount.ToString();
        session.Context = JsonSerializer.Serialize(context);
        session.MenuLevel = 3;

        // Show confirmation menu
        var confirmMessage = string.Format(
            GetMenuText(session.Language, "confirm_contribution"),
            selectedGroup.Name,
            contributionDue.Amount.ToString("F2"));

        return new UssdSessionResponseDto
        {
            SessionId = session.SessionId,
            ResponseType = "CON",
            Message = confirmMessage,
            SessionState = new UssdSessionStateDto
            {
                MenuLevel = session.MenuLevel,
                Context = context
            }
        };
    }

    private async Task<UssdSessionResponseDto> HandleContributionAsync(
        UssdSessionRequestDto request, UssdSession session, Dictionary<string, object> context)
    {
        _logger.LogInformation("Handling contribution for session {SessionId}, Input: {Input}",
            session.SessionId, request.UserInput);

        var input = request.UserInput?.Trim();

        // Handle cancellation
        if (input == "2" || input == "0")
        {
            return await ShowMainMenuAsync(session);
        }

        // Handle confirmation (1 = Yes) or collect PIN
        if (!context.ContainsKey("awaiting_pin"))
        {
            // First interaction - user confirmed payment
            if (input == "1")
            {
                // Ask for PIN
                context["awaiting_pin"] = "true";
                session.Context = JsonSerializer.Serialize(context);

                return new UssdSessionResponseDto
                {
                    SessionId = session.SessionId,
                    ResponseType = "CON",
                    Message = GetMenuText(session.Language, "enter_pin"),
                    SessionState = new UssdSessionStateDto
                    {
                        MenuLevel = session.MenuLevel,
                        Context = context
                    }
                };
            }
            else
            {
                // Invalid input, show confirmation again
                var groupName = context.ContainsKey("selected_group_name") ? context["selected_group_name"]!.ToString() : "";
                var amountStr = context.ContainsKey("contribution_amount") ? context["contribution_amount"]!.ToString() : "0";
                
                var confirmMessage = string.Format(
                    GetMenuText(session.Language, "confirm_contribution"),
                    groupName, amountStr);

                return new UssdSessionResponseDto
                {
                    SessionId = session.SessionId,
                    ResponseType = "CON",
                    Message = confirmMessage,
                    SessionState = new UssdSessionStateDto
                    {
                        MenuLevel = session.MenuLevel,
                        Context = context
                    }
                };
            }
        }

        // PIN was entered, process payment
        var pin = input; // In production, this would be encrypted

        if (!Guid.TryParse(context["selected_group_id"]?.ToString(), out var groupId))
        {
            _logger.LogError("Invalid group ID in context");
            return new UssdSessionResponseDto
            {
                SessionId = session.SessionId,
                ResponseType = "END",
                Message = GetMenuText(session.Language, "payment_error"),
                SessionState = null
            };
        }

        if (!decimal.TryParse(context["contribution_amount"]?.ToString(), out var amount))
        {
            _logger.LogError("Invalid contribution amount in context");
            return new UssdSessionResponseDto
            {
                SessionId = session.SessionId,
                ResponseType = "END",
                Message = GetMenuText(session.Language, "payment_error"),
                SessionState = null
            };
        }

        // Process contribution
        var paymentRequest = new ContributionPaymentRequestDto
        {
            GroupId = groupId,
            Amount = amount,
            PaymentMethod = "linked_account",
            Pin = pin
        };

        var paymentResult = await _apiGatewayClient.ProcessContributionAsync(paymentRequest, session.PhoneNumber);

        if (paymentResult == null || paymentResult.Status != "completed")
        {
            _logger.LogWarning("Payment failed for group {GroupId}", groupId);
            return new UssdSessionResponseDto
            {
                SessionId = session.SessionId,
                ResponseType = "END",
                Message = GetMenuText(session.Language, "payment_failed"),
                SessionState = null
            };
        }

        // Payment successful
        var successMessage = string.Format(
            GetMenuText(session.Language, "payment_success"),
            amount.ToString("F2"),
            paymentResult.Receipt?.ReceiptNumber ?? paymentResult.TransactionRef);

        _logger.LogInformation("Payment successful. Transaction: {TransactionRef}, Receipt: {ReceiptNumber}",
            paymentResult.TransactionRef, paymentResult.Receipt?.ReceiptNumber);

        return new UssdSessionResponseDto
        {
            SessionId = session.SessionId,
            ResponseType = "END",
            Message = successMessage,
            SessionState = null
        };
    }

    private async Task<UssdSessionResponseDto> HandleBalanceCheckAsync(
        UssdSessionRequestDto request, UssdSession session, Dictionary<string, object> context)
    {
        _logger.LogInformation("Handling balance check for session {SessionId}", session.SessionId);

        // Placeholder implementation - will be expanded in later tasks
        return await Task.FromResult(new UssdSessionResponseDto
        {
            SessionId = session.SessionId,
            ResponseType = "END",
            Message = GetMenuText(session.Language, "feature_coming_soon"),
            SessionState = new UssdSessionStateDto
            {
                MenuLevel = session.MenuLevel,
                Context = context
            }
        });
    }

    private async Task<UssdSessionResponseDto> ShowGroupSelectionAsync(
        UssdSession session, Dictionary<string, object> context)
    {
        _logger.LogInformation("Showing group selection for session {SessionId}", session.SessionId);

        context["action"] = "select_group";
        session.MenuLevel = 2;

        // Fetch user's groups from API
        var groupsResponse = await _apiGatewayClient.GetUserGroupsAsync(session.PhoneNumber);

        if (groupsResponse == null || !groupsResponse.Groups.Any())
        {
            _logger.LogWarning("No groups found for phone {PhoneNumber}", session.PhoneNumber);
            return new UssdSessionResponseDto
            {
                SessionId = session.SessionId,
                ResponseType = "END",
                Message = GetMenuText(session.Language, "no_groups"),
                SessionState = null
            };
        }

        // Store groups in context for later use
        var groupsList = groupsResponse.Groups.Select((g, index) => new { Index = index + 1, Group = g }).ToList();
        context["groups"] = JsonSerializer.Serialize(groupsList.Select(g => new { g.Index, g.Group.Id, g.Group.Name }));
        session.Context = JsonSerializer.Serialize(context);

        // Build menu text with actual groups
        var menuBuilder = new StringBuilder();
        menuBuilder.AppendLine(GetMenuText(session.Language, "select_group_header"));
        
        foreach (var item in groupsList.Take(9)) // Limit to 9 groups (options 1-9)
        {
            menuBuilder.AppendLine($"{item.Index}. {item.Group.Name}");
        }
        menuBuilder.AppendLine("0. " + GetMenuText(session.Language, "back"));

        return new UssdSessionResponseDto
        {
            SessionId = session.SessionId,
            ResponseType = "CON",
            Message = menuBuilder.ToString(),
            SessionState = new UssdSessionStateDto
            {
                MenuLevel = session.MenuLevel,
                Context = context
            }
        };
    }

    private async Task<UssdSessionResponseDto> ShowBalanceCheckAsync(
        UssdSession session, Dictionary<string, object> context)
    {
        _logger.LogInformation("Showing balance check for session {SessionId}", session.SessionId);

        context["action"] = "balance";
        session.Context = JsonSerializer.Serialize(context);
        session.MenuLevel = 2;

        // Placeholder implementation - will fetch actual balance in later tasks
        var menuText = GetMenuText(session.Language, "balance_check");

        return await Task.FromResult(new UssdSessionResponseDto
        {
            SessionId = session.SessionId,
            ResponseType = "END",
            Message = menuText,
            SessionState = new UssdSessionStateDto
            {
                MenuLevel = session.MenuLevel,
                Context = context
            }
        });
    }

    private async Task<UssdSessionResponseDto> ShowExitMessageAsync(UssdSession session)
    {
        _logger.LogInformation("Showing exit message for session {SessionId}", session.SessionId);

        return await Task.FromResult(new UssdSessionResponseDto
        {
            SessionId = session.SessionId,
            ResponseType = "END",
            Message = GetMenuText(session.Language, "exit"),
            SessionState = null
        });
    }

    private string GetMenuText(string language, string menuKey)
    {
        if (_menuTexts.ContainsKey(language) && _menuTexts[language].ContainsKey(menuKey))
        {
            return _menuTexts[language][menuKey];
        }

        // Fallback to English
        if (_menuTexts.ContainsKey("en") && _menuTexts["en"].ContainsKey(menuKey))
        {
            return _menuTexts["en"][menuKey];
        }

        return "Menu not available";
    }

    private Dictionary<string, Dictionary<string, string>> InitializeMenuTexts()
    {
        // Initialize menu texts for all supported languages
        // Design requirement: Support 5 languages (en, zu, xh, st, af)
        return new Dictionary<string, Dictionary<string, string>>
        {
            ["en"] = new Dictionary<string, string>
            {
                ["main_menu"] = "Welcome to Digital Stokvel\n1. Make Contribution\n2. Check Balance\n0. Exit",
                ["authentication"] = "Welcome to Digital Stokvel\nAuthentication required.\nPlease register via mobile app first.",
                ["authentication_pending"] = "Please register via the Digital Stokvel mobile app to use USSD services.",
                ["select_group_header"] = "Select your stokvel group:",
                ["back"] = "Back",
                ["no_groups"] = "You are not a member of any stokvel groups. Please join a group via the mobile app.",
                ["error_fetching_amount"] = "Unable to fetch contribution amount. Please try again later.",
                ["confirm_contribution"] = "Pay R{1} to {0}?\n1. Yes\n2. No",
                ["enter_pin"] = "Enter your PIN:",
                ["payment_error"] = "Payment error. Please try again.",
                ["payment_failed"] = "Payment failed. Please check your account and try again.",
                ["payment_success"] = "Payment successful! R{0} paid.\nReceipt: {1}\nThank you!",
                ["balance_check"] = "Your balance: R0.00\nThank you for using Digital Stokvel.",
                ["feature_coming_soon"] = "This feature is coming soon. Thank you for using Digital Stokvel.",
                ["exit"] = "Thank you for using Digital Stokvel. Goodbye!"
            },
            ["zu"] = new Dictionary<string, string>
            {
                ["main_menu"] = "Siyakwamukela ku-Digital Stokvel\n1. Yenza Umnikelo\n2. Bheka Ibhalansi\n0. Phuma",
                ["authentication"] = "Siyakwamukela ku-Digital Stokvel\nKudingeka ukuqinisekiswa.\nSicela ubhalise nge-app yeselula kuqala.",
                ["authentication_pending"] = "Sicela ubhalise nge-app yeselula ye-Digital Stokvel ukuze usebenzise amasevisi e-USSD.",
                ["select_group_header"] = "Khetha iqembu lakho le-stokvel:",
                ["back"] = "Emuva",
                ["no_groups"] = "Awuyona ilungu lanoma yiliphi iqembu le-stokvel. Sicela ujoyine iqembu nge-app yeselula.",
                ["error_fetching_amount"] = "Ayikwazi ukuthola inani lomnikelo. Sicela uzame kamuva.",
                ["confirm_contribution"] = "Khokha u-R{1} ku-{0}?\n1. Yebo\n2. Cha",
                ["enter_pin"] = "Faka i-PIN yakho:",
                ["payment_error"] = "Iphutha lokhokho. Sicela uzame futhi.",
                ["payment_failed"] = "Ukukhokha kuhlulekile. Sicela uhlole i-akhawunti yakho uzame futhi.",
                ["payment_success"] = "Ukukhokha kuphumelele! U-R{0} ukhokhiwe.\nIrisidi: {1}\nSiyabonga!",
                ["balance_check"] = "Ibhalansi yakho: R0.00\nSiyabonga ngokusebenzisa i-Digital Stokvel.",
                ["feature_coming_soon"] = "Lesi sici sizofika maduze. Siyabonga ngokusebenzisa i-Digital Stokvel.",
                ["exit"] = "Siyabonga ngokusebenzisa i-Digital Stokvel. Hamba kahle!"
            },
            ["xh"] = new Dictionary<string, string>
            {
                ["main_menu"] = "Wamkelekile kwi-Digital Stokvel\n1. Yenza Igalelo\n2. Jonga Ibhalansi\n0. Phuma",
                ["authentication"] = "Wamkelekile kwi-Digital Stokvel\nUkuqinisekiswa kuyafuneka.\nNceda ubhalise nge-app yeselula kuqala.",
                ["authentication_pending"] = "Nceda ubhalise nge-app yeselula ye-Digital Stokvel ukuze usebenzise iinkonzo ze-USSD.",
                ["select_group_header"] = "Khetha iqela lakho le-stokvel:",
                ["back"] = "Emva",
                ["no_groups"] = "Awunalungu kulo naliphi na iqela le-stokvel. Nceda ujoyine iqela nge-app yeselula.",
                ["error_fetching_amount"] = "Akukwazeki ukufumana isixa segolelo. Nceda uzame kwakhona kamva.",
                ["confirm_contribution"] = "Hlawula u-R{1} ku-{0}?\n1. Ewe\n2. Hayi",
                ["enter_pin"] = "Faka i-PIN yakho:",
                ["payment_error"] = "Impazamo yentlawulo. Nceda uzame kwakhona.",
                ["payment_failed"] = "Intlawulo ayiphumelelanga. Nceda ujonga i-akhawunti yakho uphinde uzame.",
                ["payment_success"] = "Intlawulo iphumelele! U-R{0} uhlawuliwe.\nIrisithi: {1}\nEnkosi!",
                ["balance_check"] = "Ibhalansi yakho: R0.00\nEnkosi ngokusebenzisa i-Digital Stokvel.",
                ["feature_coming_soon"] = "Le nkonzo izakufika kungekudala. Enkosi ngokusebenzisa i-Digital Stokvel.",
                ["exit"] = "Enkosi ngokusebenzisa i-Digital Stokvel. Hamba kakuhle!"
            },
            ["st"] = new Dictionary<string, string>
            {
                ["main_menu"] = "Rea o amohela ho Digital Stokvel\n1. Etsa Seabo\n2. Sheba Balance\n0. Tswa",
                ["authentication"] = "Rea o amohela ho Digital Stokvel\nHo hlokahala netefatso.\nKa kopo ingodise ka app ea mohala pele.",
                ["authentication_pending"] = "Ka kopo ingodise ka app ea mohala ea Digital Stokvel ho sebedisa ditshebeletso tsa USSD.",
                ["select_group_header"] = "Kgetha sehlopha sa hao sa stokvel:",
                ["back"] = "Morao",
                ["no_groups"] = "Ha o setho sa sehlopha sefeseafe sa stokvel. Ka kopo kenela sehlopha ka app ea mohala.",
                ["error_fetching_amount"] = "Ha ho kgonehe ho fumana seabo. Ka kopo leka hape hamorao.",
                ["confirm_contribution"] = "Lefa R{1} ho {0}?\n1. E\n2. Tjhe",
                ["enter_pin"] = "Kenya PIN ea hao:",
                ["payment_error"] = "Phoso ea tefello. Ka kopo leka hape.",
                ["payment_failed"] = "Tefo e hloleha. Ka kopo hlahloba akhaonto ea hao o leke hape.",
                ["payment_success"] = "Tefo e atlehile! R{0} e lefetsoe.\nResiti: {1}\nRea leboha!",
                ["balance_check"] = "Balance ea hao: R0.00\nRea leboha ho sebedisa Digital Stokvel.",
                ["feature_coming_soon"] = "Sebopeho sena se tla tla haufinyane. Rea leboha ho sebedisa Digital Stokvel.",
                ["exit"] = "Rea leboha ho sebedisa Digital Stokvel. Sala hantle!"
            },
            ["af"] = new Dictionary<string, string>
            {
                ["main_menu"] = "Welkom by Digital Stokvel\n1. Maak Bydrae\n2. Kyk Balans\n0. Verlaat",
                ["authentication"] = "Welkom by Digital Stokvel\nVerifikasie vereis.\nRegistreer asseblief eers via die mobiele app.",
                ["authentication_pending"] = "Registreer asseblief via die Digital Stokvel mobiele app om USSD-dienste te gebruik.",
                ["select_group_header"] = "Kies jou stokvel-groep:",
                ["back"] = "Terug",
                ["no_groups"] = "Jy is nie 'n lid van enige stokvel-groepe nie. Sluit asseblief 'n groep aan via die mobiele app.",
                ["error_fetching_amount"] = "Kan nie bydraebedrag kry nie. Probeer asseblief later weer.",
                ["confirm_contribution"] = "Betaal R{1} aan {0}?\n1. Ja\n2. Nee",
                ["enter_pin"] = "Voer jou PIN in:",
                ["payment_error"] = "Betalingsfout. Probeer asseblief weer.",
                ["payment_failed"] = "Betaling het misluk. Kontroleer asseblief jou rekening en probeer weer.",
                ["payment_success"] = "Betaling suksesvol! R{0} betaal.\nKwitansie: {1}\nDankie!",
                ["balance_check"] = "Jou balans: R0.00\nDankie vir die gebruik van Digital Stokvel.",
                ["feature_coming_soon"] = "Hierdie funksie kom binnekort. Dankie vir die gebruik van Digital Stokvel.",
                ["exit"] = "Dankie vir die gebruik van Digital Stokvel. Totsiens!"
            }
        };
    }
}
