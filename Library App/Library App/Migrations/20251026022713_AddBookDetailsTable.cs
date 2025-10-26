using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library_App.Migrations
{
    /// <inheritdoc />
    public partial class AddBookDetailsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1,1"),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    AverageRating = table.Column<double>(type: "float", nullable: true),
                    SmallThumbnail = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Thumbnail = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Small = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Medium = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Large = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookDetails_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookDetails_BookId",
                table: "BookDetails",
                column: "BookId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookDetails");
        }
    }
}
