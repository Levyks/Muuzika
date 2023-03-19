using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Muuzika.Gateway.Enums;

namespace Muuzika.Gateway.Entities;

[Table("servers")]
[Index(nameof(Name), IsUnique = true)]
public class ServerEntity: AuthenticatableEntity
{
    public string Name { get; set; } = null!;
    public string Token { get; set; } = null!;
    
    public EnvironmentEntity Environment { get; set; } = null!;
    public ICollection<RoomEntity> Rooms { get; set; } = null!;
    
    [NotMapped]
    public override AuthenticatableTypeEnum Type => AuthenticatableTypeEnum.Server;
}