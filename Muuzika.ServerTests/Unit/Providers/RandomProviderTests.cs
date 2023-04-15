using Muuzika.Server.Providers;

namespace Muuzika.ServerTests.Unit.Providers;

public class RandomProviderTests
{
    private RandomProvider _randomProvider = null!;

    [SetUp]
    public void Setup()
    {
        _randomProvider = new RandomProvider();
    }

    [Test]
    public void GetRandom_ShouldReturnRandomInstance()
    {
        var random = _randomProvider.GetRandom();

        Assert.That(random, Is.Not.Null);
    }
}