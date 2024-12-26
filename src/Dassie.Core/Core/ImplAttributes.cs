﻿using System;

namespace Dassie.Core;

// TODO: Replace by one singular attribute once attributes are fully supported

/// <summary>
/// Sets the 'runtime' implementation flag.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RuntimeImplemented : Attribute { }

/// <summary>
/// Sets the 'hidebysig' method attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class HideBySig : Attribute { }

/// <summary>
/// Sets the 'newslot' method attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class NewSlot : Attribute { }