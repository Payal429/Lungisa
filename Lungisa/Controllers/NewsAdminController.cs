using Lungisa.Models;
using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lungisa.Controllers
{
    public class NewsAdminController : Controller
    {
        // Firebase service to fetch, save, and delete news articles
        private readonly FirebaseService firebase;
        // Web hosting environment to manage file paths for uploaded images
        private readonly IWebHostEnvironment _env;

        // Constructor: inject FirebaseService and IWebHostEnvironment
        public NewsAdminController(FirebaseService firebase, IWebHostEnvironment env)
        {
            _env = env;
            this.firebase = firebase;
        }

        // GET: /NewsAdmin/Index
        // Loads the News management page and retrieves all news articles
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var news = await firebase.GetAllNewsWithKeys()
                       ?? new List<FirebaseService.FirebaseNewsArticle>();

            // Pass the news articles to the NewsAdmin view
            return View("~/Views/Admin/NewsAdmin.cshtml", news);
        }

        // POST: /NewsAdmin/AddNews
        // Adds a new news article with optional image upload
        [HttpPost]
        public async Task<ActionResult> AddNews(string title, string summary, string body, IFormFile image)
        {
            // Default fallback image if no image is uploaded
            string imageUrl = "/Content/Images/default-news.png";

            // If an image was uploaded, save it to the server
            if (image != null && image.Length > 0)
            {
                string fileName = Path.GetFileName(image.FileName);
                string path = Path.Combine(_env.WebRootPath, "Content", "Images", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Set the image URL to the uploaded file path
                imageUrl = "/Content/Images/" + fileName;
            }

            // Create a new news article model
            var article = new NewsArticleModel
            {
                Title = title,
                Summary = summary,
                Body = body,
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                ImageUrl = imageUrl
            };

            // Save the news article to Firebase
            await firebase.SaveNews(article);

            // Redirect to Index to refresh the list of news articles
            return RedirectToAction("Index");
        }

        // GET: /NewsAdmin/Details/{id}
        // Displays the details of a specific news article
        [HttpGet]
        public async Task<ActionResult> Details(string id)
        {
            var allNews = await firebase.GetAllNewsWithKeys();
            var article = allNews.FirstOrDefault(a => a.Key == id)?.Article;

            // Return 404 if the article does not exist
            if (article == null)
                return NotFound();

            return View("~/Views/Home/NewsDetails.cshtml", article);
        }

        // GET or POST: /NewsAdmin/Delete/{id}
        // Deletes a specific news article by its ID
        public async Task<ActionResult> Delete(string id)
        {
            // Delete the article from Firebase
            await firebase.DeleteNews(id);

            // Optional: set a success message to show after redirect
            TempData["Success"] = "🗑️ News article deleted successfully!";

            // Redirect back to Index to refresh the news list
            return RedirectToAction("Index");
        }
    }
}