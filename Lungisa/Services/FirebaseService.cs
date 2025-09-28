﻿using Firebase.Database;
using Firebase.Database.Query;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Lungisa.Models;

namespace Lungisa.Services
{
    public class FirebaseService
    {
        private readonly FirebaseApp _firebaseApp;
        private readonly FirebaseClient _firebaseClient;

        public FirebaseService(IConfiguration config, IWebHostEnvironment env)
        {
            GoogleCredential credential = null;

            // 1️⃣ Check if FIREBASE_CONFIG environment variable exists (Production / Render)
            string firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");

            if (!string.IsNullOrEmpty(firebaseJson))
            {
                credential = GoogleCredential.FromJson(firebaseJson);
            }
            else
            {
                // 2️⃣ Fallback to local JSON file (Development)
                string path = Path.Combine(env.ContentRootPath, "Config", "firebaseServiceAccount.json");
                if (!File.Exists(path))
                    throw new Exception("Firebase service account file not found.");

                credential = GoogleCredential.FromFile(path);
            }

            // 3️⃣ Initialize Firebase Admin SDK
            if (FirebaseApp.DefaultInstance == null)
            {
                _firebaseApp = FirebaseApp.Create(new AppOptions
                {
                    Credential = credential
                });
            }
            else
            {
                _firebaseApp = FirebaseApp.DefaultInstance;
            }

            // 4️⃣ Initialize Firebase Realtime Database client
            _firebaseClient = new FirebaseClient(
                config["Firebase:DatabaseUrl"],
                new FirebaseOptions
                {
                    // Optional: you can remove AuthTokenAsyncFactory if using Admin SDK
                    AuthTokenAsyncFactory = async () =>
                    {
                        var token = await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync("test-uid");
                        return token;
                    }
                });
        }

        // Example method: create a new user
        public async Task<UserRecord> CreateUserAsync(string email, string password)
        {
            var args = new UserRecordArgs()
            {
                Email = email,
                Password = password
            };
            return await FirebaseAuth.DefaultInstance.CreateUserAsync(args);
        }
    

    public async Task<FirebaseToken> VerifyIdTokenAsync(string idToken)
            => await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);


        // ===================== PROJECTS =====================
        // Get all projects along with their Firebase keys
        public async Task<List<FirebaseProject>> GetAllProjectsWithKeys()
        {
            var firebaseProjects = await _firebaseClient.Child("Projects").OnceAsync<ProjectModel>();

            if (!firebaseProjects.Any())
                Console.WriteLine("No projects found in Firebase.");

            return firebaseProjects.Select(p => new FirebaseProject
            {
                Key = p.Key,
                Project = p.Object
            }).ToList();
        }

        // Save a new project to Firebase
        public async Task SaveProject(ProjectModel project)
        {
            await _firebaseClient.Child("Projects").PostAsync(project);
        }

        // Delete a project by Firebase key
        public async Task DeleteProject(string key)
        {
            await _firebaseClient.Child("Projects").Child(key).DeleteAsync();
        }

        // Class representing a project with its Firebase key
        public class FirebaseProject
        {
            public string Key { get; set; }
            public ProjectModel Project { get; set; }
        }

        // Fetch all projects without keys
        public async Task<List<ProjectModel>> GetAllProjects()
        {
            var projects = await _firebaseClient.Child("Projects").OnceAsync<ProjectModel>();
            return projects.Select(p => p.Object).ToList();
        }

        // ===================== EVENTS =====================
        // Get all events along with their Firebase keys
        public async Task<List<FirebaseEvent>> GetAllEventsWithKeys()
        {
            var firebaseEvents = await _firebaseClient.Child("Events").OnceAsync<EventModel>();
            return firebaseEvents.Select(e => new FirebaseEvent
            {
                Key = e.Key,
                Event = e.Object
            }).ToList();
        }

        // Save a new event to Firebase
        public async Task SaveEvent(EventModel eventModel)
        {
            await _firebaseClient.Child("Events").PostAsync(eventModel);
        }

        // Delete an event by Firebase key
        public async Task DeleteEvent(string key)
        {
            await _firebaseClient.Child("Events").Child(key).DeleteAsync();
        }

        // Class representing an event with its Firebase key
        public class FirebaseEvent
        {
            public string Key { get; set; }
            public EventModel Event { get; set; }
        }

        // Fetch all events without keys
        public async Task<List<EventModel>> GetAllEvents()
        {
            var events = await _firebaseClient.Child("Events").OnceAsync<EventModel>();
            return events.Select(e => e.Object).ToList();
        }

        // ===================== NEWS =====================
        // Get all news articles along with their Firebase keys
        public async Task<List<FirebaseNewsArticle>> GetAllNewsWithKeys()
        {
            var newsList = await _firebaseClient.Child("News").OnceAsync<NewsArticleModel>();
            return newsList.Select(n => new FirebaseNewsArticle
            {
                Key = n.Key,
                Article = n.Object
            }).ToList();
        }

        // Save a new news article to Firebase
        public async Task SaveNews(NewsArticleModel article)
        {
            await _firebaseClient.Child("News").PostAsync(article);
        }

        // Delete a news article by Firebase key
        public async Task DeleteNews(string key)
        {
            await _firebaseClient.Child("News").Child(key).DeleteAsync();
        }

        // Class representing a news article with its Firebase key
        public class FirebaseNewsArticle
        {
            public string Key { get; set; }
            public NewsArticleModel Article { get; set; }
        }

        // ===================== SUBSCRIBERS =====================
        // Save a new subscriber
        public async Task SaveSubscriber(SubscriberModel subscriber)
        {
            await _firebaseClient.Child("Subscribers").PostAsync(subscriber);
        }

        // Fetch all subscribers
        public async Task<List<SubscriberModel>> GetAllSubscribers()
        {
            var subscribers = await _firebaseClient.Child("Subscribers").OnceAsync<SubscriberModel>();
            return subscribers.Select(x => x.Object).ToList();
        }

        // ===================== CONTACTS =====================
        // Save a new contact message
        public async Task SaveContact(ContactModel contact)
        {
            await _firebaseClient.Child("Contacts").PostAsync(contact);
        }

        // Fetch all contact messages
        public async Task<List<ContactModel>> GetAllContacts()
        {
            var contacts = await _firebaseClient.Child("Contacts").OnceAsync<ContactModel>();
            return contacts.Select(x => x.Object).ToList();
        }

        // ===================== VOLUNTEERS =====================
        // Save a new volunteer
        public async Task SaveVolunteer(VolunteerModel volunteer)
        {
            await _firebaseClient.Child("Volunteers").PostAsync(volunteer);
        }

        // Fetch all volunteers
        public async Task<List<VolunteerModel>> GetAllVolunteers()
        {
            var volunteers = await _firebaseClient.Child("Volunteers").OnceAsync<VolunteerModel>();
            return volunteers.Select(x => x.Object).ToList();
        }

        // ===================== DONATIONS =====================
        // Save a new donation
        public async Task SaveDonation(DonationModel donation)
        {
            await _firebaseClient.Child("Donations").PostAsync(donation);
        }

        // Fetch all donations
        public async Task<List<DonationModel>> GetDonations()
        {
            var donationsData = await _firebaseClient
                .Child("Donations")
                .OnceAsync<dynamic>();

            var donations = new List<DonationModel>();

            foreach (var d in donationsData)
            {
                var obj = d.Object;

                // Safely parse timestamp
                DateTime timestamp;
                try
                {
                    if (obj.timestamp is long l) // stored as Unix milliseconds
                        timestamp = DateTimeOffset.FromUnixTimeMilliseconds(l).UtcDateTime;
                    else if (obj.timestamp is string s && DateTime.TryParse(s, out DateTime dt))
                        timestamp = dt;
                    else
                        timestamp = DateTime.UtcNow;
                }
                catch
                {
                    timestamp = DateTime.UtcNow;
                }

                // Safely parse amount
                decimal amount = 0;
                try
                {
                    if (obj.Amount != null)
                        amount = Convert.ToDecimal(obj.Amount);
                }
                catch
                {
                    amount = 0;
                }

                donations.Add(new DonationModel
                {
                    DonorName = obj.DonorName ?? "",
                    Email = obj.Email ?? "",
                    Amount = amount,
                    Timestamp = timestamp,
                    Status = obj.Status ?? "Pending",
                    FirstName = obj.FirstName ?? "",
                    LastName = obj.LastName ?? "",
                    PayFastPaymentId = obj.PayFastPaymentId ?? "",
                    M_PaymentId = obj.M_PaymentId ?? "",
                    PaymentReference = obj.PaymentReference ?? ""
                });
            }

            return donations;
        }
        // ===================== ADMINS =====================
        public async Task SaveAdmin(string uid, string email, string firstName, string lastName, string phoneNumber, string role)
        {
            var adminData = new
            {
                Uid = uid,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                Role = role,
                CreatedAt = DateTime.UtcNow.ToString("o")
            };

            await _firebaseClient.Child("Admins").Child(uid).PutAsync(adminData);
        }





    }
}