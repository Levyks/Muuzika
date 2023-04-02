using Muuzika.Server.Dtos.Misc;
using Muuzika.Server.Exceptions;

namespace Muuzika.Server.Mappers.Interfaces;

public interface IExceptionMapper
{
    BaseExceptionDto ToDto(BaseException exception);
}