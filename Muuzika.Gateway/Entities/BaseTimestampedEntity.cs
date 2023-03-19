using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muuzika.Gateway.Entities;

public class BaseLogEntity: BaseEntity
{
    [ForeignKey("CreatedById")]
    public AuthenticatableEntity? CreatedBy { get; set; }
    public int? CreatedById { get; set; }
    
    [ForeignKey("UpdatedById")]
    public AuthenticatableEntity? UpdatedBy { get; set; }
    public int? UpdatedById { get; set; }
}