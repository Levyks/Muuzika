using System.Net;

namespace Muuzika.Domain.Exceptions.Interfaces;

public interface IWithStatusCode
{
    HttpStatusCode StatusCode { get; }
}