using Dassie.Messages;
using Dassie.Text.FragmentStore;
using Dassie.Text.Regions;
using System.Collections.Generic;

namespace Dassie.CodeGeneration.Structure;

/// <summary>
/// File-specific metadata to support various IDE features.
/// </summary>
public class EditorInfo
{
    /// <summary>
    /// The errors in the code.
    /// </summary>
    public List<MessageInfo> Errors { get; set; } = new();

    /// <summary>
    /// Fragments to support semantic syntax highlighting.
    /// </summary>
    public FileFragment Fragments { get; set; }

    /// <summary>
    /// Folding regions to support code folding mechanisms offered by many editors.
    /// </summary>
    public List<FoldingRegion> FoldingRegions { get; set; } = new();

    /// <summary>
    /// A list of lines to support structure guide lines.
    /// </summary>
    public List<GuideLine> GuideLines { get; set; } = new();
}