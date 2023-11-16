

using CarBlogApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarBlogApp.Areas.Admin.Models
{
    public class CreatePostViewModel
    {
        public Post? Post { get; set; }
        public SelectList? Categories { get; set; }
    }
}
