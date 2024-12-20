﻿using System.Text.Json;
using System.Xml.Linq;

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
        public string DataPath { get; private set; }

        /// <summary>
        /// Gets or sets the JSON serialization options used for serialization.
        /// </summary>
        public JsonSerializerOptions JsonOptions { get; private init; }

        /// <summary>
        /// Gets or sets a value indicating whether to create a new file if the specified file does not exist.
        /// </summary>
        public bool CreateNew { get; private protected init; }

        private bool _isDirectory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonIOBase"/> class.
        /// </summary>
        /// <param name="dataPath">The path to the JSON file.</param>
        /// <param name="createNew">Indicates whether to create a new file if the specified file does not exist.</param>
        /// <param name="jsonOptions">The JSON serialization options used for serialization.</param>
        public JsonIOBase(string dataPath, bool createNew, JsonSerializerOptions? jsonOptions, bool isDirectory)
        {
            CreateNew = createNew;
            JsonOptions = jsonOptions ?? DefaultJsonOptions;
            
            DataPath = null!;
            _isDirectory = isDirectory;
            UpdatePath(dataPath);
        }

        /// <inheritdoc/>
        public virtual void UpdatePath(string path)
        {
            DataPath = path;
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (_isDirectory)
            {
                if (!Directory.Exists(DataPath))
                {
                    if (CreateNew)
                    {
                        Directory.CreateDirectory(DataPath);
                    }
                    else
                    {
                        throw new DirectoryNotFoundException($"The directory {DataPath} does not exist, and creation of new files is disabled.");
                    }
                }
            }
            else
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
        }
    }
}