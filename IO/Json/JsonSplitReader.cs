using System.Text.Json;

namespace SKitLs.Data.IO.Json
{
    /// <summary>
    /// A data reader that reads JSON data from a directory, where each file represents a separate object of type <typeparamref name="TData"/>.
    /// </summary>
    /// <typeparam name="TData">The type of data to read. It must implement a model with a unique identifier.</typeparam>
    public class JsonSplitReader<TData> : JsonIOBase, IDataReader<TData>
    {
        /// <inheritdoc/>
        public string GetSourceName() => SourceName;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSingleReader{TData}"/> class with the specified data file path.
        /// </summary>
        /// <param name="dataPath">The path to the JSON file.</param>
        /// <param name="createNew">Indicates whether to create a new folder if the specified one does not exist.</param>
        /// <param name="jsonOptions">The JSON serialization options used for serialization.</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public JsonSplitReader(string dataPath, bool createNew = true, JsonSerializerOptions? jsonOptions = null) : base(dataPath, createNew, jsonOptions, true) { }

        /// <inheritdoc/>
        /// <inheritdoc cref="ReadDataAsync{T}(CancellationTokenSource?)"/>
        public IEnumerable<T> ReadData<T>() where T : class => ReadDataAsync<T>().Result;

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
        /// <exception cref="IOException">Thrown when an error occurs during the writing process, with detailed context about the issue.</exception>
        public async Task<IEnumerable<TData>> ReadDataAsync(CancellationTokenSource? cts = default)
        {
            cts ??= new();
            try
            {
                var result = new List<TData>();
                foreach (var fileInfo in Directory.GetFiles(DataPath))
                {
                    var data = await HotIO.LoadJsonAsync<TData>(fileInfo, JsonOptions, cts);
                    if (data is not null)
                        result.Add(data);
                }
                return result;
            }
            catch (Exception ex)
            {
                cts.Cancel();
                throw new IOException($"Error reading data from '{SourceName}' ({GetType().Name}) at file: '{DataPath}'.", ex);
            }
        }
    }
}