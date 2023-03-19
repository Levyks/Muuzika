using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Muuzika.Gateway.Entities;

[Table("environments")]
[Index(nameof(Name), IsUnique = true)]
public class EnvironmentEntity: BaseLogEntity
{
    public string Name { get; set; } = null!;
    public int RoomCodeLength { get; set; }
    public int NicknameMinLength { get; set; }
    public int NicknameMaxLength { get; set; }
    public int MinNumberOfRounds { get; set; }
    public int MaxNumberOfRounds { get; set; }
    public int DefaultNumberOfRounds { get; set; }
    public int MaxNumberOfPlayers { get; set; }
    public int MinRoundDuration { get; set; }
    public int MaxRoundDuration { get; set; }
    public int DefaultRoundDuration { get; set; }
    
    public ICollection<ServerEntity> Servers { get; set; } = null!;
}