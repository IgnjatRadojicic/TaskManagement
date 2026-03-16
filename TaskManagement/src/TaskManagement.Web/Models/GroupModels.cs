namespace TaskManagement.Web.Models;

// ===== Group Models =====

public class GroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GroupCode { get; set; } = string.Empty;
    public bool IsPasswordProtected { get; set; }
    public int MemberCount { get; set; }
    public int UserRole { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GroupDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GroupCode { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public bool IsPasswordProtected { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<GroupMemberDto> Members { get; set; } = new();
}

public class GroupMemberDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public class CreateGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Password { get; set; }
}

public class JoinGroupRequest
{
    public string GroupCode { get; set; } = string.Empty;
    public string? Password { get; set; }
}

public enum TreeStage
{
    EmptySoil = 0,
    Seed = 1,
    Sprout = 2,
    Sapling = 3,
    YoungTree = 4,
    FullTree = 5,
    FloweringTree = 6
}

public class FieldTreeDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public double CompletionPercentage { get; set; }
    public TreeStage CurrentTreeStage { get; set; }
    public int MemberCount { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
}

public class FieldTreeViewModel
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public double CompletionPercentage { get; set; }
    public TreeStage CurrentTreeStage { get; set; }
    public int MemberCount { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    public static FieldTreeViewModel FromDto(FieldTreeDto dto, double x, double y) => new()
    {
        GroupId = dto.GroupId,
        GroupName = dto.GroupName,
        CompletionPercentage = dto.CompletionPercentage,
        CurrentTreeStage = dto.CurrentTreeStage,
        MemberCount = dto.MemberCount,
        TotalTasks = dto.TotalTasks,
        CompletedTasks = dto.CompletedTasks,
        X = x,
        Y = y
    };
}