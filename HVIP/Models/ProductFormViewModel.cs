using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace HVIP.Models
{
    public class ProductFormViewModel
    {
        public int    Id              { get; set; }

        [Required] public string  Name             { get; set; }
        public string  ShortDescription { get; set; }
        public string  Description      { get; set; }

        [Required] public decimal Price          { get; set; }
        public decimal? OriginalPrice  { get; set; }

        [Required] public int     CategoryId      { get; set; }
        public string  Brand           { get; set; } = "HVIP";
        public int     Stock           { get; set; } = 100;
        public string  Size            { get; set; }

        public bool    IsFeatured      { get; set; }
        public bool    IsBestseller    { get; set; }
        public bool    IsNew           { get; set; }
        public bool    IsActive        { get; set; } = true;
        public double  Rating          { get; set; } = 4.0;
        public int     ReviewCount     { get; set; }

        // Image
        public string          ImageUrl  { get; set; }   // current saved URL (shown in edit)
        public HttpPostedFileBase ImageFile { get; set; } // new upload
        public bool            RemoveImage { get; set; } // tick to clear existing image

        public List<SelectListItem> CategoryOptions { get; set; }
        public bool IsEdit => Id > 0;
    }
}
