namespace SKitLs.Data.IO.Tables
{
    /// <summary>
    /// Base class for reading data from CSV files, providing synchronous and asynchronous operations.
    /// </summary>
    /// <typeparam name="TData">The type to which each row in the CSV file should be converted.</typeparam>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CsvReaderBase{T}"/> class with the specified parameters.
    /// </remarks>
    /// <param name="dataPath">The path to the CSV file.</param>
    /// <param name="createNew">Indicates whether to create a new file if the specified file does not exist.</param>
    /// <param name="startRow">The starting row index for reading data.</param>
    /// <param name="startColumn">The starting column index for reading data.</param>
    /// <param name="endColumn">The ending column index for reading data.</param>
    /// <param name="emptyRowsBreakHit">The maximum number of consecutive empty rows to encounter before stopping.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataPath"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startRow"/>, <paramref name="startColumn"/>, or <paramref name="emptyRowsBreakHit"/> is less than or equal to 0.</exception>
    public abstract class CsvReaderBase<TData>(string dataPath, int startRow = 1, int startColumn = 1, int endColumn = 100, int emptyRowsBreakHit = 3, bool createNew = false) : TableIOBase("CSV File", dataPath, createNew, "CSV Sheet", ",", startRow, startColumn, endColumn, emptyRowsBreakHit), IDataReader<TablePartRow> where TData : class
    {
        /// <inheritdoc/>
        public string GetSourceName() => SourceName;

        /// <summary>
        /// Converts a <see cref="TablePartRow"/> instance into the desired type <typeparamref name="TData"/>.
        /// </summary>
        /// <param name="row">The <see cref="TablePartRow"/> instance to convert.</param>
        /// <returns>The converted object of type <typeparamref name="TData"/>.</returns>
        public abstract TData Convert(TablePartRow row);

        /// <inheritdoc/>
        /// <inheritdoc cref="ReadDataAsync{T}(CancellationTokenSource?)"/>
        public virtual IEnumerable<T> ReadData<T>() where T : class => ReadDataAsync<T>().Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="ReadDataAsync(CancellationTokenSource?)"/>
        public virtual IEnumerable<TablePartRow> ReadData() => ReadDataAsync().Result;

        /// <inheritdoc/>
        /// <exception cref="NotImplementedException">Thrown when conversion to <typeparamref name="T"/> is not implemented.</exception>
        public virtual async Task<IEnumerable<T>> ReadDataAsync<T>(CancellationTokenSource? cts = default) where T : class
        {
            if (typeof(T) == typeof(string))
            {
                var read = await ReadDataAsync(cts);
                return read.Select(x => (x.Join(DataSeparator) as T)!);
            }
            else if (typeof(T) == typeof(TData))
            {
                var read = await ReadDataAsync();
                return read.Select(x => (Convert(x) as T)!);
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(T).Name} is not supported for data conversion.");
            }
        }

        /// <inheritdoc/>
        /// <exception cref="IOException">Thrown when an error occurs during the writing process, with detailed context about the issue.</exception>
        public virtual async Task<IEnumerable<TablePartRow>> ReadDataAsync(CancellationTokenSource? cts = default)
        {
            cts ??= new();
            var result = new List<TablePartRow>();
            try
            {
                if (!File.Exists(DataPath))
                {
                    throw new FileNotFoundException($"The CSV file at path {DataPath} was not found.");
                }

                using var reader = new StreamReader(DataPath);
                var currentRow = 0;
                var line = string.Empty;
                var emptyCounter = 0;

                while ((line = await reader.ReadLineAsync(cts.Token)) is not null)
                {
                    currentRow++;
                    if (currentRow < StartRow)
                        continue;

                    var values = line.Split(DataSeparator);
                    if (values.All(string.IsNullOrEmpty))
                    {
                        emptyCounter++;
                        if (emptyCounter > EmptyRowsBreakHit)
                            break;
                        continue;
                    }

                    emptyCounter = 0;
                    var subValues = new List<string>();
                    var endIndex = Math.Min(values.Length - 1, EndColumn);
                    for (var i = StartColumn; i <= endIndex; i++)
                    {
                        subValues.Add(values[i]);
                        if (i >= values.Length)
                        {
                            cts.Cancel();
                            throw new IndexOutOfRangeException($"Column index {i} is out of range for the provided row values. Reading from col {StartColumn} to {EndColumn}.");
                        }
                    }

                    var row = new TablePartRow(currentRow, StartColumn, endIndex)
                    {
                        Values = subValues,
                    };
                    result.Add(row);
                }
            }
            catch (Exception ex)
            {
                cts.Cancel();
                throw new IOException($"Error on reading data '{SourceName}' ({GetType().Name}) at the file: '{DataPath}'.", ex);
            }
            return result;
        }
    }
}