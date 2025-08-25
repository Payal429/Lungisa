namespace Lungisa.Models
{
    internal class FirebaseProject
    {
        public string Key { get; set; }
        public ProjectModel Project { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string StartDate { get; set; }   // maybe this is what you have
        public string EndDate { get; set; }
    }
}