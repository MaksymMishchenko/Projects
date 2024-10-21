using Microsoft.EntityFrameworkCore;
using PostApiService.Models;
using PostApiService.Services;

namespace PostApiService.Tests.UnitTests
{
    public class CommentServiceTests
    {
        private (CommentService commentService, ApplicationDbContext context) GetCommentService()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dbContext = new ApplicationDbContext(options);
            var commentService = new CommentService(dbContext);

            return (commentService, dbContext);
        }

        [Fact]
        public async Task AddCommentAsync_Should_Add_Comment_With_CorrectPostId_And_CreatedAt()
        {
            // Arrange
            var (commentService, context) = GetCommentService();

            var postId = 1;
            var comment = new Comment { Content = "Some test comment" };

            // Act
            await commentService.AddCommentAsync(postId, comment);

            // Assert
            Assert.Equal(postId, comment.PostId);
            Assert.True(comment.CreatedAt <= DateTime.Now);

            var addedComment = await context.Comments.FirstOrDefaultAsync(c => c.PostId == postId && c.Content == "Some test comment");
            Assert.NotNull(addedComment);
            Assert.Equal("Some test comment", addedComment.Content);
            Assert.Equal(postId, addedComment.PostId);
        }
    }
}
