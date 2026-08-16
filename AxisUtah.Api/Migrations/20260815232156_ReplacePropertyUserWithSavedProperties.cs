using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AxisUtah.Api.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePropertyUserWithSavedProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavedProperties",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ListingKey = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedProperties", x => new { x.UserId, x.ListingKey });
                    table.ForeignKey(
                        name: "FK_SavedProperties_Properties_ListingKey",
                        column: x => x.ListingKey,
                        principalTable: "Properties",
                        principalColumn: "ListingKey",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedProperties_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO [SavedProperties] ([UserId], [ListingKey], [Active])
                SELECT [UserId], [ListingKey], CAST(1 AS bit)
                FROM [Properties]
                WHERE [UserId] IS NOT NULL;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Users_UserId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_UserId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Properties");

            migrationBuilder.CreateIndex(
                name: "IX_SavedProperties_ListingKey",
                table: "SavedProperties",
                column: "ListingKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE properties
                SET UserId = saved.UserId
                FROM [Properties] AS properties
                INNER JOIN [SavedProperties] AS saved
                    ON saved.ListingKey = properties.ListingKey;
                """);

            migrationBuilder.DropTable(
                name: "SavedProperties");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_UserId",
                table: "Properties",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Users_UserId",
                table: "Properties",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
