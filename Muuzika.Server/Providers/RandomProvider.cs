using Muuzika.Server.Providers.Interfaces;

namespace Muuzika.Server.Providers;

public class RandomProvider : IRandomProvider
{
    public Random GetRandom()
    {
        return new Random();
    }
}