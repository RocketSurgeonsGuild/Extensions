﻿using System.Text.Json.Serialization;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal record AnyTypeData
(
    [property: JsonPropertyName("a")]
    string Assembly,
    [property: JsonPropertyName("t")]
    string Type,
    [property: JsonPropertyName("u")]
    bool UnboundGenericType);