using Microsoft.ML.Runtime.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CustomerDashboard.Functions.Data
{
    /// <summary>
    /// A specialized data source that reads from a single stream
    /// </summary>
    public class StreamDataSource : IMultiStreamSource
    {
        private Stream _inputStream;

        /// <summary>
        /// Initializes a new instance of <see cref="StreamDataSource"/>
        /// </summary>
        /// <param name="inputStream">Input stream to read from</param>
        public StreamDataSource(Stream inputStream)
        {
            _inputStream = inputStream;
        }

        /// <summary>
        /// Gets the number of streams in the data source
        /// </summary>
        public int Count => 1;

        /// <summary>
        /// Gets the path for a stream in the data source
        /// </summary>
        /// <param name="index">Index of the stream</param>
        /// <returns></returns>
        public string GetPathOrNull(int index) => null;

        /// <summary>
        /// Opens a stream for the data source
        /// </summary>
        /// <param name="index">Index of the stream in the data source</param>
        /// <returns>Returns the input stream</returns>
        public Stream Open(int index) => CopyStream(_inputStream);

        /// <summary>
        /// Opens a text reader for the input stream
        /// </summary>
        /// <param name="index">Index of the stream in the data source</param>
        /// <returns>Returns the text reader for the input stream</returns>
        public TextReader OpenTextReader(int index) => new StreamReader(CopyStream(_inputStream));

        /// <summary>
        /// This method ensures that we get a copy of the inner stream rather than the real one.
        /// The inner stream should not be closed by the consumer of this data source, yet it is.
        /// So we fix that by copying the inner stream here.
        /// </summary>
        /// <param name="inner"></param>
        /// <returns></returns>
        private Stream CopyStream(Stream inner)
        {
            var newStream = new MemoryStream();

            inner.Seek(0, SeekOrigin.Begin);
            inner.CopyTo(newStream);

            newStream.Position = 0;

            return newStream;
        }
    }
}
