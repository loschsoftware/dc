using System;

namespace LoschScript.Runtime;

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
    /// Creates a new instance of <see cref="ConstraintViolationException"/> with the specified exception message.
    /// </summary>
    /// <param name="message">The exception message to display.</param>
    public ConstraintViolationException(string message) : base(message) { }

    /// <summary>
    /// Creates a new instance of <see cref="ConstraintViolationException"/> with the specified message and inner exception.
    /// </summary>
    /// <param name="message">The exception message to display.</param>
    /// <param name="inner">The inner exception.</param>
    public ConstraintViolationException(string message, Exception inner) : base(message, inner) { }
}