using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesTelegramBotApp.Migrations
{
    /// <inheritdoc />
    public partial class CartoonCartoonGenreFixGenreBug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cartoon_CartoonGenre_CartoonGenreId",
                table: "Cartoon");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cartoon",
                table: "Cartoon");

            migrationBuilder.RenameTable(
                name: "Cartoon",
                newName: "Cartoons");

            migrationBuilder.RenameIndex(
                name: "IX_Cartoon_CartoonGenreId",
                table: "Cartoons",
                newName: "IX_Cartoons_CartoonGenreId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cartoons",
                table: "Cartoons",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "CartoonGenre",
                keyColumn: "Id",
                keyValue: 1,
                column: "Genre",
                value: "Slapstick Comedy");

            migrationBuilder.UpdateData(
                table: "CartoonGenre",
                keyColumn: "Id",
                keyValue: 2,
                column: "Genre",
                value: "Children's Film");

            migrationBuilder.UpdateData(
                table: "CartoonGenre",
                keyColumn: "Id",
                keyValue: 3,
                column: "Genre",
                value: "Adventure");

            migrationBuilder.UpdateData(
                table: "Cartoons",
                keyColumn: "Id",
                keyValue: 1,
                column: "Title",
                value: "Tom and Jerry");

            migrationBuilder.AddForeignKey(
                name: "FK_Cartoons_CartoonGenre_CartoonGenreId",
                table: "Cartoons",
                column: "CartoonGenreId",
                principalTable: "CartoonGenre",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cartoons_CartoonGenre_CartoonGenreId",
                table: "Cartoons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cartoons",
                table: "Cartoons");

            migrationBuilder.RenameTable(
                name: "Cartoons",
                newName: "Cartoon");

            migrationBuilder.RenameIndex(
                name: "IX_Cartoons_CartoonGenreId",
                table: "Cartoon",
                newName: "IX_Cartoon_CartoonGenreId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cartoon",
                table: "Cartoon",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Cartoon",
                keyColumn: "Id",
                keyValue: 1,
                column: "Title",
                value: "MYKITA KOZHUMYAKA");

            migrationBuilder.UpdateData(
                table: "CartoonGenre",
                keyColumn: "Id",
                keyValue: 1,
                column: "Genre",
                value: null);

            migrationBuilder.UpdateData(
                table: "CartoonGenre",
                keyColumn: "Id",
                keyValue: 2,
                column: "Genre",
                value: null);

            migrationBuilder.UpdateData(
                table: "CartoonGenre",
                keyColumn: "Id",
                keyValue: 3,
                column: "Genre",
                value: null);

            migrationBuilder.AddForeignKey(
                name: "FK_Cartoon_CartoonGenre_CartoonGenreId",
                table: "Cartoon",
                column: "CartoonGenreId",
                principalTable: "CartoonGenre",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
