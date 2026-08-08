using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AxisUtah.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeOnDeleteToRestrictForProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyMedia_Properties_ListingKey",
                table: "PropertyMedia");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyMedia_Properties_ListingKey",
                table: "PropertyMedia",
                column: "ListingKey",
                principalTable: "Properties",
                principalColumn: "ListingKey",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyMedia_Properties_ListingKey",
                table: "PropertyMedia");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyMedia_Properties_ListingKey",
                table: "PropertyMedia",
                column: "ListingKey",
                principalTable: "Properties",
                principalColumn: "ListingKey",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
