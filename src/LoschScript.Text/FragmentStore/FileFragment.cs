using System;
using System.Collections.Generic;

namespace LoschScript.Text.FragmentStore;

/// <summary>
/// Represents a list of fragments belonging to a file.
/// </summary>
[Serializable]
public struct FileFragment
{
    /// <summary>
    /// The path to the file.
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// The list of fragments belonging to the file.
    /// </summary>
    public List<Fragment> Fragments { get; set; }
}