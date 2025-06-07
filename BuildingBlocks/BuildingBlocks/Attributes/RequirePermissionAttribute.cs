namespace BuildingBlocks.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : Attribute
{
    public required string Module { get; init; }
    public required string Action { get; init; }

    //  Computed property with expression body
    public string Permission => $"{Module}.{Action}";

    /// <summary>
    /// Constructor با Module و Action جداگانه
    /// </summary>
    public RequirePermissionAttribute(string module, string action)
    {
        Module = module;
        Action = action;
    }

    /// <summary>
    /// Constructor با Permission Code کامل (backward compatibility)
    /// </summary>
    public RequirePermissionAttribute(string permission)
    {
        //  Enhanced pattern matching
        var (module, action) = permission.Split('.') switch
        {
            [var m, var a] => (m, a),
            _ => throw new ArgumentException($"Invalid permission format: {permission}. Expected format: Module.Action", nameof(permission))
        };

        Module = module;
        Action = action;
    }

    //  Object creation expression
    public RequirePermissionAttribute() { }
}