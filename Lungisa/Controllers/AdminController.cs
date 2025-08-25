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
        public ActionResult Contacts()
        {
            return View();
        }
        public ActionResult Volunteers()
        {
            return View();
        }
        public ActionResult Subscribers()
        {
            return View();
        }


        private readonly FirebaseService firebase;


        // GET: Admin/AdminDashboard
        public ActionResult AdminDashboard()
        {
            var adminUid = HttpContext.Session.GetString("AdminUid");
            if (string.IsNullOrEmpty(adminUid))
                return RedirectToAction("Index", "Login");

            return View();
        }

        // GET: Admin/CreateAdmin
        public ActionResult CreateAdmin()
        {
            var adminUid = HttpContext.Session.GetString("AdminUid");
            if (string.IsNullOrEmpty(adminUid))
                return RedirectToAction("Index", "Login");

            return View();
        }

        // POST: Admin/CreateAdmin
        [HttpPost]
        public async Task<ActionResult> CreateAdmin(string email, string password)
        {
            var adminUid = HttpContext.Session.GetString("AdminUid");
            if (string.IsNullOrEmpty(adminUid))
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
                var newUser = await firebase.CreateUserAsync(email, password);
                ViewBag.Success = $"Admin created successfully! UID: {newUser.Uid}";
                return View();
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        // GET: Admin/Volunteers
        /*public async Task<ActionResult> Volunteers()
        {
            var volunteers = await _firebase.GetAllVolunteers() ?? new List<VolunteerModel>();
            return View(volunteers);
        }

        // Fetch all volunteers
        [HttpPost]
        public ActionResult SendVolunteerEmail(string email, string name)
        {
            try
            {
                var fromAddress = new MailAddress("payal.sunil429@gmail.com", "Lungisa NPO");
                var toAddress = new MailAddress(email, name);
                string fromPassword = "bpzk wbru lwur xbrl"; // Gmail App Password
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

        // GET USERS THAT CONTACTED LUNGISA

        public async Task<ActionResult> Contacts()
        {
            var contacts = await _firebase.GetAllContacts() ?? new List<ContactModel>();
            return View(contacts); // send Dictionary<string, ContactModel> to the view
        }
        [HttpPost]
        public ActionResult SendContactEmail(string email, string name)
        {
            try
            {
                var fromAddress = new MailAddress("payal.sunil429@gmail.com", "Lungisa NPO");
                var toAddress = new MailAddress(email, name);
                string fromPassword = "bpzk wbru lwur xbrl";
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
        }*/

        // GET: Admin/Subscribers
/*        public async Task<ActionResult> Subscribers()
        {
            var subscribers = await _firebase.GetAllSubscribers() ?? new List<SubscriberModel>();
            return View(subscribers);
        }

        // Send notification to subscriber
        [HttpPost]
        public ActionResult NotifySubscriber(string email, string name)
        {
            try
            {
                var fromAddress = new MailAddress("payal.sunil429@gmail.com", "Lungisa NPO");
                var toAddress = new MailAddress(email, name);
                string fromPassword = "bpzk wbru lwur xbrl"; // replace with Gmail App Password
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
        }*/
    }
}