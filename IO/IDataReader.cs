namespace SKitLs.Data.IO
{
    /// <summary>
    /// Represents a data reader that provides methods for reading data from a specified source.
    /// </summary>
    public interface IDataReader
    {
        /// <summary>
        /// Gets the name of the data source.
        /// </summary>
        /// <returns>The name of the data source.</returns>
        public string GetSourceName();

        /// <summary>
        /// Reads data from the source synchronously.
        /// </summary>
        /// <typeparam name="T">The type of data to read.</typeparam>
        /// <returns>An enumerable collection of data items of type <typeparamref name="T"/>.</returns>
        public IEnumerable<T> ReadData<T>() where T : class;

        /// <summary>
        /// Reads data from the source asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of data to read.</typeparam>
        /// <param name="cts">Optional: cancellation token source to cancel the read operation.</param>
        /// <returns>An enumerable collection of data items of type <typeparamref name="T"/> as the result.</returns>
        public Task<IEnumerable<T>> ReadDataAsync<T>(CancellationTokenSource? cts = null) where T : class;
    }
}