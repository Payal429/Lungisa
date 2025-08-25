using Lungisa.Models;
using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lungisa.Controllers
{
    public class HomeController : Controller
    {
        private readonly FirebaseService _firebase;
        private readonly EmailHelper _emailHelper;

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

        // ==== INDEX ====
        public async Task<ActionResult> Index()
        {
            // ==== EVENTS ====
            var allEvents = await _firebase.GetAllEvents() ?? new List<EventModel>();
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

            var completedProjects = allProjects
                .Where(p => p.Project.Type?.ToLower() == "completed")
                .OrderByDescending(p => p.Project.StartDate)   // Sort by date, newest first
                .Select(p => p.Project)
                .Take(1)
                .ToList();

            var currentProjects = allProjects
                .Where(p => p.Project.Type?.ToLower() == "currently")
                .OrderByDescending(p => p.Project.StartDate)
                .Select(p => p.Project)
                .Take(1)
                .ToList();

            var upcomingProjects = allProjects
                .Where(p => p.Project.Type?.ToLower() == "upcoming")
                .OrderByDescending(p => p.Project.StartDate)
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

        // ==== SUBSCRIBE ====
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

            var contact = new ContactModel
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Subject = subject,
                Message = message,
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            await _firebase.SaveContact(contact);

            try
            {
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

            var volunteer = new VolunteerModel
            {
                FullName = fullName,
                Email = email,
                Phone = phone,
                Availability = availability,
                RolePreference = rolePreference,
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            await _firebase.SaveVolunteer(volunteer);

            try
            {
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
            var allNews = await _firebase.GetAllNewsWithKeys();
            var article = allNews.FirstOrDefault(a => a.Key == id)?.Article;

            if (article == null)
                return NotFound();

            return View("~/Views/Home/NewsDetails.cshtml", article);
        }





        /*// ==== SUBSCRIPTION ====
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
                var fromAddress = new MailAddress("noreplylungisa@gmail.com", "Lungisa NPO");
                var toAddress = new MailAddress(email);
                string fromPassword = "rxue wsuh idcr oupw"; // Gmail App Password
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

        }*/



        // ==== CONTACT DETAILS ====
        /*[HttpPost]
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
       *//* }*//*
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

            // --- SEND AUTOMATED THANK YOU EMAIL ---
            try
            {
                var fromAddress = new MailAddress("noreplylungisa@gmail.com", "Lungisa NPO");
                var toAddress = new MailAddress(email, firstName);
                string fromPassword = "rxue wsuh idcr oupw"; // Gmail App Password
                string emailSubject = "Thank You for Contacting Us!";
                string emailBody = $"Hi {firstName},\n\n" +
                                   "Thank you for reaching out to Lungisa NPO. " +
                                   "We have received your message and will get back to you shortly.\n\n" +
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
                using (var messageMail = new MailMessage(fromAddress, toAddress)
                {
                    Subject = emailSubject,
                    Body = emailBody
                })
                {
                    smtp.Send(messageMail);
                }
            }
            catch (Exception ex)
            {
                // Optional: log error or show TempData message
                TempData["Error"] = "Message sent, but confirmation email could not be delivered: " + ex.Message;
                return RedirectToAction("Contact");
            }

            TempData["Success"] = "✅ Your message has been sent! A confirmation email has been sent to you.";
            return RedirectToAction("Contact");
        }
*/

        /* [HttpPost]
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
         }*/
        /*  [HttpPost]
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

              // --- SEND AUTOMATED THANK YOU EMAIL ---
              try
              {
                  var fromAddress = new MailAddress("noreplylungisa@gmail.com", "Lungisa NPO");
                  var toAddress = new MailAddress(email, fullName);
                  string fromPassword = "rxue wsuh idcr oupw"; // Gmail App Password
                  string emailSubject = "Thank You for Signing up!";
                  string emailBody = $"Hi {fullName},\n\n" +
                                     "Thank you for signing up as a volunteer with Lungisa NPO. " +
                                     "We appreciate your willingness to help and will get back to you with more details soon.\n\n" +
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
                  using (var messageMail = new MailMessage(fromAddress, toAddress)
                  {
                      Subject = emailSubject,
                      Body = emailBody
                  })
                  {
                      smtp.Send(messageMail);
                  }
              }
              catch (Exception ex)
              {
                  TempData["Error"] = "Volunteer saved, but confirmation email could not be sent: " + ex.Message;
                  return RedirectToAction("Volunteer");
              }

              TempData["Success"] = "✅ Thank you for signing up! A confirmation email has been sent.";
              return RedirectToAction("Volunteer");
          }
  */


    }
}
