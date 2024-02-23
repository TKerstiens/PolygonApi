using System.Reflection;

namespace PolygonApi.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class QueryParameter : Attribute
{
    public string? Name { get; }

    public QueryParameter(string name)
    {
        Name = name;
    }
}