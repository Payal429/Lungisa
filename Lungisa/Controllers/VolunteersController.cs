using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lungisa.Controllers
{
    public class VolunteersController : Controller
    {
        // GET: Volunteers
        public ActionResult Index()
        {
            return View();
        }
    }
}