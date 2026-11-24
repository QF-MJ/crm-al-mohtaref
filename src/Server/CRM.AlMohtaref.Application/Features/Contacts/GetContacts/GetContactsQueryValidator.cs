using CRM.AlMohtaref.Application.Common.Queries;
using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Contacts.GetContacts;

public class GetContactsQueryValidator : AbstractValidator<GetContactsQuery>
{
    public GetContactsQueryValidator()
    {
        Include(new PagedQueryValidator());

        RuleFor(x => x.TrackId)
            .NotEmpty()
            .WithMessage("معرف المسار غير صالح")
            .When(x => x.TrackId.HasValue);

        RuleFor(x => x.LeadStatus)
            .IsInEnum()
            .WithMessage("حالة العميل المحتمل غير صالحة")
            .When(x => x.LeadStatus.HasValue);

        RuleFor(x => x.TagIds)
            .Must(tags => tags!.Distinct().Count() == tags.Count)
            .WithMessage("لا يمكن تكرار معرفات الوسوم")
            .When(x => x.TagIds != null);

        RuleForEach(x => x.TagIds)
            .NotEmpty()
            .WithMessage("معرف الوسم غير صالح")
            .When(x => x.TagIds != null);

        RuleFor(x => x.CampaignId)
            .NotEmpty()
            .WithMessage("معرف الحملة غير صالح")
            .When(x => x.CampaignId != null);

        RuleFor(x => x.StudentName)
            .NotEmpty()
            .WithMessage("اسم الطالب لا يمكن أن يكون مسافات فارغة فقط")
            .MaximumLength(150)
            .WithMessage("اسم الطالب يجب ألا يتجاوز 150 حرفاً")
            .When(x => x.StudentName != null);

        RuleFor(x => x.ContactName)
            .NotEmpty()
            .WithMessage("اسم جهة الاتصال لا يمكن أن يكون مسافات فارغة فقط")
            .MaximumLength(150)
            .WithMessage("اسم جهة الاتصال يجب ألا يتجاوز 150 حرفاً")
            .When(x => x.ContactName != null);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("رقم الهاتف لا يمكن أن يكون مسافات فارغة فقط")
            .MaximumLength(20)
            .WithMessage("رقم الهاتف يجب ألا يتجاوز 20 خانة")
            .Matches(@"^\+?[0-9\s\-()]*[0-9][0-9\s\-()]*$")
            .WithMessage("رقم الهاتف يجب أن يحتوي على أرقام ورموز هاتف صالحة فقط")
            .When(x => x.PhoneNumber != null);
    }
}
