// Payal and Nyanda 
// 15 August 2025
// Handles all administrative endpoints for the Lungisa application.

using FirebaseAdmin.Auth;
using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lungisa.Controllers
{
    public class AdminController : Controller
    {
        private readonly FirebaseService firebase;

        public AdminController(FirebaseService firebaseService)
        {
            firebase = firebaseService;
        }
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
        public ActionResult DonationsAdmin()
        {
            return View();
        }
        public ActionResult Analytics()
        {
            return View();
        }


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
        public async Task<IActionResult> CreateAdmin(string firstName, string lastName, string email, string password, string phoneNumber, string role)
        {
            try
            {
                // 1️⃣ Create admin user in Firebase Authentication
                var userRecordArgs = new UserRecordArgs
                {
                    Email = email,
                    Password = password,
                    DisplayName = $"{firstName} {lastName}",
                    PhoneNumber = string.IsNullOrEmpty(phoneNumber) ? null : phoneNumber
                };

                var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userRecordArgs);

                // 2️⃣ Save extra admin info in Firebase Realtime Database
                await firebase.SaveAdmin(userRecord.Uid, email, firstName, lastName, phoneNumber, role);

                ViewBag.Success = "Admin created successfully!";
            }
            catch (FirebaseAuthException ex)
            {
                ViewBag.Error = $"Firebase Auth Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
            }

            return View();
        }
    

    }
}