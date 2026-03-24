using DigitalStokvel.UssdGateway.DTOs;
using DigitalStokvel.UssdGateway.Entities;
using System.Text.Json;

namespace DigitalStokvel.UssdGateway.Services;

/// <summary>
/// Implementation of USSD menu service
/// Handles 3-level menu navigation per design constraints
/// </summary>
public class UssdMenuService : IUssdMenuService
{
    private readonly ILogger<UssdMenuService> _logger;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, Dictionary<string, string>> _menuTexts;

    public UssdMenuService(
        ILogger<UssdMenuService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
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
        _logger.LogInformation("Handling group selection for session {SessionId}", session.SessionId);

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

    private async Task<UssdSessionResponseDto> HandleContributionAsync(
        UssdSessionRequestDto request, UssdSession session, Dictionary<string, object> context)
    {
        _logger.LogInformation("Handling contribution for session {SessionId}", session.SessionId);

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
        session.Context = JsonSerializer.Serialize(context);
        session.MenuLevel = 2;

        // Placeholder implementation - will fetch actual groups in later tasks
        var menuText = GetMenuText(session.Language, "select_group");

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
                ["select_group"] = "Select your stokvel group:\n1. Example Group 1\n2. Example Group 2\n0. Back",
                ["balance_check"] = "Your balance: R0.00\nThank you for using Digital Stokvel.",
                ["feature_coming_soon"] = "This feature is coming soon. Thank you for using Digital Stokvel.",
                ["exit"] = "Thank you for using Digital Stokvel. Goodbye!"
            },
            ["zu"] = new Dictionary<string, string>
            {
                ["main_menu"] = "Siyakwamukela ku-Digital Stokvel\n1. Yenza Umnikelo\n2. Bheka Ibhalansi\n0. Phuma",
                ["authentication"] = "Siyakwamukela ku-Digital Stokvel\nKudingeka ukuqinisekiswa.\nSicela ubhalise nge-app yeselula kuqala.",
                ["authentication_pending"] = "Sicela ubhalise nge-app yeselula ye-Digital Stokvel ukuze usebenzise amasevisi e-USSD.",
                ["select_group"] = "Khetha iqembu lakho le-stokvel:\n1. Iqembu Lesibonelo 1\n2. Iqembu Lesibonelo 2\n0. Emuva",
                ["balance_check"] = "Ibhalansi yakho: R0.00\nSiyabonga ngokusebenzisa i-Digital Stokvel.",
                ["feature_coming_soon"] = "Lesi sici sizofika maduze. Siyabonga ngokusebenzisa i-Digital Stokvel.",
                ["exit"] = "Siyabonga ngokusebenzisa i-Digital Stokvel. Hamba kahle!"
            },
            ["xh"] = new Dictionary<string, string>
            {
                ["main_menu"] = "Wamkelekile kwi-Digital Stokvel\n1. Yenza Igalelo\n2. Jonga Ibhalansi\n0. Phuma",
                ["authentication"] = "Wamkelekile kwi-Digital Stokvel\nUkuqinisekiswa kuyafuneka.\nNceda ubhalise nge-app yeselula kuqala.",
                ["authentication_pending"] = "Nceda ubhalise nge-app yeselula ye-Digital Stokvel ukuze usebenzise iinkonzo ze-USSD.",
                ["select_group"] = "Khetha iqela lakho le-stokvel:\n1. Umzekelo Weqela 1\n2. Umzekelo Weqela 2\n0. Emva",
                ["balance_check"] = "Ibhalansi yakho: R0.00\nEnkosi ngokusebenzisa i-Digital Stokvel.",
                ["feature_coming_soon"] = "Le nkonzo izakufika kungekudala. Enkosi ngokusebenzisa i-Digital Stokvel.",
                ["exit"] = "Enkosi ngokusebenzisa i-Digital Stokvel. Hamba kakuhle!"
            },
            ["st"] = new Dictionary<string, string>
            {
                ["main_menu"] = "Rea o amohela ho Digital Stokvel\n1. Etsa Seabo\n2. Sheba Balance\n0. Tswa",
                ["authentication"] = "Rea o amohela ho Digital Stokvel\nHo hlokahala netefatso.\nKa kopo ingodise ka app ea mohala pele.",
                ["authentication_pending"] = "Ka kopo ingodise ka app ea mohala ea Digital Stokvel ho sebedisa ditshebeletso tsa USSD.",
                ["select_group"] = "Kgetha sehlopha sa hao sa stokvel:\n1. Mohlala wa Sehlopha 1\n2. Mohlala wa Sehlopha 2\n0. Morao",
                ["balance_check"] = "Balance ea hao: R0.00\nRea leboha ho sebedisa Digital Stokvel.",
                ["feature_coming_soon"] = "Sebopeho sena se tla tla haufinyane. Rea leboha ho sebedisa Digital Stokvel.",
                ["exit"] = "Rea leboha ho sebedisa Digital Stokvel. Sala hantle!"
            },
            ["af"] = new Dictionary<string, string>
            {
                ["main_menu"] = "Welkom by Digital Stokvel\n1. Maak Bydrae\n2. Kyk Balans\n0. Verlaat",
                ["authentication"] = "Welkom by Digital Stokvel\nVerifikasie vereis.\nRegistreer asseblief eers via die mobiele app.",
                ["authentication_pending"] = "Registreer asseblief via die Digital Stokvel mobiele app om USSD-dienste te gebruik.",
                ["select_group"] = "Kies jou stokvel-groep:\n1. Voorbeeld Groep 1\n2. Voorbeeld Groep 2\n0. Terug",
                ["balance_check"] = "Jou balans: R0.00\nDankie vir die gebruik van Digital Stokvel.",
                ["feature_coming_soon"] = "Hierdie funksie kom binnekort. Dankie vir die gebruik van Digital Stokvel.",
                ["exit"] = "Dankie vir die gebruik van Digital Stokvel. Totsiens!"
            }
        };
    }
}
