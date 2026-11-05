using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities;
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

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}