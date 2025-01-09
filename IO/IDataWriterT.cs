namespace SKitLs.Data.IO
{
    /// <summary>
    /// Interface for a generic data writer that reads data of type <typeparamref name="TData"/>.
    /// </summary>
    /// <typeparam name="TData">The type of data to read.</typeparam>
    public interface IDataWriter<TData> : IDataWriter where TData : class
    {
        /// <summary>
        /// Writes a single data item synchronously to the data source.
        /// </summary>
        /// <param name="item">The data item to write.</param>
        /// <returns><see langword="true"/> if the write operation was successful; otherwise, <see langword="false"/>.</returns>
        public bool WriteData(TData item);

        /// <summary>
        /// Writes multiple data items synchronously to the data source.
        /// </summary>
        /// <param name="items">The collection of data items to write.</param>
        /// <returns><see langword="true"/> if the write operation was successful; otherwise, <see langword="false"/>.</returns>
        public bool WriteDataList(IEnumerable<TData> items);

        /// <summary>
        /// Writes a single data item asynchronously to the data source.
        /// </summary>
        /// <param name="item">The data item to write.</param>
        /// <param name="cts">The optional cancellation token source to cancel the write operation.</param>
        /// <returns><see langword="true"/> if the write operation was successful; otherwise, <see langword="false"/>.</returns>
        public Task<bool> WriteDataAsync(TData item, CancellationTokenSource? cts = null);

        /// <summary>
        /// Writes multiple data items asynchronously to the data source.
        /// </summary>
        /// <param name="items">The collection of data items to write.</param>
        /// <param name="cts">The optional cancellation token source to cancel the write operation.</param>
        /// <returns><see langword="true"/> if the write operation was successful; otherwise, <see langword="false"/>.</returns>
        public Task<bool> WriteDataListAsync(IEnumerable<TData> items, CancellationTokenSource? cts = null);

        /// <summary>
        /// Deletes a single data item synchronously from the data source.
        /// </summary>
        /// <param name="item">The data item to write.</param>
        /// <returns><see langword="true"/> if the write operation was successful; otherwise, <see langword="false"/>.</returns>
        public bool DeleteData(TData item);

        /// <summary>
        /// Deletes multiple data items synchronously from the data source.
        /// </summary>
        /// <param name="items">The collection of data items to write.</param>
        /// <returns><see langword="true"/> if the write operation was successful; otherwise, <see langword="false"/>.</returns>
        public bool DeleteDataList(IEnumerable<TData> items);

        /// <summary>
        /// Deletes a single data item asynchronously from the data source.
        /// </summary>
        /// <param name="item">The data item to write.</param>
        /// <param name="cts">The optional cancellation token source to cancel the write operation.</param>
        /// <returns><see langword="true"/> if the write operation was successful; otherwise, <see langword="false"/>.</returns>
        public Task<bool> DeleteDataAsync(TData item, CancellationTokenSource? cts = null);

        /// <summary>
        /// Deletes multiple data items asynchronously from the data source.
        /// </summary>
        /// <param name="items">The collection of data items to write.</param>
        /// <param name="cts">The optional cancellation token source to cancel the write operation.</param>
        /// <returns><see langword="true"/> if the write operation was successful; otherwise, <see langword="false"/>.</returns>
        public Task<bool> DeleteDataListAsync(IEnumerable<TData> items, CancellationTokenSource? cts = null);
    }
}