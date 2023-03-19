using Muuzika.Gateway.Services.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace Muuzika.Gateway.Services;

public class HashService: IHashService
{
    public string Hash(string input)
    {
        return BC.HashPassword(input);
    }

    public bool Verify(string input, string hash)
    {
        return BC.Verify(input, hash);
    }
}