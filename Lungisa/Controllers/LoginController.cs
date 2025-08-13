using Lungisa.Services;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System;
using System.Security.Cryptography;
using System.Linq;

public class LoginController : Controller
{
    private readonly FirebaseService _firebase = new FirebaseService();

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
            Session["AdminUid"] = decodedToken.Uid;
            return RedirectToAction("AdminDashboard", "Admin");
        }

        ViewBag.Error = "Invalid login.";
        return View();
    }

    public ActionResult Logout()
    {
        Session.Clear();
        return RedirectToAction("Index","Login");
    }
}
