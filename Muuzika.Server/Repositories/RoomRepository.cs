using System.Text;
using Muuzika.Server.Models;
using Muuzika.Server.Repositories.Interfaces;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Repositories;

public class RoomRepository: IRoomRepository
{
    private const int CodeLength = 4;

    private readonly Queue<ushort> _availableCodes = new();
    private readonly Dictionary<string, Room> _rooms = new();
    private readonly Func<Random> _randomFactory;
    
    public RoomRepository(Func<Random> randomFactory)
    {
        _randomFactory = randomFactory;
        PopulateAvailableCodes();
    }

    public Room? FindRoomByCode(string code)
    {
        _rooms.TryGetValue(code, out var value);
        return value;
    }

    public string? PopAvailableCode()
    {
        if (_availableCodes.Count == 0)
        {
            PopulateAvailableCodes();
        }
        return _availableCodes.Dequeue().ToString().PadLeft(CodeLength, '0');
    }
    
    private void PushAvailableCode(ushort code)
    {
        _availableCodes.Enqueue(code);
    }    
    
    public void PushAvailableCode(string code)
    {
        PushAvailableCode(ushort.Parse(code));
    }
    
    private void PopulateAvailableCodes()
    {
        var random = _randomFactory();
        var codes = Enumerable.Range(0, (int) Math.Pow(10, CodeLength));

        foreach (var code in codes.OrderBy(_ => random.Next()))
        {
            PushAvailableCode((ushort) code);
        }
    }

}