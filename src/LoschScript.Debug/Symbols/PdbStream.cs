using System;
using System.Collections.Generic;
using System.IO;

namespace RedFlag.Symbols
{
    /// <summary>
    /// Representation of a stream within a PDB file
    /// </summary>
    internal sealed class PdbStream : Stream
    {
        #region Member variables

        /// <summary>
        /// The stream that contains the PDB file
        /// </summary>
        private readonly Stream m_Stream;

        /// <summary>
        /// The size of a page
        /// </summary>
        private readonly int m_PageSize;

        /// <summary>
        /// The pages that should make up this stream
        /// </summary>
        private readonly List<int> m_Pages;

        /// <summary>
        /// The current position within the stream
        /// </summary>
        private long m_Pos;

        #endregion

        #region Initialisation

        public PdbStream(Stream stream, int pageSize, IEnumerable<int> pages)
        {
            // Sanity check
            if (stream == null) throw new ArgumentNullException("stream");
            if (pages == null) throw new ArgumentNullException("pages");

            // Set up
            m_Stream = stream;
            m_PageSize = pageSize;
            m_Pages = new List<int>(pages);
        }

        #endregion

        #region Stream implementation

        /// <summary>
        ///                     When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">
        ///                     An I/O error occurs. 
        ///                 </exception><filterpriority>2</filterpriority>
        public override void Flush()
        {
            m_Stream.Flush();
        }

        /// <summary>
        ///                     When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <returns>
        ///                     The new position within the current stream.
        /// </returns>
        /// <param name="offset">
        ///                     A byte offset relative to the <paramref name="origin" /> parameter. 
        ///                 </param>
        /// <param name="origin">
        ///                     A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position. 
        ///                 </param>
        /// <exception cref="T:System.IO.IOException">
        ///                     An I/O error occurs. 
        ///                 </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The stream does not support seeking, such as if the stream is constructed from a pipe or console output. 
        ///                 </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        ///                     Methods were called after the stream was closed. 
        ///                 </exception><filterpriority>1</filterpriority>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    m_Pos = offset;
                    break;
                case SeekOrigin.Current:
                    m_Pos += offset;
                    break;
                case SeekOrigin.End:
                default:
                    // Other types of seek aren't supported
                    throw new InvalidOperationException();
            }

            return m_Pos;
        }

        /// <summary>
        ///                     When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">
        ///                     The desired length of the current stream in bytes. 
        ///                 </param>
        /// <exception cref="T:System.IO.IOException">
        ///                     An I/O error occurs. 
        ///                 </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. 
        ///                 </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        ///                     Methods were called after the stream was closed. 
        ///                 </exception><filterpriority>2</filterpriority>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///                     When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <returns>
        ///                     The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <param name="buffer">
        ///                     An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source. 
        ///                 </param>
        /// <param name="offset">
        ///                     The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream. 
        ///                 </param>
        /// <param name="count">
        ///                     The maximum number of bytes to be read from the current stream. 
        ///                 </param>
        /// <exception cref="T:System.ArgumentException">
        ///                     The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length. 
        ///                 </exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" /> is null. 
        ///                 </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset" /> or <paramref name="count" /> is negative. 
        ///                 </exception>
        /// <exception cref="T:System.IO.IOException">
        ///                     An I/O error occurs. 
        ///                 </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The stream does not support reading. 
        ///                 </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        ///                     Methods were called after the stream was closed. 
        ///                 </exception><filterpriority>1</filterpriority>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (m_Pos + count > Length) count = (int)(Length - m_Pos);

            int numRead = 0;
            int remaining = count;
            int pos = offset;

            while (numRead < count)
            {
                // Number of bytes to read this pass
                int toRead = remaining;

                // Work out the current page
                int page = (int)(m_Pos / m_PageSize);
                int pageOffset = (int)(m_Pos % m_PageSize);
                int pageRemaining = (int)(((page + 1) * m_PageSize) - m_Pos);

                // Don't read any more than to the end of the page
                if (pageRemaining < toRead) toRead = pageRemaining;

                // Read the bytes from the current page
                m_Stream.Position = ((m_Pages[page]) * m_PageSize) + pageOffset;
                m_Stream.Read(buffer, pos, toRead);

                // Update the counters
                m_Pos += toRead;
                numRead += toRead;
                pos += toRead;
                remaining -= toRead;
            }

            return numRead;
        }

        /// <summary>
        ///                     When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">
        ///                     An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream. 
        ///                 </param>
        /// <param name="offset">
        ///                     The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream. 
        ///                 </param>
        /// <param name="count">
        ///                     The number of bytes to be written to the current stream. 
        ///                 </param>
        /// <exception cref="T:System.ArgumentException">
        ///                     The sum of <paramref name="offset" /> and <paramref name="count" /> is greater than the buffer length. 
        ///                 </exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" /> is null. 
        ///                 </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset" /> or <paramref name="count" /> is negative. 
        ///                 </exception>
        /// <exception cref="T:System.IO.IOException">
        ///                     An I/O error occurs. 
        ///                 </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The stream does not support writing. 
        ///                 </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        ///                     Methods were called after the stream was closed. 
        ///                 </exception><filterpriority>1</filterpriority>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (m_Pos + count > Length) count = (int)(Length - m_Pos);

            int numWritten = 0;
            int remaining = count;
            int pos = offset;

            while (numWritten < count)
            {
                // Number of bytes to read this pass
                int toWrite = remaining;

                // Work out the current page
                int page = (int)(m_Pos / m_PageSize);
                int pageOffset = (int)(m_Pos % m_PageSize);
                int pageRemaining = (int)(((page + 1) * m_PageSize) - m_Pos);

                // Don't read any more than to the end of the page
                if (pageRemaining < toWrite) toWrite = pageRemaining;

                // Write the bytes from the current page
                m_Stream.Position = ((m_Pages[page]) * m_PageSize) + pageOffset;
                m_Stream.Write(buffer, pos, toWrite);

                // Update the counters
                m_Pos += toWrite;
                numWritten += toWrite;
                pos += toWrite;
                remaining -= toWrite;
            }
        }

        /// <summary>
        ///                     When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>
        /// true if the stream supports reading; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanRead
        {
            get { return m_Stream.CanRead; }
        }

        /// <summary>
        ///                     When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>
        /// true if the stream supports seeking; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanSeek
        {
            get { return true; }
        }

        /// <summary>
        ///                     When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>
        /// true if the stream supports writing; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanWrite
        {
            get { return m_Stream.CanWrite; }
        }

        /// <summary>
        ///                     When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <returns>
        ///                     A long value representing the length of the stream in bytes.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        ///                     A class derived from Stream does not support seeking. 
        ///                 </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        ///                     Methods were called after the stream was closed. 
        ///                 </exception><filterpriority>1</filterpriority>
        public override long Length
        {
            get { return m_Pages.Count * m_PageSize; }
        }

        /// <summary>
        ///                     When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>
        ///                     The current position within the stream.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">
        ///                     An I/O error occurs. 
        ///                 </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///                     The stream does not support seeking. 
        ///                 </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        ///                     Methods were called after the stream was closed. 
        ///                 </exception><filterpriority>1</filterpriority>
        public override long Position
        {
            get { return m_Pos; }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        #endregion
    }
}
