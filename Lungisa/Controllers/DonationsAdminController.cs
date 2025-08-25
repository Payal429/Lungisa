using Microsoft.AspNetCore.Mvc;

namespace Lungisa.Controllers
{
    public class DonationsAdminController : Controller
    {
        // GET: DonationsAdmin
        public ActionResult Index()
        {
            return View();
        }
    }
}