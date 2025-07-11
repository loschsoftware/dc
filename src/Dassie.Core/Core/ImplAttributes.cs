using System;

namespace Dassie.Core;

// TODO: Replace by one singular attribute once attributes are fully supported

/// <summary>
/// Sets the 'runtime' implementation flag.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RuntimeImplementedAttribute : Attribute { }

/// <summary>
/// Sets the 'hidebysig' method attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class HideBySigAttribute : Attribute { }

/// <summary>
/// Sets the 'newslot' method attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class NewSlotAttribute : Attribute { }

/// <summary>
/// Sets the 'vararg' method attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class VarArgsAttribute : Attribute { }