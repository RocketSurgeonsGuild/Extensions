namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal record ServiceTypeData(char Identifier, AnyTypeData? TypeData = null, TypeFilterData? TypeFilter = null);