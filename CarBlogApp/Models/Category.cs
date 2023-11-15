using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CarBlogApp.Models
{
    public class Category
    {
        [HiddenInput]
        public int Id { get; set; }
        [Display(Name = "Category Name")]
        [Required(ErrorMessage = "Field name is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Field must contain min 3 & max 20 symbols")]
        public string? Name { get; set; }
        public List<Post>? Posts { get; set; } = new List<Post>();
    }
}
