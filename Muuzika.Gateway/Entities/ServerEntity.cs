using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Muuzika.Gateway.Enums;

namespace Muuzika.Gateway.Entities;

[Table("servers")]
[Index(nameof(Name), IsUnique = true)]
public class ServerEntity: AuthenticatableEntity
{
    public string Name { get; set; }
    public string Token { get; set; }
    
    
    [NotMapped]
    public override AuthenticatableTypeEnum Type => AuthenticatableTypeEnum.Server;
}