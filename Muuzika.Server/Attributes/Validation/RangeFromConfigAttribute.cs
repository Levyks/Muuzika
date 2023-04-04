using System.ComponentModel.DataAnnotations;
using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Providers.Interfaces;

namespace Muuzika.Server.Attributes.Validation;

public class RangeFromConfigAttribute: ValidationFromConfigAttribute
{
    private readonly string _minConfigKey;
    private readonly string _maxConfigKey;

    public RangeFromConfigAttribute(string minConfigKey, string maxConfigKey)
    {
        _minConfigKey = minConfigKey;
        _maxConfigKey = maxConfigKey;
    }
    
    protected override ValidationAttribute GetAttribute(IConfigProvider configProvider)
    {
        var min = GetConfigValue<int>(configProvider, _minConfigKey);
        var max = GetConfigValue<int>(configProvider, _maxConfigKey);

        return new RangeAttribute(min, max);
    }
}