using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TravelToursWebsite.Infrastructure.Persistence;

#nullable disable

namespace TravelToursWebsite.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260703010000_AddAuditLogsAndHardeningIndexes")]
public partial class AddAuditLogsAndHardeningIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AuditLogs",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<int>(type: "int", nullable: true),
                Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                HttpMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                QueryString = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Area = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                StatusCode = table.Column<int>(type: "int", nullable: false),
                Succeeded = table.Column<bool>(type: "bit", nullable: false),
                ElapsedMilliseconds = table.Column<long>(type: "bigint", nullable: false),
                IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                TraceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditLogs", x => x.Id);
                table.ForeignKey(
                    name: "FK_AuditLogs_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(name: "IX_AuditLogs_Area", table: "AuditLogs", column: "Area");
        migrationBuilder.CreateIndex(name: "IX_AuditLogs_CreatedAtUtc", table: "AuditLogs", column: "CreatedAtUtc");
        migrationBuilder.CreateIndex(name: "IX_AuditLogs_HttpMethod", table: "AuditLogs", column: "HttpMethod");
        migrationBuilder.CreateIndex(name: "IX_AuditLogs_StatusCode", table: "AuditLogs", column: "StatusCode");
        migrationBuilder.CreateIndex(name: "IX_AuditLogs_Succeeded", table: "AuditLogs", column: "Succeeded");
        migrationBuilder.CreateIndex(name: "IX_AuditLogs_UserId", table: "AuditLogs", column: "UserId");

        migrationBuilder.CreateIndex(name: "IX_TourTranslations_Language_Slug", table: "TourTranslations", columns: new[] { "Language", "Slug" });
        migrationBuilder.CreateIndex(name: "IX_TourCategoryTranslations_Language_Slug", table: "TourCategoryTranslations", columns: new[] { "Language", "Slug" });
        migrationBuilder.CreateIndex(name: "IX_BlogPostTranslations_Language_Slug", table: "BlogPostTranslations", columns: new[] { "Language", "Slug" });
        migrationBuilder.CreateIndex(name: "IX_BlogCategoryTranslations_Language_Slug", table: "BlogCategoryTranslations", columns: new[] { "Language", "Slug" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_TourTranslations_Language_Slug", table: "TourTranslations");
        migrationBuilder.DropIndex(name: "IX_TourCategoryTranslations_Language_Slug", table: "TourCategoryTranslations");
        migrationBuilder.DropIndex(name: "IX_BlogPostTranslations_Language_Slug", table: "BlogPostTranslations");
        migrationBuilder.DropIndex(name: "IX_BlogCategoryTranslations_Language_Slug", table: "BlogCategoryTranslations");
        migrationBuilder.DropTable(name: "AuditLogs");
    }
}
