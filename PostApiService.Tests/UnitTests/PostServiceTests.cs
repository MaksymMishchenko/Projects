using Microsoft.EntityFrameworkCore;
using PostApiService.Models;
using PostApiService.Services;

public class PostServiceTests
{
    private PostService GetPostService()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDatabase")
            .Options;

        var context = new ApplicationDbContext(options);
        return new PostService(context);
    }

    [Fact]
    public async Task AddPostAsync_Should_Add_Post_And_SaveChanges()
    {
        // Arrange
        var postService = GetPostService();
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
        await postService.AddPostAsync(post);

        // Assert
        using (var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDatabase").Options))
        {
            var addedPost = await context.Posts.FindAsync(post.PostId);
            Assert.NotNull(addedPost);
            Assert.Equal(post.Title, addedPost.Title);
            Assert.Equal(post.ImageUrl, addedPost.ImageUrl);
            Assert.Equal(post.MetaTitle, addedPost.MetaTitle);
            Assert.Equal(post.MetaDescription, addedPost.MetaDescription);
            Assert.Equal(post.Slug, addedPost.Slug);
            Assert.True(addedPost.CreateAt <= DateTime.Now);
        }
    }

    [Fact]
    public async Task DeletePostAsync_Should_DeletePost_IfExists()
    {
        // Arrange
        var postService = GetPostService();
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

        await postService.AddPostAsync(post);

        // Act
        await postService.DeletePostAsync(post.PostId);

        // Assert
        using (var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDatabase")
            .Options))
        {
            var deletedPost = await context.Posts.FindAsync(post.PostId);
            Assert.Null(deletedPost);
        }
    }
}