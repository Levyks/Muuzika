using System.ComponentModel.DataAnnotations;

namespace Muuzika.Server.Dtos.Gateway;

public record CreateOrJoinRoomDto(
    [Required]
    [StringLength(50, MinimumLength = 3)]
    string Username,
    
    [Required]
    string CaptchaToken
    );