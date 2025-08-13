/*using Lungisa.Models;
using Lungisa.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Lungisa.Controllers
{
    public class UsersController : Controller
    {
        private readonly FirebaseService firebase = new FirebaseService();

        // GET: /Users
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var users = await firebase.GetAllUsers() ?? new List<UserModel>();
            ViewBag.Success = TempData["Success"];
            // Explicitly specify the view users.cshtml path if needed
            return View("~/Views/Admin/users.cshtml", users);
        }

        // POST: /Users
        [HttpPost]
        public async Task<ActionResult> Index(UserModel model, string password)
        {
            if (ModelState.IsValid)
            {
                model.PasswordHash = HashPassword(password);
                model.Status = "Active";

                await firebase.AddUser(model);

                TempData["Success"] = "✅ User added successfully!";

                // Redirect to GET Index to reload and show updated users
                return RedirectToAction("Index");
            }

            // On validation failure, reload users and return the view with model list
            var usersList = await firebase.GetAllUsers() ?? new List<UserModel>();
            return View("~/Views/Admin/users.cshtml", usersList);
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
*/