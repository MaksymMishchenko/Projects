using Microsoft.EntityFrameworkCore;
using PostApiService.Models;
using PostApiService.Services;

namespace PostApiService.Tests.IntegrationTests
{
    public class PostServiceIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly ApplicationDbContext _context;
        private PostService _postService;

        public PostServiceIntegrationTests(IntegrationTestFixture fixture)
        {
            _context = fixture.Context;
            _postService = new PostService(_context);
        }

        [Fact]
        public async Task AddPostAsync_Should_Add_Post_And_SaveChanges()
        {
            const int postsCollectionCount = 2;
            // Arrange
            var post1 = new Post
            {                
                Title = "Test Post",
                Description = "This is a test post.",
                Content = "Content of the test post.",
                ImageUrl = "http://example.com/image.jpg",
                MetaTitle = "Test Post Meta Title",
                MetaDescription = "Test Post Meta Description",
                Slug = "test-post-one"
            };

            var post2 = new Post
            {                
                Title = "Test Post 2",
                Description = "This is a test post 2.",
                Content = "Content of the test post 2.",
                ImageUrl = "http://example.com/image2.jpg",
                MetaTitle = "Test Post Meta Title 2",
                MetaDescription = "Test Post Meta Description 2",
                Slug = "test-post-two"
            };

            // Act

            await _postService.AddPostAsync(post1);
            await _postService.AddPostAsync(post2);

            // Assert
            var addedPost = await _context.Posts.FindAsync(post1.PostId);
            var postCount = await _context.Posts.CountAsync();
            Assert.NotNull(addedPost);            
            Assert.Equal(postsCollectionCount, postCount);
            Assert.Equal(post1.Title, addedPost.Title);
        }
    }
}
