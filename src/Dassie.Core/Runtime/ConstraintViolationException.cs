using System;

namespace Dassie.Runtime;

/// <summary>
/// Exception raised when an unchecked parameter constraint is violated.
/// </summary>
[Serializable]
public class ConstraintViolationException : Exception
{
	/// <summary>
	/// Creates a new instance of <see cref="ConstraintViolationException"/>.
	/// </summary>
	public ConstraintViolationException() { }

    /// <summary>
    /// The name of the parameter that violates a constraint.
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    /// The constraint that was violated.
    /// </summary>
    public string Constraint { get; }

    /// <summary>
    /// Creates a new instance of <see cref="ConstraintViolationException"/> with the specified exception message.
    /// </summary>
    /// <param name="parameter">The parameter that violates a constraint.</param>
    /// <param name="constraint">The constraint that was violated.</param>
    public ConstraintViolationException(string parameter, string constraint) : base($"Parameter '{parameter}' violates constraint '{constraint}'.")
    {
        ParameterName = parameter;
        Constraint = constraint;
    }
}