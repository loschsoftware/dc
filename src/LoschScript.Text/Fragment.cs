using System;
using System.Collections.Generic;

namespace LoschScript.Text;

/// <summary>
/// Represents a fragment of text in an editor. Used for context-specific syntax highlighting.
/// </summary>
[Serializable]
public struct Fragment : IEquatable<Fragment>
{
    /// <summary>
    /// The line which contains the first character of the fragment.
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// The column that is the first character of the fragment.
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// The length of the fragment.
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// The color of the fragment.
    /// </summary>
    public Color Color { get; set; }
    
    /// <summary>
    /// If the desired color is not contained in the <see cref="LoschScript.Text.Color"/> enumeration, this property is used to set a specific color.
    /// </summary>
    public string SpecialColor { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="Fragment"/> class.
    /// </summary>
    /// <param name="line">The line which contains the first character of the fragment.</param>
    /// <param name="column">The column that is the first character of the fragment.</param>
    /// <param name="length">The length of the fragment.</param>
    /// <param name="color">The color of the fragment.</param>
    public Fragment(int line, int column, int length, Color color)
    {
        Line = line;
        Column = column;
        Length = length;
        Color = color;
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the other parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Fragment other) => 
        Line == other.Line
        && Column == other.Column
        && Length == other.Length
        && Color == other.Color;

    /// <summary>
    /// Compares two instances of <see cref="Fragment"/>.
    /// </summary>
    /// <param name="a">The first instance to compare.</param>
    /// <param name="b">The second instance to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="a"/> is equal to <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Fragment a, Fragment b) => a.Equals(b);

    /// <summary>
    /// Compares two instances of <see cref="Fragment"/>.
    /// </summary>
    /// <param name="a">The first instance to compare.</param>
    /// <param name="b">The second instance to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="a"/> is not equal to <paramref name="b"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Fragment a, Fragment b) => !a.Equals(b);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="obj">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the other parameter; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object obj)
    {
        if (obj is Fragment f)
            return f.Equals(obj);

        return base.Equals(obj);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash representing the current object.</returns>
    public override int GetHashCode()
    {
        int hashCode = -113600966;
        hashCode = hashCode * -1521134295 + Line.GetHashCode();
        hashCode = hashCode * -1521134295 + Column.GetHashCode();
        hashCode = hashCode * -1521134295 + Length.GetHashCode();
        hashCode = hashCode * -1521134295 + Color.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SpecialColor);
        return hashCode;
    }
}