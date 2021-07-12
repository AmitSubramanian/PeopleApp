using System;

namespace PeopleApp.Models
{
    public class Person
    {
        public Guid PersonId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }

        public DateTime LastUpdatedDttm { get; set; }
    }
}
