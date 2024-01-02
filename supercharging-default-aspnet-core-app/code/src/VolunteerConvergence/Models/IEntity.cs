using System;

namespace VolunteerConvergence.Models
{
    public interface IDomainEntity : IEntityWithId, IEntityWithRowVersion
    {
    }

    public interface IEntityWithId
    {
        Guid Id { get; }
    }

    public interface IEntityWithRowVersion
    {
        byte[] RowVersion { get; set; }
    }
}