using System.ComponentModel.DataAnnotations;
using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Providers.Interfaces;

namespace Muuzika.Server.Attributes.Validation;

public class TimeSpanRangeFromConfigAttribute: ValidationFromConfigAttribute
{
    private readonly string _minConfigKey;
    private readonly string _maxConfigKey;

    public TimeSpanRangeFromConfigAttribute(string minConfigKey, string maxConfigKey)
    {
        _minConfigKey = minConfigKey;
        _maxConfigKey = maxConfigKey;
    }
    
    protected override ValidationAttribute GetAttribute(IConfigProvider configProvider)
    {
        var min = GetConfigValue<TimeSpan>(configProvider, _minConfigKey).ToString();
        var max = GetConfigValue<TimeSpan>(configProvider, _maxConfigKey).ToString();

        return new RangeAttribute(typeof(TimeSpan), min, max);
    }
}