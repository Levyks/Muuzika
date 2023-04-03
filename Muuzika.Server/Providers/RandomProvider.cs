using Muuzika.Server.Providers.Interfaces;

namespace Muuzika.Server.Providers;

public class RandomProvider : IRandomProvider
{
    private readonly int? _seed;
    
    public RandomProvider(int? seed = null)
    {
        _seed = seed;
    }

    public Random GetRandom()
    {
        return _seed == null ? new Random() : new Random(_seed.Value);
    }
}