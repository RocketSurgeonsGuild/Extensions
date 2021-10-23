namespace Rocket.Surgery.DependencyInjection.Analyzers.Internals;

internal enum NamespaceFilter
{
    Exact = 1,
    In = 2,
    NotIn = 3
}

internal enum TextDirectionFilter
{
    StartsWith,
    EndsWith,
    Contains
}
