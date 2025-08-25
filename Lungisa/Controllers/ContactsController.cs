using System.Net.Mail;
using System.Net;
using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lungisa.Controllers
{
    public class ContactsController : Controller
    {
        private readonly FirebaseService firebase;

        public ContactsController(FirebaseService firebase)
        {
            this.firebase = firebase;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            // Retrieve all contacts from Firebase
            var contacts = await firebase.GetAllContacts() ?? new List<Lungisa.Models.ContactModel>();

            // Pass any temporary success or error messages to the view
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];

            // Render the Contacts view with the retrieved list
            return View("~/Views/Admin/Contacts.cshtml", contacts);
        }

        // This action explicitly loads the contacts management page.
        [HttpGet]
        public async Task<ActionResult> Contacts()
        {
            // Retrieve all contacts from Firebase
            var contacts = await firebase.GetAllContacts() ?? new List<Lungisa.Models.ContactModel>();

            // Pass any temporary success or error messages to the view
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];

            // Render the Contacts view with the retrieved list
            return View("~/Views/Admin/Contacts.cshtml", contacts);
        }

        // GET USERS THAT CONTACTED LUNGISA

        /*        public async Task<ActionResult> Contacts()
                {
                    var contacts = await _firebase.GetAllContacts() ?? new List<ContactModel>();
                    return View(contacts); // send Dictionary<string, ContactModel> to the view
                }*/
        [HttpPost]
        public ActionResult SendContactEmail(string email, string name)
        {
            try
            {
                var fromAddress = new MailAddress("noreplylungisa@gmail.com", "Lungisa NPO");
                var toAddress = new MailAddress(email, name);
                string fromPassword = "rxue wsuh idcr oupw";
                string subject = "Thank You for Contacting Us";
                string body = $"Dear {name},\n\nThank you for reaching out to Lungisa NPO. We have received your message and will get back to you shortly.\n\nBest Regards,\nLungisa NPO Team";

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

            return RedirectToAction("Contacts");
        }
    }
}