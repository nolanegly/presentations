using System;
using System.ComponentModel.DataAnnotations;

namespace VolunteerConvergence.Models
{
    public class Applicant : IDomainEntity
    {
        public Guid Id { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Essay { get; set; }
    }
}