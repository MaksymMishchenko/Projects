namespace CarBlogApp.Models
{
    public class PostCategoryViewModel
    {
        public IEnumerable<Post>? Posts { get; set; }
        public IEnumerable<Category>? Categories { get; set; }
    }
}
