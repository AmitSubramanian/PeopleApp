using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeopleApp.ViewModels
{
    public class ReadPerson
    {
        public Guid PersonId { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }
}
