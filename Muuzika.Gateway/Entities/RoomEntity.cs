using System.ComponentModel.DataAnnotations.Schema;

namespace Muuzika.Gateway.Entities;

[Table("rooms")]
public class RoomEntity: BaseEntity
{
    public string Code { get; set; } = null!;
    public bool Pending { get; set; } = true;
    public bool Finished { get; set; }
    public ServerEntity Server { get; set; } = null!;
}