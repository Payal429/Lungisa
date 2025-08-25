using Microsoft.AspNetCore.Mvc;

namespace Lungisa.Controllers
{
    public class DonationsController : Controller
    {
        // GET: Donations
        public ActionResult Index()
        {
            return View();
        }
    }
}