using Muuzika.Server.Providers;

namespace Muuzika.ServerTests.Unit;

public class DateTimeProviderTests
{
    private DateTimeProvider _dateTimeProvider = null!;
    private readonly TimeSpan _margin = TimeSpan.FromSeconds(1);
    
    [SetUp]
    public void Setup()
    {
        _dateTimeProvider = new DateTimeProvider();
    }

    [Test]
    public void ShouldReturnUtcNowApproximately()
    {
        var realNow = DateTime.UtcNow;
        var now = _dateTimeProvider.GetNow();
        
        Assert.That(now, Is.EqualTo(realNow).Within(_margin));
    }

    [Test]
    public async Task ShouldReturnAValue1SecondBiggerAfter1Second()
    {
        var now1 = _dateTimeProvider.GetNow();
        await Task.Delay(1000);
        var now2 = _dateTimeProvider.GetNow();
        
        Assert.That(now2, Is.EqualTo(now1.AddSeconds(1)).Within(_margin));
    }
}