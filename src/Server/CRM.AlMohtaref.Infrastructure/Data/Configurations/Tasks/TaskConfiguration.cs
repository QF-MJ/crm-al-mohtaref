using CRM.AlMohtaref.Domain.Entities.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = CRM.AlMohtaref.Domain.Entities.Tasks.Task;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Tasks;

public class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder.ToTable("Tasks", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Tasks_EndTime_GreaterThanOrEqual_StartTime",
                "[EndTime] >= [StartTime]");

            tableBuilder.HasCheckConstraint(
                "CK_Tasks_ParentTask_NotSelf",
                "[ParentTaskId] IS NULL OR [ParentTaskId] <> [Id]");
        });

        builder.HasKey(t => t.Id);

        builder.HasIndex(t => new { t.Id, t.TaskType })
            .IsUnique();

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired(false)
            .HasMaxLength(2000);

        builder.Property(t => t.TaskType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.TaskStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.StartTime)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(t => t.EndTime)
            .IsRequired()
            .HasColumnType("date");

        builder.HasOne(t => t.ParentTask)
            .WithMany(t => t.SubTasks)
            .HasForeignKey(t => t.ParentTaskId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CreatedByUser)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedToUser)
            .WithMany()
            .HasForeignKey(t => t.AssignedToUserId )
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.ParentTaskId);
        builder.HasIndex(t => t.CreatedByUserId);
        builder.HasIndex(t => t.AssignedToUserId );
        builder.HasIndex(t => t.TaskStatus);
        builder.HasIndex(t => t.TaskType);
        builder.HasIndex(t => new { t.StartTime, t.EndTime });
    }
}
