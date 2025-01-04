using System.Reflection.Emit;

namespace Dassie.CodeGeneration;

/// <summary>
/// Emits IL for literals of type <see cref="decimal"/>.
/// </summary>
internal static class DecimalLiteralCodeGeneration
{
    /// <summary>
    /// Emits the IL necessary to construct an object of type <see cref="decimal"/>.
    /// </summary>
    /// <param name="value">The value to initialize the <see cref="decimal"/> with.</param>
    public static void EmitDecimal(decimal value)
    {
        if (decimal.IsInteger(value))
        {
            if (value < int.MaxValue)
            {
                EmitLdcI4((int)value);
                CurrentMethod.IL.Emit(OpCodes.Newobj, typeof(decimal).GetConstructor([typeof(int)]));
                return;
            }
            else if (value < long.MaxValue)
            {
                CurrentMethod.IL.Emit(OpCodes.Ldc_I8, (long)value);
                CurrentMethod.IL.Emit(OpCodes.Newobj, typeof(decimal).GetConstructor([typeof(long)]));
                return;
            }
        }

        int[] bits = decimal.GetBits(value);
        int lo = bits[0];
        int mid = bits[1];
        int hi = bits[2];
        bool isNegative = (bits[3] & 0x80000000) != 0;
        byte scale = (byte)((bits[3] >> 16) & 0x7F);

        EmitLdcI4(lo);
        EmitLdcI4(mid);
        EmitLdcI4(hi);
        EmitLdcI4(isNegative ? 1 : 0);
        EmitLdcI4(scale);

        CurrentMethod.IL.Emit(OpCodes.Newobj, typeof(decimal).GetConstructor([typeof(int), typeof(int), typeof(int), typeof(bool), typeof(byte)]));
    }
}