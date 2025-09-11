// Payal and Nyanda
// 15 August 2025
// Handles donation processing and PayFast payment integration. Allows donors to contribute via PayFast, securely records donation details in Firebase, and
// responds to PayFast Instant Transaction Notifications (ITNs).

using Lungisa.Models;
using Lungisa.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics.Metrics;

namespace Lungisa.Controllers
{
    public class DonationsController : Controller
    {
        // App configuration (PayFast keys, URLs, etc.)
        private readonly IConfiguration _config;
        
        // Hosting environment (used for Firebase setup)
        private readonly IWebHostEnvironment _env;
        
        // Firebase service for saving donations
        private readonly FirebaseService _firebase;
       
        // For server-to-server PayFast communication
        private readonly HttpClient _httpClient;

        public DonationsController(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
            _firebase = new FirebaseService(_config, _env);
            _httpClient = new HttpClient();
        }

        // GET: Donations page
        // Displays the Donations page where users can enter their donation details.
        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/Home/Donations.cshtml");
        }

        // POST: PayFast payment redirect
        [HttpPost]
        public async Task<IActionResult> PayFastPay(DonationModel model)
        {
            // Generate merchant reference
            var mPaymentId = Guid.NewGuid().ToString("N");

            // Save donation as Pending
            var pendingDonation = new DonationModel
            {
                DonorName = $"{model.FirstName} {model.LastName}",
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Amount = model.Amount,
                Status = "Pending",
                Timestamp = DateTime.UtcNow,
                M_PaymentId = mPaymentId
            };
            await _firebase.SaveDonation(pendingDonation);

            // Build PayFast payment parameters
            var pfData = new SortedDictionary<string, string>
            {
                ["merchant_id"] = _config["PayFastSettings:MerchantId"],
                ["merchant_key"] = _config["PayFastSettings:MerchantKey"],
                ["return_url"] = _config["PayFastSettings:ReturnUrl"],
                ["cancel_url"] = _config["PayFastSettings:CancelUrl"],
                ["notify_url"] = _config["PayFastSettings:NotifyUrl"],
                ["name_first"] = model.FirstName,
                ["name_last"] = model.LastName,
                ["email_address"] = model.Email,
                ["m_payment_id"] = mPaymentId,
                
                // Using InvariantCulture via string.Format instead of ToString
                ["amount"] = string.Format(CultureInfo.InvariantCulture, "{0:F2}", model.Amount),
                ["item_name"] = "Donation to Lungisa NPO"
            };

            // Generate signature for PayFast
            var signature = GeneratePayfastSignature(pfData, _config["PayFastSettings:Passphrase"]);
            pfData.Add("signature", signature);

            var processUrl = _config["PayFastSettings:ProcessUrl"];
            var queryString = string.Join("&", pfData.Select(kvp => $"{kvp.Key}={WebUtility.UrlEncode(kvp.Value)}"));

            // Redirect donor to PayFast payment page
            return Redirect($"{processUrl}?{queryString}");
        }

        // ITN endpoint (PayFast notifies server of payment)
        [IgnoreAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> Notify()
        {
            if (!Request.HasFormContentType) return BadRequest("No form data");

            // Extract form data from PayFast notification
            var form = Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString());
            var receivedSignature = form.GetValueOrDefault("signature", "");
            form.Remove("signature");

            // Recalculate signature
            var sortedData = new SortedDictionary<string, string>(form);
            var calculatedSignature = GeneratePayfastSignature(sortedData, _config["PayFastSettings:Passphrase"]);

            // Reject if signature doesn’t match
            if (!string.Equals(receivedSignature, calculatedSignature, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid signature");

            // Map PayFast fields to DonationModel
            var donation = new DonationModel
            {
                DonorName = $"{form.GetValueOrDefault("name_first", "")} {form.GetValueOrDefault("name_last", "")}".Trim(),
                Email = form.GetValueOrDefault("email_address", ""),
                Amount = decimal.TryParse(form.GetValueOrDefault("amount_gross", form.GetValueOrDefault("amount", "0")), NumberStyles.Any, CultureInfo.InvariantCulture, out var amt) ? amt : 0,
                Status = MapPayFastStatusToLocalStatus(form.GetValueOrDefault("payment_status", "Failed")),
                PayFastPaymentId = form.GetValueOrDefault("pf_payment_id", ""),
                M_PaymentId = form.GetValueOrDefault("m_payment_id", ""),
                Timestamp = DateTime.UtcNow
            };

            try
            {
                // Save or update donation record in Firebase
                await _firebase.SaveDonation(donation); 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to save donation from ITN: " + ex.Message);
                return StatusCode(500, "Failed to save");
            }

            return Ok("ITN processed");
        }

        // Success / Cancel pages
        //Displays the Success page after a successful donation.
        [HttpGet] public IActionResult Success() => View();
        // Displays the Cancel page if a donation is cancelled.
        [HttpGet] public IActionResult Cancel() => View();

        // ================= Helper Methods =================
        // Generates MD5 signature required by PayFast for secure communication.
        private string GeneratePayfastSignature(SortedDictionary<string, string> data, string passphrase)
        {
            var sb = new StringBuilder();
            foreach (var kv in data)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    sb.Append(kv.Key).Append('=').Append(WebUtility.UrlEncode(kv.Value).Replace("%20", "+")).Append('&');
                }
            }

            // Append passphrase if configured
            if (!string.IsNullOrEmpty(passphrase))
                sb.Append("passphrase=").Append(WebUtility.UrlEncode(passphrase).Replace("%20", "+"));
            else if (sb.Length > 0 && sb[sb.Length - 1] == '&') sb.Length--; // remove trailing &

            // Hash using MD5
            using var md5 = MD5.Create();
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var hash = md5.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private string MapPayFastStatusToLocalStatus(string pfStatus)
        {
            return pfStatus.ToUpper() switch
            {
                "COMPLETE" => "Success",
                "FAILED" => "Failed",
                "PENDING" => "Pending",
                _ => pfStatus
            };
        }
    }
}

