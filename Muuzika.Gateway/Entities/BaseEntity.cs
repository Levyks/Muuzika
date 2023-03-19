using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muuzika.Gateway.Entities;

public class BaseEntity
{
    public int Id { get; set; }
    
    [ForeignKey("CreatedById")]
    public AuthenticatableEntity? CreatedBy { get; set; }
    public int? CreatedById { get; set; }
    
    [ForeignKey("UpdatedById")]
    public AuthenticatableEntity? UpdatedBy { get; set; }
    public int? UpdatedById { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}