using Lungisa.Models;
using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Text;

using Lungisa.Models;
using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Lungisa.Controllers
{
    public class DonationsController : Controller
    {
        private readonly IConfiguration _config;
        private readonly FirebaseService _firebase;

        public DonationsController(IConfiguration config)
        {
            _config = config;
            _firebase = new FirebaseService(_config, null); // Works on Render with ENV vars
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/Home/Donations.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Pay(DonationModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all fields correctly.";
                return RedirectToAction("Index");
            }

            // ✅ Generate unique payment ID
            string paymentId = Guid.NewGuid().ToString();
            model.PayFastPaymentId = paymentId;
            model.Status = "Pending";

            // ✅ Save donation to Firebase
            await _firebase.SaveDonation(model);

            // ✅ Build PayFast URL
            string processUrl = _config["PayFastSettings:ProcessUrl"];
            string merchantId = _config["PayFastSettings:MerchantId"];
            string merchantKey = _config["PayFastSettings:MerchantKey"];
            string returnUrl = _config["PayFastSettings:ReturnUrl"];
            string cancelUrl = _config["PayFastSettings:CancelUrl"];
            string notifyUrl = _config["PayFastSettings:NotifyUrl"];

            var url = new StringBuilder($"{processUrl}?");
            url.Append($"merchant_id={merchantId}");
            url.Append($"&merchant_key={merchantKey}");
            url.Append($"&m_payment_id={Uri.EscapeDataString(paymentId)}");
            url.Append($"&amount={model.Amount:0.00}");
            url.Append($"&item_name={Uri.EscapeDataString("Donation to Lungisa NPO")}");
            url.Append($"&name_first={Uri.EscapeDataString(model.FirstName)}");
            url.Append($"&name_last={Uri.EscapeDataString(model.LastName)}");
            url.Append($"&email_address={Uri.EscapeDataString(model.Email)}");
            url.Append($"&return_url={Uri.EscapeDataString(returnUrl)}");
            url.Append($"&cancel_url={Uri.EscapeDataString(cancelUrl)}");
            url.Append($"&notify_url={Uri.EscapeDataString(notifyUrl)}");

            Console.WriteLine($"PayFast Redirect URL: {url}");
            return Redirect(url.ToString());
        }

        [HttpPost]
        public async Task<IActionResult> Notify()
        {
            var form = await Request.ReadFormAsync();
            string paymentId = form["m_payment_id"];
            string status = form["payment_status"];

            var donations = await _firebase.GetDonations();
            var donation = donations.FirstOrDefault(d => d.PayFastPaymentId == paymentId);

            if (donation != null)
            {
                donation.Status = status; // COMPLETE / FAILED
                await _firebase.SaveDonation(donation);
            }

            return Ok(); // PayFast expects 200 OK
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Cancel()
        {
            return View();
        }
    }
}





















/*
        // ===================== LOCAL TEST PAY =====================
        // POST: /Donations/LocalPay
        // Simulates a donation locally without contacting PayFast
        [HttpPost]
        public async Task<IActionResult> LocalPay(DonationModel model)
        {
            // Combine first and last name for display
            model.DonorName = $"{model.FirstName} {model.LastName}";
            // Mark donation as successful
            model.Status = "Success";
            // Record current timestamp
            model.Timestamp = DateTime.UtcNow;
            // Generate a local unique payment ID
            model.PayFastPaymentId = $"LOCAL-{Guid.NewGuid()}";

            try
            {
                // Save the donation to Firebase
                await _firebase.SaveDonation(model);
                // Redirect to Success page after saving
                return RedirectToAction("Success");
            }
            catch (Exception ex)
            {
                // Log any errors and return a BadRequest response
                Console.WriteLine("Error saving donation: " + ex.Message);
                return BadRequest("Failed to save donation locally: " + ex.Message);
            }
        }

        // GET: /Donations/Success
        // Optional page displayed after a successful donation
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

        // GET: /Donations/Cancel
        // Optional page displayed if a donation is cancelled
        [HttpGet]
        public IActionResult Cancel()
        {
            return View();
        }

        // ===================== TEST ENDPOINTS =====================
        // GET: /Donations/TestSave
        // Saves a sample donation to Firebase for testing purposes
        [HttpGet]
        public async Task<IActionResult> TestSave()
        {
            // Create a sample donation
            var donation = new DonationModel
            {
                DonorName = "Test User",
                Email = "test@example.com",
                Amount = 50,
                Status = "Success",
                Timestamp = DateTime.UtcNow,
                PayFastPaymentId = "TEST123"
            };

            try
            {
                // Save test donation to Firebase
                await _firebase.SaveDonation(donation);
                return Ok("Test donation saved to Firebase!");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to save test donation: " + ex.Message);
            }
        }
    }
}*/

/*using Lungisa.Models;
using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lungisa.Controllers
{
    public class DonationsController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly FirebaseService _firebase;

        // ✅ Inject IConfiguration and IWebHostEnvironment
        public DonationsController(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
            _firebase = new FirebaseService(_config, _env);
        }

        // Display Donations page
        [HttpGet]
public IActionResult Index()
{
    return View("~/Views/Home/Donations.cshtml");
}

        // Post form → redirect to PayFast
        [HttpPost]
        public IActionResult Pay(DonationModel model)
        {
            // Combine first + last name
            model.DonorName = $"{model.FirstName} {model.LastName}";

            string processUrl = _config["PayFastSettings:ProcessUrl"];
            string merchantId = _config["PayFastSettings:MerchantId"];
            string merchantKey = _config["PayFastSettings:MerchantKey"];
            string passPhrase = _config["PayFastSettings:PassPhrase"];
            string returnUrl = _config["PayFastSettings:ReturnUrl"];
            string cancelUrl = _config["PayFastSettings:CancelUrl"];
            string notifyUrl = _config["PayFastSettings:NotifyUrl"];

            // Build PayFast URL
            string url = $"{processUrl}?merchant_id={merchantId}&merchant_key={merchantKey}";
            url += $"&amount={model.Amount}&item_name=Donation to Lungisa NPO";
            url += $"&name_first={model.FirstName}&name_last={model.LastName}";
            url += $"&email_address={model.Email}";
            url += $"&return_url={returnUrl}&cancel_url={cancelUrl}&notify_url={notifyUrl}";

            return Redirect(url);
        }

        // PayFast notify URL
        [HttpPost]
        public async Task<IActionResult> Notify()
        {
            var form = await Request.ReadFormAsync();
            var paymentStatus = form["payment_status"];

            if (form["payment_status"] == "COMPLETE")
            {
                var donation = new DonationModel
                {
                    DonorName = form["name_first"] + " " + form["name_last"],
                    Email = form["email_address"],
                    Amount = decimal.Parse(form["amount_gross"]),
                    Status = "Success",
                    Timestamp = DateTime.UtcNow,
                    PayFastPaymentId = form["pf_payment_id"]
                };

                await _firebase.SaveDonation(donation);
            }


            return Ok(); // Must respond 200 to PayFast
        }
        // **Test Notify endpoint**
        [HttpPost]
        public IActionResult TestNotify([FromForm] IFormCollection form)
        {
            Console.WriteLine($"Test Notify hit: payment_status={form["payment_status"]}, pf_payment_id={form["pf_payment_id"]}");
            return Ok(); // PayFast expects 200 OK
        }

        // Optional: Success page after payment
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

        // Optional: Cancel page
        [HttpGet]
        public IActionResult Cancel()
        {
            return View();
        }
    }
}*/