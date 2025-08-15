using Lungisa.Models;
using Lungisa.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Lungisa.Controllers
{
    public class HomeController : Controller
    {
        FirebaseService firebase = new FirebaseService();
        public ActionResult News()
        {
            return View();
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }
        public ActionResult Projects()
        {
            return View();
        }
        public ActionResult Services()
        {
            return View();
        }
        public ActionResult Volunteer()
        {
            return View();
        }
        public ActionResult Donations()
        {
            return View();
        }
        public ActionResult Subscribers()
        {
            return View();
        }
        public ActionResult Contacts()
        {
            return View();
        }

        // ==== INDEX ====
        public async Task<ActionResult> Index()
        {
            // ==== EVENTS ====
            var allEvents = await firebase.GetAllEvents() ?? new List<EventModel>();
            var latestEvents = allEvents
                .OrderByDescending(e =>
                {
                    DateTime dt;
                    DateTime.TryParse(e.DateTime, out dt);
                    return dt;
                })
                .Take(3)
                .ToList();

            // ==== PROJECTS ====
            var allProjects = await firebase.GetAllProjectsWithKeys() ?? new List<FirebaseService.FirebaseProject>();

            var completedProjects = allProjects
                .Where(p => p.Project.Type?.ToLower() == "completed")
                .Select(p => p.Project)
                .Take(1)
                .ToList();

            var currentProjects = allProjects
                .Where(p => p.Project.Type?.ToLower() == "currently")
                .Select(p => p.Project)
                .Take(1)
                .ToList();

            var upcomingProjects = allProjects
                .Where(p => p.Project.Type?.ToLower() == "upcoming")
                .Select(p => p.Project)
                .Take(1)
                .ToList();

            // ==== CREATE VIEWMODEL ====
            var model = new HomeViewModel
            {
                LatestEvents = latestEvents,
                CompletedProjects = completedProjects,
                CurrentProjects = currentProjects,
                UpcomingProjects = upcomingProjects
            };

            return View(model);
        }

        // ==== SUBSCRIPTION ====
        [HttpPost]
        public async Task<ActionResult> Subscribe(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Please enter a valid email.";
                return RedirectToAction("Index");
            }

            var subscriber = new SubscriberModel
            {
                Email = email,
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            await firebase.SaveSubscriber(subscriber);

            // --- SEND THANK YOU EMAIL ---
            try
            {
                var fromAddress = new MailAddress("payal.sunil429@gmail.com", "Lungisa NPO");
                var toAddress = new MailAddress(email);
                string fromPassword = "bpzk wbru lwur xbrl"; // Gmail App Password
                string subject = "Thank You for Subscribing!";
                string body = $"Dear Subscriber,\n\nThank you for subscribing to updates from Lungisa NPO. " +
                              "We are excited to keep you informed about our latest news, events, and opportunities.\n\n" +
                              "Best Regards,\nLungisa NPO Team";

                using (var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 20000
                })
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                // Optional: log the error or show TempData message
                TempData["Error"] = "Subscription saved, but email could not be sent: " + ex.Message;
                return RedirectToAction("Index");
            }

            TempData["Success"] = "✅ Thank you for subscribing! A confirmation email has been sent.";
            return RedirectToAction("Index");
        
        }

        // ==== NEWS ====
        [HttpGet]
        public async Task<ActionResult> News(int page = 1)
        {
            const int pageSize = 6;
            var allNews = await firebase.GetAllNewsWithKeys() ?? new List<FirebaseService.FirebaseNewsArticle>();

            var sortedNews = allNews
                             .Where(n => n.Article != null)
                             .OrderByDescending(n => DateTime.Parse(n.Article.Date))
                             .ToList();

            var pagedNews = sortedNews.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(sortedNews.Count / (double)pageSize);

            return View("~/Views/Home/News.cshtml", pagedNews);
        }

        // ==== NEWS DETAILS ====
        [HttpGet]
        public async Task<ActionResult> NewsDetails(string id)
        {
            var allNews = await firebase.GetAllNewsWithKeys();
            var article = allNews.FirstOrDefault(a => a.Key == id)?.Article;

            if (article == null)
                return HttpNotFound();

            return View("~/Views/Home/NewsDetails.cshtml", article);
        }

        // ==== CONTACT DETAILS ====
        [HttpPost]
        public async Task<ActionResult> Contact(string firstName, string lastName, string email, string subject, string message)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message))
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction("Contact");
            }

            var contact = new ContactModel
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Subject = subject,
                Message = message,
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            await firebase.SaveContact(contact);

            TempData["Success"] = "✅ Your message has been sent!";
            return RedirectToAction("Contact");
        }

        [HttpPost]
        public async Task<ActionResult> Volunteer(string fullName, string email, string phone, string availability, string rolePreference)
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction("Volunteer");
            }

            var volunteer = new VolunteerModel
            {
                FullName = fullName,
                Email = email,
                Phone = phone,
                Availability = availability,
                RolePreference = rolePreference,
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            await firebase.SaveVolunteer(volunteer);

            TempData["Success"] = "✅ Thank you for signing up!";
            return RedirectToAction("Index");
        }



    }
}
