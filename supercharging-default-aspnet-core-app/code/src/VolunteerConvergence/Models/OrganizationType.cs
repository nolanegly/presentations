using System;
using System.ComponentModel.DataAnnotations;

namespace VolunteerConvergence.Models
{
    public class OrganizationType : IDomainEntity
    {
        public Guid Id { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
    }
}