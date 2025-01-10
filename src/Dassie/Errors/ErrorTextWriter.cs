using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
    private bool _isStored;
    private TextWriter[] _store;

    private TextWriter[] _writers;
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

    public override void Flush()
    {
        foreach (TextWriter writer in _writers)
            writer.Flush();
    }

    public override async Task FlushAsync()
    {
        foreach (TextWriter writer in _writers)
            await writer.FlushAsync();
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        foreach (TextWriter writer in _writers)
            await writer.FlushAsync(cancellationToken);
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

    public void Store()
    {
        if (_isStored)
            return;

        _store = _writers;
        _writers = [];
        _isStored = true;
    }

    public void Restore()
    {
        if (_isStored)
            return;

        _writers = _store;
    }

    public void AddWriter(TextWriter writer)
    {
        _writers = _writers.Append(writer).ToArray();
    }

    public void AddWriters(TextWriter[] writer)
    {
        _writers = _writers.Concat(writer).ToArray();
    }

    public void SetWriters(TextWriter[] writers)
    {
        _writers = writers;
    }

    public void RemoveWriter(TextWriter writer)
    {
        _writers = _writers.Where(w => w != writer).ToArray();
    }
}