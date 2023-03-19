namespace Muuzika.Gateway.Dtos.Auth;

public record LoginRequestDto (
    string Email,
    string Password
);