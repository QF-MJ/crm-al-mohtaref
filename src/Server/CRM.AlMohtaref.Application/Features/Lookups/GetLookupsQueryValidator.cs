using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Lookups
{
    public class GetLookupsQueryValidator : AbstractValidator<GetLookupsQuery>
    {
        public GetLookupsQueryValidator()
        {
            RuleFor(x => x.Include).NotNull();

            RuleForEach(x => x.Include)
                .Must(i => IsAllowedWord(i));
        }

        private bool IsAllowedWord(string item)
        {
            var allowedWords = new List<string> { "Departments", "Roles" };
            return allowedWords.Contains(item, StringComparer.OrdinalIgnoreCase);
        }
    }
}
