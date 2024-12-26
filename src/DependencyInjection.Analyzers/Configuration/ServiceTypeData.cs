namespace Rocket.Surgery.DependencyInjection.Analyzers;

public record ServiceTypeData(char Identifier, AnyTypeData? TypeData = null, TypeFilterData? TypeFilter = null);
