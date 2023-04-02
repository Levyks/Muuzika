using Muuzika.Server.Dtos.Misc;
using Muuzika.Server.Exceptions;
using Muuzika.Server.Mappers.Interfaces;

namespace Muuzika.Server.Mappers;

public class ExceptionMapper: IExceptionMapper
{
    public BaseExceptionDto ToDto(BaseException exception)
    {
        return new BaseExceptionDto(
            Type: exception.Type,
            Message: exception.Message,
            Data: exception.Data
        );
    }
}