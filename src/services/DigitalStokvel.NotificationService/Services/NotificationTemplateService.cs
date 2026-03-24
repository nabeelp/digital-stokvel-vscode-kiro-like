using System.Text.RegularExpressions;

namespace DigitalStokvel.NotificationService.Services;

/// <summary>
/// Template service with multilingual support for 5 South African languages
/// Supports variable substitution with {{variableName}} syntax
/// </summary>
public class NotificationTemplateService : INotificationTemplateService
{
    private readonly ILogger<NotificationTemplateService> _logger;

    // Template storage: [templateKey][languageCode] = template
    private readonly Dictionary<string, Dictionary<string, string>> _templates;

    public NotificationTemplateService(ILogger<NotificationTemplateService> logger)
    {
        _logger = logger;
        _templates = InitializeTemplates();
    }

    public string RenderTemplate(string templateKey, string languageCode, Dictionary<string, string> variables)
    {
        try
        {
            // Get template for language (fallback to English if not found)
            if (!_templates.TryGetValue(templateKey, out var languageTemplates))
            {
                _logger.LogWarning("Template key not found: {TemplateKey}", templateKey);
                return $"[Template '{templateKey}' not found]";
            }

            if (!languageTemplates.TryGetValue(languageCode, out var template))
            {
                _logger.LogWarning(
                    "Language not found for template {TemplateKey}: {LanguageCode}, using English",
                    templateKey,
                    languageCode);
                template = languageTemplates.GetValueOrDefault("en", $"[Template '{templateKey}' not found]");
            }

            // Replace variables using regex
            var rendered = Regex.Replace(template, @"\{\{(\w+)\}\}", match =>
            {
                var variableName = match.Groups[1].Value;
                return variables.GetValueOrDefault(variableName, match.Value);
            });

            return rendered;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template {TemplateKey}", templateKey);
            return $"[Error rendering template: {ex.Message}]";
        }
    }

    /// <summary>
    /// Initialize templates for 5 South African languages
    /// Languages: en (English), zu (Zulu), st (Sesotho), xh (Xhosa), af (Afrikaans)
    /// </summary>
    private Dictionary<string, Dictionary<string, string>> InitializeTemplates()
    {
        return new Dictionary<string, Dictionary<string, string>>
        {
            // Contribution received notification
            ["contribution_received"] = new()
            {
                ["en"] = "Your contribution of R{{amount}} has been received for {{groupName}}. Thank you!",
                ["zu"] = "Umnikelo wakho we-R{{amount}} wamukelwa kwi-{{groupName}}. Siyabonga!",
                ["st"] = "Seabo sa hao sa R{{amount}} se amoheloe ho {{groupName}}. Re a leboha!",
                ["xh"] = "Igalelo lakho le-R{{amount}} lisamkelwe kwi-{{groupName}}. Enkosi!",
                ["af"] = "Jou bydrae van R{{amount}} is ontvang vir {{groupName}}. Dankie!"
            },

            // Contribution reminder
            ["contribution_reminder"] = new()
            {
                ["en"] = "Reminder: Your contribution of R{{amount}} for {{groupName}} is due on {{dueDate}}.",
                ["zu"] = "Isikhumbuzo: Umnikelo wakho we-R{{amount}} we-{{groupName}} uzokhokhelwa ngo-{{dueDate}}.",
                ["st"] = "Hopotso: Seabo sa hao sa R{{amount}} sa {{groupName}} se ka lebisoa ka {{dueDate}}.",
                ["xh"] = "Isikhumbuzo: Igalelo lakho le-R{{amount}} le-{{groupName}} liyafuneka ngo-{{dueDate}}.",
                ["af"] = "Herinnering: Jou bydrae van R{{amount}} vir {{groupName}} is verskuldig op {{dueDate}}."
            },

            // Payout approved notification
            ["payout_approved"] = new()
            {
                ["en"] = "Good news! Your payout of R{{amount}} from {{groupName}} has been approved.",
                ["zu"] = "Izindaba ezinhle! Inkokhelo yakho ye-R{{amount}} evela ku-{{groupName}} igciniwe.",
                ["st"] = "Litaba tse monate! Tefo ea hao ea R{{amount}} ho tsoa ho {{groupName}} e lumelloe.",
                ["xh"] = "Iindaba ezintle! Intlawulo yakho ye-R{{amount}} evela kwi-{{groupName}} ivunyiwe.",
                ["af"] = "Goeie nuus! Jou uitbetaling van R{{amount}} van {{groupName}} is goedgekeur."
            },

            // Payout disbursed notification
            ["payout_disbursed"] = new()
            {
                ["en"] = "R{{amount}} has been paid to your account. Reference: {{reference}}",
                ["zu"] = "I-R{{amount}} ikhokhelwe ku-akhawunti yakho. Inkomba: {{reference}}",
                ["st"] = "R{{amount}} e lefelloe akhaonteng ea hao. Tšupiso: {{reference}}",
                ["xh"] = "I-R{{amount}} ihlawulwe kwi-akhawunti yakho. Inkomba: {{reference}}",
                ["af"] = "R{{amount}} is in jou rekening betaal. Verwysing: {{reference}}"
            },

            // Vote created notification
            ["vote_created"] = new()
            {
                ["en"] = "New vote in {{groupName}}: {{voteTitle}}. Please cast your vote.",
                ["zu"] = "Ivoti elisha ku-{{groupName}}: {{voteTitle}}. Sicela uphonsele ivoti lakho.",
                ["st"] = "Kgetho e ncha ho {{groupName}}: {{voteTitle}}. Ka kopo, vouta.",
                ["xh"] = "Uvoto olutsha kwi-{{groupName}}: {{voteTitle}}. Nceda uvote.",
                ["af"] = "Nuwe stem in {{groupName}}: {{voteTitle}}. Stem asseblief."
            },

            // Vote reminder
            ["vote_reminder"] = new()
            {
                ["en"] = "Reminder: Vote on '{{voteTitle}}' in {{groupName}} ends on {{endDate}}.",
                ["zu"] = "Isikhumbuzo: Ivoti ku-'{{voteTitle}}' ku-{{groupName}} liphela ngo-{{endDate}}.",
                ["st"] = "Hopotso: Kgetho ho '{{voteTitle}}' ho {{groupName}} e fela ka {{endDate}}.",
                ["xh"] = "Isikhumbuzo: Uvoto kwi-'{{voteTitle}}' kwi-{{groupName}} luphela ngo-{{endDate}}.",
                ["af"] = "Herinnering: Stem oor '{{voteTitle}}' in {{groupName}} eindig op {{endDate}}."
            },

            // Dispute raised notification
            ["dispute_raised"] = new()
            {
                ["en"] = "A dispute has been raised in {{groupName}}. Type: {{disputeType}}",
                ["zu"] = "Ukungqubuzana kuphakanyisiwe ku-{{groupName}}. Uhlobo: {{disputeType}}",
                ["st"] = "Ho hlahisitsoe phapang ho {{groupName}}. Mofuta: {{disputeType}}",
                ["xh"] = "Impikiswano iphakanyisiwe kwi-{{groupName}}. Uhlobo: {{disputeType}}",
                ["af"] = "'n Dispuut is geopper in {{groupName}}. Tipe: {{disputeType}}"
            },

            // Welcome to group notification
            ["group_welcome"] = new()
            {
                ["en"] = "Welcome to {{groupName}}! You have been added as {{role}}.",
                ["zu"] = "Siyakwamukela ku-{{groupName}}! Wengezwe njengo-{{role}}.",
                ["st"] = "Rea o amohela ho {{groupName}}! O kentse e le {{role}}.",
                ["xh"] = "Wamkelekile kwi-{{groupName}}! Ufakwe njenge-{{role}}.",
                ["af"] = "Welkom by {{groupName}}! Jy is bygevoeg as {{role}}."
            },

            // Generic notification
            ["generic"] = new()
            {
                ["en"] = "{{message}}",
                ["zu"] = "{{message}}",
                ["st"] = "{{message}}",
                ["xh"] = "{{message}}",
                ["af"] = "{{message}}"
            }
        };
    }
}
