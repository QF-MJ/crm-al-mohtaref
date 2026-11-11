using CRM.AlMohtaref.Application.Common.Dtos;
using CRM.AlMohtaref.Application.Features.Contacts;
using CRM.AlMohtaref.Application.Features.Contacts.GetContacts;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Contacts;
using CRM.AlMohtaref.Infrastructure.Data;
using CRM.AlMohtaref.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace CRM.AlMohtaref.Infrastructure.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly AppDbContext _db;

        public ContactRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ContactsPageDto> SearchAsync(GetContactsQuery query, CancellationToken ct)
        {
            bool hasStudentFilters = query.CampaignTitle != null
                || query.TrackId != null
                || query.LeadStatusId != null
                || (query.TagIds != null && query.TagIds.Any())
                || query.StudentName != null;

            Expression<Func<Student, bool>> studentFilter = s => !hasStudentFilters || (
                (query.StudentName == null || s.Person.FullName.Contains(query.StudentName))
                && (query.TagIds == null || !query.TagIds.Any() || s.ContactTags.Count(ct => query.TagIds.Contains(ct.TagId)) == query.TagIds.Count)
                && (
                    (query.CampaignTitle == null && query.TrackId == null && query.LeadStatusId == null)
                    || s.CampaignLeads.Any
                        (
                            cl => (query.CampaignTitle == null || cl.Campaign.Title.Contains(query.CampaignTitle))
                                && (query.TrackId == null || cl.Campaign.TrackId == query.TrackId)
                                && (query.LeadStatusId == null || cl.StatusId == query.LeadStatusId)
                        )
                )
            );

            var items = await _db.Contacts
              .AsNoTracking()
              .Where(c => (query.ContactName == null || c.Person.FullName.Contains(query.ContactName))
                    && (query.PhoneNumber == null || c.PhoneNumber.Contains(query.PhoneNumber))
                    && (!hasStudentFilters || c.Students.AsQueryable().Any(studentFilter))
              )
              .OrderBy(c => c.Id)
              .Skip((query.PageNumber - 1) * PaginationSettings.PageSize)
              .Take(PaginationSettings.PageSize + 1)
              .Select(c => new ContactItemDto
              {
                  Id = c.Id,
                  Name = c.Person.FullName,
                  Students = c.Students
                    .AsQueryable()
                    .Where(studentFilter)
                    .Select(
                        s => new StudentDto(s.Id, s.Person.FullName)
                    )
                    .ToList()
              }
              )
              .ToListAsync(ct);

            PageDto page = new PageDto(PaginationSettings.PageSize, false);

            if (items.Count > PaginationSettings.PageSize)
            {
                page.HasNextPage = true;
                items.RemoveAt(items.Count - 1);
            }

            return new ContactsPageDto(items, page);
        }
    }
}
