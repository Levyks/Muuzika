using System.Text;
using Muuzika.Server.Exceptions;
using Muuzika.Server.Models;
using Muuzika.Server.Providers.Interfaces;
using Muuzika.Server.Repositories.Interfaces;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Repositories;

public class RoomRepository: IRoomRepository
{
    private const int CodeLength = 4;

    private readonly Queue<ushort> _availableCodes = new();
    private readonly Dictionary<uint, Room> _rooms = new();
    private readonly IRandomProvider _randomProvider;
    
    public RoomRepository(IRandomProvider randomProvider)
    {
        _randomProvider = randomProvider;
        PopulateAvailableCodes();
    }

    public Room? FindRoomByCode(string code)
    {
        if (!ushort.TryParse(code, out var parsedCode))
        {
            return null;
        }
        
        _rooms.TryGetValue(parsedCode, out var value);
        
        return value;
    }
    
    public Room GetRoomByCode(string code)
    {
        return FindRoomByCode(code) ?? throw new RoomNotFoundException(code);
    }
    
    public void StoreRoom(Room room)
    {
        if (!ushort.TryParse(room.Code, out var parsedCode))
        {
            throw new ArgumentException($"Room code {room.Code} is not a valid code");
        }
            
        lock (_rooms)
        {
            if (_rooms.ContainsKey(parsedCode))
            {
                throw new ArgumentException($"Room with code {room.Code} already exists");
            }
        
            _rooms[parsedCode] = room;
        }
    }
    
    public bool RemoveRoom(Room room)
    {
        if (!ushort.TryParse(room.Code, out var parsedCode))
        {
            throw new ArgumentException($"Room code {room.Code} is not a valid code");
        }

        PushAvailableCode(parsedCode);
        return _rooms.Remove(parsedCode);
    }

    public string? PopAvailableCode()
    {
        lock (_availableCodes)
        {
            return _availableCodes.Count == 0 ? null : _availableCodes.Dequeue().ToString().PadLeft(CodeLength, '0');
        }
    }
    
    private void PushAvailableCode(ushort code)
    {
        lock (_availableCodes) _availableCodes.Enqueue(code);
    }    
    
    public void PushAvailableCode(string code)
    {
        PushAvailableCode(ushort.Parse(code));
    }
    
    private void PopulateAvailableCodes()
    {
        var random = _randomProvider.GetRandom();
        var codes = Enumerable.Range(0, (int) Math.Pow(10, CodeLength));

        foreach (var code in codes.OrderBy(_ => random.Next()))
        {
            PushAvailableCode((ushort) code);
        }
    }

}