using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MoviesTelegramBotApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MovieUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InterestFactsUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BehindTheScene = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GenreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Movies_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Genres",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Action" },
                    { 2, "Comedy" },
                    { 3, "Drama" }
                });

            migrationBuilder.InsertData(
                table: "Movies",
                columns: new[] { "Id", "BehindTheScene", "Budget", "Country", "Description", "GenreId", "ImageUrl", "InterestFactsUrl", "MovieUrl", "Title" },
                values: new object[,]
                {
                    { 1, "https://www.youtube.com/watch?v=m15YteEQC7k", 100000m, "USA", "desc 1", 1, "https://i.pinimg.com/736x/f8/46/e4/f846e4d7aa3aa6b7c82799e4745f8ab1.jpg", "https://www.youtube.com/watch?v=ID_TNr5yoHE", "https://www.youtube.com/watch?v=jaJuwKCmJbY", "Die hard" },
                    { 2, "https://www.youtube.com/watch?v=iWPWZNfh-To", 1000000m, "USA", "desc 2", 2, "https://i.pinimg.com/originals/61/fa/cb/61facb2dbfa0f558c8be590d93813af5.jpg", "https://www.youtube.com/watch?v=zqAgauSv7AE", "https://www.youtube.com/watch?v=LZl69yk5lEY", "The Mask" },
                    { 3, "https://www.youtube.com/watch?v=M2Lul33Oypw", 200000m, "USA", "desc 3", 3, "https://i.pinimg.com/736x/39/66/3b/39663baaa2f92e10fa3f6757ce9b4d37.jpg", "https://www.youtube.com/watch?v=tHQEimbxdRE", "https://www.youtube.com/watch?v=PLl99DlL6b4", "The Shawshank redemption" },
                    { 4, "https://www.youtube.com/watch?v=3CuFPurGYDk", 300000m, "USA", "desc 4", 3, "https://i.pinimg.com/222x/70/2f/95/702f957e27890efd7f65d40a04c1915d.jpg", "https://www.youtube.com/shorts/N6Nv5Z8U4P0", "https://www.youtube.com/watch?v=jcDD2-s4vWA", "Point Break" },
                    { 5, "https://www.youtube.com/watch?v=MBShyOajLcg", 450000m, "USA", "desc 5", 1, "https://i.pinimg.com/736x/8d/ae/66/8dae66323e077860ea2ab571edede26c.jpg", "https://www.youtube.com/watch?v=amTSmyikwlY", "https://www.youtube.com/watch?v=CRRlbK5w8AE", "Terminator" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Movies_GenreId",
                table: "Movies",
                column: "GenreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Movies");

            migrationBuilder.DropTable(
                name: "Genres");
        }
    }
}
