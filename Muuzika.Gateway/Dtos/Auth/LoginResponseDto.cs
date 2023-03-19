using Muuzika.Gateway.Entities;

namespace Muuzika.Gateway.Dtos.Auth;

public record LoginResponseDto(
    string Token,
    UserEntity User
);