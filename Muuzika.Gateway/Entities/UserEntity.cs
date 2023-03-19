using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Muuzika.Gateway.Enums;

namespace Muuzika.Gateway.Entities;

[Table("users")]
[Index(nameof(Email), IsUnique = true)]
public class UserEntity: AuthenticatableEntity
{
    public string Name { get; set; } = null!;
    public string Password { get; set; } = null!;
    [EmailAddress] 
    public string Email { get; set; } = null!;
    public DateTime? LastTokenInvalidation { get; set; } = null!;
    [NotMapped]
    public override AuthenticatableTypeEnum Type => AuthenticatableTypeEnum.User;
}