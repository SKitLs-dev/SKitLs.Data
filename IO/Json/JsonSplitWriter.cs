using System.Text.Json;

namespace SKitLs.Data.IO.Json
{
    /// <summary>
    /// A data writer that serializes each <typeparamref name="TData"/> item into a separate JSON file.
    /// The file name is based on the item's ID.
    /// </summary>
    /// <typeparam name="TData">The type of data to write. It must implement <see cref="ModelDso{TId}"/>.</typeparam>
    /// <typeparam name="TId">The type of the unique identifier for each data item. Must be comparable and equatable.</typeparam>
    public class JsonSplitWriter<TId, TData> : JsonIOBase, IDataWriter<TData> where TData : ModelDso<TId> where TId : notnull, IEquatable<TId>, IComparable<TId>
    {
        /// <inheritdoc/>
        public string GetSourceName() => SourceName;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSplitWriter{TData, TId}"/> class with the specified data file path.
        /// </summary>
        /// <param name="dataPath">The path to the JSON file.</param>
        /// <param name="createNew">Indicates whether to create a new folder if the specified one does not exist.</param>
        /// <param name="jsonOptions">The JSON serialization options used for serialization.</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public JsonSplitWriter(string dataPath, bool createNew = true, JsonSerializerOptions? jsonOptions = null) : base(dataPath, createNew, jsonOptions, true) { }

        /// <inheritdoc/>
        /// <inheritdoc cref="WriteDataAsync(TData, CancellationTokenSource?)"/>
        public bool WriteData(TData item) => WriteDataAsync(item).Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="WriteDataAsync(TData, CancellationTokenSource?)"/>
        public bool WriteData<T>(T item) where T : class => WriteDataAsync(item).Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="WriteDataListAsync(IEnumerable{TData}, CancellationTokenSource?)"/>
        public bool WriteDataList(IEnumerable<TData> items) => WriteDataListAsync(items).Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="WriteDataListAsync(IEnumerable{TData}, CancellationTokenSource?)"/>
        public bool WriteDataList<T>(IEnumerable<T> items) where T : class => WriteDataListAsync(items).Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="WriteDataListAsync(IEnumerable{TData}, CancellationTokenSource?)"/>
        public async Task<bool> WriteDataAsync(TData item, CancellationTokenSource? cts = default) => await WriteDataListAsync([item], cts);

        /// <inheritdoc/>
        /// <inheritdoc cref="WriteDataListAsync{T}(IEnumerable{T}, CancellationTokenSource?)"/>
        public async Task<bool> WriteDataAsync<T>(T item, CancellationTokenSource? cts = default) where T : class => await WriteDataListAsync([item], cts);

        /// <inheritdoc/>
        /// <inheritdoc cref="WriteDataListAsync(IEnumerable{TData}, CancellationTokenSource?)"/>
        /// <exception cref="NotSupportedException">Thrown when the type parameter is not supported.</exception>
        public async Task<bool> WriteDataListAsync<T>(IEnumerable<T> items, CancellationTokenSource? cts = default) where T : class
        {
            if (!typeof(T).IsAssignableFrom(typeof(TData)))
                throw new NotSupportedException($"Type {typeof(T).Name} is not supported.");

            return await WriteDataListAsync(items.Select(x => (x as TData)!), cts);
        }

        /// <inheritdoc/>
        /// <exception cref="IOException">Thrown when an error occurs during the writing process, with detailed context about the issue.</exception>
        public async Task<bool> WriteDataListAsync(IEnumerable<TData> items, CancellationTokenSource? cts = default)
        {
            cts ??= new();
            try
            {
                foreach (var item in items)
                {
                    var filePath = GetFilePath(item.GetId());
                    await HotIO.SaveJsonAsync(item, filePath, JsonOptions, cts);
                }
                return true;
            }
            catch (Exception ex)
            {
                cts.Cancel();
                throw new IOException($"Error reading data from '{SourceName}' ({GetType().Name}) at file: '{DataPath}'.", ex);
            }
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="DeleteDataAsync(TData, CancellationTokenSource?)"/>
        public bool DeleteData(TData item) => DeleteDataAsync(item).Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="DeleteDataAsync{T}(T, CancellationTokenSource?)"/>
        public bool DeleteData<T>(T item) where T : class => DeleteDataAsync(item).Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="DeleteDataListAsync(IEnumerable{TData}, CancellationTokenSource?)"/>
        public bool DeleteDataList(IEnumerable<TData> items) => DeleteDataListAsync(items).Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="DeleteDataListAsync{T}(IEnumerable{T}, CancellationTokenSource?)"/>
        public bool DeleteDataList<T>(IEnumerable<T> items) where T : class => DeleteDataListAsync(items).Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="DeleteDataListAsync(IEnumerable{TData}, CancellationTokenSource?)"/>
        public async Task<bool> DeleteDataAsync(TData item, CancellationTokenSource? cts = null) => await DeleteDataListAsync([item], cts);

        /// <inheritdoc/>
        /// <inheritdoc cref="DeleteDataListAsync{T}(IEnumerable{T}, CancellationTokenSource?)"/>
        public async Task<bool> DeleteDataAsync<T>(T item, CancellationTokenSource? cts = null) where T : class => await DeleteDataListAsync([item], cts);

        /// <inheritdoc/>
        /// <inheritdoc cref="DeleteDataListAsync(IEnumerable{TData}, CancellationTokenSource?)"/>
        public async Task<bool> DeleteDataListAsync<T>(IEnumerable<T> items, CancellationTokenSource? cts = null) where T : class
        {
            if (!typeof(T).IsAssignableFrom(typeof(TData)))
                throw new NotSupportedException($"Type {typeof(T).Name} is not supported.");

            return await DeleteDataListAsync(items.Select(x => (x as TData)!), cts);
        }

        /// <inheritdoc/>
        /// <exception cref="IOException">Thrown when an error occurs during the writing process, with detailed context about the issue.</exception>
        public async Task<bool> DeleteDataListAsync(IEnumerable<TData> items, CancellationTokenSource? cts = null)
        {
            cts ??= new();
            try
            {
                foreach (var item in items)
                {
                    var filePath = GetFilePath(item.GetId());
                    await Task.Run(() => File.Delete(filePath));
                }
                return true;
            }
            catch (Exception ex)
            {
                cts.Cancel();
                throw new IOException($"Error reading data from '{SourceName}' ({GetType().Name}) at file: '{DataPath}'.", ex);
            }
        }

        /// <summary>
        /// Generates the file path for a specific data item based on its ID.
        /// </summary>
        /// <remarks>
        /// The file path is generated by combining the base data path with the item's ID.
        /// </remarks>
        /// <param name="id">The unique identifier for the data item.</param>
        /// <returns>A string representing the file path where the serialized JSON for the item will be saved.</returns>
        private string GetFilePath(TId id) => Path.Combine(DataPath, $"{id}.json");
    }
}