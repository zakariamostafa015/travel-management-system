using FluentValidation;

namespace TravelToursWebsite.Application.Common;

public class PagedQueryValidator<TQuery> : AbstractValidator<TQuery>
    where TQuery : PagedQuery
{
    private const int MaxPageSize = 100;

    public PagedQueryValidator()
    {
        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, MaxPageSize);

        RuleFor(query => query.SearchTerm)
            .MaximumLength(200);

        RuleFor(query => query.SortBy)
            .MaximumLength(100);
    }
}