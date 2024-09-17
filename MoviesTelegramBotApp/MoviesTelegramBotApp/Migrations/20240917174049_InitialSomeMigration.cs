using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesTelegramBotApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialSomeMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<long>(
                name: "ChatId",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<long>(
                name: "ChatId",
                table: "User",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavoriteMovie",
                table: "UserFavoriteMovie",
                columns: new[] { "UserId", "MovieId" });

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
    }
}
