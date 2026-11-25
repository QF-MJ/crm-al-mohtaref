using CRM.AlMohtaref.Domain.Entities.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.AlMohtaref.Infrastructure.Data.Configurations.Tasks;

public class StudentTaskConfiguration : IEntityTypeConfiguration<StudentTask>
{
    public void Configure(EntityTypeBuilder<StudentTask> builder)
    {
        builder.ToTable("StudentTasks", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_StudentTasks_TaskType_SubTaskOnly",
                "[TaskType] = 8");
        });

        builder.HasKey(st => st.Id);

        builder.Property<TaskType>("TaskType")
            .HasDefaultValue(TaskType.SubTask)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(st => new { st.TaskId, st.StudentId })
            .IsUnique();

        builder.HasOne(st => st.Task)
            .WithMany(t => t.StudentTasks)
            .HasForeignKey("TaskId", "TaskType")
            .HasPrincipalKey(t => new { t.Id, t.TaskType })
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(st => st.Student)
            .WithMany()
            .HasForeignKey(st => st.StudentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
