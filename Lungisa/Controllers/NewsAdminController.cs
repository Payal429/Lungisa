using Lungisa.Models;
using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lungisa.Controllers
{
    public class NewsAdminController : Controller
    {
        private readonly FirebaseService firebase;
        private readonly IWebHostEnvironment _env;
        public NewsAdminController(FirebaseService firebase, IWebHostEnvironment env)
        {
            _env = env;
            this.firebase = firebase;
        }

    


        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var news = await firebase.GetAllNewsWithKeys()
                       ?? new List<FirebaseService.FirebaseNewsArticle>();

            return View("~/Views/Admin/NewsAdmin.cshtml", news);
        }


        [HttpPost]
        public async Task<ActionResult> AddNews(string title, string summary, string body, IFormFile image)
        {
            string imageUrl = "/Content/Images/default-news.png"; // fallback

            // If an image was uploaded
            if (image != null && image.Length > 0)
            {
                string fileName = Path.GetFileName(image.FileName);
                string path = Path.Combine(_env.WebRootPath, "Content", "Images", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                imageUrl = "/Content/Images/" + fileName;
            }

            var article = new NewsArticleModel
            {
                Title = title,
                Summary = summary,
                Body = body,
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                ImageUrl = imageUrl
            };

            await firebase.SaveNews(article);

            // Redirect to Index to refresh the table with the latest news
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(string id)
        {
            await firebase.DeleteNews(id);

            // Redirect to Index to refresh the table after deletion
            return RedirectToAction("Index");
        }



        [HttpGet]
        public async Task<ActionResult> Details(string id)
        {
            var allNews = await firebase.GetAllNewsWithKeys();
            var article = allNews.FirstOrDefault(a => a.Key == id)?.Article;

            if (article == null)
                return NotFound();

            return View("~/Views/Home/NewsDetails.cshtml", article);
        }

    }
}