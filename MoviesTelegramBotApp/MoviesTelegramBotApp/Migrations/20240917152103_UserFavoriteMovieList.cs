using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MoviesTelegramBotApp.Migrations
{
    /// <inheritdoc />
    public partial class UserFavoriteMovieList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteMovies_Movies_MovieId",
                table: "UserFavoriteMovies");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteMovies_Users_UserId",
                table: "UserFavoriteMovies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFavoriteMovies",
                table: "UserFavoriteMovies");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "UserFavoriteMovies",
                newName: "UserFavoriteMovie");

            migrationBuilder.RenameIndex(
                name: "IX_UserFavoriteMovies_MovieId",
                table: "UserFavoriteMovie",
                newName: "IX_UserFavoriteMovie_MovieId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavoriteMovie",
                table: "UserFavoriteMovie",
                columns: new[] { "UserId", "MovieId" });

            migrationBuilder.InsertData(
                table: "CartoonGenre",
                columns: new[] { "Id", "Genre" },
                values: new object[,]
                {
                    { 1, "Slapstick Comedy" },
                    { 2, "Children's Film" },
                    { 3, "Adventure" }
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
                table: "Cartoons",
                columns: new[] { "Id", "Budget", "CartoonGenreId", "CartoonUrl", "Description", "ImageUrl", "Title" },
                values: new object[,]
                {
                    { 1, "1000000", 1, "https://www.youtube.com/watch?v=t0Q2otsqC4I", "Tom and Jerry is a classic cartoon about a never-ending battle between a cat named Tom and a mischievous mouse named Jerry.", "https://i.pinimg.com/236x/4d/6c/28/4d6c285286e59e8c4c6e4c30470db86f.jpg", "Tom and Jerry" },
                    { 2, "2000000", 2, "https://www.youtube.com/watch?v=kGngZN_savE", "Oggy and the Cockroaches is a French animated series that follows the chaotic life of a blue cat named Oggy.", "https://i.pinimg.com/736x/5d/cb/ac/5dcbac34552629b0bb4963d0223f0166.jpg", "Oggy and the Cockroaches" },
                    { 3, "3000000", 3, "https://www.youtube.com/watch?v=IbeOYixloT0", "Doraemon is a beloved children's series about a robotic cat named Doraemon who travels back in time from the 22nd century.", "https://i.pinimg.com/236x/4e/de/d4/4eded44f271d6409be1850646ec382ae.jpg", "Doraemon" }
                });

            migrationBuilder.InsertData(
                table: "Movies",
                columns: new[] { "Id", "BehindTheScene", "Budget", "Country", "Description", "GenreId", "ImageUrl", "InterestFactsUrl", "IsFavorite", "MovieUrl", "Title" },
                values: new object[,]
                {
                    { 1, "https://www.youtube.com/watch?v=m15YteEQC7k", "100000", "USA", "desc 1", 1, "https://i.pinimg.com/736x/f8/46/e4/f846e4d7aa3aa6b7c82799e4745f8ab1.jpg", "https://www.youtube.com/watch?v=ID_TNr5yoHE", false, "https://www.youtube.com/watch?v=jaJuwKCmJbY", "Die hard" },
                    { 2, "https://www.youtube.com/watch?v=iWPWZNfh-To", "1000000", "USA", "desc 2", 2, "https://i.pinimg.com/originals/61/fa/cb/61facb2dbfa0f558c8be590d93813af5.jpg", "https://www.youtube.com/watch?v=zqAgauSv7AE", false, "https://www.youtube.com/watch?v=LZl69yk5lEY", "The Mask" },
                    { 3, "https://www.youtube.com/watch?v=M2Lul33Oypw", "200000", "USA", "desc 3", 3, "https://i.pinimg.com/736x/39/66/3b/39663baaa2f92e10fa3f6757ce9b4d37.jpg", "https://www.youtube.com/watch?v=tHQEimbxdRE", false, "https://www.youtube.com/watch?v=PLl99DlL6b4", "The Shawshank redemption" },
                    { 4, "https://www.youtube.com/watch?v=3CuFPurGYDk", "300000", "USA", "desc 4", 3, "https://i.pinimg.com/222x/70/2f/95/702f957e27890efd7f65d40a04c1915d.jpg", "https://www.youtube.com/shorts/N6Nv5Z8U4P0", false, "https://www.youtube.com/watch?v=jcDD2-s4vWA", "Point Break" },
                    { 5, "https://www.youtube.com/watch?v=MBShyOajLcg", "450000", "USA", "desc 5", 1, "https://i.pinimg.com/736x/8d/ae/66/8dae66323e077860ea2ab571edede26c.jpg", "https://www.youtube.com/watch?v=amTSmyikwlY", false, "https://www.youtube.com/watch?v=CRRlbK5w8AE", "Terminator" },
                    { 6, "https://www.youtube.com/watch?v=iWPWZNfh-To", "3000000", "USA", "desc 6", 2, "https://i.pinimg.com/originals/61/fa/cb/61facb2dbfa0f558c8be590d93813af5.jpg", "https://www.youtube.com/watch?v=zqAgauSv7AE", false, "https://www.youtube.com/watch?v=LZl69yk5lEY", "The Mask 2" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteMovie_Movies_MovieId",
                table: "UserFavoriteMovie",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteMovie_User_UserId",
                table: "UserFavoriteMovie",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteMovie_Movies_MovieId",
                table: "UserFavoriteMovie");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteMovie_User_UserId",
                table: "UserFavoriteMovie");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFavoriteMovie",
                table: "UserFavoriteMovie");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DeleteData(
                table: "Cartoons",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cartoons",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cartoons",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "CartoonGenre",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CartoonGenre",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CartoonGenre",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.RenameTable(
                name: "UserFavoriteMovie",
                newName: "UserFavoriteMovies");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.RenameIndex(
                name: "IX_UserFavoriteMovie_MovieId",
                table: "UserFavoriteMovies",
                newName: "IX_UserFavoriteMovies_MovieId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavoriteMovies",
                table: "UserFavoriteMovies",
                columns: new[] { "UserId", "MovieId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteMovies_Movies_MovieId",
                table: "UserFavoriteMovies",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteMovies_Users_UserId",
                table: "UserFavoriteMovies",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
