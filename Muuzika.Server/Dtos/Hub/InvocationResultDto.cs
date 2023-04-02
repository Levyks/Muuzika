using Muuzika.Server.Dtos.Misc;
using Muuzika.Server.Exceptions;

namespace Muuzika.Server.Dtos.Hub;

public record InvocationResultDto<T>(
    bool Success,
    T? Data,
    BaseExceptionDto? Exception
);
