using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AxisUtah.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddingPropertyHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PropertyHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListingKey = table.Column<int>(type: "int", nullable: false),
                    FieldType = table.Column<int>(type: "int", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyHistories_Properties_ListingKey",
                        column: x => x.ListingKey,
                        principalTable: "Properties",
                        principalColumn: "ListingKey",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyHistories_ChangedAtUtc",
                table: "PropertyHistories",
                column: "ChangedAtUtc",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyHistories_CorrelationId",
                table: "PropertyHistories",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyHistories_ListingKey",
                table: "PropertyHistories",
                column: "ListingKey");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyHistories_ListingKey_ChangedAtUtc",
                table: "PropertyHistories",
                columns: new[] { "ListingKey", "ChangedAtUtc" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyHistories");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Properties");
        }
    }
}
