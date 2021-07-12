using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeopleApp.Infrastructure.Repository
{
    public class Result
    {
        public Guid Id { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}
