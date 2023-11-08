using System.ComponentModel.DataAnnotations;

namespace CarBlogApp.Models
{
    public class ContactForm
    {
        public int Id { get; set; }
        [Display(Name = "Name")]
        [Required(ErrorMessage = "Field name is required")]
        [StringLength(12, MinimumLength = 3, ErrorMessage = "Field must contain min 3 symbols")]
        public string? Name { get; set; }
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Field email is required")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email is incorrect")]
        public string? Email { get; set; }
        [Display(Name = "Message")]
        [Required(ErrorMessage = "Field message is required")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Field must contain from 10 to 100 symbols")]
        public string? Message { get; set; }
    }
}
