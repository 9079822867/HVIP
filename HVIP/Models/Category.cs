namespace HVIP.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public bool HomeNavbar { get; set; } = true;
        public bool FooterNavbar { get; set; } = true;
    }
}
