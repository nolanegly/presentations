using System;
using System.ComponentModel.DataAnnotations;

namespace VolunteerConvergence.Models
{
    public class VolunteerHour : IDomainEntity
    {
        public Guid Id { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public Guid VolunteerId { get; set; }
        public Volunteer Volunteer { get; set; }

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

        public int NumHours { get; set; }
    }
}