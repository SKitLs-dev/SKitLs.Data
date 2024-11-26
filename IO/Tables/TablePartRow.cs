namespace SKitLs.Data.IO.Tables
{
    /// <summary>
    /// Represents a row of data in a CSV-like structure, identified by its row index and column range.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TablePartRow"/> class with specified row and column indices.
    /// </remarks>
    /// <param name="row">The index of the row.</param>
    /// <param name="startIndex">The starting index of the first column.</param>
    /// <param name="endIndex">The ending index of the last column.</param>
    public class TablePartRow(int row, int startIndex, int endIndex)
    {
        /// <summary>
        /// Gets or sets the index of the row.
        /// </summary>
        public int RowIndex { get; set; } = row;

        /// <summary>
        /// Gets or sets the starting index of the first column.
        /// </summary>
        public int StartColumnIndex { get; set; } = startIndex;

        /// <summary>
        /// Gets or sets the ending index of the last column.
        /// </summary>
        public int EndColumnIndex { get; set; } = endIndex;

        /// <summary>
        /// Gets or sets the list of values in the row.
        /// </summary>
        public List<string> Values { get; set; } = [];

        /// <summary>
        /// Gets or sets the value at the specified column index within the row.
        /// </summary>
        /// <param name="index">The index of the column.</param>
        /// <returns>The value at the specified column index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the index is out of range.</exception>
        public string this[int index]
        {
            get
            {
                var relativeIndex = index - (StartColumnIndex - 1);
                return relativeIndex >= 0 && relativeIndex < Values.Count
                    ? Values[relativeIndex]
                    : throw new IndexOutOfRangeException($"Index {index} is out of range for the row starting at {StartColumnIndex}.");
            }
            set
            {
                var relativeIndex = index - (StartColumnIndex - 1);
                if (relativeIndex >= 0 && relativeIndex < Values.Count)
                {
                    Values[relativeIndex] = value;
                }
                else
                {
                    throw new IndexOutOfRangeException($"Index {index} is out of range for the row starting at {StartColumnIndex}.");
                }
            }
        }

        /// <summary>
        /// Adds a value to the row.
        /// </summary>
        /// <param name="value">The value to add.</param>
        public void Add(string value) => Values.Add(value);

        /// <summary>
        /// Gets the number of values in the row.
        /// </summary>
        public int Count => Values.Count;

        /// <summary>
        /// Checks if all values in the row are empty or null.
        /// </summary>
        /// <returns><see langword="true"/> if all values are empty; otherwise, <see langword="false"/>.</returns>
        public bool IsEmpty() => Values.All(x => string.IsNullOrEmpty(x));

        /// <summary>
        /// Joins all values in the row into a single string, separated by the specified separator.
        /// </summary>
        /// <param name="sep">The separator to use (default is ",").</param>
        /// <returns>A string that contains all values joined by the separator.</returns>
        public string Join(string? sep = ",") => string.Join(sep, Values);
    }
}