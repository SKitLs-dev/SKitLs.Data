﻿using System.Text.Json;

namespace SKitLs.Data.IO.Json
{
    /// <summary>
    /// Provides functionality to write data to a JSON file.
    /// </summary>
    /// <typeparam name="TData">The type of entity to write.</typeparam>
    /// <typeparam name="TId">The type of the entity's identifier.</typeparam>
    public class JsonSingleWriter<TData, TId> : JsonIOBase, IDataWriter<TData> where TData : ModelDso<TId> where TId : notnull, IEquatable<TId>, IComparable<TId>
    {
        /// <inheritdoc/>
        public string GetSourceName() => SourceName;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSingleWriter{TData, TId}"/> class with the specified data file path.
        /// </summary>
        /// <param name="dataPath">The path to the JSON file.</param>
        /// <param name="createNew">Indicates whether to create a new file if the specified file does not exist.</param>
        /// <param name="jsonOptions">The JSON serialization options used for serialization.</param>
        /// <exception cref="FileNotFoundException"></exception>
        public JsonSingleWriter(string dataPath, bool createNew = true, JsonSerializerOptions? jsonOptions = null) : base(dataPath, createNew, jsonOptions)
        {
            if (!File.Exists(DataPath))
            {
                if (CreateNew)
                {
                    File.Create(DataPath).Close();
                }
                else
                {
                    throw new FileNotFoundException($"The file {DataPath} does not exist, and creation of new files is disabled.");
                }
            }
        }

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
                return await WriteDataListAsyncInternal(items, cts);
            }
            catch (Exception ex)
            {
                cts.Cancel();
                throw new IOException($"Error reading data from '{SourceName}' ({GetType().Name}) at file: '{DataPath}'.", ex);
            }
        }

        /// <summary>
        /// Writes a list of data items to the JSON file.
        /// </summary>
        /// <param name="items">The list of data items to write.</param>
        /// <param name="cts">The cancellation token source to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous write operation. The task result contains a boolean indicating whether the write operation was successful.</returns>
        /// <inheritdoc cref="ReadAllItemsAsync{T}(CancellationTokenSource?)"/>
        /// <inheritdoc cref="WriteAllItemsAsync{T}(List{T}, CancellationTokenSource?)"/>
        private async Task<bool> WriteDataListAsyncInternal(IEnumerable<TData> items, CancellationTokenSource? cts = default)
        {
            var allItems = await ReadAllItemsAsync<TData>(cts);
            foreach (var item in items)
            {
                var existingItemIndex = allItems.FindIndex(x => x.GetId().Equals(item.GetId()));
                if (existingItemIndex > -1)
                {
                    allItems[existingItemIndex] = item;
                }
                else
                {
                    allItems.Add(item);
                }
            }
            await WriteAllItemsAsync(allItems, cts);
            return true;
        }

        /// <summary>
        /// Reads all items from the JSON file.
        /// </summary>
        /// <typeparam name="T">The type of items to read.</typeparam>
        /// <param name="cts">The cancellation token source to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous read operation. The task result contains a list of items read from the JSON file.</returns>
        private async Task<List<T>> ReadAllItemsAsync<T>(CancellationTokenSource? cts = default) where T : class
        {
            cts ??= new();
            using var reader = new StreamReader(DataPath);
            var jsonData = await reader.ReadToEndAsync(cts.Token);
            if (string.IsNullOrEmpty(jsonData))
                return [];
            return JsonSerializer.Deserialize<List<T>>(jsonData, JsonOptions) ?? throw new JsonException("Failed to deserialize JSON data.");
        }

        /// <summary>
        /// Writes all items to the JSON file.
        /// </summary>
        /// <typeparam name="T">The type of items to write.</typeparam>
        /// <param name="items">The list of items to write.</param>
        /// <param name="cts">The cancellation token source to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        private async Task WriteAllItemsAsync<T>(List<T> items, CancellationTokenSource? cts = default) where T : class
        {
            cts ??= new();
            var jsonData = JsonSerializer.Serialize(items, JsonOptions);
            await File.WriteAllTextAsync(DataPath, jsonData, cts.Token);
        }
    }
}