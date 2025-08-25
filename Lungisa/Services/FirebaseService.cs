using Firebase.Database;
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
            string firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");

            if (!string.IsNullOrEmpty(firebaseJson))
            {
                // ✅ Production (Render) → read from ENV var
                var credential = GoogleCredential.FromJson(firebaseJson);

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
            }
            else
            {
                // ✅ Local development → fall back to file in Config/
                string path = Path.Combine(env.ContentRootPath, "Config", "firebaseServiceAccount.json");
                if (!File.Exists(path))
                    throw new Exception("Firebase service account file not found.");

                if (FirebaseApp.DefaultInstance == null)
                {
                    _firebaseApp = FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(path)
                    });
                }
                else
                {
                    _firebaseApp = FirebaseApp.DefaultInstance;
                }
            }

            // Firebase Realtime Database client
            _firebaseClient = new FirebaseClient(
                config["Firebase:DatabaseUrl"],
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(config["Firebase:DatabaseSecret"])
                });
        }

        public async Task<FirebaseToken> VerifyIdTokenAsync(string idToken)
            => await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

        /*public FirebaseService(IConfiguration config)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                _firebaseApp = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(
                        Path.Combine(Directory.GetCurrentDirectory(), "Config", "firebaseServiceAccount.json"))
                });
            }
            else
            {
                _firebaseApp = FirebaseApp.DefaultInstance;
            }
        }

        public async Task<FirebaseToken> VerifyIdTokenAsync(string idToken)
        {
            return await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
        }


    public FirebaseService(IWebHostEnvironment env)
        {
            // Initialize Firebase Admin SDK
            if (FirebaseApp.DefaultInstance == null)
            {
                string path = Path.Combine(env.ContentRootPath, "Config", "firebaseServiceAccount.json");
                _firebaseApp = FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(path)
                });
            }
            else
            {
                _firebaseApp = FirebaseApp.DefaultInstance;
            }

            // Initialize Realtime Database client
            _firebaseClient = new FirebaseClient(
                "https://lungisa-e03bd-default-rtdb.firebaseio.com/",
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult("NjOwtbddPfLXqPHWEtgYQA2JSz81WCbVjylCXmfk")
                });
        }*/

        // ===================== AUTHENTICATION  =====================    
        // Verify Firebase ID token
        /*        public async Task<FirebaseToken> VerifyIdTokenAsync(string idToken)
                {
                    try
                    {
                        return await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                    }
                    catch
                    {
                        return null;
                    }
                }*/
        // Create a new user in Firebase Authentication
        public async Task<UserRecord> CreateUserAsync(string email, string password)
        {
            var userArgs = new UserRecordArgs()
            {
                Email = email,
                Password = password
            };
            return await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);
        }

        // ===================== PROJECTS =====================
        public async Task<List<FirebaseProject>> GetAllProjectsWithKeys()
        {
            var firebaseProjects = await _firebaseClient.Child("Projects").OnceAsync<ProjectModel>();

            if (!firebaseProjects.Any())
            {
                Console.WriteLine("No projects found in Firebase.");
            }

            return firebaseProjects.Select(p => new FirebaseProject
            {
                Key = p.Key,
                Project = p.Object
            }).ToList();
        }

        public async Task SaveProject(ProjectModel project)
        {
            await _firebaseClient.Child("Projects").PostAsync(project);
        }

        public async Task DeleteProject(string key)
        {
            await _firebaseClient.Child("Projects").Child(key).DeleteAsync();
        }

        public class FirebaseProject
        {
            public string Key { get; set; }
            public ProjectModel Project { get; set; }
        }

        public async Task<List<ProjectModel>> GetAllProjects()
        {
            var projects = await _firebaseClient.Child("Projects").OnceAsync<ProjectModel>();
            return projects.Select(p => p.Object).ToList();
        }

        // ===================== EVENTS =====================
        public async Task<List<FirebaseEvent>> GetAllEventsWithKeys()
        {
            var firebaseEvents = await _firebaseClient.Child("Events").OnceAsync<EventModel>();
            return firebaseEvents.Select(e => new FirebaseEvent
            {
                Key = e.Key,
                Event = e.Object
            }).ToList();
        }

        public async Task SaveEvent(EventModel eventModel)
        {
            await _firebaseClient.Child("Events").PostAsync(eventModel);
        }

        public async Task DeleteEvent(string key)
        {
            await _firebaseClient.Child("Events").Child(key).DeleteAsync();
        }

        public class FirebaseEvent
        {
            public string Key { get; set; }
            public EventModel Event { get; set; }
        }

        public async Task<List<EventModel>> GetAllEvents()
        {
            var events = await _firebaseClient.Child("Events").OnceAsync<EventModel>();
            return events.Select(e => e.Object).ToList();
        }


        // ===================== NEWS =====================
        public async Task<List<FirebaseNewsArticle>> GetAllNewsWithKeys()
        {
            var newsList = await _firebaseClient.Child("News").OnceAsync<NewsArticleModel>();
            return newsList.Select(n => new FirebaseNewsArticle
            {
                Key = n.Key,
                Article = n.Object
            }).ToList();
        }

        public async Task SaveNews(NewsArticleModel article)
        {
            await _firebaseClient.Child("News").PostAsync(article);
        }

        public async Task DeleteNews(string key)
        {
            await _firebaseClient.Child("News").Child(key).DeleteAsync();
        }

        public class FirebaseNewsArticle
        {
            public string Key { get; set; }
            public NewsArticleModel Article { get; set; }
        }

        // ===================== SUBSCRIBERS =====================
        public async Task SaveSubscriber(SubscriberModel subscriber)
        {
            await _firebaseClient.Child("Subscribers").PostAsync(subscriber);
        }

        public async Task<List<SubscriberModel>> GetAllSubscribers()
        {
            var subscribers = await _firebaseClient.Child("Subscribers").OnceAsync<SubscriberModel>();
            return subscribers.Select(x => x.Object).ToList();
        }

        // ===================== CONTACTS =====================
        public async Task SaveContact(ContactModel contact)
        {
            await _firebaseClient.Child("Contacts").PostAsync(contact);
        }

        public async Task<List<ContactModel>> GetAllContacts()
        {
            var contacts = await _firebaseClient.Child("Contacts").OnceAsync<ContactModel>();
            return contacts.Select(x => x.Object).ToList();
        }

        // ===================== VOLUNTEERS =====================
        public async Task SaveVolunteer(VolunteerModel volunteer)
        {
            await _firebaseClient.Child("Volunteers").PostAsync(volunteer);
        }

        public async Task<List<VolunteerModel>> GetAllVolunteers()
        {
            var volunteers = await _firebaseClient.Child("Volunteers").OnceAsync<VolunteerModel>();
            return volunteers.Select(x => x.Object).ToList();
        }
    }
}