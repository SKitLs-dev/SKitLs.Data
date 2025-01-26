using System.Data;
using System.Text.Json;

namespace SKitLs.Data.IO.Json
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonSingleReader{TData}"/> class with the specified data file path.
    /// </summary>
    /// <typeparam name="TData">The type of entity to read.</typeparam>
    /// <remarks>
    /// Provides functionality to read data from a JSON file.
    /// </remarks>
    /// <param name="dataPath">The path to the JSON file.</param>
    /// <param name="createNew">Indicates whether to create a new file if the specified file does not exist.</param>
    /// <param name="jsonOptions">The JSON serialization options used for serialization.</param>
    /// <exception cref="FileNotFoundException"></exception>
    public class JsonSingleReader<TData>(string dataPath, bool createNew = true, JsonSerializerOptions? jsonOptions = null) : JsonIOBase(dataPath, createNew, jsonOptions, false), IDataReader<TData>
    {
        /// <inheritdoc/>
        public string GetSourceName() => SourceName;

        /// <inheritdoc/>
        /// <inheritdoc cref="ReadDataAsync{T}(CancellationTokenSource?)"/>
        public virtual IEnumerable<T> ReadData<T>() where T : class => ReadDataAsync<T>().Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="ReadDataAsync(CancellationTokenSource?)"/>
        public virtual IEnumerable<TData> ReadData() => ReadDataAsync().Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="ReadDataAsync(CancellationTokenSource?)"/>
        /// <exception cref="NotImplementedException">Thrown when conversion to <typeparamref name="T"/> is not implemented.</exception>
        public virtual async Task<IEnumerable<T>> ReadDataAsync<T>(CancellationTokenSource? cts = default) where T : class
        {
            if (!typeof(T).IsAssignableFrom(typeof(TData)))
                throw new NotSupportedException($"Type {typeof(T).Name} is not supported.");

            return (await ReadDataAsync()).Select(x => (x as T)!);
        }

        /// <inheritdoc/>
        /// <exception cref="IOException">Thrown when an error occurs during file access or reading.</exception>
        public virtual async Task<IEnumerable<TData>> ReadDataAsync(CancellationTokenSource? cts = default)
        {
            cts ??= new();
            try
            {
                var jsonData = await File.ReadAllTextAsync(DataPath, cts.Token);
                return JsonSerializer.Deserialize<List<TData>>(jsonData, JsonOptions) ?? throw new JsonException("Deserialization returned null.");
            }
            catch (Exception ex)
            {
                cts.Cancel();
                throw new IOException($"Error reading data from '{SourceName}' ({GetType().Name}) at file: '{DataPath}'.", ex);
            }
        }
    }
}