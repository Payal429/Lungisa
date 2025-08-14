using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Firebase.Database;
using Firebase.Database.Query;
using FirebaseAdmin.Auth;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Lungisa.Models;


namespace Lungisa.Services
{
    public class FirebaseService
    {
        private readonly FirebaseApp _firebaseApp;
        private readonly FirebaseClient firebase;

        public FirebaseService()
        {
            // Initialise Admin SDK
            if (FirebaseApp.DefaultInstance == null)
            {
                _firebaseApp = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/firebaseServiceAccount.json"))
                });
            }
            else
            {
                _firebaseApp = FirebaseApp.DefaultInstance;
            }

            // Initialize Realtime Database client
            firebase = new FirebaseClient(
                "https://lungisa-e03bd-default-rtdb.firebaseio.com/",
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult("NjOwtbddPfLXqPHWEtgYQA2JSz81WCbVjylCXmfk")
                }
            );
        }



        // ===================== AUTH =====================    
        // Verify Firebase ID token

        public async Task<FirebaseToken> VerifyIdTokenAsync(string idToken)
        {
            try
            {
                return await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            }
            catch
            {
                return null;
            }
        }
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
            var firebaseProjects = await firebase
                .Child("Projects")
                .OnceAsync<ProjectModel>();

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

        public async Task SaveProject(ProjectModel Project)
        {
            await firebase.Child("Projects").PostAsync(Project);
        }

        public async Task DeleteProject(string key)
        {
            await firebase.Child("Projects").Child(key).DeleteAsync();
        }

        public class FirebaseProject
        {
            public string Key { get; set; }
            public ProjectModel Project { get; set; }
            public object Type { get; internal set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public string StartDate { get; set; }   // maybe this is what you have
            public string EndDate { get; set; }
        }

        public async Task<List<ProjectModel>> GetAllProjects()
        {
            var projects = await firebase
                .Child("Projects") // Firebase node
                .OnceAsync<ProjectModel>();

            return projects.Select(p => p.Object).ToList();
        }



        // ===================== EVENTS =====================

        public async Task<List<FirebaseEvent>> GetAllEventsWithKeys()
        {
            var firebaseEvents = await firebase
                .Child("Events")
                .OnceAsync<EventModel>();

            return firebaseEvents.Select(e => new FirebaseEvent
            {
                Key = e.Key,
                Event = e.Object
            }).ToList();
        }

        public async Task SaveEvent(EventModel eventModel)
        {
            await firebase.Child("Events").PostAsync(eventModel);
        }

        public async Task DeleteEvent(string key)
        {
            await firebase.Child("Events").Child(key).DeleteAsync();
        }

        public class FirebaseEvent
        {
            public string Key { get; set; }
            public EventModel Event { get; set; }
        }
        public async Task<List<EventModel>> GetAllEvents()
        {
            var firebaseEvents = await firebase
                .Child("Events") // Make sure the path matches your Firebase
                .OnceAsync<EventModel>();

            // Each f.Object is an EventModel
            var events = firebaseEvents.Select(f => f.Object).ToList();

            return events;
        }

    }
}