using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VolunteerConvergence.Models
{
    public class Volunteer: IDomainEntity
    {
        public Guid Id { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime JoinDate { get; set; }

        public List<VolunteerHour> VolunteerHours { get; set; } = new List<VolunteerHour>();
    }
}