using Lungisa.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Lungisa.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Events()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult NewsAdmin()
        {
            return View();
        }
        public ActionResult Projects()
        {
            return View();
        }
/*        public ActionResult CreateAdmin()
        {
            return View();
        }*/

        private readonly FirebaseService _firebase = new FirebaseService();
        // GET: Admin/AdminDashboard
        public ActionResult AdminDashboard()
        {
            if (Session["AdminUid"] == null)
                return RedirectToAction("Index", "Login");

            return View();
        }

        // GET: Admin/CreateAdmin
        public ActionResult CreateAdmin()
        {
            if (Session["AdminUid"] == null)
                return RedirectToAction("Index", "Login");

            return View();
        }

        // POST: Admin/CreateAdmin
        [HttpPost]
        public async Task<ActionResult> CreateAdmin(string email, string password)
        {
            if (Session["AdminUid"] == null)
                return RedirectToAction("Index", "Login");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and Password are required.";
                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.Error = "Password must be at least 6 characters.";
                return View();
            }

            try
            {
                var newUser = await _firebase.CreateUserAsync(email, password);
                ViewBag.Success = $"Admin created successfully! UID: {newUser.Uid}";
                return View();
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }
    }
}