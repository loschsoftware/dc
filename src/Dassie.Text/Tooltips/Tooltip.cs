using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dassie.Text.Tooltips;

/// <summary>
/// Represents a tooltip as a sequence of <see cref="Word"/>s.
/// </summary>
public class Tooltip : IEquatable<Tooltip>
{
    private ObservableCollection<Word> _words = new();
    /// <summary>
    /// The words representing the current tooltip.
    /// </summary>
    public ObservableCollection<Word> Words
    {
        get => _words;

        set
        {
            _words = value;
            RawText = new(_words.Select(w => w.Text).SelectMany(s => s.ToCharArray()).ToArray());
        }
    }

    /// <summary>
    /// Compares two tooltips.
    /// </summary>
    /// <param name="left">The first tooltip.</param>
    /// <param name="right">The second tooltip.</param>
    /// <returns>Wheter the tooltips are equal.</returns>
    public static bool operator ==(Tooltip left, Tooltip right)
    {
        left ??= new();
        right ??= new();

        return left.Words.SequenceEqual(right.Words) && left.IconResourceName == right.IconResourceName;
    }

    /// <summary>
    /// Compares two tooltips.
    /// </summary>
    /// <param name="left">The first tooltip.</param>
    /// <param name="right">The second tooltip.</param>
    /// <returns>Wheter the tooltips are not equal.</returns>
    public static bool operator !=(Tooltip left, Tooltip right) => !(left == right);

    /// <summary>
    /// The resource name of the icon associated with the tooltip. Used by LSEdit.
    /// </summary>
    public string IconResourceName { get; set; }

    /// <summary>
    /// The raw text of the current tooltip without styling information.
    /// </summary>
    public string RawText { get; private set; }

    /// <summary>
    /// Compares the current instance of <see cref="Tooltip"/> with any object.
    /// </summary>
    /// <param name="obj">The object to compare to.</param>
    /// <returns>Wheter the objects are equal.</returns>
    public override bool Equals(object obj)
    {
        if (obj is Tooltip t)
            return this == t;

        return base.Equals(obj);
    }

    /// <summary>
    /// Compares the current instance with another tooltip.
    /// </summary>
    /// <param name="other">The tooltip to compare.</param>
    /// <returns>Wheter the tooltips are equal.</returns>
    public bool Equals(Tooltip other) => this == other;

#pragma warning disable CS1591
    public override int GetHashCode()
    {
        int hashCode = 164038694;
        hashCode = hashCode * -1521134295 + EqualityComparer<ObservableCollection<Word>>.Default.GetHashCode(_words);
        hashCode = hashCode * -1521134295 + EqualityComparer<ObservableCollection<Word>>.Default.GetHashCode(Words);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(IconResourceName);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(RawText);
        return hashCode;
    }
}