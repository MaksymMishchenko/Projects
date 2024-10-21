using Microsoft.EntityFrameworkCore;
using PostApiService.Models;
using PostApiService.Services;

public class PostServiceTests
{
    private (PostService postService, ApplicationDbContext context) GetPostService()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        var postService = new PostService(context);
        return (postService, context);
    }

    [Fact]
    public async Task AddPostAsync_Should_Add_Post_And_SaveChanges()
    {
        // Arrange
        var (postService, context) = GetPostService();
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
        var addedPost = await context.Posts.FindAsync(post.PostId);
        Assert.NotNull(addedPost);
        Assert.Equal(post.Title, addedPost.Title);
        Assert.Equal(post.ImageUrl, addedPost.ImageUrl);
        Assert.Equal(post.MetaTitle, addedPost.MetaTitle);
        Assert.Equal(post.MetaDescription, addedPost.MetaDescription);
        Assert.Equal(post.Slug, addedPost.Slug);
        Assert.True(addedPost.CreateAt <= DateTime.Now);
    }

    [Fact]
    public async Task DeletePostAsync_Should_DeletePost_IfExists()
    {
        // Arrange
        var (postService, context) = GetPostService();
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
        var deletedPost = await context.Posts.FindAsync(post.PostId);
        Assert.Null(deletedPost);
    }

    [Fact]
    public async Task DeletePostAsync_Should_Not_Throw_When_Post_DoesNotExist()
    {
        // Arrange
        var (postService, context) = GetPostService();

        // Act
        await postService.DeletePostAsync(999);

        // Assert        
        var postCount = await context.Posts.CountAsync();
        Assert.Equal(0, postCount);
    }

    [Fact]
    public async Task EditPostAsync_Should_Edit_Post_ByPost()
    {
        // Arrange
        var (postService, context) = GetPostService();
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

        post.Title = "Some Post";
        post.Description = "Some Description";
        post.Slug = "http://example.com/test.jpg";

        // Act
        await postService.EditPostAsync(post);

        //Assert
        var editedPost = await context.Posts.FindAsync(post.PostId);

        Assert.NotNull(editedPost);
        Assert.Equal("Some Post", editedPost.Title);
        Assert.Equal("Some Description", editedPost.Description);
        Assert.Equal("http://example.com/test.jpg", editedPost.Slug);
    }
}