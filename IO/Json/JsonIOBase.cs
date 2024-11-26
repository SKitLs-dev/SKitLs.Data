using System.Text.Json;

namespace SKitLs.Data.IO.Json
{
    /// <summary>
    /// Provides a base class for JSON input/output operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="JsonIOBase"/> class with the specified data path.
    /// </remarks>
    public class JsonIOBase
    {
        /// <summary>
        /// Gets or sets the default JSON serialization options used for serialization.
        /// </summary>
        public static JsonSerializerOptions DefaultJsonOptions { get; set; } = new()
        {
            WriteIndented = true,
        };

        /// <summary>
        /// Gets or sets the name of the data source, defaulting to "Json File".
        /// </summary>
        public static string SourceName { get; set; } = "Json File";

        /// <summary>
        /// Gets or sets the path to the JSON file.
        /// </summary>
        public string DataPath { get; private init; }

        /// <summary>
        /// Gets or sets the JSON serialization options used for serialization.
        /// </summary>
        public JsonSerializerOptions JsonOptions { get; private init; }

        /// <summary>
        /// Gets or sets a value indicating whether to create a new file if the specified file does not exist.
        /// </summary>
        public bool CreateNew { get; private protected init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonIOBase"/> class.
        /// </summary>
        /// <param name="dataPath">The path to the JSON file.</param>
        /// <param name="createNew">Indicates whether to create a new file if the specified file does not exist.</param>
        /// <param name="jsonOptions">The JSON serialization options used for serialization.</param>
        public JsonIOBase(string dataPath, bool createNew, JsonSerializerOptions? jsonOptions)
        {
            if (string.IsNullOrEmpty(dataPath))
                throw new ArgumentNullException(nameof(dataPath));

            CreateNew = createNew;
            DataPath = dataPath;
            JsonOptions = jsonOptions ?? DefaultJsonOptions;
        }
    }
}