using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lungisa.Models
{
    public class ContactModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Date { get; set; } // When the message was sent
    }
}
