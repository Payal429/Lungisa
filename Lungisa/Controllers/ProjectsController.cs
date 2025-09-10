// Payal and Nyanda
// 15 August 2025
// Manages Lungisa projects in the admin dashboard. Interacts with Firebase to create, retrieve, and delete project records. Supports image uploads for 
// project entries and handles form data for adding new projects.

using Lungisa.Models;
using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lungisa.Controllers
{
    public class ProjectsController : Controller
    {
        // Instance of the Firebase service for accessing project data in Firebase
        private readonly FirebaseService firebase;
        private readonly IWebHostEnvironment _env;

        public ProjectsController(FirebaseService firebase, IWebHostEnvironment env)
        {
            this.firebase = firebase;
            _env = env;
        }


        // Loads the list of all projects and displays them on the Admin Project Info page.
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            // Retrieve all projects with their Firebase keys
            // If no projects exist, return an empty list to prevent null reference issues
            var projects = await firebase.GetAllProjectsWithKeys() ?? new List<Lungisa.Services.FirebaseService.FirebaseProject>();

            // Retrieve any success message stored in TempData from a previous request
            ViewBag.Success = TempData["Success"];

            // Render the ProjectInfo view with the retrieved list of projects
            return View("~/Views/Admin/Projects.cshtml", projects);
        }

        // This action explicitly loads the project management page.
        [HttpGet]
        public async Task<ActionResult> Projects()
        {
            // Retrieve all projects from Firebase
            var Projects = await firebase.GetAllProjectsWithKeys();

            // Pass any temporary success message to the view
            ViewBag.Success = TempData["Success"];

            // Render the ProjectInfo view with the retrieved projects
            return View("~/Views/Admin/Projects.cshtml", Projects);
        }

        // Handles adding a new project to Firebase when submitted from a form.
        [HttpPost]
        public async Task<ActionResult> AddProject(string title, string type, string description, string startDate, string endDate, IFormFile image)
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

            // Create a new project model with form values and default image URL
            ProjectModel Project = new ProjectModel
            {
                Title = title,
                Type = type,
                Description = description,
                StartDate = startDate,
                EndDate = endDate,
                // Default placeholder for the images for the projects
                ImageUrl = imageUrl
            };

            // Save the new project in Firebase
            await firebase.SaveProject(Project);

            // Store a success message to display after redirect
            TempData["Success"] = "✅ Project was added successfully!";

            // Redirect to the ProjectInfo page so the updated list is displayed
            return RedirectToAction("Projects");
        }

        // Deletes an existing project from Firebase by its unique ID.
        public async Task<ActionResult> Delete(string id)
        {
            // Remove the project from Firebase
            await firebase.DeleteProject(id);

            // Store a success message to display on the next page load
            TempData["Success"] = "🗑️ Project deleted successfully!";

            // Redirect to ProjectInfo page to show updated project list
            return RedirectToAction("Projects");
        }

    }
}