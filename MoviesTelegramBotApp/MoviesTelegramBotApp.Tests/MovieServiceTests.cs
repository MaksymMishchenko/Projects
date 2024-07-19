using Microsoft.EntityFrameworkCore;
using MoviesTelegramBotApp.Models;
using MoviesTelegramBotApp.Services;

namespace MoviesTelegramBotApp.Tests
{
    public class MovieServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "MovieTelegramBot")
                .Options;

            var dbContext = new ApplicationDbContext(options);
            dbContext.Database.EnsureCreated();

            // Seed data
            dbContext.Genres.Add(new Genre { Id = 4, Name = "Western" });
            dbContext.Genres.Add(new Genre { Id = 5, Name = "Horror" });

            dbContext.Movies.Add(new Movie
            {
                Id = 6,
                Title = "Die Hard 1",
                Description = "desc 6",
                Country = "USA",
                Budget = "200000",
                GenreId = 4
            });

            dbContext.Movies.Add(new Movie
            {
                Id = 7,
                Title = "The Mask 1",
                Description = "desc 7",
                Country = "USA",
                Budget = "100000",
                GenreId = 5
            });

            dbContext.SaveChanges();

            return dbContext;
        }

        [Fact]
        public async void Can_Paginate()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var movieService = new MovieService(dbContext);

            // Act
            var result = await movieService.GetAllMoviesAsync(2);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);            
            Assert.Equal("The Mask", result.First().Title);
        }
    }
}