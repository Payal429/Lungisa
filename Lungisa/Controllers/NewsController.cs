using Lungisa.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Lungisa.Controllers
{
    public class NewsController : Controller
    {
        // GET: News
            FirebaseService firebase = new FirebaseService();

        // GET: News/Index?page=1
        [HttpGet]
        public async Task<ActionResult> Index(int page = 1)
        {
            const int pageSize = 6;

            // Fetch all news from Firebase
            var allNews = await firebase.GetAllNewsWithKeys()
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
            // Fetch all news articles from Firebase
            var allNews = await firebase.GetAllNewsWithKeys();

            // Find the specific article by its Firebase key
            var article = allNews.FirstOrDefault(a => a.Key == id)?.Article;

            if (article == null)
                return HttpNotFound(); // Show 404 if the article doesn't exist

            // Pass the article to a detailed view
            return View("~/Views/Home/NewsDetails.cshtml", article);
        }
    }
}