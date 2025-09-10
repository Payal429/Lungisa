// Payal and Nyanda
// 15 August 2025
// Handles the public-facing News section. Fetches news articles from Firebase and displays article details. This controller ensures
// the latest news is shown first and routes to Home views.

using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lungisa.Controllers
{
    public class NewsController : Controller
    {
        private readonly FirebaseService _firebase;

        // Inject FirebaseService via constructor
        public NewsController(FirebaseService firebase)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        }
        // GET: News/Index?page=1
        [HttpGet]
        public async Task<ActionResult> Index(int page = 1)
        {
            const int pageSize = 6;

            // Fetch all news from Firebase
            var allNews = await _firebase.GetAllNewsWithKeys()
                          ?? new List<FirebaseService.FirebaseNewsArticle>();

            // Sort by date descending to show latest news first
            var sortedNews = allNews
                             .Where(n => n.Article != null)
                             .OrderByDescending(n => DateTime.Parse(n.Article.Date))
                             .ToList();

            // Paginate the results
            var pagedNews = sortedNews
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            // Pass pagination info to the view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(sortedNews.Count / (double)pageSize);

            // Return the News view with paged news
            return View("~/Views/Home/News.cshtml", pagedNews);
        }

        // GET: News/Details/{id}
        [HttpGet]
        public async Task<ActionResult> Details(string id)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Article ID cannot be empty.");

            List<FirebaseService.FirebaseNewsArticle> allNews;

            // Safely fetch all news from Firebase
            try
            {
                allNews = await _firebase.GetAllNewsWithKeys() ?? new List<FirebaseService.FirebaseNewsArticle>();
            }
            catch (Exception ex)
            {
                // Log the exception for debugging (use a proper logger in production)
                Console.WriteLine($"Error fetching news: {ex}");
                return StatusCode(500, "Error fetching news from Firebase.");
            }

            // Filter out null items just in case
            allNews = allNews.Where(a => a != null).ToList();

            // Find the specific article by Firebase key
            var article = allNews.FirstOrDefault(a => a.Key == id)?.Article;

            if (article == null)
                return NotFound(); // 404 if article doesn't exist

            // Return the detailed view
            return View("~/Views/Home/NewsDetails.cshtml", article);
        }
    }
}