using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using PostApiService.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace PostApiService.Tests.E2ETests
{
    public class PostsControllerE2ETests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public PostsControllerE2ETests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove any previously registered DbContextOptions<ApplicationDbContext>
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });
                });
            });
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetPosts_ReturnsOk_WithListOfPosts()
        {
            // Arrange
            await SeedPostAsync(new Post { Title = "Test Post 1" });
            await SeedPostAsync(new Post { Title = "Test Post 2" });

            // Act
            var response = await _client.GetAsync("/api/Posts");

            // Assert
            response.EnsureSuccessStatusCode(); // Verifies the status code is 2xx

            var content = await response.Content.ReadAsStringAsync();
            Assert.NotNull(content);

            var posts = JsonSerializer.Deserialize<List<Post>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(posts);
            Assert.Equal(2, posts.Count);
            Assert.Contains(posts, p => p.Title == "Test Post 1");
            Assert.Contains(posts, p => p.Title == "Test Post 2");
        }
        
        private async Task SeedPostAsync(Post post)
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Posts.Add(post);
                await context.SaveChangesAsync();
            }
        }
    }
}
