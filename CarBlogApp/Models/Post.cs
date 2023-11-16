using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CarBlogApp.Models
{
    public class Post
    {
        [HiddenInput]
        public int Id { get; set; }

        public string? Img { get; set; }

        [Display(Name = "Title")]
        [Required(ErrorMessage = "Field title is required")]
        [StringLength(25, MinimumLength = 10, ErrorMessage = "Field must contain min 10 & max 20 symbols")]
        public string? Title { get; set; }

        [Display(Name = "Author")]
        [Required(ErrorMessage = "Field author is required")]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "Field must contain min 3 & max 15 symbols")]
        public string? Author { get; set; }

        public DateTime Date { get; set; }

        [Display(Name = "Description")]
        [Required(ErrorMessage = "Field description is required")]
        [StringLength(100, MinimumLength = 50, ErrorMessage = "Field must contain min 50 & max 15 symbols")]
        public string? Description { get; set; }

        [Display(Name = "Content")]
        [Required(ErrorMessage = "Field text is required")]
        [StringLength(2500, MinimumLength = 250, ErrorMessage = "Field must contain min 250 & max 2500 symbols")]
        public string? Body { get; set; }

        [HiddenInput]
        [Required(ErrorMessage = "Field category is required")]
        public int CategoryId { get; set; }

        [Display(Name = "Category")]
        public Category? Category { get; set; }
    }
}
