using System.Net;
using Muuzika.Domain.Exceptions.Interfaces;

namespace Muuzika.Domain.Exceptions;

public class RoomNotFoundException: BaseException, IWithStatusCode
{
    public HttpStatusCode StatusCode => HttpStatusCode.NotFound;
}