using CRM.AlMohtaref.Domain.Entities.Questions;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Campaigns;
using CRM.AlMohtaref.Domain.Entities.Contacts;
using CRM.AlMohtaref.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace CRM.AlMohtaref.Infrastructure.Data;

public class AppDbContext : DbContext, IUnitOfWork
{
    public DbSet<User> Users { get; set; }

    public DbSet<Person> People { get; set; }

    public DbSet<UserPermission> UserPermissions { get; set; }

    public DbSet<Department> Departments { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<Contact> Contacts { get; set; }

    public DbSet<LeadStatus> LeadStatuses { get; set; }

    public DbSet<Track> Tracks { get; set; }

    public DbSet<Platform> Platforms { get; set; } = null!;

    public DbSet<PlatformCampaign> PlatformCampaigns { get; set; } = null!;

    public DbSet<Field> Fields { get; set; } = null!;

    public DbSet<QuestionConfiguration> QuestionConfigurations { get; set; } = null!;

    public DbSet<Question> Questions { get; set; } = null!;

    public DbSet<Option> Options { get; set; } = null!;

    public DbSet<OptionTag> OptionTags { get; set; } = null!;

    public DbSet<OptionSet> OptionSets { get; set; } = null!;

    public DbSet<OptionSetItem> OptionSetItems { get; set; } = null!;

    public DbSet<QuestionCampaign> QuestionCampaigns { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}