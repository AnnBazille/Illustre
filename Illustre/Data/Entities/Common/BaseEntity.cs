using System.ComponentModel.DataAnnotations;

namespace Data.Entities.Common;

public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }

    public bool IsActive { get; set; } = true;
}
