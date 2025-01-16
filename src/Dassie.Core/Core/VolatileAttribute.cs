using System;

namespace Dassie.Core;

/// <summary>
/// Applies the 'IsVolatile' required custom modifier (modreq) to a field.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class VolatileAttribute : Attribute { }