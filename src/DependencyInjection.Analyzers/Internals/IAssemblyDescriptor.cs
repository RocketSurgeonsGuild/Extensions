using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers.Internals;

internal interface IAssemblyDescriptor
{
}

internal struct AssemblyDescriptor : IAssemblyDescriptor
{
    public IAssemblySymbol AssemblySymbol { get; }

    public AssemblyDescriptor(IAssemblySymbol assemblySymbol)
    {
        AssemblySymbol = assemblySymbol;
    }

    public override string ToString()
    {
        return Helpers.GetFullMetadataName(AssemblySymbol);
    }
}

internal struct AllAssemblyDescriptor : IAssemblyDescriptor
{
    public override string ToString()
    {
        return "All";
    }
}

internal struct CompiledAssemblyDescriptor : IAssemblyDescriptor
{
    public INamedTypeSymbol TypeFromAssembly { get; }

    public CompiledAssemblyDescriptor(INamedTypeSymbol typeFromAssembly)
    {
        TypeFromAssembly = typeFromAssembly;
    }

    public override string ToString()
    {
        return Helpers.GetFullMetadataName(TypeFromAssembly);
    }
}

internal struct CompiledAssemblyDependenciesDescriptor : IAssemblyDescriptor
{
    public INamedTypeSymbol TypeFromAssembly { get; }

    public CompiledAssemblyDependenciesDescriptor(INamedTypeSymbol typeFromAssembly)
    {
        TypeFromAssembly = typeFromAssembly;
    }

    public override string ToString()
    {
        return Helpers.GetFullMetadataName(TypeFromAssembly);
    }
}
