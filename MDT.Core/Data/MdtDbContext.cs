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
    public DbSet<BootMediaEntity> BootMedias { get; set; } = null!;
    public DbSet<BootMediaBuildEntity> BootMediaBuilds { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskSequenceEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Version).HasMaxLength(50);
            entity.Property(e => e.BaseTaskSequenceId).HasMaxLength(200);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.BaseTaskSequenceId);
            entity.HasIndex(e => new { e.BaseTaskSequenceId, e.VersionNumber });
            entity.HasIndex(e => new { e.BaseTaskSequenceId, e.IsActive, e.Status });
        });

        modelBuilder.Entity<ExecutionContextEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TaskSequenceId).IsRequired();
            entity.Property(e => e.Status).IsRequired();
        });

        modelBuilder.Entity<BootMediaEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Architecture).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ServerUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.CreatedDate);
            entity.HasIndex(e => e.Architecture);
        });

        modelBuilder.Entity<BootMediaBuildEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CurrentStep).HasMaxLength(500);
            entity.Property(e => e.BuildOptions).IsRequired();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartTime);
        });
    }
}

public class TaskSequenceEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = "Development"; // Development, Testing, Production
    public string Version { get; set; } = "1.0.0";
    public string BaseTaskSequenceId { get; set; } = string.Empty; // Links versions together
    public int VersionNumber { get; set; } = 1; // Incremental version number
    public bool IsActive { get; set; } = true; // Active version in production
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

public class BootMediaEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Architecture { get; set; } = string.Empty;
    public string ServerUrl { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = string.Empty;
    public string IncludedDrivers { get; set; } = string.Empty;
    public string OptionalComponents { get; set; } = string.Empty;
    public string BuildLog { get; set; } = string.Empty;
}

public class BootMediaBuildEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string CurrentStep { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? ErrorMessage { get; set; }
    public string BuildOptions { get; set; } = string.Empty;
    public string? ResultingBootMediaId { get; set; }
}
