using Microsoft.EntityFrameworkCore;
using MDT.Core.Models;

namespace MDT.Core.Data;

public class MdtDbContext : DbContext
{
    public MdtDbContext(DbContextOptions<MdtDbContext> options) : base(options)
    {
    }

    public DbSet<TaskSequenceEntity> TaskSequences { get; set; } = null!;
    public DbSet<ExecutionContextEntity> Executions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskSequenceEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
        });

        modelBuilder.Entity<ExecutionContextEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TaskSequenceId).IsRequired();
            entity.Property(e => e.Status).IsRequired();
        });
    }
}

public class TaskSequenceEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
}

public class ExecutionContextEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TaskSequenceId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Variables { get; set; } = string.Empty;
    public string Results { get; set; } = string.Empty;
}
