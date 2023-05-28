using System.Collections.ObjectModel;

namespace LoschScript.Text.FragmentStore;

/// <summary>
/// Represents a list of files with fragment info.
/// </summary>
public static class FragmentList
{
    /// <summary>
    /// The files of the list.
    /// </summary>
    public static ObservableCollection<FileFragment> Files { get; } = new();
}