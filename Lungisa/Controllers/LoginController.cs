// Payal and Nyanda 
// 15 August 2025
// Handles sign-in and sign-out flows for administrators.


using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;

public class LoginController : Controller
{
    // Relies on Firebase Authentication for credential verification.
    private readonly FirebaseService _firebase;

    /// Constructor – receives FirebaseService via dependency injection.
    public LoginController(FirebaseService firebase)
    {
        _firebase = firebase;
    }

    [HttpGet]
    // Displays the login page (GET /Login/Index).
    public ActionResult Index()
    {
        // Returns the Login page
        return View(); 
    }

    [HttpPost]
    public async Task<ActionResult> Index(string idToken)
    {
        // Guard clause – no token supplied.
        if (string.IsNullOrEmpty(idToken))
        {
            ViewBag.Error = "Invalid login.";
            return View();
        }

        // Verify the token with Firebase.
        var decodedToken = await _firebase.VerifyIdTokenAsync(idToken);


        if (decodedToken != null)
        {
            // Optionally check if UID exists in Admin list in Firebase Realtime Database
            // If yes:
            // Store UID in session using ASP.NET Core session
            HttpContext.Session.SetString("AdminUid", decodedToken.Uid);

            return RedirectToAction("AdminDashboard", "Admin");
        }

        // Token verification failed.
        ViewBag.Error = "Invalid login.";
        return View();
    }

    public ActionResult Logout()
    {
        // Clear all session values
        HttpContext.Session.Clear();
        return RedirectToAction("Index","Login");
    }
}
