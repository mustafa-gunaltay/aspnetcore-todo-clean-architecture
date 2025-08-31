using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoBackend.Domain.Models.BuildingBlocks;

public abstract class Entity
{
    [Key]
    public int Id { get; set; }
    
    public override bool Equals(object? obj)
    {
        if (obj is Entity entity)
        {
            return Id == entity.Id;
        }
        return false;
    }
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }



}
