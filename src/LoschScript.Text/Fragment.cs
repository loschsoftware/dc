using LoschScript.Text.Tooltips;
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
    /// Specifies the type of a navigation target.
    /// </summary>
    public enum NavigationKind : byte
    {
        /// <summary>
        /// Chooses the most fitting navigation kind based on fragment color.
        /// </summary>
        Default,
        /// <summary>
        /// A local variable.
        /// </summary>
        Local,
        /// <summary>
        /// A function.
        /// </summary>
        Function,
        /// <summary>
        /// A type.
        /// </summary>
        Type,
        /// <summary>
        /// A constructor.
        /// </summary>
        Constructor,
        /// <summary>
        /// A field.
        /// </summary>
        Field
    }

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
    /// A tooltip corresponding to the fragment.
    /// </summary>
    public Tooltip ToolTip { get; set; }
    
    /// <summary>
    /// Specifies wheter the fragment can be reached by a navigation command. Supports the "Go to definition" feature of many editors.
    /// </summary>
    public bool IsNavigationTarget { get; set; }

    /// <summary>
    /// For navigation targets that could have different roles based on their fragment color, specifies the kind of navigation target.
    /// </summary>
    public NavigationKind NavigationTargetKind { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="Fragment"/> structure.
    /// </summary>
    /// <param name="line">The line which contains the first character of the fragment.</param>
    /// <param name="column">The column that is the first character of the fragment.</param>
    /// <param name="length">The length of the fragment.</param>
    /// <param name="color">The color of the fragment.</param>
    /// <param name="navigable">Specifies wheter the fragment can be reached by a navigation command.</param>
    /// <param name="navigationKind">Specifies the type of navigation target.</param>
    public Fragment(int line, int column, int length, Color color, bool navigable = false, NavigationKind navigationKind = NavigationKind.Default)
    {
        Line = line;
        Column = column;
        Length = length;
        Color = color;
        IsNavigationTarget = navigable;
        NavigationTargetKind = navigationKind;
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the other parameter; otherwise, <see langword="false"/>.</returns>
    public readonly bool Equals(Fragment other) => 
        Line == other.Line
        && Column == other.Column
        && Length == other.Length
        && Color == other.Color
        && IsNavigationTarget == other.IsNavigationTarget;

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
    public override readonly bool Equals(object obj)
    {
        if (obj is Fragment f)
            return f.Equals(obj);

        return base.Equals(obj);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash representing the current object.</returns>
    public override readonly int GetHashCode()
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