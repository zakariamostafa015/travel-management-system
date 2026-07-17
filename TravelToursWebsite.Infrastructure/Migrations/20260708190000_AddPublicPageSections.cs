using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TravelToursWebsite.Infrastructure.Persistence;

#nullable disable

namespace TravelToursWebsite.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260708190000_AddPublicPageSections")]
public partial class AddPublicPageSections : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PublicPageSections",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PageKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                SectionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                LayoutVariant = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Theme = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                DesktopMediaUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                MobileMediaUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                MediaAlt = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                CtaLabel = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                CtaUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                LinkedTourId = table.Column<int>(type: "int", nullable: true),
                LinkedTourCategoryId = table.Column<int>(type: "int", nullable: true),
                LinkedBlogPostId = table.Column<int>(type: "int", nullable: true),
                SortOrder = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PublicPageSections", x => x.Id);
                table.ForeignKey(
                    name: "FK_PublicPageSections_BlogPosts_LinkedBlogPostId",
                    column: x => x.LinkedBlogPostId,
                    principalTable: "BlogPosts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.NoAction);
                table.ForeignKey(
                    name: "FK_PublicPageSections_TourCategories_LinkedTourCategoryId",
                    column: x => x.LinkedTourCategoryId,
                    principalTable: "TourCategories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.NoAction);
                table.ForeignKey(
                    name: "FK_PublicPageSections_Tours_LinkedTourId",
                    column: x => x.LinkedTourId,
                    principalTable: "Tours",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.NoAction);
            });

        migrationBuilder.CreateTable(
            name: "PublicPageSectionItems",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PublicPageSectionId = table.Column<int>(type: "int", nullable: false),
                ItemKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Label = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                Value = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                IconClass = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                SortOrder = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PublicPageSectionItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_PublicPageSectionItems_PublicPageSections_PublicPageSectionId",
                    column: x => x.PublicPageSectionId,
                    principalTable: "PublicPageSections",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PublicPageSectionTranslations",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PublicPageSectionId = table.Column<int>(type: "int", nullable: false),
                Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                Eyebrow = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                Title = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                Subtitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                SupportingCopy = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PublicPageSectionTranslations", x => x.Id);
                table.ForeignKey(
                    name: "FK_PublicPageSectionTranslations_PublicPageSections_PublicPageSectionId",
                    column: x => x.PublicPageSectionId,
                    principalTable: "PublicPageSections",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_PublicPageSectionItems_PublicPageSectionId", table: "PublicPageSectionItems", column: "PublicPageSectionId");
        migrationBuilder.CreateIndex(name: "IX_PublicPageSectionItems_SortOrder", table: "PublicPageSectionItems", column: "SortOrder");
        migrationBuilder.CreateIndex(name: "IX_PublicPageSections_IsActive", table: "PublicPageSections", column: "IsActive");
        migrationBuilder.CreateIndex(name: "IX_PublicPageSections_LinkedBlogPostId", table: "PublicPageSections", column: "LinkedBlogPostId");
        migrationBuilder.CreateIndex(name: "IX_PublicPageSections_LinkedTourCategoryId", table: "PublicPageSections", column: "LinkedTourCategoryId");
        migrationBuilder.CreateIndex(name: "IX_PublicPageSections_LinkedTourId", table: "PublicPageSections", column: "LinkedTourId");
        migrationBuilder.CreateIndex(name: "IX_PublicPageSections_PageKey", table: "PublicPageSections", column: "PageKey");
        migrationBuilder.CreateIndex(name: "IX_PublicPageSections_PageKey_SectionKey", table: "PublicPageSections", columns: new[] { "PageKey", "SectionKey" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_PublicPageSections_SectionKey", table: "PublicPageSections", column: "SectionKey");
        migrationBuilder.CreateIndex(name: "IX_PublicPageSections_SortOrder", table: "PublicPageSections", column: "SortOrder");
        migrationBuilder.CreateIndex(name: "IX_PublicPageSectionTranslations_Language", table: "PublicPageSectionTranslations", column: "Language");
        migrationBuilder.CreateIndex(name: "IX_PublicPageSectionTranslations_PublicPageSectionId", table: "PublicPageSectionTranslations", column: "PublicPageSectionId");
        migrationBuilder.CreateIndex(name: "IX_PublicPageSectionTranslations_PublicPageSectionId_Language", table: "PublicPageSectionTranslations", columns: new[] { "PublicPageSectionId", "Language" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PublicPageSectionItems");
        migrationBuilder.DropTable(name: "PublicPageSectionTranslations");
        migrationBuilder.DropTable(name: "PublicPageSections");
    }
}
