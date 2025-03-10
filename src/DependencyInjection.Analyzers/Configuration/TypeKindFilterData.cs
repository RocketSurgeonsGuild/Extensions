﻿using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

public record TypeKindFilterData
(
    [property: JsonPropertyName("f")]
    bool Include,
    [property: JsonPropertyName("t")]
    ImmutableArray<TypeKind> TypeKinds);
