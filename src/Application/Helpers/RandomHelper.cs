using Muuzika.Application.Helpers.Interfaces;
using Muuzika.Application.Providers;

namespace Muuzika.Application.Helpers;

internal class RandomHelper: IRandomHelper
{
    private readonly IRandomProvider _randomProvider;
    
    public RandomHelper(IRandomProvider randomProvider)
    {
        _randomProvider = randomProvider;
    }

    public T TakeRandom<T>(ICollection<T> collection)
    {
        var index = _randomProvider.Random.Next(0, collection.Count);
        return collection.ElementAt(index);
    }

    public ICollection<T> TakeRandom<T>(ICollection<T> collection, int count)
    {
        var set = new HashSet<T>(collection);

        return Enumerable.Range(0, count)
            .Select(_ =>
            {
                var item = TakeRandom(set);
                set.Remove(item);
                return item;
            })
            .ToList();
    }
}