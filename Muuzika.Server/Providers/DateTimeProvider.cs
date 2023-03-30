using Muuzika.Server.Providers.Interfaces;

namespace Muuzika.Server.Providers;

public class DateTimeProvider: IDateTimeProvider
{
    public DateTime GetNow()
    {
        return DateTime.UtcNow;
    }
}