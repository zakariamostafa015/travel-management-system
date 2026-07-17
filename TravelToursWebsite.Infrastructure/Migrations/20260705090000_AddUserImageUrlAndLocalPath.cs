using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TravelToursWebsite.Infrastructure.Persistence;

#nullable disable

namespace TravelToursWebsite.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260705090000_AddUserImageUrlAndLocalPath")]
public partial class AddUserImageUrlAndLocalPath : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ProfileImageUrl",
            table: "Users",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ProfileImageLocalPath",
            table: "Users",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.Sql("UPDATE Users SET ProfileImageUrl = ProfileImagePath WHERE ProfileImagePath IS NOT NULL AND ProfileImageUrl IS NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ProfileImageUrl", table: "Users");
        migrationBuilder.DropColumn(name: "ProfileImageLocalPath", table: "Users");
    }
}
