using Muuzika.Domain.Enums;

namespace Muuzika.Domain.Models;

public class Round
{
    public int Number { get; }
    public RoundType Type { get; }

    public Round(int number, RoundType type)
    {
        Number = number;
        Type = type;
    }
}