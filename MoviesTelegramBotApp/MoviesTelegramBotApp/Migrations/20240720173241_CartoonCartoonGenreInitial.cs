using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MoviesTelegramBotApp.Migrations
{
    /// <inheritdoc />
    public partial class CartoonCartoonGenreInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CartoonGenre",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Genre = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartoonGenre", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cartoon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Budget = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CartoonUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CartoonGenreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cartoon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cartoon_CartoonGenre_CartoonGenreId",
                        column: x => x.CartoonGenreId,
                        principalTable: "CartoonGenre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CartoonGenre",
                columns: new[] { "Id", "Genre" },
                values: new object[,]
                {
                    { 1, null },
                    { 2, null },
                    { 3, null }
                });

            migrationBuilder.InsertData(
                table: "Cartoon",
                columns: new[] { "Id", "Budget", "CartoonGenreId", "CartoonUrl", "Description", "ImageUrl", "Title" },
                values: new object[,]
                {
                    { 1, "1000000", 1, "https://www.youtube.com/watch?v=t0Q2otsqC4I", "Tom and Jerry is a classic cartoon about a never-ending battle between a cat named Tom and a mischievous mouse named Jerry.", "https://i.pinimg.com/236x/4d/6c/28/4d6c285286e59e8c4c6e4c30470db86f.jpg", "MYKITA KOZHUMYAKA" },
                    { 2, "2000000", 2, "https://www.youtube.com/watch?v=kGngZN_savE", "Oggy and the Cockroaches is a French animated series that follows the chaotic life of a blue cat named Oggy.", "https://i.pinimg.com/736x/5d/cb/ac/5dcbac34552629b0bb4963d0223f0166.jpg", "Oggy and the Cockroaches" },
                    { 3, "3000000", 3, "https://www.youtube.com/watch?v=IbeOYixloT0", "Doraemon is a beloved children's series about a robotic cat named Doraemon who travels back in time from the 22nd century.", "https://i.pinimg.com/236x/4e/de/d4/4eded44f271d6409be1850646ec382ae.jpg", "Doraemon" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cartoon_CartoonGenreId",
                table: "Cartoon",
                column: "CartoonGenreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cartoon");

            migrationBuilder.DropTable(
                name: "CartoonGenre");
        }
    }
}
