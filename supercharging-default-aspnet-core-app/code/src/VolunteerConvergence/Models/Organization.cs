using System;
using System.ComponentModel.DataAnnotations;

namespace VolunteerConvergence.Models
{
    public class Organization : IDomainEntity
    {
        public Guid Id { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public Guid OrganizationTypeId { get; set; }
        public OrganizationType OrganizationType { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
    }
}