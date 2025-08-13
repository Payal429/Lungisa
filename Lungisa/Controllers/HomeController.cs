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

        public async Task<ActionResult> Index()
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
        }
        
    }
}
