using FluentValidation;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Domain.Entities;
using TravelToursWebsite.Domain.Enums;

namespace TravelToursWebsite.Application.Features.Administration;

public sealed record UserQuery : PagedQuery
{
    public UserRole? Role { get; init; }
    public bool? IsActive { get; init; }
}

public sealed record LanguageQuery : PagedQuery
{
    public bool? IsActive { get; init; }
}

public sealed record DepartmentQuery : PagedQuery
{
    public string Language { get; init; } = "en";
    public bool? IsActive { get; init; }
}

public sealed record TeamMemberQuery : PagedQuery
{
    public int? DepartmentId { get; init; }
    public bool? IsActive { get; init; }
}

public sealed record SiteSettingsQuery : PagedQuery
{
    public string? Category { get; init; }
    public bool? IsActive { get; init; }
}

public sealed record UserDto(
    int Id,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    string? Bio,
    string? ProfileImagePath,
    UserRole Role,
    bool IsActive,
    bool EmailConfirmed,
    DateTime CreatedDate,
    DateTime? LastLoginDate);

public sealed record CreateUserRequest(
    string Username,
    string Email,
    string Password,
    string? FirstName,
    string? LastName,
    UserRole Role,
    bool IsActive);

public sealed record UpdateUserRequest(
    int Id,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    string? Bio,
    string? ProfileImagePath,
    UserRole Role,
    bool IsActive,
    bool EmailConfirmed);

public sealed record ChangePasswordRequest(int UserId, string CurrentPassword, string NewPassword);
public sealed record ResetPasswordRequest(int UserId, string NewPassword);

public sealed record LanguageDto(
    int Id,
    string Code,
    string CultureCode,
    string Name,
    string NativeName,
    bool IsActive,
    bool IsDefault,
    int SortOrder);

public sealed record UpsertLanguageRequest(
    int? Id,
    string Code,
    string CultureCode,
    string Name,
    string NativeName,
    bool IsActive,
    bool IsDefault,
    int SortOrder);

public sealed record DepartmentDto(
    int Id,
    string Name,
    int SortOrder,
    bool IsActive,
    DateTime CreatedDate,
    DateTime? UpdatedDate);

public sealed record DepartmentTranslationRequest(string Language, string Name);

public sealed record UpsertDepartmentRequest(
    int? Id,
    int SortOrder,
    bool IsActive,
    IReadOnlyList<DepartmentTranslationRequest> Translations);

public sealed record TeamMemberDto(
    int Id,
    string FirstName,
    string LastName,
    string FullName,
    string Position,
    int? DepartmentId,
    string? DepartmentName,
    string Email,
    string? Bio,
    string? PhotoPath,
    int SortOrder,
    bool IsActive);

public sealed record UpsertTeamMemberRequest(
    int? Id,
    string FirstName,
    string LastName,
    string Position,
    int? DepartmentId,
    string Email,
    string? Bio,
    string? PhotoPath,
    int SortOrder,
    bool IsActive);

public sealed record SiteSettingsDto(
    int Id,
    string Key,
    string? Value,
    string? Description,
    string? Category,
    string? IconClass,
    int SortOrder,
    SettingType Type,
    bool IsActive);

public sealed record ResourceContentLanguageDto(string CultureCode, bool IsValid);

public sealed record ResourceContentItemDto(
    string Key,
    string Category,
    IReadOnlyDictionary<string, string> Translations);

public sealed record UpsertSiteSettingsRequest(
    int? Id,
    string Key,
    string? Value,
    string? Description,
    string? Category,
    string? IconClass,
    int SortOrder,
    SettingType Type,
    bool IsActive);

public sealed record UpsertResourceContentItemRequest(
    string Key,
    IReadOnlyDictionary<string, string> Translations);

public interface IUserApplicationService
{
    Task<PagedResult<UserDto>> GetUsersAsync(UserQuery query, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<OperationResult<UserDto>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult<UserDto>> UpdateUserAsync(UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteUserAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> ReactivateUserAsync(int id, CancellationToken cancellationToken = default);
}

public interface ILanguageApplicationService
{
    Task<PagedResult<LanguageDto>> GetLanguagesAsync(LanguageQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetActiveLanguageCodesAsync(CancellationToken cancellationToken = default);
    Task<LanguageDto?> GetLanguageByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<LanguageDto?> GetDefaultLanguageAsync(CancellationToken cancellationToken = default);
    Task<OperationResult<LanguageDto>> UpsertLanguageAsync(UpsertLanguageRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteLanguageAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> SetDefaultLanguageAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> ToggleLanguageStatusAsync(int id, CancellationToken cancellationToken = default);
}

public interface IOperationsContentService
{
    Task<PagedResult<DepartmentDto>> GetDepartmentsAsync(DepartmentQuery query, CancellationToken cancellationToken = default);
    Task<OperationResult<DepartmentDto>> UpsertDepartmentAsync(UpsertDepartmentRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteDepartmentAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<TeamMemberDto>> GetTeamMembersAsync(TeamMemberQuery query, CancellationToken cancellationToken = default);
    Task<OperationResult<TeamMemberDto>> UpsertTeamMemberAsync(UpsertTeamMemberRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteTeamMemberAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<SiteSettingsDto>> GetSiteSettingsAsync(SiteSettingsQuery query, CancellationToken cancellationToken = default);
    Task<OperationResult<SiteSettingsDto>> UpsertSiteSettingsAsync(UpsertSiteSettingsRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteSiteSettingsAsync(int id, CancellationToken cancellationToken = default);
}

public interface IResourceContentService
{
    Task<IReadOnlyList<ResourceContentLanguageDto>> GetLanguagesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, string>> GetLanguageContentAsync(string cultureCode, CancellationToken cancellationToken = default);
    Task<ResourceContentItemDto> GetContentItemAsync(string key, CancellationToken cancellationToken = default);
    Task<OperationResult<ResourceContentItemDto>> UpsertContentItemAsync(UpsertResourceContentItemRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteContentItemAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ValidateLanguageFileAsync(string cultureCode, CancellationToken cancellationToken = default);
}

public static class AdministrationMappingExtensions
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Bio,
            user.ProfileImagePath,
            user.Role,
            user.IsActive,
            user.EmailConfirmed,
            user.CreatedDate,
            user.LastLoginDate);
    }

    public static LanguageDto ToDto(this Language language)
    {
        return new LanguageDto(
            language.Id,
            language.Code,
            language.CultureCode,
            language.Name,
            language.NativeName,
            language.IsActive,
            language.IsDefault,
            language.SortOrder);
    }

    public static DepartmentDto ToDto(this Department department, string language = "en")
    {
        var translation = department.Translations.FirstOrDefault(item => item.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
            ?? department.Translations.FirstOrDefault(item => item.Language.Equals("en", StringComparison.OrdinalIgnoreCase))
            ?? department.Translations.FirstOrDefault();

        return new DepartmentDto(
            department.Id,
            translation?.Name ?? string.Empty,
            department.SortOrder,
            department.IsActive,
            department.CreatedDate,
            department.UpdatedDate);
    }

    public static TeamMemberDto ToDto(this TeamMember member, string language = "en")
    {
        var departmentTranslation = member.Department?.Translations.FirstOrDefault(item => item.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
            ?? member.Department?.Translations.FirstOrDefault(item => item.Language.Equals("en", StringComparison.OrdinalIgnoreCase))
            ?? member.Department?.Translations.FirstOrDefault();

        return new TeamMemberDto(
            member.Id,
            member.FirstName,
            member.LastName,
            member.FullName,
            member.Position,
            member.DepartmentId,
            departmentTranslation?.Name,
            member.Email,
            member.Bio,
            member.PhotoPath,
            member.SortOrder,
            member.IsActive);
    }

    public static SiteSettingsDto ToDto(this SiteSettings settings)
    {
        return new SiteSettingsDto(
            settings.Id,
            settings.Key,
            settings.Value,
            settings.Description,
            settings.Category,
            settings.IconClass,
            settings.SortOrder,
            settings.Type,
            settings.IsActive);
    }
}

public sealed class UserQueryValidator : PagedQueryValidator<UserQuery>
{
}
public sealed class LanguageQueryValidator : PagedQueryValidator<LanguageQuery>
{
}
public sealed class SiteSettingsQueryValidator : PagedQueryValidator<SiteSettingsQuery>
{
    public SiteSettingsQueryValidator()
    {
        RuleFor(query => query.Category).MaximumLength(100);
    }
}

public sealed class DepartmentQueryValidator : PagedQueryValidator<DepartmentQuery>
{
    public DepartmentQueryValidator()
    {
        RuleFor(query => query.Language).NotEmpty().MaximumLength(10);
    }
}

public sealed class TeamMemberQueryValidator : PagedQueryValidator<TeamMemberQuery>
{
    public TeamMemberQueryValidator()
    {
        RuleFor(query => query.DepartmentId).GreaterThan(0).When(query => query.DepartmentId.HasValue);
    }
}

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(request => request.Username).NotEmpty().MaximumLength(50);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(request => request.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(request => request.FirstName).MaximumLength(100);
        RuleFor(request => request.LastName).MaximumLength(100);
    }
}

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
        RuleFor(request => request.Username).NotEmpty().MaximumLength(50);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(request => request.FirstName).MaximumLength(100);
        RuleFor(request => request.LastName).MaximumLength(100);
        RuleFor(request => request.Bio).MaximumLength(200);
        RuleFor(request => request.ProfileImagePath).MaximumLength(500);
    }
}

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(request => request.UserId).GreaterThan(0);
        RuleFor(request => request.CurrentPassword).NotEmpty();
        RuleFor(request => request.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(request => request.UserId).GreaterThan(0);
        RuleFor(request => request.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}

public sealed class UpsertLanguageRequestValidator : AbstractValidator<UpsertLanguageRequest>
{
    public UpsertLanguageRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0).When(request => request.Id.HasValue);
        RuleFor(request => request.Code).NotEmpty().MaximumLength(5);
        RuleFor(request => request.CultureCode).NotEmpty().MaximumLength(10);
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
        RuleFor(request => request.NativeName).NotEmpty().MaximumLength(100);
    }
}

public sealed class DepartmentTranslationRequestValidator : AbstractValidator<DepartmentTranslationRequest>
{
    public DepartmentTranslationRequestValidator()
    {
        RuleFor(request => request.Language).NotEmpty().MaximumLength(10);
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpsertDepartmentRequestValidator : AbstractValidator<UpsertDepartmentRequest>
{
    public UpsertDepartmentRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0).When(request => request.Id.HasValue);
        RuleFor(request => request.Translations).NotEmpty();
        RuleForEach(request => request.Translations).SetValidator(new DepartmentTranslationRequestValidator());
    }
}

public sealed class UpsertTeamMemberRequestValidator : AbstractValidator<UpsertTeamMemberRequest>
{
    public UpsertTeamMemberRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0).When(request => request.Id.HasValue);
        RuleFor(request => request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.LastName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Position).NotEmpty().MaximumLength(200);
        RuleFor(request => request.DepartmentId).GreaterThan(0).When(request => request.DepartmentId.HasValue);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(request => request.Bio).MaximumLength(500);
        RuleFor(request => request.PhotoPath).MaximumLength(200);
    }
}

public sealed class UpsertSiteSettingsRequestValidator : AbstractValidator<UpsertSiteSettingsRequest>
{
    public UpsertSiteSettingsRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0).When(request => request.Id.HasValue);
        RuleFor(request => request.Key).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Description).MaximumLength(500);
        RuleFor(request => request.Category).MaximumLength(100);
        RuleFor(request => request.IconClass).MaximumLength(50);
    }
}