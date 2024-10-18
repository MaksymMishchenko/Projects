using Microsoft.EntityFrameworkCore;
using PostApiService.Models;
using PostApiService.Services;

public class PostServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly PostService _postService;

    public PostServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDatabase")
            .Options;

        _context = new ApplicationDbContext(options);
        _postService = new PostService(_context);
    }

    [Fact]
    public async Task AddPostAsync_Should_Add_Post_And_SaveChanges()
    {
        // Arrange
        var post = new Post
        {
            PostId = 1,
            Title = "Test Post",
            Description = "This is a test post.",
            Content = "Content of the test post.",
            ImageUrl = "http://example.com/image.jpg",
            MetaTitle = "Test Post Meta Title",
            MetaDescription = "Test Post Meta Description",
            Slug = "test-post"
        };

        // Act
        await _postService.AddPostAsync(post);

        // Assert
        var addedPost = await _context.Posts.FindAsync(post.PostId);
        Assert.NotNull(addedPost);
        Assert.Equal(post.Title, addedPost.Title);
        Assert.Equal(post.ImageUrl, addedPost.ImageUrl);
        Assert.Equal(post.MetaTitle, addedPost.MetaTitle);
        Assert.Equal(post.MetaDescription, addedPost.MetaDescription);
        Assert.Equal(post.Slug, addedPost.Slug);
        Assert.True(addedPost.CreateAt <= DateTime.Now); 
    }
}
