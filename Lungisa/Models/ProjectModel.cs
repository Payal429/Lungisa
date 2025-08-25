using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lungisa.Models
{

        public class ProjectModel
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public string StartDate { get; set; } 
            public string EndDate { get; set; }   
            public string Type { get; set; }  // "Completed", "Current", "Upcoming"
        }

}