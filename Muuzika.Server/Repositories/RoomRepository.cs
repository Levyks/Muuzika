using System.Text;
using Muuzika.Server.Models;
using Muuzika.Server.Repositories.Interfaces;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Repositories;

public class RoomRepository: IRoomRepository
{
    private const int _maxNumberOfCodeGenerationAttempts = 10000;
    private const int _codeLength = 4;

    private readonly Dictionary<string, Room> _rooms = new();
    private readonly IRandomService _randomService;
    
    public RoomRepository(IRandomService randomService)
    {
        _randomService = randomService;
    }
    
    public Room? FindRoomByCode(string code)
    {
        return _rooms.ContainsKey(code) ? _rooms[code] : null;
    }
    
    public string? FindAvailableRoomCode()
    {
        for (var i = 0; i < _maxNumberOfCodeGenerationAttempts; i++)
        {
            var code = _randomService.GenerateRandomNumericString(_codeLength);
            if (!_rooms.ContainsKey(code))
            {
                return code;
            }
        }
        return null;
    }

}