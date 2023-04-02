using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Dtos.Misc;

public record BaseExceptionDto(
    ExceptionType Type,
    string Message,
    object? Data
);