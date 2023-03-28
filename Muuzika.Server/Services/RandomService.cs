using System.Text;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Services;

public class RandomService: IRandomService
{
    private readonly Func<Random> _randomFactory;
    
    public RandomService(Func<Random> randomFactory)
    {
        _randomFactory = randomFactory;
    }
    
    public RandomService(): this(() => new Random())
    {
    }
    
    public string GenerateRandomNumericString(int length)
    {
        var random = _randomFactory();
        var codeBuilder = new StringBuilder();
        for (var i = 0; i < length; i++)
        {
            codeBuilder.Append(random.Next(0, 10));
        }
        return codeBuilder.ToString();
    }

    public string GenerateRandomToken(int length)
    {
        var random = _randomFactory();
        var tokenBuilder = new StringBuilder();
        for (var i = 0; i < length; i++)
        {
            tokenBuilder.Append((char) random.Next(65, 91));
        }
        return tokenBuilder.ToString();
    }
}