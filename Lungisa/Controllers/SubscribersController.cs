using System.Net.Mail;
using System.Net;
using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lungisa.Controllers
{
    public class SubscribersController : Controller
    {
        private readonly FirebaseService firebase;

        public SubscribersController(FirebaseService firebase)
        {
            this.firebase = firebase;
        }



        // GET: Subscribers
        public async Task<ActionResult> Index()
        {
            // Loads the list of all subscribers and displays them on the Admin Subscribers page
                // Retrieve all subscribers from Firebase
                // If no subscribers exist, return an empty list to prevent null reference issues
                var subscribers = await firebase.GetAllSubscribers() ?? new List<Lungisa.Models.SubscriberModel>();

                // Retrieve any success message stored in TempData from a previous request
                ViewBag.Success = TempData["Success"];
                ViewBag.Error = TempData["Error"];

                // Render the Subscribers view with the retrieved list
                return View("~/Views/Admin/Subscribers.cshtml", subscribers);
            

        }
        // This action explicitly loads the subscribers management page.
        [HttpGet]
        public async Task<ActionResult> Subscribers()
        {
            // Retrieve all subscribers from Firebase
            var subscribers = await firebase.GetAllSubscribers() ?? new List<Lungisa.Models.SubscriberModel>();

            // Pass any temporary success or error messages to the view
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];

            // Render the Subscribers view with the retrieved list
            return View("~/Views/Admin/Subscribers.cshtml", subscribers);
        }


        // Send notification to subscriber
        [HttpPost]
        public ActionResult NotifySubscriber(string email, string name)
        {
            try
            {
                var fromAddress = new MailAddress("noreplylungisa@gmail.com", "Lungisa NPO");
                var toAddress = new MailAddress(email);
                string fromPassword = "rxue wsuh idcr oupw"; // Gmail App Password
                string subject = "Lungisa Update!";
                string body = $"Dear Subscriber,\n\nWe have an important update for you! Please check our website for the latest news and opportunities.\n\nBest Regards,\nLungisa NPO Team";

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

                TempData["Success"] = $"Email sent to {name}!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error sending email: " + ex.Message;
            }

            return RedirectToAction("Subscribers");
        }


    }
}