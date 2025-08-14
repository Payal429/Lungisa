using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lungisa.Models
{
    public class HomeViewModel
    {
        public List<EventModel> LatestEvents { get; set; }
        public List<ProjectModel> CompletedProjects { get; set; }
        public List<ProjectModel> CurrentProjects { get; set; }
        public List<ProjectModel> UpcomingProjects { get; set; }
    }
}