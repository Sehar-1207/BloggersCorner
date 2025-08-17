using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BloggingCorner.Models.ViewModels
{
    public class PostViewModel
    {
        public Post post { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> categories { get; set; }
        public IFormFile publishImage { get; set; }
    }
}
