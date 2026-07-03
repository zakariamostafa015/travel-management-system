using Microsoft.EntityFrameworkCore;
using TravelToursWebsite.Application.Features.Auth;
using TravelToursWebsite.Domain.Entities;
using TravelToursWebsite.Domain.Enums;

namespace TravelToursWebsite.Infrastructure.Persistence;

public sealed record AdminSeedOptions(
    string Username,
    string Email,
    string Password,
    bool UpdatePassword = false);

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedLanguagesAsync(context, cancellationToken);
        await SeedSiteSettingsAsync(context, cancellationToken);
    }

    public static async Task SeedAsync(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        AdminSeedOptions? adminOptions,
        CancellationToken cancellationToken = default)
    {
        await SeedAsync(context, cancellationToken);
        await SeedAdminAsync(context, passwordHasher, adminOptions, cancellationToken);
    }


    private static async Task SeedAdminAsync(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        AdminSeedOptions? adminOptions,
        CancellationToken cancellationToken)
    {
        if (adminOptions is null
            || string.IsNullOrWhiteSpace(adminOptions.Username)
            || string.IsNullOrWhiteSpace(adminOptions.Email)
            || string.IsNullOrWhiteSpace(adminOptions.Password))
        {
            return;
        }

        var username = adminOptions.Username.Trim();
        var email = adminOptions.Email.Trim();
        var existingAdmin = await context.Users
            .FirstOrDefaultAsync(user => user.Username == username, cancellationToken);

        if (existingAdmin is not null)
        {
            existingAdmin.Email = email;
            existingAdmin.Role = UserRole.Admin;
            existingAdmin.IsActive = true;
            existingAdmin.EmailConfirmed = true;

            if (adminOptions.UpdatePassword)
            {
                existingAdmin.PasswordHash = passwordHasher.HashPassword(adminOptions.Password);
            }

            await context.SaveChangesAsync(cancellationToken);
            return;
        }

        var emailInUse = await context.Users.AnyAsync(user => user.Email == email, cancellationToken);
        if (emailInUse)
        {
            return;
        }

        context.Users.Add(new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHasher.HashPassword(adminOptions.Password),
            FirstName = "System",
            LastName = "Administrator",
            Bio = "Seeded administrator account.",
            Role = UserRole.Admin,
            IsActive = true,
            EmailConfirmed = true,
            CreatedDate = DateTime.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedLanguagesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Languages.AnyAsync(cancellationToken))
        {
            return;
        }

        context.Languages.AddRange(
            new Language
            {
                Code = "en",
                CultureCode = "en-US",
                Name = "English",
                NativeName = "English",
                IsActive = true,
                IsDefault = true,
                SortOrder = 0
            },
            new Language
            {
                Code = "it",
                CultureCode = "it-IT",
                Name = "Italian",
                NativeName = "Italiano",
                IsActive = true,
                SortOrder = 1
            },
            new Language
            {
                Code = "de",
                CultureCode = "de-DE",
                Name = "German",
                NativeName = "Deutsch",
                IsActive = true,
                SortOrder = 2
            });

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedSiteSettingsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var settings = new[]
        {
            new SiteSettings
            {
                Key = "Logo.Main",
                Value = "/uploads/default/logo.png",
                Description = "Main website logo",
                Category = "Logo",
                Type = SettingType.Url,
                SortOrder = 1,
                IsActive = true
            },
            new SiteSettings
            {
                Key = "HeroImage.Home",
                Value = "https://firebasestorage.googleapis.com/v0/b/editor-7bae0.appspot.com/o/viaitalia%2F1739464498614_samuelferrarauNvgvo2cs7kunsplash.jpg?alt=media&token=3f1674eb-6035-4b41-8930-d53799702b95",
                Description = "Home page hero background image",
                Category = "HeroImages",
                Type = SettingType.Url,
                SortOrder = 1,
                IsActive = true
            },
            new SiteSettings
            {
                Key = "HeroImage.About",
                Value = "https://images.unsplash.com/photo-1529260830199-42c24126f198?ixlib=rb-4.0.3&auto=format&fit=crop&w=2070&q=80",
                Description = "About page hero background image",
                Category = "HeroImages",
                Type = SettingType.Url,
                SortOrder = 2,
                IsActive = true
            },
            new SiteSettings
            {
                Key = "HeroImage.Contact",
                Value = "https://images.unsplash.com/photo-1488646953014-85cb44e25828?ixlib=rb-4.0.3&auto=format&fit=crop&w=2070&q=80",
                Description = "Contact page hero background image",
                Category = "HeroImages",
                Type = SettingType.Url,
                SortOrder = 3,
                IsActive = true
            },
            new SiteSettings
            {
                Key = "HeroImage.Quote",
                Value = "https://images.unsplash.com/photo-1488646953014-85cb44e25828?ixlib=rb-4.0.3&auto=format&fit=crop&w=2070&q=80",
                Description = "Quote page hero background image",
                Category = "HeroImages",
                Type = SettingType.Url,
                SortOrder = 4,
                IsActive = true
            },
            new SiteSettings
            {
                Key = "DefaultImage.Photo",
                Value = "/uploads/default/default-photo.jpg",
                Description = "Default photo for tours and blog posts",
                Category = "DefaultImages",
                Type = SettingType.Url,
                SortOrder = 1,
                IsActive = true
            },
            new SiteSettings
            {
                Key = "DefaultImage.User",
                Value = "/uploads/default/default-user.jpg",
                Description = "Default user profile image",
                Category = "DefaultImages",
                Type = SettingType.Url,
                SortOrder = 2,
                IsActive = true
            },
            new SiteSettings
            {
                Key = "SocialMedia.Facebook",
                Value = "#",
                Description = "Facebook page URL",
                Category = "SocialMedia",
                IconClass = "fab fa-facebook-f",
                Type = SettingType.Url,
                SortOrder = 1,
                IsActive = true
            },
            new SiteSettings
            {
                Key = "SocialMedia.LinkedIn",
                Value = "#",
                Description = "LinkedIn page URL",
                Category = "SocialMedia",
                IconClass = "fab fa-linkedin-in",
                Type = SettingType.Url,
                SortOrder = 2,
                IsActive = true
            },
            new SiteSettings
            {
                Key = "SocialMedia.Instagram",
                Value = "https://www.instagram.com/viaitaliatours",
                Description = "Instagram page URL",
                Category = "SocialMedia",
                IconClass = "fab fa-instagram",
                Type = SettingType.Url,
                SortOrder = 3,
                IsActive = true
            },
            new SiteSettings
            {
                Key = "Contact.CompanyName",
                Value = "Viaitalia SRL",
                Description = "Company name",
                Category = "ContactInfo",
                Type = SettingType.Text,
                SortOrder = 1,
                IsActive = true
            },
            new SiteSettings
            {
                Key = "Contact.Phone",
                Value = "+378 0549 902934",
                Description = "Primary phone number",
                Category = "ContactInfo",
                IconClass = "fas fa-phone",
                Type = SettingType.Text,
                SortOrder = 2,
                IsActive = true
            },
            new SiteSettings
            {
                Key = "Contact.Email",
                Value = "office@viaitaliatours.com",
                Description = "Primary email address",
                Category = "ContactInfo",
                IconClass = "fas fa-envelope",
                Type = SettingType.Email,
                SortOrder = 3,
                IsActive = true
            },
            new SiteSettings
            {
                Key = "Contact.Address",
                Value = "Via 3 Settembre 99, Dogana, San Marino, Admiral Point",
                Description = "Company address",
                Category = "ContactInfo",
                IconClass = "fas fa-map-marker-alt",
                Type = SettingType.Text,
                SortOrder = 4,
                IsActive = true
            },
            new SiteSettings
            {
                Key = "Footer.Copyright",
                Value = "Copyright 2024 Viaitalia SRL. All rights reserved.",
                Description = "Copyright text",
                Category = "Footer",
                Type = SettingType.Text,
                SortOrder = 1,
                IsActive = true
            },
            new SiteSettings
            {
                Key = "Footer.PrivacyPolicy",
                Value = "Privacy Policy",
                Description = "Privacy policy link text",
                Category = "Footer",
                Type = SettingType.Text,
                SortOrder = 2,
                IsActive = true
            },
            new SiteSettings
            {
                Key = "Footer.DeveloperLink",
                Value = "https://kreocloud.com",
                Description = "Developer website link",
                Category = "Footer",
                Type = SettingType.Url,
                SortOrder = 3,
                IsActive = true
            }
        };

        foreach (var setting in settings)
        {
            var exists = await context.SiteSettings.AnyAsync(s => s.Key == setting.Key, cancellationToken);
            if (!exists)
            {
                context.SiteSettings.Add(setting);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}