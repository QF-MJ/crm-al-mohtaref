using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Features.Contacts;
using CRM.AlMohtaref.Application.Features.Contacts.GetContacts;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Contacts;
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
            bool hasStudentFilters = query.CampaignTitle != null
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
                    (query.CampaignTitle == null && query.TrackId == null && query.LeadStatus == null)
                    || s.CampaignLeads.Any(cl =>
                        (query.CampaignTitle == null || cl.Campaign.Title.Contains(query.CampaignTitle))
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
    }
}
