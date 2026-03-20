using System;

namespace Dassie.Configuration;

/// <summary>
/// The abstract base class for all references.
/// </summary>
[Serializable]
public abstract class Reference : ConfigObject
{
    /// <inheritdoc/>
    protected Reference(PropertyStore store) : base(store) { }
}