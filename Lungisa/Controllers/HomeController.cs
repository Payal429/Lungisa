using Lungisa.Models;
using Lungisa.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Lungisa.Controllers
{
    public class HomeController : Controller
    {
        FirebaseService firebase = new FirebaseService();
        // GET: Home
/*        public ActionResult Index()
        {
            return View();
        }*/
        public ActionResult News()
        {
            return View();
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }
        public ActionResult Projects()
        {
            return View();
        }
        public ActionResult Services()
        {
            return View();
        }
        public ActionResult Volunteer()
        {
            return View();
        }
        public ActionResult Donations()
        {
            return View();
        }

        /*        public async Task<ActionResult> Index()
                {
                    var allEvents = await firebase.GetAllEvents() ?? new List<EventModel>();

                    var latestEvents = allEvents
                        .OrderByDescending(e =>
                        {
                            DateTime dt;
                            DateTime.TryParse(e.DateTime, out dt);
                            return dt;
                        })
                        .Take(3)
                        .ToList();

                    return View(latestEvents); // send to Index.cshtml
                }*/
        public async Task<ActionResult> Index()
        {
            // ==== EVENTS ====
            var allEvents = await firebase.GetAllEvents() ?? new List<EventModel>();
            var latestEvents = allEvents
                .OrderByDescending(e =>
                {
                    DateTime dt;
                    DateTime.TryParse(e.DateTime, out dt);
                    return dt;
                })
                .Take(1)
                .ToList();

            // ==== PROJECTS ====
            var allProjects = await firebase.GetAllProjectsWithKeys() ?? new List<FirebaseService.FirebaseProject>();

            var completedProjects = allProjects
                .Where(p => p.Project.Type?.ToLower() == "completed")
                .Select(p => p.Project)
                .Take(1)
                .ToList();

            var currentProjects = allProjects
                .Where(p => p.Project.Type?.ToLower() == "currently")
                .Select(p => p.Project)
                .Take(3)
                .ToList();

            var upcomingProjects = allProjects
                .Where(p => p.Project.Type?.ToLower() == "upcoming")
                .Select(p => p.Project)
                .Take(1)
                .ToList();


            // ==== CREATE VIEWMODEL ====
            var model = new HomeViewModel
            {
                LatestEvents = latestEvents,
                CompletedProjects = completedProjects,
                CurrentProjects = currentProjects,
                UpcomingProjects = upcomingProjects
            };

            return View(model);
        }
    }
}