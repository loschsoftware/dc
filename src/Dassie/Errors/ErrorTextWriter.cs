using System;
using System.IO;
using System.Text;

namespace Dassie.Errors;

#pragma warning disable CS1591

/// <summary>
/// Implements a text writer which writes to multiple different text writers at once.
/// </summary>
/// <remarks>
/// Only overrides members used by <see cref="ErrorWriter"/>.
/// </remarks>
public class ErrorTextWriter : TextWriter
{
    private readonly TextWriter[] _writers;
    private bool _isDisposed;

    public ErrorTextWriter(TextWriter[] writers)
    {
        _writers = writers;
    }

    public TextWriter[] Writers => _writers;

    public override Encoding Encoding => throw new NotImplementedException();

    public override void Write(string value)
    {
        foreach (TextWriter writer in _writers)
            writer.Write(value);
    }

    public override void WriteLine()
    {
        foreach (TextWriter writer in _writers)
            writer.WriteLine();
    }

    public override void WriteLine(string value)
    {
        foreach (TextWriter writer in _writers)
            writer.WriteLine(value);
    }

    protected override void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            foreach (TextWriter writer in _writers)
                writer.Dispose();
        }

        _isDisposed = true;
    }

    public new void Dispose()
    {
        Dispose(true);
    }
}