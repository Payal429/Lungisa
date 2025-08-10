using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Firebase.Database;


namespace Lungisa.Services
{
    public class FirebaseService
    {
        private FirebaseClient client;

        public FirebaseService()
        {
            client = new FirebaseClient(
                "https://lungisa-e03bd-default-rtdb.firebaseio.com/" // Replace with your actual DB URL
            );
        }
    }
}