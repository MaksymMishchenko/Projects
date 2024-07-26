using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesTelegramBotApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMigrationAddNewMovie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Movies",
                columns: new[] { "Id", "BehindTheScene", "Budget", "Country", "Description", "GenreId", "ImageUrl", "InterestFactsUrl", "MovieUrl", "Title" },
                values: new object[] { 6, "https://www.youtube.com/watch?v=iWPWZNfh-To", "3000000", "USA", "desc 6", 2, "https://i.pinimg.com/originals/61/fa/cb/61facb2dbfa0f558c8be590d93813af5.jpg", "https://www.youtube.com/watch?v=zqAgauSv7AE", "https://www.youtube.com/watch?v=LZl69yk5lEY", "The Mask 2" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
