using Application.Shared.Enums;

namespace Application.Shared.Models;

public class DependencyTree
{
    public string EntityId { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public EntityType EntityType { get; set; }
    public EntityStatus CurrentStatus { get; set; }
    public List<DependencyTreeNode> Dependencies { get; set; } = new();
    public List<DependencyTreeNode> Dependents { get; set; } = new();
}

public class DependencyTreeNode
{
    public string EntityId { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public EntityType EntityType { get; set; }
    public EntityStatus CurrentStatus { get; set; }
    public bool IsCritical { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    public int Level { get; set; } = 0; // Depth in the tree (0 = direct dependency/dependent)
    public List<DependencyTreeNode> Children { get; set; } = new(); // For nested dependencies
}
