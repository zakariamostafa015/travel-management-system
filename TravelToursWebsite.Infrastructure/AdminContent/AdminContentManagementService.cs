using Microsoft.EntityFrameworkCore;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.AdminContent;
using TravelToursWebsite.Application.Features.Blog;
using TravelToursWebsite.Application.Features.Tours;
using TravelToursWebsite.Domain.Entities;
using TravelToursWebsite.Infrastructure.Persistence;
using BlogCategoryTranslationRequest = TravelToursWebsite.Application.Features.Blog.BlogCategoryTranslationRequest;
using BlogPostTranslationRequest = TravelToursWebsite.Application.Features.Blog.BlogPostTranslationRequest;
using TourCategoryTranslationRequest = TravelToursWebsite.Application.Features.Tours.TourCategoryTranslationRequest;
using TourTranslationRequest = TravelToursWebsite.Application.Features.Tours.TourTranslationRequest;

namespace TravelToursWebsite.Infrastructure.AdminContent;

public sealed class AdminContentManagementService(ApplicationDbContext context)
    : ITourManagementService, IBlogManagementService, IAdminTourContentService, IAdminBlogContentService
{
    public async Task<OperationResult<TourDetailsDto>> CreateTourAsync(CreateTourRequest request, CancellationToken cancellationToken = default)
    {
        if (!await context.TourCategories.AnyAsync(category => category.Id == request.CategoryId, cancellationToken))
        {
            return OperationResult<TourDetailsDto>.Failure("Tour category was not found.");
        }

        var tour = new Tour
        {
            Price = request.Price,
            Duration = request.Duration,
            IsPackage = request.IsPackage,
            DurationText = NormalizeOptional(request.DurationText),
            CategoryId = request.CategoryId,
            FeaturedImagePath = NormalizeOptional(request.FeaturedImagePath),
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured,
            SortOrder = request.SortOrder,
            CreatedDate = DateTime.UtcNow,
            Translations = request.Translations.Select(ToTourTranslation).ToList()
        };

        context.Tours.Add(tour);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<TourDetailsDto>.Success((await LoadTourAsync(tour.Id, cancellationToken))!.ToDetailsDto(), "Tour created.");
    }

    public async Task<OperationResult<TourDetailsDto>> UpdateTourAsync(UpdateTourRequest request, CancellationToken cancellationToken = default)
    {
        var tour = await context.Tours
            .Include(item => item.Translations)
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (tour is null)
        {
            return OperationResult<TourDetailsDto>.Failure("Tour was not found.");
        }

        if (!await context.TourCategories.AnyAsync(category => category.Id == request.CategoryId, cancellationToken))
        {
            return OperationResult<TourDetailsDto>.Failure("Tour category was not found.");
        }

        tour.Price = request.Price;
        tour.Duration = request.Duration;
        tour.IsPackage = request.IsPackage;
        tour.DurationText = NormalizeOptional(request.DurationText);
        tour.CategoryId = request.CategoryId;
        tour.FeaturedImagePath = NormalizeOptional(request.FeaturedImagePath);
        tour.IsActive = request.IsActive;
        tour.IsFeatured = request.IsFeatured;
        tour.SortOrder = request.SortOrder;
        tour.UpdatedDate = DateTime.UtcNow;
        UpsertTourTranslations(tour.Translations, request.Translations);

        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<TourDetailsDto>.Success((await LoadTourAsync(tour.Id, cancellationToken))!.ToDetailsDto(), "Tour updated.");
    }

    public async Task<OperationResult> DeleteTourAsync(int id, CancellationToken cancellationToken = default)
    {
        var tour = await context.Tours.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (tour is null)
        {
            return OperationResult.Failure("Tour was not found.");
        }

        context.Tours.Remove(tour);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Tour deleted.");
    }

    public async Task<OperationResult<TourCategoryDto>> CreateCategoryAsync(CreateTourCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = new TourCategory
        {
            IconClass = NormalizeOptional(request.IconClass),
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            CreatedDate = DateTime.UtcNow,
            Translations = request.Translations.Select(ToTourCategoryTranslation).ToList()
        };

        context.TourCategories.Add(category);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<TourCategoryDto>.Success((await LoadTourCategoryAsync(category.Id, cancellationToken))!.ToDto(), "Tour category created.");
    }

    public async Task<OperationResult<TourCategoryDto>> UpdateCategoryAsync(UpdateTourCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await context.TourCategories
            .Include(item => item.Translations)
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (category is null)
        {
            return OperationResult<TourCategoryDto>.Failure("Tour category was not found.");
        }

        category.IconClass = NormalizeOptional(request.IconClass);
        category.IsActive = request.IsActive;
        category.SortOrder = request.SortOrder;
        UpsertTourCategoryTranslations(category.Translations, request.Translations);
        await context.SaveChangesAsync(cancellationToken);

        return OperationResult<TourCategoryDto>.Success((await LoadTourCategoryAsync(category.Id, cancellationToken))!.ToDto(), "Tour category updated.");
    }

    public async Task<OperationResult> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await context.TourCategories.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (category is null)
        {
            return OperationResult.Failure("Tour category was not found.");
        }

        if (await context.Tours.AnyAsync(tour => tour.CategoryId == id, cancellationToken))
        {
            return OperationResult.Failure("Tour category cannot be deleted while tours reference it.");
        }

        context.TourCategories.Remove(category);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Tour category deleted.");
    }

    public async Task<OperationResult<TourImageDto>> AddTourImageAsync(TourImageRequest request, CancellationToken cancellationToken = default)
    {
        if (!await context.Tours.AnyAsync(tour => tour.Id == request.TourId, cancellationToken))
        {
            return OperationResult<TourImageDto>.Failure("Tour was not found.");
        }

        if (request.IsMainImage)
        {
            await ClearMainTourImagesAsync(request.TourId, cancellationToken);
        }

        var image = new TourImage
        {
            TourId = request.TourId,
            ImagePath = request.ImagePath,
            ImageUrl = NormalizeOptional(request.ImageUrl),
            ImageLocalPath = NormalizeOptional(request.ImageLocalPath),
            ThumbnailPath = NormalizeOptional(request.ThumbnailPath),
            MediumPath = NormalizeOptional(request.MediumPath),
            AltText = NormalizeOptional(request.AltText),
            Caption = NormalizeOptional(request.Caption),
            SortOrder = request.SortOrder,
            IsMainImage = request.IsMainImage,
            CreatedDate = DateTime.UtcNow
        };

        context.TourImages.Add(image);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<TourImageDto>.Success(image.ToDto(), "Tour image added.");
    }

    public async Task<OperationResult<TourImageDto>> UpdateTourImageAsync(UpdateTourImageRequest request, CancellationToken cancellationToken = default)
    {
        var image = await context.TourImages.FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
        if (image is null)
        {
            return OperationResult<TourImageDto>.Failure("Tour image was not found.");
        }

        if (request.IsMainImage)
        {
            await ClearMainTourImagesAsync(image.TourId, cancellationToken, image.Id);
        }

        image.ImagePath = request.ImagePath;
        image.ImageUrl = NormalizeOptional(request.ImageUrl);
        image.ImageLocalPath = NormalizeOptional(request.ImageLocalPath);
        image.ThumbnailPath = NormalizeOptional(request.ThumbnailPath);
        image.MediumPath = NormalizeOptional(request.MediumPath);
        image.AltText = NormalizeOptional(request.AltText);
        image.Caption = NormalizeOptional(request.Caption);
        image.SortOrder = request.SortOrder;
        image.IsMainImage = request.IsMainImage;
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<TourImageDto>.Success(image.ToDto(), "Tour image updated.");
    }

    public async Task<OperationResult> DeleteTourImageAsync(int id, CancellationToken cancellationToken = default)
    {
        var image = await context.TourImages.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (image is null)
        {
            return OperationResult.Failure("Tour image was not found.");
        }

        context.TourImages.Remove(image);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Tour image deleted.");
    }

    public async Task<OperationResult<TourItineraryDto>> UpsertItineraryAsync(UpsertTourItineraryRequest request, CancellationToken cancellationToken = default)
    {
        if (!await context.Tours.AnyAsync(tour => tour.Id == request.TourId, cancellationToken))
        {
            return OperationResult<TourItineraryDto>.Failure("Tour was not found.");
        }

        TourItinerary itinerary;
        if (request.Id.HasValue)
        {
            var existingItinerary = await context.TourItineraries
                .Include(item => item.Translations)
                .FirstOrDefaultAsync(item => item.Id == request.Id.Value, cancellationToken);
            if (existingItinerary is null)
            {
                return OperationResult<TourItineraryDto>.Failure("Tour itinerary was not found.");
            }

            itinerary = existingItinerary;
        }
        else
        {
            itinerary = new TourItinerary { TourId = request.TourId, CreatedDate = DateTime.UtcNow };
            context.TourItineraries.Add(itinerary);
        }

        itinerary.TourId = request.TourId;
        itinerary.Day = request.Day;
        itinerary.SortOrder = request.SortOrder;
        UpsertItineraryTranslations(itinerary.Translations, request.Translations);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<TourItineraryDto>.Success(itinerary.ToDto(), request.Id.HasValue ? "Itinerary updated." : "Itinerary created.");
    }

    public async Task<OperationResult> DeleteItineraryAsync(int id, CancellationToken cancellationToken = default)
    {
        var itinerary = await context.TourItineraries.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (itinerary is null)
        {
            return OperationResult.Failure("Tour itinerary was not found.");
        }

        context.TourItineraries.Remove(itinerary);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Tour itinerary deleted.");
    }

    public async Task<OperationResult<TourSpotDto>> UpsertSpotAsync(UpsertTourSpotRequest request, CancellationToken cancellationToken = default)
    {
        if (!await context.Tours.AnyAsync(tour => tour.Id == request.TourId, cancellationToken))
        {
            return OperationResult<TourSpotDto>.Failure("Tour was not found.");
        }

        TourSpot spot;
        if (request.Id.HasValue)
        {
            var existingSpot = await context.TourSpots.FirstOrDefaultAsync(item => item.Id == request.Id.Value, cancellationToken);
            if (existingSpot is null)
            {
                return OperationResult<TourSpotDto>.Failure("Tour spot was not found.");
            }

            spot = existingSpot;
            spot.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            spot = new TourSpot { TourId = request.TourId, CreatedDate = DateTime.UtcNow };
            context.TourSpots.Add(spot);
        }

        spot.TourId = request.TourId;
        spot.Latitude = request.Latitude;
        spot.Longitude = request.Longitude;
        spot.Order = request.Order;
        spot.Name = NormalizeOptional(request.Name);
        spot.Description = NormalizeOptional(request.Description);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<TourSpotDto>.Success(spot.ToDto(), request.Id.HasValue ? "Tour spot updated." : "Tour spot created.");
    }

    public async Task<OperationResult> DeleteSpotAsync(int id, CancellationToken cancellationToken = default)
    {
        var spot = await context.TourSpots.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (spot is null)
        {
            return OperationResult.Failure("Tour spot was not found.");
        }

        context.TourSpots.Remove(spot);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Tour spot deleted.");
    }

    public async Task<OperationResult<BlogPostDetailsDto>> CreatePostAsync(CreateBlogPostRequest request, CancellationToken cancellationToken = default)
    {
        if (!await context.BlogCategories.AnyAsync(category => category.Id == request.CategoryId, cancellationToken))
        {
            return OperationResult<BlogPostDetailsDto>.Failure("Blog category was not found.");
        }

        if (!await context.Users.AnyAsync(user => user.Id == request.AuthorId, cancellationToken))
        {
            return OperationResult<BlogPostDetailsDto>.Failure("Author was not found.");
        }

        var post = new BlogPost
        {
            FeaturedImagePath = NormalizeOptional(request.FeaturedImagePath),
            CategoryId = request.CategoryId,
            AuthorId = request.AuthorId,
            IsPublished = request.IsPublished,
            IsFeatured = request.IsFeatured,
            IsEvent = request.IsEvent,
            PublishedDate = request.PublishedDate,
            CreatedDate = DateTime.UtcNow,
            Translations = request.Translations.Select(ToBlogPostTranslation).ToList()
        };

        context.BlogPosts.Add(post);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<BlogPostDetailsDto>.Success((await LoadBlogPostAsync(post.Id, cancellationToken))!.ToDetailsDto(), "Blog post created.");
    }

    public async Task<OperationResult<BlogPostDetailsDto>> UpdatePostAsync(UpdateBlogPostRequest request, CancellationToken cancellationToken = default)
    {
        var post = await context.BlogPosts
            .Include(item => item.Translations)
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (post is null)
        {
            return OperationResult<BlogPostDetailsDto>.Failure("Blog post was not found.");
        }

        if (!await context.BlogCategories.AnyAsync(category => category.Id == request.CategoryId, cancellationToken))
        {
            return OperationResult<BlogPostDetailsDto>.Failure("Blog category was not found.");
        }

        if (!await context.Users.AnyAsync(user => user.Id == request.AuthorId, cancellationToken))
        {
            return OperationResult<BlogPostDetailsDto>.Failure("Author was not found.");
        }

        post.FeaturedImagePath = NormalizeOptional(request.FeaturedImagePath);
        post.CategoryId = request.CategoryId;
        post.AuthorId = request.AuthorId;
        post.IsPublished = request.IsPublished;
        post.IsFeatured = request.IsFeatured;
        post.IsEvent = request.IsEvent;
        post.PublishedDate = request.PublishedDate;
        post.UpdatedDate = DateTime.UtcNow;
        UpsertBlogPostTranslations(post.Translations, request.Translations);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<BlogPostDetailsDto>.Success((await LoadBlogPostAsync(post.Id, cancellationToken))!.ToDetailsDto(), "Blog post updated.");
    }

    public async Task<OperationResult> DeletePostAsync(int id, CancellationToken cancellationToken = default)
    {
        var post = await context.BlogPosts.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (post is null)
        {
            return OperationResult.Failure("Blog post was not found.");
        }

        context.BlogPosts.Remove(post);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Blog post deleted.");
    }

    public async Task<OperationResult<BlogCategoryDto>> CreateCategoryAsync(CreateBlogCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = new BlogCategory
        {
            IconClass = NormalizeOptional(request.IconClass),
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            CreatedDate = DateTime.UtcNow,
            Translations = request.Translations.Select(ToBlogCategoryTranslation).ToList()
        };

        context.BlogCategories.Add(category);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<BlogCategoryDto>.Success((await LoadBlogCategoryAsync(category.Id, cancellationToken))!.ToDto(), "Blog category created.");
    }

    public async Task<OperationResult<BlogCategoryDto>> UpdateCategoryAsync(UpdateBlogCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await context.BlogCategories
            .Include(item => item.Translations)
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (category is null)
        {
            return OperationResult<BlogCategoryDto>.Failure("Blog category was not found.");
        }

        category.IconClass = NormalizeOptional(request.IconClass);
        category.IsActive = request.IsActive;
        category.SortOrder = request.SortOrder;
        UpsertBlogCategoryTranslations(category.Translations, request.Translations);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<BlogCategoryDto>.Success((await LoadBlogCategoryAsync(category.Id, cancellationToken))!.ToDto(), "Blog category updated.");
    }

    async Task<OperationResult> IBlogManagementService.DeleteCategoryAsync(int id, CancellationToken cancellationToken) {
        var category = await context.BlogCategories.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (category is null)
        {
            return OperationResult.Failure("Blog category was not found.");
        }

        if (await context.BlogPosts.AnyAsync(post => post.CategoryId == id, cancellationToken))
        {
            return OperationResult.Failure("Blog category cannot be deleted while posts reference it.");
        }

        context.BlogCategories.Remove(category);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Blog category deleted.");
    }

    public async Task IncrementViewCountAsync(int postId, CancellationToken cancellationToken = default)
    {
        var post = await context.BlogPosts.FirstOrDefaultAsync(item => item.Id == postId, cancellationToken);
        if (post is null)
        {
            return;
        }

        post.ViewCount++;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<OperationResult<BlogImageDto>> AddBlogImageAsync(BlogImageRequest request, CancellationToken cancellationToken = default)
    {
        if (!await context.BlogPosts.AnyAsync(post => post.Id == request.BlogPostId, cancellationToken))
        {
            return OperationResult<BlogImageDto>.Failure("Blog post was not found.");
        }

        var image = new BlogImage
        {
            BlogPostId = request.BlogPostId,
            ImagePath = request.ImagePath,
            ImageUrl = NormalizeOptional(request.ImageUrl),
            ImageLocalPath = NormalizeOptional(request.ImageLocalPath),
            ThumbnailPath = NormalizeOptional(request.ThumbnailPath),
            MediumPath = NormalizeOptional(request.MediumPath),
            AltText = NormalizeOptional(request.AltText),
            Caption = NormalizeOptional(request.Caption),
            SortOrder = request.SortOrder,
            CreatedDate = DateTime.UtcNow
        };

        context.BlogImages.Add(image);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<BlogImageDto>.Success(image.ToDto(), "Blog image added.");
    }

    public async Task<OperationResult<BlogImageDto>> UpdateBlogImageAsync(UpdateBlogImageRequest request, CancellationToken cancellationToken = default)
    {
        var image = await context.BlogImages.FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
        if (image is null)
        {
            return OperationResult<BlogImageDto>.Failure("Blog image was not found.");
        }

        image.ImagePath = request.ImagePath;
        image.ImageUrl = NormalizeOptional(request.ImageUrl);
        image.ImageLocalPath = NormalizeOptional(request.ImageLocalPath);
        image.ThumbnailPath = NormalizeOptional(request.ThumbnailPath);
        image.MediumPath = NormalizeOptional(request.MediumPath);
        image.AltText = NormalizeOptional(request.AltText);
        image.Caption = NormalizeOptional(request.Caption);
        image.SortOrder = request.SortOrder;
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<BlogImageDto>.Success(image.ToDto(), "Blog image updated.");
    }

    public async Task<OperationResult> DeleteBlogImageAsync(int id, CancellationToken cancellationToken = default)
    {
        var image = await context.BlogImages.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (image is null)
        {
            return OperationResult.Failure("Blog image was not found.");
        }

        context.BlogImages.Remove(image);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Blog image deleted.");
    }

    public async Task<IReadOnlyList<TourTranslationDto>> GetTourTranslationsAsync(int tourId, CancellationToken cancellationToken = default)
    {
        return await context.TourTranslations
            .AsNoTracking()
            .Where(item => item.TourId == tourId)
            .OrderBy(item => item.Language)
            .Select(item => ToDto(item))
            .ToListAsync(cancellationToken);
    }

    public async Task<OperationResult<TourTranslationDto>> UpsertTourTranslationAsync(UpsertTourTranslationRequest request, CancellationToken cancellationToken = default)
    {
        if (!await context.Tours.AnyAsync(item => item.Id == request.TourId, cancellationToken))
        {
            return OperationResult<TourTranslationDto>.Failure("Tour was not found.");
        }

        var translation = await context.TourTranslations
            .FirstOrDefaultAsync(item => item.TourId == request.TourId && item.Language == request.Translation.Language, cancellationToken);

        if (translation is null)
        {
            translation = ToTourTranslation(request.Translation);
            translation.TourId = request.TourId;
            context.TourTranslations.Add(translation);
        }
        else
        {
            Apply(translation, request.Translation);
        }

        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<TourTranslationDto>.Success(ToDto(translation), "Tour translation saved.");
    }

    public async Task<IReadOnlyList<TourCategoryTranslationDto>> GetTourCategoryTranslationsAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await context.TourCategoryTranslations
            .AsNoTracking()
            .Where(item => item.TourCategoryId == categoryId)
            .OrderBy(item => item.Language)
            .Select(item => ToDto(item))
            .ToListAsync(cancellationToken);
    }

    public async Task<OperationResult<TourCategoryTranslationDto>> UpsertTourCategoryTranslationAsync(UpsertTourCategoryTranslationRequest request, CancellationToken cancellationToken = default)
    {
        if (!await context.TourCategories.AnyAsync(item => item.Id == request.TourCategoryId, cancellationToken))
        {
            return OperationResult<TourCategoryTranslationDto>.Failure("Tour category was not found.");
        }

        var translation = await context.TourCategoryTranslations
            .FirstOrDefaultAsync(item => item.TourCategoryId == request.TourCategoryId && item.Language == request.Translation.Language, cancellationToken);

        if (translation is null)
        {
            translation = ToTourCategoryTranslation(request.Translation);
            translation.TourCategoryId = request.TourCategoryId;
            context.TourCategoryTranslations.Add(translation);
        }
        else
        {
            Apply(translation, request.Translation);
        }

        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<TourCategoryTranslationDto>.Success(ToDto(translation), "Tour category translation saved.");
    }

    public async Task<IReadOnlyList<TourItineraryTranslationDto>> GetItineraryTranslationsAsync(int itineraryId, CancellationToken cancellationToken = default)
    {
        return await context.TourItineraryTranslations
            .AsNoTracking()
            .Where(item => item.TourItineraryId == itineraryId)
            .OrderBy(item => item.Language)
            .Select(item => ToDto(item))
            .ToListAsync(cancellationToken);
    }

    public async Task<OperationResult<TourItineraryTranslationDto>> UpsertItineraryTranslationAsync(UpsertTourItineraryTranslationRequest request, CancellationToken cancellationToken = default)
    {
        if (!await context.TourItineraries.AnyAsync(item => item.Id == request.TourItineraryId, cancellationToken))
        {
            return OperationResult<TourItineraryTranslationDto>.Failure("Tour itinerary was not found.");
        }

        var translation = await context.TourItineraryTranslations
            .FirstOrDefaultAsync(item => item.TourItineraryId == request.TourItineraryId && item.Language == request.Translation.Language, cancellationToken);

        if (translation is null)
        {
            translation = ToItineraryTranslation(request.Translation);
            translation.TourItineraryId = request.TourItineraryId;
            context.TourItineraryTranslations.Add(translation);
        }
        else
        {
            Apply(translation, request.Translation);
        }

        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<TourItineraryTranslationDto>.Success(ToDto(translation), "Itinerary translation saved.");
    }

    public async Task<IReadOnlyList<BlogPostTranslationDto>> GetBlogPostTranslationsAsync(int postId, CancellationToken cancellationToken = default)
    {
        return await context.BlogPostTranslations
            .AsNoTracking()
            .Where(item => item.BlogPostId == postId)
            .OrderBy(item => item.Language)
            .Select(item => ToDto(item))
            .ToListAsync(cancellationToken);
    }

    public async Task<OperationResult<BlogPostTranslationDto>> UpsertBlogPostTranslationAsync(UpsertBlogPostTranslationRequest request, CancellationToken cancellationToken = default)
    {
        if (!await context.BlogPosts.AnyAsync(item => item.Id == request.BlogPostId, cancellationToken))
        {
            return OperationResult<BlogPostTranslationDto>.Failure("Blog post was not found.");
        }

        var translation = await context.BlogPostTranslations
            .FirstOrDefaultAsync(item => item.BlogPostId == request.BlogPostId && item.Language == request.Translation.Language, cancellationToken);

        if (translation is null)
        {
            translation = ToBlogPostTranslation(request.Translation);
            translation.BlogPostId = request.BlogPostId;
            context.BlogPostTranslations.Add(translation);
        }
        else
        {
            Apply(translation, request.Translation);
        }

        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<BlogPostTranslationDto>.Success(ToDto(translation), "Blog post translation saved.");
    }

    public async Task<IReadOnlyList<BlogCategoryTranslationDto>> GetBlogCategoryTranslationsAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await context.BlogCategoryTranslations
            .AsNoTracking()
            .Where(item => item.BlogCategoryId == categoryId)
            .OrderBy(item => item.Language)
            .Select(item => ToDto(item))
            .ToListAsync(cancellationToken);
    }

    public async Task<OperationResult<BlogCategoryTranslationDto>> UpsertBlogCategoryTranslationAsync(UpsertBlogCategoryTranslationRequest request, CancellationToken cancellationToken = default)
    {
        if (!await context.BlogCategories.AnyAsync(item => item.Id == request.BlogCategoryId, cancellationToken))
        {
            return OperationResult<BlogCategoryTranslationDto>.Failure("Blog category was not found.");
        }

        var translation = await context.BlogCategoryTranslations
            .FirstOrDefaultAsync(item => item.BlogCategoryId == request.BlogCategoryId && item.Language == request.Translation.Language, cancellationToken);

        if (translation is null)
        {
            translation = ToBlogCategoryTranslation(request.Translation);
            translation.BlogCategoryId = request.BlogCategoryId;
            context.BlogCategoryTranslations.Add(translation);
        }
        else
        {
            Apply(translation, request.Translation);
        }

        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<BlogCategoryTranslationDto>.Success(ToDto(translation), "Blog category translation saved.");
    }

    private async Task<Tour?> LoadTourAsync(int id, CancellationToken cancellationToken)
    {
        return await context.Tours
            .AsNoTracking()
            .Include(item => item.Category)
                .ThenInclude(category => category!.Translations)
            .Include(item => item.Images)
            .Include(item => item.Itineraries)
                .ThenInclude(itinerary => itinerary.Translations)
            .Include(item => item.Spots)
            .Include(item => item.Translations)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    private async Task<TourCategory?> LoadTourCategoryAsync(int id, CancellationToken cancellationToken)
    {
        return await context.TourCategories
            .AsNoTracking()
            .Include(item => item.Translations)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    private async Task<BlogPost?> LoadBlogPostAsync(int id, CancellationToken cancellationToken)
    {
        return await context.BlogPosts
            .AsNoTracking()
            .Include(item => item.Category)
                .ThenInclude(category => category!.Translations)
            .Include(item => item.Author)
            .Include(item => item.Images)
            .Include(item => item.Translations)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    private async Task<BlogCategory?> LoadBlogCategoryAsync(int id, CancellationToken cancellationToken)
    {
        return await context.BlogCategories
            .AsNoTracking()
            .Include(item => item.Translations)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    private async Task ClearMainTourImagesAsync(int tourId, CancellationToken cancellationToken, int? exceptImageId = null)
    {
        var images = await context.TourImages
            .Where(image => image.TourId == tourId && image.IsMainImage && (!exceptImageId.HasValue || image.Id != exceptImageId.Value))
            .ToListAsync(cancellationToken);

        foreach (var image in images)
        {
            image.IsMainImage = false;
        }
    }

    private static void UpsertTourTranslations(ICollection<TourTranslation> existing, IEnumerable<TourTranslationRequest> requests)
    {
        foreach (var request in requests)
        {
            var translation = existing.FirstOrDefault(item => item.Language == request.Language);
            if (translation is null)
            {
                existing.Add(ToTourTranslation(request));
                continue;
            }

            Apply(translation, request);
        }
    }

    private static void UpsertTourCategoryTranslations(ICollection<TourCategoryTranslation> existing, IEnumerable<TourCategoryTranslationRequest> requests)
    {
        foreach (var request in requests)
        {
            var translation = existing.FirstOrDefault(item => item.Language == request.Language);
            if (translation is null)
            {
                existing.Add(ToTourCategoryTranslation(request));
                continue;
            }

            Apply(translation, request);
        }
    }

    private static void UpsertItineraryTranslations(ICollection<TourItineraryTranslation> existing, IEnumerable<TourItineraryTranslationRequest> requests)
    {
        foreach (var request in requests)
        {
            var translation = existing.FirstOrDefault(item => item.Language == request.Language);
            if (translation is null)
            {
                existing.Add(ToItineraryTranslation(request));
                continue;
            }

            Apply(translation, request);
        }
    }

    private static void UpsertBlogPostTranslations(ICollection<BlogPostTranslation> existing, IEnumerable<BlogPostTranslationRequest> requests)
    {
        foreach (var request in requests)
        {
            var translation = existing.FirstOrDefault(item => item.Language == request.Language);
            if (translation is null)
            {
                existing.Add(ToBlogPostTranslation(request));
                continue;
            }

            Apply(translation, request);
        }
    }

    private static void UpsertBlogCategoryTranslations(ICollection<BlogCategoryTranslation> existing, IEnumerable<BlogCategoryTranslationRequest> requests)
    {
        foreach (var request in requests)
        {
            var translation = existing.FirstOrDefault(item => item.Language == request.Language);
            if (translation is null)
            {
                existing.Add(ToBlogCategoryTranslation(request));
                continue;
            }

            Apply(translation, request);
        }
    }

    private static TourTranslation ToTourTranslation(TourTranslationRequest request)
    {
        var translation = new TourTranslation();
        Apply(translation, request);
        return translation;
    }

    private static TourCategoryTranslation ToTourCategoryTranslation(TourCategoryTranslationRequest request)
    {
        var translation = new TourCategoryTranslation();
        Apply(translation, request);
        return translation;
    }

    private static TourItineraryTranslation ToItineraryTranslation(TourItineraryTranslationRequest request)
    {
        var translation = new TourItineraryTranslation();
        Apply(translation, request);
        return translation;
    }

    private static BlogPostTranslation ToBlogPostTranslation(BlogPostTranslationRequest request)
    {
        var translation = new BlogPostTranslation();
        Apply(translation, request);
        return translation;
    }

    private static BlogCategoryTranslation ToBlogCategoryTranslation(BlogCategoryTranslationRequest request)
    {
        var translation = new BlogCategoryTranslation();
        Apply(translation, request);
        return translation;
    }

    private static void Apply(TourTranslation translation, TourTranslationRequest request)
    {
        translation.Language = NormalizeLanguage(request.Language);
        translation.Title = request.Title.Trim();
        translation.ShortDescription = request.ShortDescription.Trim();
        translation.Description = request.Description.Trim();
        translation.Slug = NormalizeOptional(request.Slug);
        translation.MetaDescription = NormalizeOptional(request.MetaDescription);
        translation.MetaKeywords = NormalizeOptional(request.MetaKeywords);
        translation.DurationUnit = NormalizeOptional(request.DurationUnit);
        translation.ActivityHighlights = NormalizeOptional(request.ActivityHighlights);
    }

    private static void Apply(TourCategoryTranslation translation, TourCategoryTranslationRequest request)
    {
        translation.Language = NormalizeLanguage(request.Language);
        translation.Name = request.Name.Trim();
        translation.Description = NormalizeOptional(request.Description);
        translation.Slug = NormalizeOptional(request.Slug);
    }

    private static void Apply(TourItineraryTranslation translation, TourItineraryTranslationRequest request)
    {
        translation.Language = NormalizeLanguage(request.Language);
        translation.Title = request.Title.Trim();
        translation.Description = request.Description.Trim();
        translation.Location = NormalizeOptional(request.Location);
        translation.Accommodation = NormalizeOptional(request.Accommodation);
        translation.Meals = NormalizeOptional(request.Meals);
    }

    private static void Apply(BlogPostTranslation translation, BlogPostTranslationRequest request)
    {
        translation.Language = NormalizeLanguage(request.Language);
        translation.Title = request.Title.Trim();
        translation.Excerpt = request.Excerpt.Trim();
        translation.Content = request.Content.Trim();
        translation.Slug = NormalizeOptional(request.Slug);
        translation.MetaDescription = NormalizeOptional(request.MetaDescription);
        translation.MetaKeywords = NormalizeOptional(request.MetaKeywords);
    }

    private static void Apply(BlogCategoryTranslation translation, BlogCategoryTranslationRequest request)
    {
        translation.Language = NormalizeLanguage(request.Language);
        translation.Name = request.Name.Trim();
        translation.Description = NormalizeOptional(request.Description);
        translation.Slug = NormalizeOptional(request.Slug);
    }

    private static TourTranslationDto ToDto(TourTranslation translation)
    {
        return new TourTranslationDto(translation.Id, translation.TourId, translation.Language, translation.Title, translation.ShortDescription, translation.Description, translation.Slug, translation.MetaDescription, translation.MetaKeywords, translation.DurationUnit, translation.ActivityHighlights);
    }

    private static TourCategoryTranslationDto ToDto(TourCategoryTranslation translation)
    {
        return new TourCategoryTranslationDto(translation.Id, translation.TourCategoryId, translation.Language, translation.Name, translation.Description, translation.Slug);
    }

    private static TourItineraryTranslationDto ToDto(TourItineraryTranslation translation)
    {
        return new TourItineraryTranslationDto(translation.Id, translation.TourItineraryId, translation.Language, translation.Title, translation.Description, translation.Location, translation.Accommodation, translation.Meals);
    }

    private static BlogPostTranslationDto ToDto(BlogPostTranslation translation)
    {
        return new BlogPostTranslationDto(translation.Id, translation.BlogPostId, translation.Language, translation.Title, translation.Excerpt, translation.Content, translation.Slug, translation.MetaDescription, translation.MetaKeywords);
    }

    private static BlogCategoryTranslationDto ToDto(BlogCategoryTranslation translation)
    {
        return new BlogCategoryTranslationDto(translation.Id, translation.BlogCategoryId, translation.Language, translation.Name, translation.Description, translation.Slug);
    }

    private static string NormalizeLanguage(string language)
    {
        return language.Trim().ToLowerInvariant();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
