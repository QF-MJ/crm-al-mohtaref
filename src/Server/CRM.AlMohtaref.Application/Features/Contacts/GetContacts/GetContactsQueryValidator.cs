using CRM.AlMohtaref.Application.Common.Queries;
using FluentValidation;

namespace CRM.AlMohtaref.Application.Features.Contacts.GetContacts;

public class GetContactsQueryValidator : AbstractValidator<GetContactsQuery>
{
    public GetContactsQueryValidator()
    {
        Include(new PagedQueryValidator());

        RuleFor(x => x.TrackId)
            .GreaterThan(0)
            .WithMessage("معرف المسار يجب أن يكون أكبر من الصفر")
            .When(x => x.TrackId.HasValue);

        RuleFor(x => x.LeadStatusId)
            .GreaterThan(0)
            .WithMessage("معرف حالة العميل المحتمل يجب أن يكون أكبر من الصفر")
            .When(x => x.LeadStatusId.HasValue);

        RuleForEach(x => x.TagIds)
            .GreaterThan(0)
            .WithMessage("معرف الوسم يجب أن يكون أكبر من الصفر")
            .When(x => x.TagIds != null);

        RuleFor(x => x.CampaignTitle)
            .MaximumLength(200)
            .WithMessage("عنوان الحملة يجب ألا يتجاوز 200 حرف");

        RuleFor(x => x.StudentName)
            .MaximumLength(150)
            .WithMessage("اسم الطالب يجب ألا يتجاوز 150 حرفاً");

        RuleFor(x => x.ContactName)
            .MaximumLength(150)
            .WithMessage("اسم جهة الاتصال يجب ألا يتجاوز 150 حرفاً");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .WithMessage("رقم الهاتف يجب ألا يتجاوز 20 خانة");
    }
}
