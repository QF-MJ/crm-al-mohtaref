using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Features.Contacts;
using CRM.AlMohtaref.Application.Features.Contacts.GetContacts;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Contacts;
using CRM.AlMohtaref.Domain.Extensions;
using CRM.AlMohtaref.Infrastructure.Data;
using CRM.AlMohtaref.Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CRM.AlMohtaref.Infrastructure.Repositories
{
    public class ContactRepository(AppDbContext db)
        : IContactRepository
    {

        public async Task<ContactsPageDto> SearchAsync(GetContactsQuery query, CancellationToken ct)
        {
            bool hasStudentFilters = query.CampaignId != null
                || query.TrackId != null
                || query.LeadStatus != null
                || (query.TagIds != null && query.TagIds.Any())
                || query.StudentName != null;

            Expression<Func<Student, bool>> studentFilter = s => !hasStudentFilters || (
                (query.StudentName == null || s.FullName.Contains(query.StudentName))
                && (
                    query.TagIds == null
                    || !query.TagIds.Any()
                    || query.TagIds.All(tagId =>
                        s.Contact.ContactTags.Any(ct => ct.TagId == tagId)
                        || s.StudentTags.Any(st => st.TagId == tagId))
                )
                && (
                    (query.CampaignId == null && query.TrackId == null && query.LeadStatus == null)
                    || s.CampaignLeads.Any(cl =>
                        (query.CampaignId == null || cl.Campaign.Id == query.CampaignId)
                        && (query.TrackId == null || cl.Campaign.TrackId == query.TrackId)
                        && (query.LeadStatus == null || cl.Status == query.LeadStatus))
                )
            );

            var items = await db.Contacts
                .AsNoTracking()
                .Where(c => (query.ContactName == null || c.FullName.Contains(query.ContactName))
                    && (query.PhoneNumber == null || c.PhoneNumber.Contains(query.PhoneNumber))
                    && (!hasStudentFilters || c.Students.AsQueryable().Any(studentFilter))
                )
                .OrderBy(c => c.Id)
                .Skip((query.PageNumber - 1) * PaginationSettings.PageSize)
                .Take(PaginationSettings.PageSize + 1)
                .Select(c => new ContactItemDto
                {
                    Id = c.Id,
                    Name = c.FullName,
                    Students = c.Students
                        .AsQueryable()
                        .Where(studentFilter)
                        .Select(s => new StudentDto(s.Id, s.FullName))
                        .ToList()
                })
                .ToListAsync(ct);

            PageDto page = new PageDto(query.PageNumber, false);

            if (items.Count > PaginationSettings.PageSize)
            {
                page.HasNextPage = true;
                items.RemoveAt(items.Count - 1);
            }

            return new ContactsPageDto(items, page);
        }

        public async Task<ContactDetailDto?> GetDetailAsync(Guid id, CancellationToken ct)
        {
            // المتاح حالياً في المخطط: جهة التواصل، الطلاب، وفرص الحملات.
            // الأقسام التي لا يوجد لها جدول بعد تُعاد بحالة empty بدل بيانات وهمية.
            var contact = await db.Contacts
                .AsNoTracking()
                .AsSplitQuery()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.FullName,
                    c.PhoneNumber,
                    Students = c.Students.Select(s => new
                    {
                        s.Id,
                        s.FullName,
                        Leads = s.CampaignLeads.Select(cl => new
                        {
                            CampaignTitle = cl.Campaign.Title,
                            TrackName = cl.Campaign.Track.Name,
                            Status = cl.Status
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync(ct);

            if (contact is null)
                return null;

            return new ContactDetailDto
            {
                Id = contact.Id,
                Name = contact.FullName ?? string.Empty,
                PhoneNumber = contact.PhoneNumber,
                Students = contact.Students.Select(student => new ContactDetailStudentDto
                {
                    Id = student.Id,
                    Name = student.FullName ?? string.Empty,
                    Sections = BuildSections(student.Leads.Select(lead => new ContactLeadRowDto
                    {
                        CampaignTitle = lead.CampaignTitle,
                        TrackName = lead.TrackName,
                        Status = lead.Status
                    }).ToList())
                }).ToList()
            };
        }

        private static List<ContactProfileSectionDto> BuildSections(List<ContactLeadRowDto> leads)
        {
            var opportunityRows = leads.Select(lead => new ContactProfileRowDto
            {
                Title = lead.CampaignTitle,
                Description = lead.TrackName,
                Meta = lead.Status.ToLeadStatusString()
            }).ToList();

            return new List<ContactProfileSectionDto>
            {
                new()
                {
                    Key = "opportunities",
                    State = opportunityRows.Count > 0 ? "loaded" : "empty",
                    Rows = opportunityRows
                },
                new() { Key = "communications", State = "empty" },
                new() { Key = "subscriptions", State = "empty" },
                new() { Key = "guidance", State = "empty" },
                new() { Key = "support", State = "empty" },
            };
        }
    }
}
