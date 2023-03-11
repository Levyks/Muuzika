using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper.Configuration.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Muuzika.Gateway.Enums;

namespace Muuzika.Gateway.Entities;

[Table("authenticatables")]
public abstract class AuthenticatableEntity: BaseEntity
{
    [Column(TypeName = "text")]
    [ValueConverter(typeof(EnumToStringConverter<AuthenticatableTypeEnum>))]
    public virtual AuthenticatableTypeEnum Type { get; set; }
}