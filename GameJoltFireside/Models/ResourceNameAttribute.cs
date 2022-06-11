namespace GameJoltFireside.Models;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class ResourceNameAttribute : Attribute
{
    internal string ResourceName { get; }

    internal ResourceNameAttribute(string resourceName)
    {
        ResourceName = resourceName;
    }
}
