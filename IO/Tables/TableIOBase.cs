namespace SKitLs.Data.IO.Tables
{
    /// <summary>
    /// Base class for handling common properties and methods related to table-based (CSV, Excel, etc) I/O operations.
    /// </summary>
    public class TableIOBase
    {
        /// <summary>
        /// Gets or sets the name of the data source, defaulting to "CSV File".
        /// </summary>
        public string SourceName { get; private init; }

        /// <summary>
        /// Gets or sets the data separator used for joining data fields in CSV (default is ",").
        /// </summary>
        public string DataSeparator { get; private init; }

        /// <summary>
        /// Gets or sets the maximum number of consecutive empty rows to encounter before stopping.
        /// </summary>
        public int EmptyRowsBreakHit { get; private init; }

        /// <summary>
        /// Gets or sets the row separator used for separating rows in CSV (default is newline "\n").
        /// </summary>
        public static string RowSep { get; set; } = "\n";

        /// <summary>
        /// Gets or sets the path to the CSV file.
        /// </summary>
        public string DataPath { get; private init; }

        private string? _worksheetName;
        /// <summary>
        /// Gets or sets the name of the logical dataset.
        /// </summary>
        public string WorksheetName { get => _worksheetName ?? throw new ArgumentNullException(nameof(_worksheetName)); }

        /// <summary>
        /// Gets or sets the starting row index for reading/writing data from the CSV file (default is 1).
        /// </summary>
        public int StartRow { get; private init; }

        /// <summary>
        /// Gets or sets the starting column index for reading/writing data from the CSV file (default is 1).
        /// </summary>
        public int StartColumn { get; private init; }

        /// <summary>
        /// Gets or sets the ending column index for reading data from the CSV file (default is 100).
        /// </summary>
        public int EndColumn { get; private init; }

        /// <summary>
        /// Gets or sets a value indicating whether to create a new file if the specified file does not exist.
        /// </summary>
        public bool CreateNew { get; private init; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableIOBase"/> class with the specified parameters.
        /// </summary>
        /// <param name="sourceName">The displaying source file name.</param>
        /// <param name="dataPath">The path to the CSV file.</param>
        /// <param name="createNew">Indicates whether to create a new file if the specified file does not exist.</param>
        /// <param name="worksheetName">The name of the dataset (used as a logical grouping name).</param>
        /// <param name="dataSep">The character used to separate data fields (default is ",").</param>
        /// <param name="startRow">The starting row index for reading data (default is 1).</param>
        /// <param name="startColumn">The starting column index for reading data (default is 1).</param>
        /// <param name="endColumn">The ending column index for reading data (default is 100).</param>
        /// <param name="emptyRowsBreakHit">The maximum number of consecutive empty rows to encounter before stopping.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataPath"/> or <paramref name="worksheetName"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startRow"/>, <paramref name="startColumn"/>, or <paramref name="endColumn"/> is less than or equal to 0.</exception>
        public TableIOBase(string sourceName, string dataPath, bool createNew, string? worksheetName, string dataSep, int startRow = 1, int startColumn = 1, int endColumn = 100, int emptyRowsBreakHit = 3)
        {
            if (string.IsNullOrEmpty(dataPath))
                throw new ArgumentNullException(nameof(dataPath));

            SourceName = sourceName;
            DataPath = dataPath;
            DataSeparator = dataSep;
            EmptyRowsBreakHit = emptyRowsBreakHit > 0 ? emptyRowsBreakHit : throw new ArgumentOutOfRangeException(nameof(emptyRowsBreakHit));
            _worksheetName = worksheetName;
            StartRow = startRow > 0 ? startRow : throw new ArgumentOutOfRangeException(nameof(startRow));
            StartColumn = startColumn > 0 ? startColumn : throw new ArgumentOutOfRangeException(nameof(startColumn));
            EndColumn = endColumn > 0 ? endColumn : throw new ArgumentOutOfRangeException(nameof(endColumn));
            CreateNew = createNew;

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
    }
}