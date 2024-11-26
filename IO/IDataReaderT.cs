namespace SKitLs.Data.IO
{
    /// <summary>
    /// Interface for a generic data reader that reads data of type <typeparamref name="TData"/>.
    /// </summary>
    /// <typeparam name="TData">The type of data to read.</typeparam>
    public interface IDataReader<TData> : IDataReader
    {
        /// <summary>
        /// Reads data of type <typeparamref name="TData"/> synchronously from the source.
        /// </summary>
        /// <returns>An enumerable collection of data items of type <typeparamref name="TData"/>.</returns>
        public IEnumerable<TData> ReadData();

        /// <summary>
        /// Reads data of type <typeparamref name="TData"/> asynchronously from the source.
        /// </summary>
        /// <param name="cts">Optional: cancellation token source to cancel the read operation.</param>
        /// <returns>An enumerable collection of data items of type <typeparamref name="TData"/> as the result.</returns>
        public Task<IEnumerable<TData>> ReadDataAsync(CancellationTokenSource? cts = null);
    }
}