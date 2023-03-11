using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Muuzika.Gateway.Enums;

namespace Muuzika.Gateway.Entities;

[Table("users")]
[Index(nameof(Email), IsUnique = true)]
public class UserEntity: AuthenticatableEntity
{
    public string Name { get; set; }
    public string Password { get; set; }
    [EmailAddress] 
    public string Email { get; set; }
    
    [NotMapped]
    public override AuthenticatableTypeEnum Type => AuthenticatableTypeEnum.User;
}