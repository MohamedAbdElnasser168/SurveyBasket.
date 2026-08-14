using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SurveyBasket.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedIdentityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "IsDefault", "IsDeleted", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "019fbb38-1e83-7c94-9ca4-204b37d45c2e", "019fbb38-1e83-7c94-9ca4-204d91723a96", false, false, "Admin", "ADMIN" },
                    { "019fbb38-1e83-7c94-9ca4-204cb935ecb1", "019fbb38-1e83-7c94-9ca4-204efdc69163", true, false, "Member", "MEMBER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "019fb07b-e292-76da-a661-c34c66eaae14", 0, "019fb080-34e4-73e2-9135-1180e2d33599", "admin@survey-basket.com", true, "SurveyBasket", "Admin", false, null, "ADMIN@SURVEY-BASKET.COM", "ADMIN@SURVEY-BASKET.COM", "AQAAAAIAAYagAAAAEEYKHTCzKJC5Y4WXB/aalWsUrl5EPNzURlCk78PfvJ0nzGRgQqyquUj8qT8jDCYcnQ==", null, false, "D1BD541E7BAF42D5948B26DBE24C8578", false, "admin@survey-basket.com" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 1, "permissions", "polls:read", "019fbb38-1e83-7c94-9ca4-204b37d45c2e" },
                    { 2, "permissions", "polls:add", "019fbb38-1e83-7c94-9ca4-204b37d45c2e" },
                    { 3, "permissions", "polls:update", "019fbb38-1e83-7c94-9ca4-204b37d45c2e" },
                    { 4, "permissions", "polls:delete", "019fbb38-1e83-7c94-9ca4-204b37d45c2e" },
                    { 5, "permissions", "questions:read", "019fbb38-1e83-7c94-9ca4-204b37d45c2e" },
                    { 6, "permissions", "questions:add", "019fbb38-1e83-7c94-9ca4-204b37d45c2e" },
                    { 7, "permissions", "questions:update", "019fbb38-1e83-7c94-9ca4-204b37d45c2e" },
                    { 8, "permissions", "users:read", "019fbb38-1e83-7c94-9ca4-204b37d45c2e" },
                    { 9, "permissions", "users:add", "019fbb38-1e83-7c94-9ca4-204b37d45c2e" },
                    { 10, "permissions", "users:update", "019fbb38-1e83-7c94-9ca4-204b37d45c2e" },
                    { 11, "permissions", "roles:read", "019fbb38-1e83-7c94-9ca4-204b37d45c2e" },
                    { 12, "permissions", "roles:add", "019fbb38-1e83-7c94-9ca4-204b37d45c2e" },
                    { 13, "permissions", "roles:update", "019fbb38-1e83-7c94-9ca4-204b37d45c2e" },
                    { 14, "permissions", "results:read", "019fbb38-1e83-7c94-9ca4-204b37d45c2e" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "019fbb38-1e83-7c94-9ca4-204b37d45c2e", "019fb07b-e292-76da-a661-c34c66eaae14" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "019fbb38-1e83-7c94-9ca4-204cb935ecb1");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "019fbb38-1e83-7c94-9ca4-204b37d45c2e", "019fb07b-e292-76da-a661-c34c66eaae14" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "019fbb38-1e83-7c94-9ca4-204b37d45c2e");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019fb07b-e292-76da-a661-c34c66eaae14");
        }
    }
}
