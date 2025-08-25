using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lungisa.Models
{
    public class VolunteerModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Availability { get; set; } // You can store as comma-separated string
        public string RolePreference { get; set; }
        public string Date { get; set; } // When they signed up
    }
}
