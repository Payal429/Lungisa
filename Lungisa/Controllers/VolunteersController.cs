using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace Lungisa.Controllers
{
    public class VolunteersController : Controller
    {
        private readonly FirebaseService firebase;

        public VolunteersController(FirebaseService firebase)
        {
            this.firebase = firebase;
        }



        // This action explicitly loads the volunteers management page
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            // Retrieve all volunteers from Firebase
            var volunteers = await firebase.GetAllVolunteers() ?? new List<Lungisa.Models.VolunteerModel>();

            // Pass any temporary success or error messages to the view
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];

            // Render the Volunteers view with the retrieved list
            return View("~/Views/Admin/Volunteers.cshtml", volunteers);
        }

        // This action explicitly loads the volunteers management page
        [HttpGet]
        public async Task<ActionResult> Volunteers()
        {
            // Retrieve all volunteers from Firebase
            var volunteers = await firebase.GetAllVolunteers() ?? new List<Lungisa.Models.VolunteerModel>();

            // Pass any temporary success or error messages to the view
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];

            // Render the Volunteers view with the retrieved list
            return View("~/Views/Admin/Volunteers.cshtml", volunteers);
        }


        // Fetch all volunteers
        [HttpPost]
        public ActionResult SendVolunteerEmail(string email, string name)
        {
            try
            {
                var fromAddress = new MailAddress("noreplylungisa@gmail.com", "Lungisa NPO");
                var toAddress = new MailAddress(email, name);
                string fromPassword = "rxue wsuh idcr oupw"; // Gmail App Password
                string subject = "Volunteering Opportunity";
                string body = $"Dear {name},\n\nThank you for volunteering with Lungisa NPO. We have new opportunities available and would love your help!\n\nBest Regards,\nLungisa NPO Team";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 20000
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }

                TempData["Success"] = "Email sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error sending email: " + ex.Message;
            }

            return RedirectToAction("Volunteers");
        }
    }
}