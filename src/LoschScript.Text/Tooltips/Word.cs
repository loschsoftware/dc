using System;
using System.Collections.Generic;

namespace LoschScript.Text.Tooltips;

/// <summary>
/// Represents a word as a <see cref="Text.Fragment"/> combined with a string of text.
/// </summary>
[Serializable]
public struct Word : IEquatable<Word>
{
    /// <summary>
    /// The fragment containing styling information associated with the word.
    /// </summary>
    public Fragment Fragment { get; set; }
    
    /// <summary>
    /// The text of the word.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Compares two instances of <see cref="Word"/>.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns>Wheter the words are equal.</returns>
    public static bool operator==(Word left, Word right) => left.Fragment == right.Fragment && left.Text == right.Text;

    /// <summary>
    /// Compares two instances of <see cref="Word"/>.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns>Wheter the words are not equal.</returns>
    public static bool operator!=(Word left, Word right) => !(left == right);

    /// <summary>
    /// Compares the current instance of <see cref="Word"/> with any object.
    /// </summary>
    /// <param name="obj">The object to compare to.</param>
    /// <returns>Wheter the objects are equal.</returns>
    public override readonly bool Equals(object obj)
    {
        if (obj is Word w)
            return this == w;

        return base.Equals(obj);
    }

    /// <summary>
    /// Compares two instances of <see cref="Word"/>.
    /// </summary>
    /// <param name="other">The word to compare.</param>
    /// <returns>Wheter the words are equal.</returns>
    public readonly bool Equals(Word other) => this == other;

    #pragma warning disable CS1591
    public override readonly int GetHashCode()
    {
        int hashCode = 1848792547;
        hashCode = hashCode * -1521134295 + Fragment.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
        return hashCode;
    }
}