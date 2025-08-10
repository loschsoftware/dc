using System;

namespace Dassie;

/// <summary>
/// Thrown when a compilation process is interrupted due to the maximum error message count being reached.
/// </summary>
internal class TerminationException : Exception { }