using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskManagement.Core.Common;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Entities.Lookups;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<TaskAttachment> TaskAttachments { get; set; }

    public DbSet<TaskStatusLookup> TaskStatuses { get; set; }
    public DbSet<TaskPriorityLookup> TaskPriorities { get; set; }
    public DbSet<GroupRoleLookup> GroupRoles { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);

        //.Where(e => !e.IsDeleted) Building Logical Expression Tree

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = Expression.Lambda(Expression.Not(property), parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }

            if(typeof(SelfManagedEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(SelfManagedEntity.IsDeleted));
                var filter = Expression.Lambda(Expression.Not(property), parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.UserName).IsUnique();

            entity.Property(e => e.UserName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);

        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.GroupCode).IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.GroupCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500);

            entity.HasOne(e => e.Owner)
                .WithMany(u => u.OwnedGroups)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);  

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(e => e.Id);

            
            entity.HasIndex(e => new { e.GroupId, e.UserId }).IsUnique();

            entity.HasOne(e => e.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.GroupMembers)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);


        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.GroupId);
            entity.HasIndex(e => e.AssignedToId);
            entity.HasIndex(e => e.StatusId);
            entity.HasIndex(e => e.DueDate);

            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);

            entity.HasOne(e => e.Group)
                .WithMany(g => g.Tasks)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AssignedTo)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(e => e.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Creator)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Status)
                .WithMany(s => s.Tasks)
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);


            entity.HasOne(e => e.Priority)
                .WithMany(p => p.Tasks)
                .HasForeignKey(e => e.PriorityId)
                .OnDelete(DeleteBehavior.Restrict);

        });

        modelBuilder.Entity<TaskAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.TaskId);

            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(100).IsRequired();


            entity.HasOne(e => e.Task)
                .WithMany(t => t.Attachments)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Uploader)
                .WithMany(u => u.UploadedAttachments)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);


        });


        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TokenHash);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);

            entity.Property(e => e.TokenHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);


        });

        modelBuilder.Entity<TaskStatusLookup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Color).HasMaxLength(20);

            entity.HasData(
                new TaskStatusLookup { Id = 0, Name = "NotStarted", DisplayName = "Not Started", Description = "Task has not been started yet", Color = "#6c757d", DisplayOrder = 1, IsActive = true },
                new TaskStatusLookup { Id = 1, Name = "InProgress", DisplayName = "In Progress", Description = "Task is currently being worked on", Color = "#0dcaf0", DisplayOrder = 2, IsActive = true },
                new TaskStatusLookup { Id = 2, Name = "UnderReview", DisplayName = "Under Review", Description = "Task is under review", Color = "#ffc107", DisplayOrder = 3, IsActive = true },
                new TaskStatusLookup { Id = 3, Name = "Completed", DisplayName = "Completed", Description = "Task is completed", Color = "#198754", DisplayOrder = 4, IsActive = true }
            );
        });

        modelBuilder.Entity<TaskPriorityLookup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Color).HasMaxLength(20);

            entity.HasData(
                new TaskPriorityLookup { Id = 0, Name = "Low", Description = "Low priority task", Color = "#6c757d", DisplayOrder = 1, IsActive = true },
                new TaskPriorityLookup { Id = 1, Name = "Medium", Description = "Medium priority task", Color = "#0dcaf0", DisplayOrder = 2, IsActive = true },
                new TaskPriorityLookup { Id = 2, Name = "High", Description = "High priority task", Color = "#ffc107", DisplayOrder = 3, IsActive = true },
                new TaskPriorityLookup { Id = 3, Name = "Urgent", Description = "Urgent priority task", Color = "#dc3545", DisplayOrder = 4, IsActive = true }
            );
        });

        modelBuilder.Entity<GroupRoleLookup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200);

            entity.HasData(
                new GroupRoleLookup { Id = 0, Name = "Owner", DisplayName = "Owner", Description = "Full control over the group", PermissionLevel = 100, IsActive = true },
                new GroupRoleLookup { Id = 1, Name = "Manager", DisplayName = "Manager", Description = "Can manage members and tasks", PermissionLevel = 75, IsActive = true },
                new GroupRoleLookup { Id = 2, Name = "TeamLead", DisplayName = "Team Lead", Description = "Can manage tasks", PermissionLevel = 50, IsActive = true },
                new GroupRoleLookup { Id = 3, Name = "Member", DisplayName = "Member", Description = "Can view and work on tasks", PermissionLevel = 25, IsActive = true }
            );
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.EntityId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.EntityType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PropertyName).HasMaxLength(100);
            entity.Property(e => e.NewValue).HasMaxLength(1000);
            entity.Property(e => e.OldValue).HasMaxLength(1000);
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.IpAddress).HasMaxLength(1000);
            entity.Property(e => e.UserAgent).HasMaxLength(1000);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

        });

    }


    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach(var entry in entries)
        {
            if(entry.Entity is BaseEntity baseEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    baseEntity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    baseEntity.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if(entry.Entity is SelfManagedEntity selfManaged)
            {
                if(entry.State == EntityState.Added)
                {
                    selfManaged.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    selfManaged.CreatedAt = DateTime.UtcNow;
                }
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
