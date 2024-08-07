﻿using System;

namespace Dassie.Extensions;

/// <summary>
/// Defines a Dassie compiler extension.
/// </summary>
public interface IPackage
{
    /// <summary>
    /// The metadata for the extension.
    /// </summary>
    public PackageMetadata Metadata { get; }

    /// <summary>
    /// An array of compiler commands added by this extension.
    /// </summary>
    public Type[] Commands { get; }
}