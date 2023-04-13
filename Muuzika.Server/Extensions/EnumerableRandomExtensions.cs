namespace Muuzika.Server.Extensions;

public static class EnumerableRandomExtensions
{
    /*
     * Picking random indexes with Random.Next() and checking if they were already picked might
     * be faster than our approach of using a HashSet, but if `count` is close to the number of
     * elements in `source`, the HashSet approach should be faster.
     *
     * TODO: Benchmark this method and the alternative approach with real-world data.
     */
    public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> source, int count, Random? random = null)
    {
        random ??= new Random();
        var sourceSet = new HashSet<T>(source);

        return System.Linq.Enumerable.Range(0, count)
            .Select(_ =>
            {
                var song = sourceSet.PickRandom(random);
                sourceSet.Remove(song);
                return song;
            });
    }
    
    private static T PickRandom<T>(this IReadOnlyCollection<T> source, Random random)
    {
        var idx = random.Next(source.Count);
        return source.ElementAt(idx);
    }
    
    public static T PickRandom<T>(this IEnumerable<T> source, Random? random = null)
    {
        random ??= new Random();
        return source switch
        {
            IReadOnlyCollection<T> readOnlyCollection => readOnlyCollection.PickRandom(random),
            _ => source.ToArray().PickRandom(random)
        };
    }
}