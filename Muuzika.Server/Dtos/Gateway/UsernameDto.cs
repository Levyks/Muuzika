using System.ComponentModel.DataAnnotations;

namespace Muuzika.Server.Dtos.Gateway;

public record UsernameDto(
    [Required]
    [StringLength(50, MinimumLength = 3)]
    string Username
);