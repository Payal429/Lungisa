using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lungisa.Models
{
    public class NewsArticleModel
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Body { get; set; }
        public string ImageUrl { get; set; }
/*        public string Status { get; set; } // Draft / Published*/
        public string Date { get; set; }
    }
}