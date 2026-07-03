
using Microsoft.EntityFrameworkCore;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.Administration;
using TravelToursWebsite.Application.Features.Auth;
using TravelToursWebsite.Domain.Entities;
using TravelToursWebsite.Infrastructure.Persistence;

namespace TravelToursWebsite.Infrastructure.Administration;

public sealed class AdminOperationsService(ApplicationDbContext context, IPasswordHasher passwordHasher)
    : IUserApplicationService, ILanguageApplicationService, IOperationsContentService
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<UserDto>> GetUsersAsync(UserQuery query, CancellationToken cancellationToken = default)
    {
        var pageNumber = NormalizePageNumber(query.PageNumber);
        var pageSize = NormalizePageSize(query.PageSize);
        var users = context.Users.AsNoTracking().AsQueryable();

        if (query.Role.HasValue)
        {
            users = users.Where(user => user.Role == query.Role.Value);
        }

        if (query.IsActive.HasValue)
        {
            users = users.Where(user => user.IsActive == query.IsActive.Value);
        }

        users = ApplyUserSearch(users, query.SearchTerm);
        var totalCount = await users.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return PagedResult<UserDto>.Empty(pageNumber, pageSize);
        }

        var items = await ApplyUserSorting(users, query.SortBy, query.SortDirection)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<UserDto>(items.Select(user => user.ToDto()).ToArray(), pageNumber, pageSize, totalCount);
    }

    public async Task<UserDto?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return user?.ToDto();
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        var normalizedUsername = username.Trim();
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(item => item.Username == normalizedUsername, cancellationToken);
        return user?.ToDto();
    }

    public async Task<OperationResult<UserDto>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();

        if (await context.Users.AnyAsync(user => user.Username == username, cancellationToken))
        {
            return OperationResult<UserDto>.Failure("Username already exists.");
        }

        if (await context.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            return OperationResult<UserDto>.Failure("Email already exists.");
        }

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHasher.HashPassword(request.Password),
            FirstName = NormalizeOptional(request.FirstName),
            LastName = NormalizeOptional(request.LastName),
            Role = request.Role,
            IsActive = request.IsActive,
            EmailConfirmed = false,
            CreatedDate = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<UserDto>.Success(user.ToDto(), "User created.");
    }

    public async Task<OperationResult<UserDto>> UpdateUserAsync(UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
        if (user is null)
        {
            return OperationResult<UserDto>.Failure("User was not found.");
        }

        var username = request.Username.Trim();
        var email = request.Email.Trim();

        if (await context.Users.AnyAsync(item => item.Id != user.Id && item.Username == username, cancellationToken))
        {
            return OperationResult<UserDto>.Failure("Username already exists.");
        }

        if (await context.Users.AnyAsync(item => item.Id != user.Id && item.Email == email, cancellationToken))
        {
            return OperationResult<UserDto>.Failure("Email already exists.");
        }

        user.Username = username;
        user.Email = email;
        user.FirstName = NormalizeOptional(request.FirstName);
        user.LastName = NormalizeOptional(request.LastName);
        user.Bio = NormalizeOptional(request.Bio);
        user.ProfileImagePath = NormalizeOptional(request.ProfileImagePath);
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.EmailConfirmed = request.EmailConfirmed;

        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<UserDto>.Success(user.ToDto(), "User updated.");
    }

    public async Task<OperationResult> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(item => item.Id == request.UserId && item.IsActive, cancellationToken);
        if (user is null)
        {
            return OperationResult.Failure("User was not found.");
        }

        if (!passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return OperationResult.Failure("Current password is incorrect.");
        }

        user.PasswordHash = passwordHasher.HashPassword(request.NewPassword);
        await RevokeUserRefreshTokensAsync(user.Id, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Password changed.");
    }

    public async Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(item => item.Id == request.UserId, cancellationToken);
        if (user is null)
        {
            return OperationResult.Failure("User was not found.");
        }

        user.PasswordHash = passwordHasher.HashPassword(request.NewPassword);
        await RevokeUserRefreshTokensAsync(user.Id, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Password reset.");
    }

    public async Task<OperationResult> DeleteUserAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (user is null)
        {
            return OperationResult.Failure("User was not found.");
        }

        if (!user.IsActive)
        {
            return OperationResult.Success("User is already inactive.");
        }

        user.IsActive = false;
        await RevokeUserRefreshTokensAsync(user.Id, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("User deactivated.");
    }

    public async Task<OperationResult> ReactivateUserAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (user is null)
        {
            return OperationResult.Failure("User was not found.");
        }

        user.IsActive = true;
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("User reactivated.");
    }
    public async Task<PagedResult<LanguageDto>> GetLanguagesAsync(LanguageQuery query, CancellationToken cancellationToken = default)
    {
        var pageNumber = NormalizePageNumber(query.PageNumber);
        var pageSize = NormalizePageSize(query.PageSize);
        var languages = context.Languages.AsNoTracking().AsQueryable();

        if (query.IsActive.HasValue)
        {
            languages = languages.Where(language => language.IsActive == query.IsActive.Value);
        }

        languages = ApplyLanguageSearch(languages, query.SearchTerm);
        var totalCount = await languages.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return PagedResult<LanguageDto>.Empty(pageNumber, pageSize);
        }

        var items = await ApplyLanguageSorting(languages, query.SortBy, query.SortDirection)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<LanguageDto>(items.Select(language => language.ToDto()).ToArray(), pageNumber, pageSize, totalCount);
    }

    public async Task<IReadOnlyList<string>> GetActiveLanguageCodesAsync(CancellationToken cancellationToken = default)
    {
        return await context.Languages
            .AsNoTracking()
            .Where(language => language.IsActive)
            .OrderBy(language => language.SortOrder)
            .ThenBy(language => language.Name)
            .Select(language => language.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<LanguageDto?> GetLanguageByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var language = await context.Languages.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return language?.ToDto();
    }

    public async Task<LanguageDto?> GetDefaultLanguageAsync(CancellationToken cancellationToken = default)
    {
        var language = await context.Languages.AsNoTracking().FirstOrDefaultAsync(item => item.IsDefault && item.IsActive, cancellationToken);
        return language?.ToDto();
    }

    public async Task<OperationResult<LanguageDto>> UpsertLanguageAsync(UpsertLanguageRequest request, CancellationToken cancellationToken = default)
    {
        var code = NormalizeCode(request.Code);
        var cultureCode = request.CultureCode.Trim();

        if (await context.Languages.AnyAsync(item => item.Code == code && item.Id != request.Id, cancellationToken))
        {
            return OperationResult<LanguageDto>.Failure("Language code already exists.");
        }

        if (await context.Languages.AnyAsync(item => item.CultureCode == cultureCode && item.Id != request.Id, cancellationToken))
        {
            return OperationResult<LanguageDto>.Failure("Culture code already exists.");
        }

        Language language;
        var created = !request.Id.HasValue;
        if (request.Id.HasValue)
        {
            var existing = await context.Languages.FirstOrDefaultAsync(item => item.Id == request.Id.Value, cancellationToken);
            if (existing is null)
            {
                return OperationResult<LanguageDto>.Failure("Language was not found.");
            }

            language = existing;
            language.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            language = new Language { CreatedDate = DateTime.UtcNow };
            context.Languages.Add(language);
        }

        var hasAnyLanguage = await context.Languages.AnyAsync(cancellationToken);
        language.Code = code;
        language.CultureCode = cultureCode;
        language.Name = request.Name.Trim();
        language.NativeName = request.NativeName.Trim();
        language.IsActive = request.IsActive;
        language.IsDefault = request.IsDefault || !hasAnyLanguage;
        language.SortOrder = request.SortOrder;

        if (language.IsDefault)
        {
            language.IsActive = true;
            await ClearOtherDefaultLanguagesAsync(language, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<LanguageDto>.Success(language.ToDto(), created ? "Language created." : "Language updated.");
    }

    public async Task<OperationResult> DeleteLanguageAsync(int id, CancellationToken cancellationToken = default)
    {
        var language = await context.Languages.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (language is null)
        {
            return OperationResult.Failure("Language was not found.");
        }

        if (language.IsDefault)
        {
            return OperationResult.Failure("Cannot delete the default language.");
        }

        var activeLanguageCount = await context.Languages.CountAsync(item => item.IsActive, cancellationToken);
        if (language.IsActive && activeLanguageCount <= 1)
        {
            return OperationResult.Failure("Cannot delete the only active language.");
        }

        if (await HasExistingTranslationsAsync(language.Code, cancellationToken))
        {
            return OperationResult.Failure("Cannot delete language with existing translations.");
        }

        context.Languages.Remove(language);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Language deleted.");
    }

    public async Task<OperationResult> SetDefaultLanguageAsync(int id, CancellationToken cancellationToken = default)
    {
        var language = await context.Languages.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (language is null)
        {
            return OperationResult.Failure("Language was not found.");
        }

        if (!language.IsActive)
        {
            return OperationResult.Failure("Only active languages can be set as default.");
        }

        await ClearOtherDefaultLanguagesAsync(language, cancellationToken);
        language.IsDefault = true;
        language.UpdatedDate = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Default language updated.");
    }

    public async Task<OperationResult> ToggleLanguageStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var language = await context.Languages.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (language is null)
        {
            return OperationResult.Failure("Language was not found.");
        }

        if (language.IsDefault && language.IsActive)
        {
            return OperationResult.Failure("Cannot disable the default language.");
        }

        var activeLanguageCount = await context.Languages.CountAsync(item => item.IsActive, cancellationToken);
        if (language.IsActive && activeLanguageCount <= 1)
        {
            return OperationResult.Failure("Cannot disable the only active language.");
        }

        language.IsActive = !language.IsActive;
        language.UpdatedDate = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(language.IsActive ? "Language activated." : "Language deactivated.");
    }
    public async Task<PagedResult<DepartmentDto>> GetDepartmentsAsync(DepartmentQuery query, CancellationToken cancellationToken = default)
    {
        var pageNumber = NormalizePageNumber(query.PageNumber);
        var pageSize = NormalizePageSize(query.PageSize);
        var language = NormalizeCode(query.Language);
        var departments = context.Departments.AsNoTracking().Include(department => department.Translations).AsQueryable();

        if (query.IsActive.HasValue)
        {
            departments = departments.Where(department => department.IsActive == query.IsActive.Value);
        }

        departments = ApplyDepartmentSearch(departments, query.SearchTerm);
        var totalCount = await departments.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return PagedResult<DepartmentDto>.Empty(pageNumber, pageSize);
        }

        var items = await ApplyDepartmentSorting(departments, query.SortBy, query.SortDirection)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<DepartmentDto>(items.Select(department => department.ToDto(language)).ToArray(), pageNumber, pageSize, totalCount);
    }

    public async Task<OperationResult<DepartmentDto>> UpsertDepartmentAsync(UpsertDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        Department department;
        var created = !request.Id.HasValue;
        if (request.Id.HasValue)
        {
            var existing = await context.Departments.Include(item => item.Translations).FirstOrDefaultAsync(item => item.Id == request.Id.Value, cancellationToken);
            if (existing is null)
            {
                return OperationResult<DepartmentDto>.Failure("Department was not found.");
            }

            department = existing;
            department.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            department = new Department { CreatedDate = DateTime.UtcNow };
            context.Departments.Add(department);
        }

        department.SortOrder = request.SortOrder;
        department.IsActive = request.IsActive;
        UpsertDepartmentTranslations(department.Translations, request.Translations);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<DepartmentDto>.Success(department.ToDto(), created ? "Department created." : "Department updated.");
    }

    public async Task<OperationResult> DeleteDepartmentAsync(int id, CancellationToken cancellationToken = default)
    {
        var department = await context.Departments.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (department is null)
        {
            return OperationResult.Failure("Department was not found.");
        }

        if (await context.TeamMembers.AnyAsync(member => member.DepartmentId == id, cancellationToken))
        {
            return OperationResult.Failure("Department cannot be deleted while team members reference it.");
        }

        context.Departments.Remove(department);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Department deleted.");
    }

    public async Task<PagedResult<TeamMemberDto>> GetTeamMembersAsync(TeamMemberQuery query, CancellationToken cancellationToken = default)
    {
        var pageNumber = NormalizePageNumber(query.PageNumber);
        var pageSize = NormalizePageSize(query.PageSize);
        var members = context.TeamMembers.AsNoTracking().Include(member => member.Department).ThenInclude(department => department!.Translations).AsQueryable();

        if (query.DepartmentId.HasValue)
        {
            members = members.Where(member => member.DepartmentId == query.DepartmentId.Value);
        }

        if (query.IsActive.HasValue)
        {
            members = members.Where(member => member.IsActive == query.IsActive.Value);
        }

        members = ApplyTeamMemberSearch(members, query.SearchTerm);
        var totalCount = await members.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return PagedResult<TeamMemberDto>.Empty(pageNumber, pageSize);
        }

        var items = await ApplyTeamMemberSorting(members, query.SortBy, query.SortDirection)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TeamMemberDto>(items.Select(member => member.ToDto()).ToArray(), pageNumber, pageSize, totalCount);
    }

    public async Task<OperationResult<TeamMemberDto>> UpsertTeamMemberAsync(UpsertTeamMemberRequest request, CancellationToken cancellationToken = default)
    {
        if (request.DepartmentId.HasValue && !await context.Departments.AnyAsync(department => department.Id == request.DepartmentId.Value, cancellationToken))
        {
            return OperationResult<TeamMemberDto>.Failure("Department was not found.");
        }

        TeamMember member;
        var created = !request.Id.HasValue;
        if (request.Id.HasValue)
        {
            var existing = await context.TeamMembers.FirstOrDefaultAsync(item => item.Id == request.Id.Value, cancellationToken);
            if (existing is null)
            {
                return OperationResult<TeamMemberDto>.Failure("Team member was not found.");
            }

            member = existing;
            member.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            member = new TeamMember { CreatedDate = DateTime.UtcNow };
            context.TeamMembers.Add(member);
        }

        member.FirstName = request.FirstName.Trim();
        member.LastName = request.LastName.Trim();
        member.Position = request.Position.Trim();
        member.DepartmentId = request.DepartmentId;
        member.Email = request.Email.Trim();
        member.Bio = NormalizeOptional(request.Bio);
        member.PhotoPath = NormalizeOptional(request.PhotoPath);
        member.SortOrder = request.SortOrder;
        member.IsActive = request.IsActive;

        await context.SaveChangesAsync(cancellationToken);
        var loaded = await LoadTeamMemberAsync(member.Id, cancellationToken);
        return OperationResult<TeamMemberDto>.Success(loaded!.ToDto(), created ? "Team member created." : "Team member updated.");
    }

    public async Task<OperationResult> DeleteTeamMemberAsync(int id, CancellationToken cancellationToken = default)
    {
        var member = await context.TeamMembers.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (member is null)
        {
            return OperationResult.Failure("Team member was not found.");
        }

        context.TeamMembers.Remove(member);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Team member deleted.");
    }
    public async Task<PagedResult<SiteSettingsDto>> GetSiteSettingsAsync(SiteSettingsQuery query, CancellationToken cancellationToken = default)
    {
        var pageNumber = NormalizePageNumber(query.PageNumber);
        var pageSize = NormalizePageSize(query.PageSize);
        var settings = context.SiteSettings.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var category = query.Category.Trim();
            settings = settings.Where(setting => setting.Category == category);
        }

        if (query.IsActive.HasValue)
        {
            settings = settings.Where(setting => setting.IsActive == query.IsActive.Value);
        }

        settings = ApplySiteSettingsSearch(settings, query.SearchTerm);
        var totalCount = await settings.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return PagedResult<SiteSettingsDto>.Empty(pageNumber, pageSize);
        }

        var items = await ApplySiteSettingsSorting(settings, query.SortBy, query.SortDirection)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<SiteSettingsDto>(items.Select(setting => setting.ToDto()).ToArray(), pageNumber, pageSize, totalCount);
    }

    public async Task<OperationResult<SiteSettingsDto>> UpsertSiteSettingsAsync(UpsertSiteSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var key = request.Key.Trim();
        if (await context.SiteSettings.AnyAsync(setting => setting.Key == key && setting.Id != request.Id, cancellationToken))
        {
            return OperationResult<SiteSettingsDto>.Failure("Site setting key already exists.");
        }

        SiteSettings settings;
        var created = !request.Id.HasValue;
        if (request.Id.HasValue)
        {
            var existing = await context.SiteSettings.FirstOrDefaultAsync(item => item.Id == request.Id.Value, cancellationToken);
            if (existing is null)
            {
                return OperationResult<SiteSettingsDto>.Failure("Site setting was not found.");
            }

            settings = existing;
            settings.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            settings = new SiteSettings { CreatedDate = DateTime.UtcNow };
            context.SiteSettings.Add(settings);
        }

        settings.Key = key;
        settings.Value = NormalizeOptional(request.Value);
        settings.Description = NormalizeOptional(request.Description);
        settings.Category = NormalizeOptional(request.Category);
        settings.IconClass = NormalizeOptional(request.IconClass);
        settings.SortOrder = request.SortOrder;
        settings.Type = request.Type;
        settings.IsActive = request.IsActive;

        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<SiteSettingsDto>.Success(settings.ToDto(), created ? "Site setting created." : "Site setting updated.");
    }

    public async Task<OperationResult> DeleteSiteSettingsAsync(int id, CancellationToken cancellationToken = default)
    {
        var settings = await context.SiteSettings.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (settings is null)
        {
            return OperationResult.Failure("Site setting was not found.");
        }

        context.SiteSettings.Remove(settings);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Site setting deleted.");
    }

    private async Task RevokeUserRefreshTokensAsync(int userId, CancellationToken cancellationToken)
    {
        var tokens = await context.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAtUtc == null && token.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
        }
    }

    private async Task ClearOtherDefaultLanguagesAsync(Language language, CancellationToken cancellationToken)
    {
        var otherDefaults = await context.Languages.Where(item => item.IsDefault && item.Id != language.Id).ToListAsync(cancellationToken);
        foreach (var otherDefault in otherDefaults)
        {
            otherDefault.IsDefault = false;
            otherDefault.UpdatedDate = DateTime.UtcNow;
        }
    }

    private async Task<bool> HasExistingTranslationsAsync(string languageCode, CancellationToken cancellationToken)
    {
        return await context.TourTranslations.AnyAsync(item => item.Language == languageCode, cancellationToken)
            || await context.BlogPostTranslations.AnyAsync(item => item.Language == languageCode, cancellationToken)
            || await context.TourCategoryTranslations.AnyAsync(item => item.Language == languageCode, cancellationToken)
            || await context.BlogCategoryTranslations.AnyAsync(item => item.Language == languageCode, cancellationToken)
            || await context.TourItineraryTranslations.AnyAsync(item => item.Language == languageCode, cancellationToken)
            || await context.DepartmentTranslations.AnyAsync(item => item.Language == languageCode, cancellationToken);
    }

    private async Task<TeamMember?> LoadTeamMemberAsync(int id, CancellationToken cancellationToken)
    {
        return await context.TeamMembers
            .AsNoTracking()
            .Include(member => member.Department)
                .ThenInclude(department => department!.Translations)
            .FirstOrDefaultAsync(member => member.Id == id, cancellationToken);
    }

    private static void UpsertDepartmentTranslations(ICollection<DepartmentTranslation> existing, IEnumerable<DepartmentTranslationRequest> requests)
    {
        foreach (var request in requests)
        {
            var language = NormalizeCode(request.Language);
            var translation = existing.FirstOrDefault(item => item.Language == language);
            if (translation is null)
            {
                existing.Add(new DepartmentTranslation { Language = language, Name = request.Name.Trim() });
                continue;
            }

            translation.Name = request.Name.Trim();
        }
    }
    private static IQueryable<User> ApplyUserSearch(IQueryable<User> users, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return users;
        var term = searchTerm.Trim();
        return users.Where(user => user.Username.Contains(term) || user.Email.Contains(term) || user.FirstName != null && user.FirstName.Contains(term) || user.LastName != null && user.LastName.Contains(term));
    }

    private static IQueryable<Language> ApplyLanguageSearch(IQueryable<Language> languages, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return languages;
        var term = searchTerm.Trim();
        return languages.Where(language => language.Code.Contains(term) || language.CultureCode.Contains(term) || language.Name.Contains(term) || language.NativeName.Contains(term));
    }

    private static IQueryable<Department> ApplyDepartmentSearch(IQueryable<Department> departments, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return departments;
        var term = searchTerm.Trim();
        return departments.Where(department => department.Translations.Any(translation => translation.Name.Contains(term)));
    }

    private static IQueryable<TeamMember> ApplyTeamMemberSearch(IQueryable<TeamMember> members, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return members;
        var term = searchTerm.Trim();
        return members.Where(member => member.FirstName.Contains(term) || member.LastName.Contains(term) || member.Position.Contains(term) || member.Email.Contains(term) || member.Bio != null && member.Bio.Contains(term));
    }

    private static IQueryable<SiteSettings> ApplySiteSettingsSearch(IQueryable<SiteSettings> settings, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return settings;
        var term = searchTerm.Trim();
        return settings.Where(setting => setting.Key.Contains(term) || setting.Value != null && setting.Value.Contains(term) || setting.Description != null && setting.Description.Contains(term) || setting.Category != null && setting.Category.Contains(term));
    }

    private static IQueryable<User> ApplyUserSorting(IQueryable<User> users, string? sortBy, SortDirection sortDirection)
    {
        var descending = sortDirection == SortDirection.Descending;
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "email" => descending ? users.OrderByDescending(user => user.Email) : users.OrderBy(user => user.Email),
            "role" => descending ? users.OrderByDescending(user => user.Role) : users.OrderBy(user => user.Role),
            "createddate" => descending ? users.OrderByDescending(user => user.CreatedDate) : users.OrderBy(user => user.CreatedDate),
            "lastlogindate" => descending ? users.OrderByDescending(user => user.LastLoginDate) : users.OrderBy(user => user.LastLoginDate),
            "isactive" => descending ? users.OrderByDescending(user => user.IsActive) : users.OrderBy(user => user.IsActive),
            _ => descending ? users.OrderByDescending(user => user.Username) : users.OrderBy(user => user.Username)
        };
    }

    private static IQueryable<Language> ApplyLanguageSorting(IQueryable<Language> languages, string? sortBy, SortDirection sortDirection)
    {
        var descending = sortDirection == SortDirection.Descending;
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "code" => descending ? languages.OrderByDescending(language => language.Code) : languages.OrderBy(language => language.Code),
            "culturecode" => descending ? languages.OrderByDescending(language => language.CultureCode) : languages.OrderBy(language => language.CultureCode),
            "name" => descending ? languages.OrderByDescending(language => language.Name) : languages.OrderBy(language => language.Name),
            "isactive" => descending ? languages.OrderByDescending(language => language.IsActive) : languages.OrderBy(language => language.IsActive),
            _ => descending ? languages.OrderByDescending(language => language.SortOrder).ThenByDescending(language => language.Name) : languages.OrderBy(language => language.SortOrder).ThenBy(language => language.Name)
        };
    }

    private static IQueryable<Department> ApplyDepartmentSorting(IQueryable<Department> departments, string? sortBy, SortDirection sortDirection)
    {
        var descending = sortDirection == SortDirection.Descending;
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "createddate" => descending ? departments.OrderByDescending(department => department.CreatedDate) : departments.OrderBy(department => department.CreatedDate),
            "isactive" => descending ? departments.OrderByDescending(department => department.IsActive) : departments.OrderBy(department => department.IsActive),
            _ => descending ? departments.OrderByDescending(department => department.SortOrder).ThenByDescending(department => department.Id) : departments.OrderBy(department => department.SortOrder).ThenBy(department => department.Id)
        };
    }

    private static IQueryable<TeamMember> ApplyTeamMemberSorting(IQueryable<TeamMember> members, string? sortBy, SortDirection sortDirection)
    {
        var descending = sortDirection == SortDirection.Descending;
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "firstname" => descending ? members.OrderByDescending(member => member.FirstName) : members.OrderBy(member => member.FirstName),
            "lastname" => descending ? members.OrderByDescending(member => member.LastName) : members.OrderBy(member => member.LastName),
            "position" => descending ? members.OrderByDescending(member => member.Position) : members.OrderBy(member => member.Position),
            "createddate" => descending ? members.OrderByDescending(member => member.CreatedDate) : members.OrderBy(member => member.CreatedDate),
            "isactive" => descending ? members.OrderByDescending(member => member.IsActive) : members.OrderBy(member => member.IsActive),
            _ => descending ? members.OrderByDescending(member => member.SortOrder).ThenByDescending(member => member.FirstName) : members.OrderBy(member => member.SortOrder).ThenBy(member => member.FirstName)
        };
    }

    private static IQueryable<SiteSettings> ApplySiteSettingsSorting(IQueryable<SiteSettings> settings, string? sortBy, SortDirection sortDirection)
    {
        var descending = sortDirection == SortDirection.Descending;
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "key" => descending ? settings.OrderByDescending(setting => setting.Key) : settings.OrderBy(setting => setting.Key),
            "category" => descending ? settings.OrderByDescending(setting => setting.Category) : settings.OrderBy(setting => setting.Category),
            "type" => descending ? settings.OrderByDescending(setting => setting.Type) : settings.OrderBy(setting => setting.Type),
            "isactive" => descending ? settings.OrderByDescending(setting => setting.IsActive) : settings.OrderBy(setting => setting.IsActive),
            _ => descending ? settings.OrderByDescending(setting => setting.Category).ThenByDescending(setting => setting.SortOrder).ThenByDescending(setting => setting.Key) : settings.OrderBy(setting => setting.Category).ThenBy(setting => setting.SortOrder).ThenBy(setting => setting.Key)
        };
    }

    private static int NormalizePageNumber(int pageNumber) => Math.Max(1, pageNumber);
    private static int NormalizePageSize(int pageSize) => Math.Clamp(pageSize, 1, MaxPageSize);
    private static string NormalizeCode(string value) => value.Trim().ToLowerInvariant();
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
