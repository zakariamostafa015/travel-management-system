using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TravelToursWebsite.Infrastructure.Persistence;

#nullable disable

namespace TravelToursWebsite.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260701001000_AddImageUrlAndLocalPath")]
public partial class AddImageUrlAndLocalPath : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ImageUrl",
            table: "TourImages",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ImageLocalPath",
            table: "TourImages",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ImageUrl",
            table: "BlogImages",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ImageLocalPath",
            table: "BlogImages",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ImageUrl", table: "TourImages");
        migrationBuilder.DropColumn(name: "ImageLocalPath", table: "TourImages");
        migrationBuilder.DropColumn(name: "ImageUrl", table: "BlogImages");
        migrationBuilder.DropColumn(name: "ImageLocalPath", table: "BlogImages");
    }
}