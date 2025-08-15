using Lungisa.Models;
using Lungisa.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Lungisa.Controllers
{
    public class NewsAdminController : Controller
    {
        FirebaseService firebase = new FirebaseService();

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var news = await firebase.GetAllNewsWithKeys()
                       ?? new List<FirebaseService.FirebaseNewsArticle>();

            return View("~/Views/Admin/NewsAdmin.cshtml", news);
        }

        [HttpPost]
        public async Task<ActionResult> AddNews(string title, string summary, string body, HttpPostedFileBase image)
        {
            string imageUrl = "/Content/Images/default-news.png"; // fallback

            // If an image was uploaded
            if (image != null && image.ContentLength > 0)
            {
                // Get filename
                string fileName = System.IO.Path.GetFileName(image.FileName);

                // Create path in Content/Images folder
                string path = Server.MapPath("~/Content/Images/" + fileName);

                // Save file to server
                image.SaveAs(path);

                // Set imageUrl to be stored in Firebase
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
                return HttpNotFound();

            return View("~/Views/Home/NewsDetails.cshtml", article);
        }

    }
}