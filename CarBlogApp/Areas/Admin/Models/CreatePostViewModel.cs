using CarBlogApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CarBlogApp.Areas.Admin.Models
{
    public class CreatePostViewModel
    {
        [Display(Name = "Image")]
        [Required(ErrorMessage = "Field image is required")]
        [DataType(DataType.Upload)]
        public IFormFile? ImageFile { get; set; }
        public Post? Post { get; set; }
        public SelectList? Categories { get; set; }

    }
}
