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
    private readonly Lock _syncRoot = new();
    private bool _isStored;
    private TextWriter[] _store;

    private TextWriter[] _writers;
    private volatile bool _isDisposed;

    public ErrorTextWriter(TextWriter[] writers)
    {
        _writers = writers ?? throw new ArgumentNullException(nameof(writers));
    }

    public TextWriter[] Writers 
    { 
        get
        {
            lock (_syncRoot)
            {
                return _writers.ToArray(); // Return a copy to prevent external modification
            }
        }
    }

    public override Encoding Encoding => throw new NotImplementedException();

    public override void Write(string value)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        TextWriter[] writersCopy;
        lock (_syncRoot)
        {
            writersCopy = _writers.ToArray();
        }

        foreach (TextWriter writer in writersCopy)
        {
            try
            {
                writer.Write(value);
            }
            catch (ObjectDisposedException)
            {
                // Ignore disposed writers
            }
        }
    }

    public override void WriteLine()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        TextWriter[] writersCopy;
        lock (_syncRoot)
        {
            writersCopy = _writers.ToArray();
        }

        foreach (TextWriter writer in writersCopy)
        {
            try
            {
                writer.WriteLine();
            }
            catch (ObjectDisposedException)
            {
                // Ignore disposed writers
            }
        }
    }

    public override void WriteLine(string value)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        TextWriter[] writersCopy;
        lock (_syncRoot)
        {
            writersCopy = _writers.ToArray();
        }

        foreach (TextWriter writer in writersCopy)
        {
            try
            {
                writer.WriteLine(value);
            }
            catch (ObjectDisposedException)
            {
                // Ignore disposed writers
            }
        }
    }

    public override void Flush()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        TextWriter[] writersCopy;
        lock (_syncRoot)
        {
            writersCopy = _writers.ToArray();
        }

        foreach (TextWriter writer in writersCopy)
        {
            try
            {
                writer.Flush();
            }
            catch (ObjectDisposedException)
            {
                // Ignore disposed writers
            }
        }
    }

    public override async Task FlushAsync()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        TextWriter[] writersCopy;
        lock (_syncRoot)
        {
            writersCopy = _writers.ToArray();
        }

        foreach (TextWriter writer in writersCopy)
        {
            try
            {
                await writer.FlushAsync();
            }
            catch (ObjectDisposedException)
            {
                // Ignore disposed writers
            }
        }
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        TextWriter[] writersCopy;
        lock (_syncRoot)
        {
            writersCopy = _writers.ToArray();
        }

        foreach (TextWriter writer in writersCopy)
        {
            try
            {
                await writer.FlushAsync(cancellationToken);
            }
            catch (ObjectDisposedException)
            {
                // Ignore disposed writers
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            lock (_syncRoot)
            {
                foreach (TextWriter writer in _writers)
                {
                    try
                    {
                        writer.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Ignore already disposed writers
                    }
                }
                _writers = [];
            }
        }

        _isDisposed = true;
        base.Dispose(disposing);
    }

    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Store()
    {
        lock (_syncRoot)
        {
            if (_isStored)
                return;

            _store = _writers;
            _writers = [];
            _isStored = true;
        }
    }

    public void Restore()
    {
        lock (_syncRoot)
        {
            if (!_isStored)
                return;

            _writers = _store;
            _store = null;
            _isStored = false;
        }
    }

    public void AddWriter(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        lock (_syncRoot)
        {
            _writers = _writers.Append(writer).ToArray();
        }
    }

    public void AddWriters(TextWriter[] writers)
    {
        ArgumentNullException.ThrowIfNull(writers);

        lock (_syncRoot)
        {
            _writers = _writers.Concat(writers).ToArray();
        }
    }

    public void SetWriters(TextWriter[] writers)
    {
        ArgumentNullException.ThrowIfNull(writers);

        lock (_syncRoot)
        {
            _writers = writers.ToArray(); // Create a copy to prevent external modification
        }
    }

    public void RemoveWriter(TextWriter writer)
    {
        if (writer == null)
            return;

        lock (_syncRoot)
        {
            _writers = _writers.Where(w => w != writer).ToArray();
        }
    }
}