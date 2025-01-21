using System;

namespace Dassie.Core;

#pragma warning disable IDE1006

/// <summary>
/// Defines 'int8' as an alias for <see cref="System.SByte"/>.
/// </summary>
[Alias(typeof(sbyte))]
public struct int8 { }

/// <summary>
/// Defines 'uint8' as an alias for <see cref="System.Byte"/>.
/// </summary>
[Alias(typeof(byte))]
public struct uint8 { }

/// <summary>
/// Defines 'int16' as an alias for <see cref="System.Int16"/>.
/// </summary>
[Alias(typeof(short))]
public struct int16 { }

/// <summary>
/// Defines 'uint16' as an alias for <see cref="System.UInt16"/>.
/// </summary>
[Alias(typeof(ushort))]
public struct uint16 { }

/// <summary>
/// Defines 'int32' as an alias for <see cref="System.Int32"/>.
/// </summary>
[Alias(typeof(int))]
public struct int32 { }

/// <summary>
/// Defines 'uint32' as an alias for <see cref="System.UInt32"/>.
/// </summary>
[Alias(typeof(uint))]
public struct uint32 { }

/// <summary>
/// Defines 'int' as an alias for <see cref="System.Int32"/>.
/// </summary>
[Alias(typeof(int))]
public struct @int { }

/// <summary>
/// Defines 'uint' as an alias for <see cref="System.UInt32"/>.
/// </summary>
[Alias(typeof(uint))]
public struct @uint { }

/// <summary>
/// Defines 'int64' as an alias for <see cref="System.Int64"/>.
/// </summary>
[Alias(typeof(long))]
public struct int64 { }

/// <summary>
/// Defines 'uint64' as an alias for <see cref="System.UInt64"/>.
/// </summary>
[Alias(typeof(ulong))]
public struct uint64 { }

/// <summary>
/// Defines 'int128' as an alias for <see cref="System.Int128"/>.
/// </summary>
[Alias(typeof(Int128))]
public struct int128 { }

/// <summary>
/// Defines 'uint128' as an alias for <see cref="System.UInt128"/>.
/// </summary>
[Alias(typeof(UInt128))]
public struct uint128 { }

/// <summary>
/// Defines 'native' as an alias for <see cref="System.IntPtr"/>.
/// </summary>
[Alias(typeof(nint))]
public struct @native { }

/// <summary>
/// Defines 'unative' as an alias for <see cref="System.UIntPtr"/>.
/// </summary>
[Alias(typeof(nuint))]
public struct @unative { }

/// <summary>
/// Defines 'float16' as an alias for <see cref="System.Half"/>.
/// </summary>
[Alias(typeof(Half))]
public struct float16 { }

/// <summary>
/// Defines 'float32' as an alias for <see cref="System.Single"/>.
/// </summary>
[Alias(typeof(float))]
public struct float32 { }

/// <summary>
/// Defines 'float64' as an alias for <see cref="System.Double"/>.
/// </summary>
[Alias(typeof(double))]
public struct float64 { }

/// <summary>
/// Defines 'decimal' as an alias for <see cref="System.Decimal"/>.
/// </summary>
[Alias(typeof(decimal))]
public struct @decimal { }

/// <summary>
/// Defines 'bool' as an alias for <see cref="System.Boolean"/>.
/// </summary>
[Alias(typeof(bool))]
public struct @bool { }

/// <summary>
/// Defines 'null' as an alias for <see cref="System.Void"/>.
/// </summary>
[Alias(typeof(void))]
public sealed class @null { }

/// <summary>
/// Defines 'object' as an alias for <see cref="System.Object"/>.
/// </summary>
[Alias(typeof(object))]
public sealed class @object { }

/// <summary>
/// Defines 'string' as an alias for <see cref="System.String"/>.
/// </summary>
[Alias(typeof(string))]
public sealed class @string { }

/// <summary>
/// Defines 'char' as an alias for <see cref="System.Char"/>.
/// </summary>
[Alias(typeof(char))]
public struct @char { }