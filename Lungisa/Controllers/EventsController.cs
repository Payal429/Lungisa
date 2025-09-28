// Payal and Nyanda
// 15 August 2025
// Manages organisational events for both admin and public views. Provides functionality to add new events, list existing ones, and delete events.
// Integrates with Firebase for persistent storage.

using Lungisa.Models;
using Lungisa.Services;
using Microsoft.AspNetCore.Mvc; // for DateTime parsing

namespace Lungisa.Controllers
{
    public class EventsController : Controller
    {
        // Instance of FirebaseService that provides methods to interact with Firebase
        private readonly FirebaseService firebase;

        public EventsController(FirebaseService firebase)
        {
            this.firebase = firebase;
        }



        // GET: /Events
        // Loads all events from Firebase and passes them to the Admin Events view.
        public async Task<ActionResult> Index()
        {
            // Retrieve all events along with their unique Firebase keys
            // If no events are found, default to an empty list to avoid null reference errors
            var events = await firebase.GetAllEventsWithKeys() ?? new List<Lungisa.Services.FirebaseService.FirebaseEvent>();

            // Pass any success messages from TempData to the view (TempData persists across redirects)
            ViewBag.Success = TempData["Success"];

            // Return the Admin Events view with the list of events
            return View("~/Views/Admin/Events.cshtml", events);
        }

        [HttpGet]
        public async Task<ActionResult> EventsPage()
        {
            // Fetch all events from Firebase
            var events = await firebase.GetAllEventsWithKeys();

            // Pass any temporary success message to the view
            ViewBag.Success = TempData["Success"];

            // Render the same Admin Events view with the fetched events
            return View("~/Views/Admin/Events.cshtml", events);
        }

        [HttpPost]
        // Handles form submission for adding a new event.
        public async Task<ActionResult> AddEvent(string name, string dateTime, string venue, string description)
        {
            // Create a new EventModel instance with the provided form data
            EventModel newEvent = new EventModel
            {
                Name = name,
                DateTime = dateTime,
                Venue = venue,
                Description = description
            };

            // Save the new event to Firebase
            await firebase.SaveEvent(newEvent);

            // Store a success message to be shown after redirect
            TempData["Success"] = "✅ Event added successfully!";

            // Redirect to the EventsPage to refresh the list
            return RedirectToAction("EventsPage");
        }

        // Deletes an event from Firebase using its unique key.
        public async Task<ActionResult> Delete(string id)
        {
            // Call FirebaseService to remove the event by its ID
            await firebase.DeleteEvent(id);

            // Store a success message for display on the next page load
            TempData["Success"] = "🗑️ Event deleted successfully!";

            // Redirect to the EventsPage so the user sees the updated list
            return RedirectToAction("EventsPage");
        }
        [HttpGet]
        public async Task<ActionResult> EditEvent(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("EventsPage");

            // Retrieve the event by key from Firebase
            var eventToEdit = await firebase.GetEventByKey(id);

            // Pass the event and key to the view
            ViewBag.EventToEdit = eventToEdit;
            ViewBag.EventKey = id;

            // Load all events to display in the table
            var events = await firebase.GetAllEventsWithKeys();
            return View("~/Views/Admin/Events.cshtml", events);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateEvent(string eventKey, string name, string dateTime, string venue, string description)
        {
            var existingEvent = await firebase.GetEventByKey(eventKey);

            if (existingEvent == null)
            {
                TempData["Success"] = "⚠️ Event not found!";
                return RedirectToAction("EventsPage");
            }

            // Update properties
            existingEvent.Event.Name = name;
            existingEvent.Event.DateTime = dateTime;
            existingEvent.Event.Venue = venue;
            existingEvent.Event.Description = description;

            await firebase.UpdateEvent(eventKey, existingEvent.Event);

            TempData["Success"] = "✅ Event updated successfully!";
            return RedirectToAction("EventsPage");
        }

    }
}