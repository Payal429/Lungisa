// Payal and Nyanda 
// 15 August 2025
// Handles all administrative endpoints for the Lungisa application.

using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lungisa.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        // GET: Admin/Events
        public ActionResult Events()
        {
            return View();
        }

        // GET: Admin/Login
        public ActionResult Login()
        {
            return View();
        }

        // GET: Admin/NewsAdmin
        public ActionResult NewsAdmin()
        {
            return View();
        }

        // GET: Admin/Projects
        public ActionResult Projects()
        {
            return View();
        }

        // GET: Admin/Contacts
        public ActionResult Contacts()
        {
            return View();
        }

        // GET: Admin/Volunteers
        public ActionResult Volunteers()
        {
            return View();
        }

        // GET: Admin/Subscribers 
        public ActionResult Subscribers()
        {
            return View();
        }

        // Firebase service injected via constructor
        private readonly FirebaseService firebase;


        // GET: Admin/AdminDashboard
        public ActionResult AdminDashboard()
        {
            // Ensure the session contains the admin UID; otherwise redirect to login.
            var adminUid = HttpContext.Session.GetString("AdminUid");
            if (string.IsNullOrEmpty(adminUid))
                return RedirectToAction("Index", "Login");

            return View();
        }

        // GET: Admin/CreateAdmin
        public ActionResult CreateAdmin()
        {
            // Ensure the session contains the admin UID; otherwise redirect to login.
            var adminUid = HttpContext.Session.GetString("AdminUid");
            if (string.IsNullOrEmpty(adminUid))
                return RedirectToAction("Index", "Login");

            return View();
        }


        // POST: Admin/CreateAdmin
        [HttpPost]
        public async Task<ActionResult> CreateAdmin(string email, string password)
        {
            // Re-authorize the current user before allowing action.
            var adminUid = HttpContext.Session.GetString("AdminUid");
            if (string.IsNullOrEmpty(adminUid))
                return RedirectToAction("Index", "Login");

            // Basic validation.
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
                // Attempt to create the user in Firebase Authentication.
                var newUser = await firebase.CreateUserAsync(email, password);

                // Provide feedback to the view.
                ViewBag.Success = $"Admin created successfully! UID: {newUser.Uid}";

                return View();
            }
            catch (System.Exception ex)
            {
                // Display any Firebase-related errors.
                ViewBag.Error = ex.Message;
                return View();
            }
        }
    }
}