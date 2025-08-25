using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lungisa.Models
{
    public class ArticleModel
    {
        public string Id { get; set; }            // Unique ID for Firebase
        public string Headline { get; set; }
        public string Summary { get; set; }
        public string Body { get; set; }
        public string ImageUrl { get; set; } // Optional: store image path or URL
        public string DateCreated { get; set; }     // Timestamp
    }
}