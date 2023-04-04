using System.ComponentModel.DataAnnotations;
using Muuzika.Server.Providers.Interfaces;

namespace Muuzika.Server.Attributes.Validation;

public abstract class ValidationFromConfigAttribute: ValidationAttribute
{
    private static readonly Type ConfigProviderType = typeof(IConfigProvider);
    protected abstract ValidationAttribute GetAttribute(IConfigProvider configProvider);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var configProvider = validationContext.GetRequiredService<IConfigProvider>();

        var attribute = GetAttribute(configProvider);

        if (attribute.IsValid(value)) return ValidationResult.Success;
        
        var memberNames = validationContext.MemberName is { } memberName
            ? new[] { memberName }
            : null;
        
        return new ValidationResult(attribute.FormatErrorMessage(validationContext.DisplayName), memberNames);
    }
    
    protected T GetConfigValue<T>(IConfigProvider configProvider, string configKey)
    {
        return (T) ConfigProviderType.GetProperty(configKey)!.GetValue(configProvider)!;
    }
}