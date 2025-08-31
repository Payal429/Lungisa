using Lungisa.Models;
using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lungisa.Controllers
{
    public class HomeController : Controller
    {
        // Firebase service to fetch and save data (events, projects, subscribers, contacts, volunteers, news)
        private readonly FirebaseService _firebase;
        // Email helper service to send confirmation emails
        private readonly EmailHelper _emailHelper;

        // Constructor: inject FirebaseService and EmailHelper
        public HomeController(FirebaseService firebase, EmailHelper emailHelper)
        {
            _firebase = firebase;
            _emailHelper = emailHelper;
        }
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
        public ActionResult FAQs()
        {
            return View();
        }

        // ===================== INDEX PAGE =====================
        public async Task<ActionResult> Index()
        {
            // ==== EVENTS ====
            var allEvents = await _firebase.GetAllEvents() ?? new List<EventModel>();
            // Get latest 3 events sorted by date descending
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
            var allProjects = await _firebase.GetAllProjectsWithKeys() ?? new List<FirebaseService.FirebaseProject>();

            // Get latest completed project
            var completedProjects = allProjects
                .Where(p => p.Project.Type?.ToLower() == "completed")
                .OrderByDescending(p => p.Project.StartDate)   // Sort by date, newest first
                .Select(p => p.Project)
                .Take(1)
                .ToList();

            // Get latest current project
            var currentProjects = allProjects
                .Where(p => p.Project.Type?.ToLower() == "currently")
                .OrderByDescending(p => p.Project.StartDate)
                .Select(p => p.Project)
                .Take(1)
                .ToList();

            // Get latest upcoming project
            var upcomingProjects = allProjects
                .Where(p => p.Project.Type?.ToLower() == "upcoming")
                .OrderByDescending(p => p.Project.StartDate)
                .Select(p => p.Project)
                .Take(1)
                .ToList();


            // Create view model containing all data for the homepage
            var model = new HomeViewModel
            {
                LatestEvents = latestEvents,
                CompletedProjects = completedProjects,
                CurrentProjects = currentProjects,
                UpcomingProjects = upcomingProjects
            };

            return View(model);
        }

        // ==== SUBSCRIBE ====
        [HttpPost]
        public async Task<ActionResult> Subscribe(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Please enter a valid email.";
                return RedirectToAction("Index");
            }

            // Create subscriber model
            var subscriber = new SubscriberModel
            {
                Email = email,
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Save subscriber to Firebase
            await _firebase.SaveSubscriber(subscriber);

            try
            {
                string subject = "Thank You for Subscribing!";
                string body = $"Dear Subscriber,\n\nThank you for subscribing to updates from Lungisa NPO. " +
                              "We are excited to keep you informed about our latest news, events, and opportunities.\n\n" +
                              "Best Regards,\nLungisa NPO Team";

                await _emailHelper.SendEmailAsync(email, email, subject, body);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Subscription saved, but email could not be sent: " + ex.Message;
                return RedirectToAction("Index");
            }

            TempData["Success"] = "✅ Thank you for subscribing! A confirmation email has been sent.";
            return RedirectToAction("Index");
        }

        // ==== CONTACT ====
        [HttpPost]
        public async Task<ActionResult> Contact(string firstName, string lastName, string email, string subject, string message)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message))
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction("Contact");
            }

            // Create contact model
            var contact = new ContactModel
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Subject = subject,
                Message = message,
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Save contact message to Firebase
            await _firebase.SaveContact(contact);

            try
            {
                // Send thank-you email to the contact
                string emailSubject = "Thank You for Contacting Us!";
                string emailBody = $"Hi {firstName},\n\nThank you for reaching out to Lungisa NPO. " +
                                   "We have received your message and will get back to you shortly.\n\n" +
                                   "Best Regards,\nLungisa NPO Team";

                await _emailHelper.SendEmailAsync(email, firstName, emailSubject, emailBody);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Message sent, but confirmation email could not be delivered: " + ex.Message;
                return RedirectToAction("Contact");
            }

            TempData["Success"] = "✅ Your message has been sent! A confirmation email has been sent to you.";
            return RedirectToAction("Contact");
        }

        // ==== VOLUNTEER ====
        [HttpPost]
        public async Task<ActionResult> Volunteer(string fullName, string email, string phone, string availability, string rolePreference)
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction("Volunteer");
            }

            // Create volunteer model
            var volunteer = new VolunteerModel
            {
                FullName = fullName,
                Email = email,
                Phone = phone,
                Availability = availability,
                RolePreference = rolePreference,
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Save volunteer to Firebase
            await _firebase.SaveVolunteer(volunteer);

            try
            {
                // Send confirmation email to volunteer
                string subject = "Thank You for Signing up!";
                string body = $"Hi {fullName},\n\nThank you for signing up as a volunteer with Lungisa NPO. " +
                              "We appreciate your willingness to help and will get back to you with more details soon.\n\n" +
                              "Best Regards,\nLungisa NPO Team";

                await _emailHelper.SendEmailAsync(email, fullName, subject, body);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Volunteer saved, but confirmation email could not be sent: " + ex.Message;
                return RedirectToAction("Volunteer");
            }

            TempData["Success"] = "✅ Thank you for signing up! A confirmation email has been sent.";
            return RedirectToAction("Volunteer");
        }
        // ==== NEWS ====
        [HttpGet]
        public async Task<ActionResult> News(int page = 1)
        {
            const int pageSize = 6;
            var allNews = await _firebase.GetAllNewsWithKeys() ?? new List<FirebaseService.FirebaseNewsArticle>();

            // Sort news by date descending
            var sortedNews = allNews
                             .Where(n => n.Article != null)
                             .OrderByDescending(n => DateTime.Parse(n.Article.Date))
                             .ToList();

            // Paginate news articles
            var pagedNews = sortedNews.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(sortedNews.Count / (double)pageSize);

            return View("~/Views/Home/News.cshtml", pagedNews);
        }

        // ==== NEWS DETAILS ====
        [HttpGet]
        public async Task<ActionResult> NewsDetails(string id)
        {
            var allNews = await _firebase.GetAllNewsWithKeys();
            var article = allNews.FirstOrDefault(a => a.Key == id)?.Article;

            if (article == null)
                return NotFound();

            return View("~/Views/Home/NewsDetails.cshtml", article);
        }

    }
}
