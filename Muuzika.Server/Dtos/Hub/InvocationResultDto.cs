using Muuzika.Server.Dtos.Misc;

namespace Muuzika.Server.Dtos.Hub;

public record InvocationResultDto<T>(
    bool Success,
    T? Data,
    BaseExceptionDto? Exception
);
