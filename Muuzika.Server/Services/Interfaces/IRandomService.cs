namespace Muuzika.Server.Services.Interfaces;

public interface IRandomService
{
    string GenerateRandomNumericString(int length);
    string GenerateRandomToken(int length);
}