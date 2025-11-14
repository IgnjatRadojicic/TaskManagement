using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;



namespace TaskManagement.Core.Interfaces

{
    public interface IApplicationDbContext
    {

        DbSet<User> Users { get; }
        DbSet<Group> Groups { get; }
        DbSet<GroupMember> GroupMembers { get; }
        DbSet<TaskItem> Tasks { get; }
        DbSet<TaskAttachment> TaskAttachments { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<PasswordResetToken> PasswordResetTokens { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    }
}
