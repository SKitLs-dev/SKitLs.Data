﻿using SKitLs.Utils.Extensions.Lists;

namespace SKitLs.Data.IO.Tables
{
    /// <summary>
    /// Base class for writing data to Excel files, providing synchronous and asynchronous operations.
    /// </summary>
    /// <typeparam name="TData">The type to which each row in the Excel file should be converted.</typeparam>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CsvWriterBase{T}"/> class with the specified parameters.
    /// </remarks>
    /// <param name="dataPath">The path to the Excel file.</param>
    /// <param name="createNew">Indicates whether to create a new file if the specified file does not exist.</param>
    /// <param name="startRow">The starting row index for writing data.</param>
    /// <param name="startColumn">The starting column index for writing data.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataPath"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startRow"/> or <paramref name="startColumn"/> is less than or equal to 0.</exception>
    public abstract class CsvWriterBase<TData>(string dataPath, int startRow = 1, int startColumn = 1, bool createNew = true) : TableIOBase("CSV File", dataPath, createNew, "CSV Sheet", ",", startRow, startColumn), IDataWriter<TablePartRow> where TData : class
    {
        /// <inheritdoc/>
        public string GetSourceName() => SourceName;

        /// <summary>
        /// Converts data of type <typeparamref name="TData"/> into an <see cref="TablePartRow"/>.
        /// </summary>
        /// <param name="data">The data of type <typeparamref name="TData"/> to convert.</param>
        /// <returns>An <see cref="TablePartRow"/> instance representing the converted data.</returns>
        public abstract TablePartRow Convert(TData data);

        /// <inheritdoc/>
        /// <inheritdoc cref="WriteDataAsync{T}(T, CancellationTokenSource?)"/>
        public virtual bool WriteData<T>(T item) where T : class => WriteDataAsync(item).Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="WriteDataAsync(TablePartRow, CancellationTokenSource?)"/>
        public virtual bool WriteData(TablePartRow value) => WriteDataAsync(value).Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="WriteDataListAsync{T}(IEnumerable{T}, CancellationTokenSource?)"/>
        public virtual bool WriteDataList<T>(IEnumerable<T> items) where T : class => WriteDataListAsync(items).Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="WriteDataListAsync(IEnumerable{TablePartRow}, CancellationTokenSource?)"/>
        public virtual bool WriteDataList(IEnumerable<TablePartRow> values) => WriteDataListAsync(values).Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="WriteDataListAsync{T}(IEnumerable{T}, CancellationTokenSource?)"/>
        public virtual async Task<bool> WriteDataAsync<T>(T item, CancellationTokenSource? cts = default) where T : class => await WriteDataListAsync([item], cts);

        /// <inheritdoc/>
        /// <inheritdoc cref="WriteDataListAsync(IEnumerable{TablePartRow}, CancellationTokenSource?)"/>
        public virtual async Task<bool> WriteDataAsync(TablePartRow value, CancellationTokenSource? cts = default) => await WriteDataListAsync([value], cts);

        /// <inheritdoc/>
        /// <inheritdoc cref="WriteDataListAsync(IEnumerable{TablePartRow}, CancellationTokenSource?)"/>
        /// <exception cref="NotSupportedException">Thrown when writing data of type <typeparamref name="TData"/> is not supported.</exception>
        public virtual async Task<bool> WriteDataListAsync<T>(IEnumerable<T> items, CancellationTokenSource? cts = default) where T : class
        {
            if (typeof(T) == typeof(TData))
            {
                return await WriteDataListAsync(items.Select(x => Convert((x as TData)!)).ToList(), cts);
            }
            else if (typeof(T) == typeof(TablePartRow))
            {
                return await WriteDataListAsync(items.Select(x => (x as TablePartRow)!).ToList(), cts);
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(T).Name} is not supported for data conversion.");
            }
        }

        /// <inheritdoc/>
        /// <exception cref="IOException">Thrown when an error occurs during the writing process, with detailed context about the issue.</exception>
        public virtual async Task<bool> WriteDataListAsync(IEnumerable<TablePartRow> rows, CancellationTokenSource? cts = default)
        {
            cts ??= new();
            try
            {
                var sourceFile = new FileInfo(DataPath);
                if (!File.Exists(sourceFile.FullName))
                {
                    if (CreateNew)
                    {
                        File.Create(sourceFile.FullName).Close();
                    }
                    else
                    {
                        throw new FileNotFoundException($"The file {DataPath} does not exist, and creation of new files is disabled.");
                    }
                }

                using var writer = new StreamWriter(DataPath);
                var maxI = rows.Max(x => x.RowIndex);
                var maxJ = rows.Max(x => x.Values.Count);
                var dump = (maxJ - 1).ToRange().Select(x => ",").JoinString(string.Empty);
                for (int i = StartRow; i < StartRow + maxI; i++)
                {
                    var row = rows.First(x => x.RowIndex == i);
                    if (row is null)
                    {
                        await writer.WriteLineAsync(dump);
                    }
                    else
                    {
                        await writer.WriteLineAsync(row.Join());
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                cts.Cancel();
                throw new IOException($"Error on saving data '{SourceName}' ({GetType().Name}) at the file: '{DataPath}'.", ex);
            }
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="DeleteDataAsync(TablePartRow, CancellationTokenSource?)"/>
        public bool DeleteData(TablePartRow item) => DeleteDataAsync(item).Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="DeleteDataAsync(TablePartRow, CancellationTokenSource?)"/>
        public bool DeleteData<T>(T item) where T : class => DeleteDataAsync(item).Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="DeleteDataListAsync(IEnumerable{TablePartRow}, CancellationTokenSource?)"/>
        public bool DeleteDataList(IEnumerable<TablePartRow> items) => DeleteDataListAsync(items).Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="DeleteDataListAsync{T}(IEnumerable{T}, CancellationTokenSource?)"/>
        public bool DeleteDataList<T>(IEnumerable<T> items) where T : class => DeleteDataListAsync(items).Result;

        /// <inheritdoc/>
        /// <inheritdoc cref="DeleteDataListAsync(IEnumerable{TablePartRow}, CancellationTokenSource?)"/>
        public async Task<bool> DeleteDataAsync(TablePartRow item, CancellationTokenSource? cts = null) => await DeleteDataListAsync([item], cts);

        /// <inheritdoc/>
        /// <inheritdoc cref="DeleteDataListAsync{T}(IEnumerable{T}, CancellationTokenSource?)"/>
        public async Task<bool> DeleteDataAsync<T>(T item, CancellationTokenSource? cts = null) where T : class => await DeleteDataListAsync([item], cts);

        /// <inheritdoc/>
        /// <inheritdoc cref="DeleteDataListAsync(IEnumerable{TablePartRow}, CancellationTokenSource?)"/>
        public async Task<bool> DeleteDataListAsync<T>(IEnumerable<T> items, CancellationTokenSource? cts = null) where T : class
        {
            if (typeof(T) == typeof(TData))
            {
                return await DeleteDataListAsync(items.Select(x => Convert((x as TData)!)).ToList(), cts);
            }
            else if (typeof(T) == typeof(TablePartRow))
            {
                return await DeleteDataListAsync(items.Select(x => (x as TablePartRow)!).ToList(), cts);
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(T).Name} is not supported for data conversion.");
            }
        }

        /// <inheritdoc/>
        /// <exception cref="IOException">Thrown when an error occurs during the writing process, with detailed context about the issue.</exception>
        public async Task<bool> DeleteDataListAsync(IEnumerable<TablePartRow> rows, CancellationTokenSource? cts = null)
        {
            cts ??= new();
            try
            {
                var sourceFile = new FileInfo(DataPath);
                if (!File.Exists(sourceFile.FullName))
                {
                    return true;
                }

                var lines = HotIO.LoadLines(DataPath);
                using var writer = new StreamWriter(DataPath);
                var maxJ = rows.Max(x => x.Values.Count);
                var dump = (maxJ - 1).ToRange().Select(x => ",").JoinString(string.Empty);
                for (var i = StartRow; i < StartRow + lines.Count; i++)
                {
                    var row = rows.FirstOrDefault(x => x.RowIndex == i);
                    if (row is not null)
                    {
                        await writer.WriteLineAsync(dump);
                    }
                    else
                    {
                        await writer.WriteLineAsync(lines[i - StartRow]);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                cts.Cancel();
                throw new IOException($"Error on deleting data '{SourceName}' ({GetType().Name}) at the file: '{DataPath}'.", ex);
            }
        }
    }
}