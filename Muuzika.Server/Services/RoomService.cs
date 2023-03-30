using Muuzika.Server.Dtos.Gateway;
using Muuzika.Server.Enums.Misc;
using Muuzika.Server.Exceptions;
using Muuzika.Server.Models;
using Muuzika.Server.Models.Extensions.Room;
using Muuzika.Server.Repositories.Interfaces;
using Muuzika.Server.Services.Interfaces;

namespace Muuzika.Server.Services;

public class RoomService: IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IJwtService _jwtService;
    private readonly ICaptchaService _captchaService;
    
    public RoomService(IRoomRepository roomRepository, IJwtService jwtService, ICaptchaService captchaService)
    {
        _roomRepository = roomRepository;
        _jwtService = jwtService;
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
            var room = new Room(code, createRoomDto.Username, _jwtService);

            return new RoomCreatedOrJoinedDto
            (
                Username: room.Leader.Username,
                RoomCode: room.Code,
                Token: room.GetTokenForPlayer(room.Leader)
            );   
        }
        catch (Exception e)
        {
            _roomRepository.PushAvailableCode(code);
            throw;
        }
    }

    public Task<RoomCreatedOrJoinedDto> JoinRoom(string roomCode, CreateOrJoinRoomDto joinRoomDto)
    {
        throw new NotImplementedException();
    }
}