using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;

public class LoginController : Controller
{
    private readonly FirebaseService _firebase;
    public LoginController(FirebaseService firebase)
    {
        _firebase = firebase;
    }

    [HttpGet]
    public ActionResult Index()
    {
        return View(); // Returns the Login page
    }

    [HttpPost]
    public async Task<ActionResult> Index(string idToken)
    {
        if (string.IsNullOrEmpty(idToken))
        {
            ViewBag.Error = "Invalid login.";
            return View();
        }

        var decodedToken = await _firebase.VerifyIdTokenAsync(idToken);
        if (decodedToken != null)
        {
            // Optionally check if UID exists in Admin list in Firebase Realtime Database
            // If yes:
            // Store UID in session using ASP.NET Core session
            HttpContext.Session.SetString("AdminUid", decodedToken.Uid);

            return RedirectToAction("AdminDashboard", "Admin");
        }

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
