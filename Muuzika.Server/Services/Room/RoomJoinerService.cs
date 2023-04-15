using Muuzika.Server.Dtos.Gateway;
using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Exceptions;
using Muuzika.Server.Mappers.Interfaces;
using Muuzika.Server.Repositories.Interfaces;
using Muuzika.Server.Services.Interfaces;
using Muuzika.Server.Services.Room.Interfaces;

namespace Muuzika.Server.Services.Room;

public class RoomJoinerService: IRoomJoinerService
{
    private readonly IRoomRepository _roomRepository;
    private readonly ILogger<RoomJoinerService> _logger;
    private readonly IRoomMapper _roomMapper;
    private readonly ICaptchaService _captchaService;
    private readonly IServiceProvider _serviceProvider;
    
    public RoomJoinerService(IServiceProvider serviceProvider, ILogger<RoomJoinerService> logger, IRoomRepository roomRepository, IRoomMapper roomMapper, ICaptchaService captchaService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _roomRepository = roomRepository;
        _roomMapper = roomMapper;
        _captchaService = captchaService;
    }

    private async Task ValidateCaptcha(CaptchaAction action, CreateOrJoinRoomDto createOrJoinRoomDto)
    {
        var captchaResult = await _captchaService.ValidateCaptchaAsync(CaptchaAction.CreateRoom, createOrJoinRoomDto.CaptchaToken);
        
        if (!captchaResult)
        {
            throw new InvalidCaptchaException();
        }
    }

    public async Task<RoomCreatedOrJoinedDto> CreateRoom(CreateOrJoinRoomDto createRoomDto)
    {
        await ValidateCaptcha(CaptchaAction.CreateRoom, createRoomDto);

        var code = _roomRepository.PopAvailableCode();
        
        if (code == null)
        {
            throw new OutOfAvailableRoomCodesException();
        }
        
        try
        {
            var room = new Models.Room(code, createRoomDto.Username, _roomRepository, _serviceProvider);
            
            _roomRepository.StoreRoom(room);

            return _roomMapper.ToCreatedOrJoinedDto(room, room.Leader);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating room");
            
            var possibleRoom = _roomRepository.FindRoomByCode(code);
            
            if (possibleRoom == null)
            {
                _roomRepository.PushAvailableCode(code);
            }
            else
            {
                possibleRoom.Dispose();
            }
            
            throw;
        }
    }

    public async Task<RoomCreatedOrJoinedDto> JoinRoom(string roomCode, CreateOrJoinRoomDto joinRoomDto)
    {
        await ValidateCaptcha(CaptchaAction.JoinRoom, joinRoomDto);

        var room = _roomRepository.GetRoomByCode(roomCode);
        
        var roomPlayerService = room.ServiceProvider.GetRequiredService<IRoomPlayerService>();

        var player = roomPlayerService.AddPlayer(joinRoomDto.Username);
        
        return _roomMapper.ToCreatedOrJoinedDto(room, player);
    }
}