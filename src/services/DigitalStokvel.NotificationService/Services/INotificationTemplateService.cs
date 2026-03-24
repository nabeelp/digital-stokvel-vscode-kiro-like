namespace DigitalStokvel.NotificationService.Services;

/// <summary>
/// Interface for notification template management with multilingual support
/// </summary>
public interface INotificationTemplateService
{
    /// <summary>
    /// Renders a template with variable substitution
    /// </summary>
    /// <param name="templateKey">Template identifier</param>
    /// <param name="languageCode">Language code (en, zu, st, xh, af)</param>
    /// <param name="variables">Variables to substitute (e.g., {{memberName}})</param>
    /// <returns>Formatted message</returns>
    string RenderTemplate(string templateKey, string languageCode, Dictionary<string, string> variables);
}
