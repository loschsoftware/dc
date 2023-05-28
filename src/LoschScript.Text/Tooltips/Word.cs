using System;

namespace LoschScript.Text.Tooltips;

/// <summary>
/// Represents a word as a <see cref="LoschScript.Text.Fragment"/> combined with a string of text.
/// </summary>
[Serializable]
public struct Word
{
    /// <summary>
    /// The fragment containing styling information associated with the word.
    /// </summary>
    public Fragment Fragment { get; set; }
    
    /// <summary>
    /// The text of the word.
    /// </summary>
    public string Text { get; set; }
}