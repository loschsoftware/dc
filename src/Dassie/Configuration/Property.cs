using System;

namespace Dassie.Configuration;

public record Property(
    string Name,
    Type Type,
    object Default = null,
    string Description = null,
    bool CanBeCached = true);