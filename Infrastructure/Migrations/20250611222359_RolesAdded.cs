using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RolesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3274ef7d-74bc-4649-a514-d73d0389e5fc", "8899cbf2-f95e-4180-ba51-e34f6ba0bd63", "Admin", "ADMIN" },
                    { "411b574d-ea60-4d45-80cc-8133e8f245ee", "ed24cc96-780e-4c2a-8c57-faac08e1b234", "Customer", "CUSTOMER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3274ef7d-74bc-4649-a514-d73d0389e5fc");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "411b574d-ea60-4d45-80cc-8133e8f245ee");
        }
    }
}
