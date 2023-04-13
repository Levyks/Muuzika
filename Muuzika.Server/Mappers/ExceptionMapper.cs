using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Muuzika.Server.Dtos.Misc;
using Muuzika.Server.Exceptions;
using Muuzika.Server.Mappers.Interfaces;

namespace Muuzika.Server.Mappers;

public class ExceptionMapper: IExceptionMapper
{
    private const string HubExceptionMessagePrefix = "@BaseException:";
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    
    public ExceptionMapper(JsonSerializerOptions jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
    }
    
    public BaseExceptionDto ToDto(BaseException exception)
    {
        return new BaseExceptionDto(
            Type: exception.Type,
            Message: exception.Message,
            Data: exception.Data
        );
    }

    public HubException ToHubException(Exception exception)
    {
        var baseException = exception as BaseException ?? new UnknownException();
        var baseExceptionJson = JsonSerializer.Serialize(ToDto(baseException), _jsonSerializerOptions);
        return new HubException(HubExceptionMessagePrefix + baseExceptionJson);
    }

    public BaseExceptionDto ParseHubException(HubException exception)
    {
        var baseExceptionJsonIndex = exception.Message.IndexOf(HubExceptionMessagePrefix, StringComparison.Ordinal);

        if (baseExceptionJsonIndex == -1) return ToDto(new UnknownException());
        
        var baseExceptionJson = exception.Message[(baseExceptionJsonIndex + HubExceptionMessagePrefix.Length)..];

        return JsonSerializer.Deserialize<BaseExceptionDto>(baseExceptionJson, _jsonSerializerOptions) ?? ToDto(new UnknownException());
    }
}