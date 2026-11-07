using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Lookups
{
    public class GetLookupsQueryValidator : AbstractValidator<GetLookupsQuery>
    {
        public GetLookupsQueryValidator()
        {
            RuleFor(x => x.Include)
                .NotNull()
                .WithMessage("يرجى تحديد البيانات المطلوب استرجاعها");

            RuleForEach(x => x.Include)
                .Must(IsAllowedWord)
                .WithMessage("أحد الخيارات المحددة غير صحيح، يرجى اختيار خيارات قائمة صالحة مثل الأقسام أو الأدوار");
        }

        private bool IsAllowedWord(string item)
        {
            var allowedWords = new List<string> { "Departments", "Roles" };
            return allowedWords.Contains(item, StringComparer.OrdinalIgnoreCase);
        }
    }
}
