using Firebase.Database;
using Firebase.Database.Query;
using Lungisa.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Lungisa.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly FirebaseClient _firebaseClient;

        private readonly string[] _homePages = new[]
        {
            "About", "AskCommunity", "Contact", "Donations", "FAQ",
            "Index", "News", "NewsDetails", "Projects", "Requests",
            "Services", "Volunteer"
        };

        public AnalyticsController(IConfiguration config)
        {
            string dbUrl = config["Firebase:DatabaseUrl"];
            _firebaseClient = new FirebaseClient(dbUrl);
        }

        public async Task<IActionResult> Analytics()
        {
            // Fetch data from Firebase
            var donations = await _firebaseClient.Child("Donations").OnceAsync<DonationData>();
            var volunteers = await _firebaseClient.Child("Volunteers").OnceAsync<VolunteerData>();
            var subscribers = await _firebaseClient.Child("Subscribers").OnceAsync<SubscriberData>();
            var visits = await _firebaseClient.Child("analytics").Child("visits").OnceAsync<VisitData>();

            // Home page visits
            var homeVisits = visits
                .Where(v => !string.IsNullOrEmpty(v.Object.Page) &&
                            _homePages.Contains(v.Object.Page.Replace("/Home/", "").Trim('/'),
                                StringComparer.OrdinalIgnoreCase))
                .ToList();

            // Quick stats
            ViewBag.DonationsCount = donations.Count;
            ViewBag.VolunteersCount = volunteers.Count;
            ViewBag.SubscribersCount = subscribers.Count;
            ViewBag.TotalVisits = homeVisits.Count;
            ViewBag.AvgDuration = homeVisits.Any()
                ? homeVisits.Average(v => (double?)v.Object.Duration ?? 0)
                : 0;

            // Page-wise stats
            ViewBag.PageStats = _homePages.Select(page =>
            {
                var pageVisits = homeVisits
                    .Where(v => string.Equals(v.Object.Page.Replace("/Home/", "").Trim('/'),
                        page, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return new PageAnalytics
                {
                    Page = page,
                    VisitCount = pageVisits.Count,
                    AvgDuration = pageVisits.Any() ? pageVisits.Average(v => (double?)v.Object.Duration ?? 0) : 0
                };
            }).ToList();

            // Trends: last 7 days for donations & traffic, last 7 days for volunteers
            var today = DateTime.Today;

            // Donations: unique donors per day
            ViewBag.DonationTrendLabels = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-i).ToString("yyyy-MM-dd"))
                .Reverse()
                .ToArray();

            ViewBag.DonationTrendValues = Enumerable.Range(0, 7)
                .Select(i => donations
                    .Where(d => d.Object.TimestampDate.HasValue &&
                                d.Object.TimestampDate.Value.Date == today.AddDays(-i) &&
                                !string.IsNullOrEmpty(d.Object.Email))
                    .Select(d => d.Object.Email)
                    .Distinct()
                    .Count())
                .Reverse()
                .ToArray();

            // Volunteers: unique volunteers per day
            ViewBag.VolunteerTrendLabels = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-i).ToString("yyyy-MM-dd"))
                .Reverse()
                .ToArray();

            ViewBag.VolunteerTrendValues = Enumerable.Range(0, 7)
                .Select(i => volunteers
                    .Where(v => v.Object.TimestampDate.HasValue &&
                                v.Object.TimestampDate.Value.Date == today.AddDays(-i) &&
                                !string.IsNullOrEmpty(v.Object.FullName))
                    .Select(v => v.Object.FullName)
                    .Distinct()
                    .Count())
                .Reverse()
                .ToArray();

            // Traffic: visits per day
            ViewBag.TrafficTrendLabels = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-i).ToString("yyyy-MM-dd"))
                .Reverse()
                .ToArray();

            ViewBag.TrafficTrendValues = Enumerable.Range(0, 7)
                .Select(i => homeVisits.Count(v => v.Object.TimestampDate.HasValue &&
                                                  v.Object.TimestampDate.Value.Date == today.AddDays(-i)))
                .Reverse()
                .ToArray();

            return View("~/Views/Admin/Analytics.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> TrackVisit([FromBody] VisitData data)
        {
            if (data == null || string.IsNullOrEmpty(data.Page))
                return BadRequest();

            await _firebaseClient.Child("analytics").Child("visits").PostAsync(new
            {
                Page = data.Page,
                Duration = data.Duration,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });

            return Ok(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetTrends(DateTime startDate, DateTime endDate)
        {
            var donations = await _firebaseClient.Child("Donations").OnceAsync<DonationData>();
            var volunteers = await _firebaseClient.Child("Volunteers").OnceAsync<VolunteerData>();
            var visits = await _firebaseClient.Child("analytics").Child("visits").OnceAsync<VisitData>();

            var donationTrend = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(i =>
                {
                    var day = startDate.AddDays(i);
                    return new
                    {
                        Date = day.ToString("yyyy-MM-dd"),
                        Count = donations
                            .Where(d => d.Object.TimestampDate.HasValue &&
                                        d.Object.TimestampDate.Value.Date == day &&
                                        !string.IsNullOrEmpty(d.Object.Email))
                            .Select(d => d.Object.Email)
                            .Distinct()
                            .Count()
                    };
                }).ToList();

            var volunteerTrend = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(i =>
                {
                    var day = startDate.AddDays(i);
                    return new
                    {
                        Date = day.ToString("yyyy-MM-dd"),
                        Count = volunteers
                            .Where(v => v.Object.TimestampDate.HasValue &&
                                        v.Object.TimestampDate.Value.Date == day &&
                                        !string.IsNullOrEmpty(v.Object.FullName))
                            .Select(v => v.Object.FullName)
                            .Distinct()
                            .Count()
                    };
                }).ToList();

            var trafficTrend = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(i =>
                {
                    var day = startDate.AddDays(i);
                    return new
                    {
                        Date = day.ToString("yyyy-MM-dd"),
                        Count = visits
                            .Where(v => v.Object.TimestampDate.HasValue &&
                                        v.Object.TimestampDate.Value.Date == day)
                            .Count()
                    };
                }).ToList();

            return Json(new
            {
                Donations = donationTrend,
                Volunteers = volunteerTrend,
                Traffic = trafficTrend
            });
        }

        public IActionResult Index() => RedirectToAction("Analytics");

        #region Firebase Models

        public class VisitData
        {
            public string Page { get; set; } = "";
            public int? Duration { get; set; }
            [JsonProperty("Timestamp")] public object RawTimestamp { get; set; }
            [JsonIgnore] public DateTime? TimestampDate => NormalizeTimestamp(RawTimestamp);
        }

        public class DonationData
        {
            public double? Amount { get; set; }
            public string DonorName { get; set; } = "";
            public string Email { get; set; } = "";
            public string Timestamp { get; set; } // ISO string
            [JsonIgnore] public DateTime? TimestampDate => ParseTimestamp(Timestamp);
        }

        public class VolunteerData
        {
            public string FullName { get; set; } = "";
            public string Email { get; set; } = "";
            public string Date { get; set; } // string "yyyy-MM-dd HH:mm:ss"
            [JsonIgnore] public DateTime? TimestampDate => ParseTimestamp(Date);
        }

        public class SubscriberData
        {
            public string Email { get; set; } = "";
            public long? Timestamp { get; set; }
            [JsonIgnore]
            public DateTime? TimestampDate =>
                Timestamp.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(Timestamp.Value).UtcDateTime : null;
        }

        #endregion

        #region Helpers
        private static DateTime? NormalizeTimestamp(object raw)
        {
            return raw switch
            {
                null => null,
                long l => DateTimeOffset.FromUnixTimeMilliseconds(l).UtcDateTime,
                double d => DateTimeOffset.FromUnixTimeMilliseconds((long)d).UtcDateTime,
                string s when DateTime.TryParse(s, out var dt) => dt,
                _ => null
            };
        }

        private static DateTime? ParseTimestamp(string ts)
        {
            if (string.IsNullOrEmpty(ts)) return null;
            if (DateTime.TryParse(ts, out var dt)) return dt;
            return null;
        }
        #endregion
    }
}
