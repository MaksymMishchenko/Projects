using Microsoft.EntityFrameworkCore;
using MoviesTelegramBotApp.Models;

namespace MoviesTelegramBotApp
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Cartoon> Cartoons { get; set; }
        public DbSet<CartoonGenre> CartoonGenre { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Movie>()
                .HasOne(g => g.Genre)
                .WithMany(g => g.Movies)
                .HasForeignKey(m => m.GenreId);

            modelBuilder.Entity<Genre>().HasData(
                new Genre { Id = 1, Name = "Action" },
                new Genre { Id = 2, Name = "Comedy" },
                new Genre { Id = 3, Name = "Drama" }
                );

            modelBuilder.Entity<Movie>().HasData(
                new Movie
                {
                    Id = 1,
                    Title = "Die hard",
                    Description = "desc 1",
                    Country = "USA",
                    Budget = "100000",
                    ImageUrl = "https://i.pinimg.com/736x/f8/46/e4/f846e4d7aa3aa6b7c82799e4745f8ab1.jpg",
                    MovieUrl = "https://www.youtube.com/watch?v=jaJuwKCmJbY",
                    InterestFactsUrl = "https://www.youtube.com/watch?v=ID_TNr5yoHE",
                    BehindTheScene = "https://www.youtube.com/watch?v=m15YteEQC7k",
                    GenreId = 1
                },
                new Movie
                {
                    Id = 2,
                    Title = "The Mask",
                    Description = "desc 2",
                    Country = "USA",
                    Budget = "1000000",
                    ImageUrl = "https://i.pinimg.com/originals/61/fa/cb/61facb2dbfa0f558c8be590d93813af5.jpg",
                    MovieUrl = "https://www.youtube.com/watch?v=LZl69yk5lEY",
                    InterestFactsUrl = "https://www.youtube.com/watch?v=zqAgauSv7AE",
                    BehindTheScene = "https://www.youtube.com/watch?v=iWPWZNfh-To",
                    GenreId = 2
                },
                new Movie
                {
                    Id = 3,
                    Title = "The Shawshank redemption",
                    Description = "desc 3",
                    Country = "USA",
                    Budget = "200000",
                    ImageUrl = "https://i.pinimg.com/736x/39/66/3b/39663baaa2f92e10fa3f6757ce9b4d37.jpg",
                    MovieUrl = "https://www.youtube.com/watch?v=PLl99DlL6b4",
                    InterestFactsUrl = "https://www.youtube.com/watch?v=tHQEimbxdRE",
                    BehindTheScene = "https://www.youtube.com/watch?v=M2Lul33Oypw",
                    GenreId = 3
                },
                new Movie
                {
                    Id = 4,
                    Title = "Point Break",
                    Description = "desc 4",
                    Country = "USA",
                    Budget = "300000",
                    ImageUrl = "https://i.pinimg.com/222x/70/2f/95/702f957e27890efd7f65d40a04c1915d.jpg",
                    MovieUrl = "https://www.youtube.com/watch?v=jcDD2-s4vWA",
                    InterestFactsUrl = "https://www.youtube.com/shorts/N6Nv5Z8U4P0",
                    BehindTheScene = "https://www.youtube.com/watch?v=3CuFPurGYDk",
                    GenreId = 3
                },
                new Movie
                {
                    Id = 5,
                    Title = "Terminator",
                    Description = "desc 5",
                    Country = "USA",
                    Budget = "450000",
                    ImageUrl = "https://i.pinimg.com/736x/8d/ae/66/8dae66323e077860ea2ab571edede26c.jpg",
                    MovieUrl = "https://www.youtube.com/watch?v=CRRlbK5w8AE",
                    InterestFactsUrl = "https://www.youtube.com/watch?v=amTSmyikwlY",
                    BehindTheScene = "https://www.youtube.com/watch?v=MBShyOajLcg",
                    GenreId = 1
                },
                new Movie
                {
                    Id = 6,
                    Title = "The Mask 2",
                    Description = "desc 6",
                    Country = "USA",
                    Budget = "3000000",
                    ImageUrl = "https://i.pinimg.com/originals/61/fa/cb/61facb2dbfa0f558c8be590d93813af5.jpg",
                    MovieUrl = "https://www.youtube.com/watch?v=LZl69yk5lEY",
                    InterestFactsUrl = "https://www.youtube.com/watch?v=zqAgauSv7AE",
                    BehindTheScene = "https://www.youtube.com/watch?v=iWPWZNfh-To",
                    GenreId = 2
                });

            modelBuilder.Entity<Cartoon>()
                .HasOne(g => g.Genre)
                .WithMany(g => g.Cartoons)
                .HasForeignKey(m => m.CartoonGenreId);

            modelBuilder.Entity<CartoonGenre>().HasData(
                new CartoonGenre { Id = 1, Genre = "Slapstick Comedy" },
                new CartoonGenre { Id = 2, Genre = "Children's Film" },
                new CartoonGenre { Id = 3, Genre = "Adventure" }
                );

            modelBuilder.Entity<Cartoon>().HasData(
                new Cartoon
                {
                    Id = 1,
                    Title = "Tom and Jerry",
                    Description = "Tom and Jerry is a classic cartoon about a never-ending battle between a cat named Tom and a mischievous mouse named Jerry.",
                    Budget = "1000000",
                    ImageUrl = "https://i.pinimg.com/236x/4d/6c/28/4d6c285286e59e8c4c6e4c30470db86f.jpg",
                    CartoonUrl = "https://www.youtube.com/watch?v=t0Q2otsqC4I",
                    CartoonGenreId = 1
                },
                new Cartoon
                {
                    Id = 2,
                    Title = "Oggy and the Cockroaches",
                    Description = "Oggy and the Cockroaches is a French animated series that follows the chaotic life of a blue cat named Oggy.",
                    Budget = "2000000",
                    ImageUrl = "https://i.pinimg.com/736x/5d/cb/ac/5dcbac34552629b0bb4963d0223f0166.jpg",
                    CartoonUrl = "https://www.youtube.com/watch?v=kGngZN_savE",
                    CartoonGenreId = 2
                },
                new Cartoon
                {
                    Id = 3,
                    Title = "Doraemon",
                    Description = "Doraemon is a beloved children's series about a robotic cat named Doraemon who travels back in time from the 22nd century.",
                    Budget = "3000000",
                    ImageUrl = "https://i.pinimg.com/236x/4e/de/d4/4eded44f271d6409be1850646ec382ae.jpg",
                    CartoonUrl = "https://www.youtube.com/watch?v=IbeOYixloT0",
                    CartoonGenreId = 3
                }
                );
        }
    }
}
