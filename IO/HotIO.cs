using System.Reflection;
using System.Text.Json;

namespace SKitLs.Data.IO
{
    /// <summary>
    /// Provides utility methods for saving and loading data to and from files.
    /// </summary>
    public static class HotIO
    {
        /// <summary>
        /// Gets or sets the JSON files extensions. Use <see langword="null"/> to prevent from automatic Json path fit.
        /// </summary>
        public static string? JsonExtension { get; set; } = ".json";

        /// <summary>
        /// Determines whether relative path should be unified by appending Executing Assembly location.
        /// </summary>
        public static bool AutoFitPath { get; set; } = true;

        /// <summary>
        /// Checks and corrects the provided <paramref name="path"/> as a *.json using <see cref="JsonExtension"/> property value.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string FitJsonPath(string path)
        {
            if (JsonExtension is not null && !path.EndsWith(JsonExtension))
                path += JsonExtension;
            return path;
        }

        /// <summary>
        /// Fits relative path by appending Executing Assembly location.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string FitAbsolutePath(string path)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!Path.IsPathRooted(path) && AutoFitPath && assemblyPath is not null)
            {
                return Path.Combine(assemblyPath, path);
            }
            return path;
        }

        private static string FitPath(string path)
        {
            return FitAbsolutePath(path);
        }

        /// <summary>
        /// Saves a string to a specified file.
        /// </summary>
        /// <param name="text">The text to save.</param>
        /// <param name="path">The path to the file where the text will be saved.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when there is an error writing to the file.</exception>
        public static void Save(this string text, string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            try
            {
                File.WriteAllText(FitPath(path), text);
            }
            catch (Exception ex)
            {
                throw new IOException($"An error occurred while saving data to file: {path}", ex);
            }
        }

        /// <summary>
        /// Asynchronously saves a string to a specified file.
        /// </summary>
        /// <param name="text">The text to save.</param>
        /// <param name="path">The path to the file where the text will be saved.</param>
        /// <param name="cts">The token source to monitor for cancellation requests.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when there is an error writing to the file.</exception>
        public static async Task SaveAsync(this string text, string path, CancellationTokenSource? cts = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            cts ??= new();
            try
            {
                await File.WriteAllTextAsync(FitPath(path), text, cts.Token);
            }
            catch (Exception ex)
            {
                cts.Cancel();
                throw new IOException($"An error occurred while saving data to file: {path}", ex);
            }
        }

        /// <summary>
        /// Loads the content of a file as a string.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The content of the file as a string.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs while loading the file.</exception>
        public static string Load(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            try
            {
                return File.ReadAllText(FitPath(path));
            }
            catch (Exception ex)
            {
                throw new IOException($"An error occurred while loading data from file: {path}", ex);
            }
        }

        /// <summary>
        /// Asynchronously loads the content of a file as a string.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="cts">The token source to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the content of the file as a string.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs while loading the file.</exception>
        public static async Task<string> LoadAsync(string path, CancellationTokenSource? cts = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            cts ??= new();
            try
            {
                return await File.ReadAllTextAsync(FitPath(path), cts.Token);
            }
            catch (Exception ex)
            {
                cts.Cancel();
                throw new IOException($"An error occurred while loading data from file: {path}", ex);
            }
        }

        #region JSON
        /// <summary>
        /// Default JSON serialization options.
        /// </summary>
        public static JsonSerializerOptions DefaultJsonOptions { get; set; } = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        /// <summary>
        /// Saves an object to a specified file in JSON format.
        /// </summary>
        /// <param name="obj">The object to save.</param>
        /// <param name="path">The path to the file where the object will be saved.</param>
        /// <param name="jsonOptions">The JSON serialization options to customize the serialization or deserialization process. If <see langword="null"/>, the default options specified in <see cref="DefaultJsonOptions"/> will be used.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="IOException">Thrown when there is an error writing to the file.</exception>
        public static void SaveJson(object obj, string path, JsonSerializerOptions? jsonOptions = null) => SaveJsonAsync(obj, path, jsonOptions).RunSynchronously();

        /// <summary>
        /// Asynchronously saves an object to a specified file in JSON format.
        /// </summary>
        /// <param name="obj">The object to save.</param>
        /// <param name="path">The path to the file where the object will be saved.</param>
        /// <param name="jsonOptions">The JSON serialization options to customize the serialization or deserialization process. If <see langword="null"/>, the default options specified in <see cref="DefaultJsonOptions"/> will be used.</param>
        /// <param name="cts">The token source to monitor for cancellation requests.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="IOException">Thrown when there is an error writing to the file.</exception>
        public static async Task SaveJsonAsync(object obj, string path, JsonSerializerOptions? jsonOptions = null, CancellationTokenSource? cts = default)
        {
            ArgumentNullException.ThrowIfNull(obj);
            ArgumentException.ThrowIfNullOrEmpty(path);

            cts ??= new();
            jsonOptions ??= DefaultJsonOptions;
            try
            {
                path = FitJsonPath(path);
                var jsonData = JsonSerializer.Serialize(obj, jsonOptions);
                await SaveAsync(jsonData, path, cts);
            }
            catch (Exception ex)
            {
                cts.Cancel();
                throw new IOException($"An error occurred while saving JSON data to file: {path}", ex);
            }
        }

        /// <summary>
        /// Loads an object from a specified file in JSON format.
        /// </summary>
        /// <param name="path">The path to the file where the object is saved.</param>
        /// <param name="jsonOptions">The JSON serialization options to customize the serialization or deserialization process. If <see langword="null"/>, the default options specified in <see cref="DefaultJsonOptions"/> will be used.</param>
        /// <returns>The deserialized object from the file.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when there is an error reading from the file.</exception>
        public static object LoadJson(string path, JsonSerializerOptions? jsonOptions = null) => LoadJsonAsync(path, jsonOptions).Result;

        /// <summary>
        /// Asynchronously loads an object from a specified file in JSON format.
        /// </summary>
        /// <param name="path">The path to the file where the object is saved.</param>
        /// <param name="jsonOptions">The JSON serialization options to customize the serialization or deserialization process. If <see langword="null"/>, the default options specified in <see cref="DefaultJsonOptions"/> will be used.</param>
        /// <param name="cts">The token source to monitor for cancellation requests.</param>
        /// <returns>The deserialized object from the file.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when there is an error reading from the file.</exception>
        public static async Task<object> LoadJsonAsync(string path, JsonSerializerOptions? jsonOptions = null, CancellationTokenSource? cts = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);

            cts ??= new();
            jsonOptions ??= DefaultJsonOptions;
            try
            {
                path = FitJsonPath(path);
                if (!File.Exists(path))
                    throw new FileNotFoundException($"The file specified does not exist: {path}");

                var jsonData = await LoadAsync(path, cts);
                return JsonSerializer.Deserialize<object>(jsonData, jsonOptions) ?? throw new JsonException("Unable to deserialize object.");
            }
            catch (Exception ex)
            {
                cts.Cancel();
                throw new IOException($"An error occurred while loading JSON data from file: {path}", ex);
            }
        }

        /// <summary>
        /// Loads an object of a specified type from a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of object to load.</typeparam>
        /// <param name="path">The path to the file where the object is saved.</param>
        /// <param name="jsonOptions">The JSON serialization options to customize the serialization or deserialization process. If <see langword="null"/>, the default options specified in <see cref="DefaultJsonOptions"/> will be used.</param>
        /// <returns>The deserialized object from the file.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when there is an error reading from the file.</exception>
        public static T LoadJson<T>(string path, JsonSerializerOptions? jsonOptions = null) => LoadJsonAsync<T>(path, jsonOptions).Result;

        /// <summary>
        /// Asynchronously loads an object of a specified type from a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of object to load.</typeparam>
        /// <param name="path">The path to the file where the object is saved.</param>
        /// <param name="jsonOptions">The JSON serialization options to customize the serialization or deserialization process. If <see langword="null"/>, the default options specified in <see cref="DefaultJsonOptions"/> will be used.</param>
        /// <param name="cts">The token source to monitor for cancellation requests.</param>
        /// <returns>The deserialized object from the file.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when there is an error reading from the file.</exception>
        public static async Task<T> LoadJsonAsync<T>(string path, JsonSerializerOptions? jsonOptions = null, CancellationTokenSource? cts = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);

            cts ??= new();
            jsonOptions ??= DefaultJsonOptions;
            try
            {
                path = FitJsonPath(path);
                if (!File.Exists(path))
                    throw new FileNotFoundException($"The file specified does not exist: {path}");

                string jsonData = await LoadAsync(path, cts);
                return JsonSerializer.Deserialize<T>(jsonData, jsonOptions)
                       ?? throw new JsonException("Unable to deserialize object.");
            }
            catch (Exception ex)
            {
                cts.Cancel();
                throw new IOException($"An error occurred while loading JSON data from file: {path}", ex);
            }
        }

        /// <summary>
        /// Converts an object to its JSON string representation.
        /// </summary>
        /// <typeparam name="T">The type of the object to convert.</typeparam>
        /// <param name="obj">The object to convert.</param>
        /// <param name="jsonOptions">The JSON serialization options to customize the serialization or deserialization process. If <see langword="null"/>, the default options specified in <see cref="DefaultJsonOptions"/> will be used.</param>
        /// <returns>A JSON string representation of the object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is null.</exception>
        /// <exception cref="IOException">Thrown when there is an error during serialization.</exception>
        public static string Json<T>(this T obj, JsonSerializerOptions? jsonOptions = null)
        {
            ArgumentNullException.ThrowIfNull(obj);

            jsonOptions ??= DefaultJsonOptions;
            try
            {
                return JsonSerializer.Serialize(obj, jsonOptions);
            }
            catch (Exception ex)
            {
                throw new IOException("An error occurred while serializing the object to JSON.", ex);
            }
        }
        #endregion

        #region Plain
        /// <summary>
        /// Loads the content of a file as a list of lines.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>A list of lines from the file.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs while loading the file.</exception>
        public static List<string> LoadLines(string path)
        {
            try
            {
                return [.. File.ReadAllLines(FitPath(path))];
            }
            catch (Exception ex)
            {
                throw new IOException($"An error occurred while loading data from file: {path}", ex);
            }
        }

        /// <summary>
        /// Asynchronously loads the content of a file as a list of lines.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="cts">The token source to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a list of lines from the file.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs while loading the file.</exception>
        public static async Task<List<string>> LoadLinesAsync(string path, CancellationTokenSource? cts = null)
        {
            cts ??= new();
            try
            {
                return [.. await File.ReadAllLinesAsync(FitPath(path), cts.Token)];
            }
            catch (Exception ex)
            {
                cts.Cancel();
                throw new IOException($"An error occurred while loading data from file: {path}", ex);
            }
        }

        /// <summary>
        /// Loads key-value pairs from a file where each line contains a pair separated by a specified <paramref name="separator"/>.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="separator">The string used to separate keys from values.</param>
        /// <returns>A dictionary containing the key-value pairs.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs while loading the file.</exception>
        public static Dictionary<string, string> LoadPairs(string path, string separator = "=")
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            var result = new Dictionary<string, string>();
            var lines = LoadLines(path);
            foreach (var line in lines)
            {
                var values = line.Split(separator).Select(x => x.Trim()).ToList();
                result.Add(values[0], string.Join(separator, values.Skip(1)));
            }
            return result;
        }

        /// <summary>
        /// Asynchronously loads key-value pairs from a file where each line contains a pair separated by a specified separator.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="separator">The string used to separate keys from values.</param>
        /// <param name="cts">The token source to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a dictionary with key-value pairs.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs while loading the file.</exception>
        public static async Task<Dictionary<string, string>> LoadPairsAsync(string path, string separator = "=", CancellationTokenSource? cts = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            cts ??= new();
            try
            {
                var result = new Dictionary<string, string>();
                var lines = await LoadLinesAsync(path);
                foreach (var line in lines)
                {
                    var values = line.Split(separator).Select(x => x.Trim()).ToList();
                    result.Add(values[0], string.Join(separator, values.Skip(1)));
                }
                return result;
            }
            catch (Exception ex)
            {
                cts.Cancel();
                throw new IOException($"An error occurred while loading data from file: {path}", ex);
            }
        }

        /// <summary>
        /// Saves a collection of lines to a file.
        /// </summary>
        /// <param name="lines">The lines to save.</param>
        /// <param name="path">The path to the file.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs while saving the file.</exception>
        public static void SaveLines(IEnumerable<string> lines, string path) => string.Join(Environment.NewLine, lines).Save(path);

        /// <summary>
        /// Asynchronously saves a collection of lines to a file.
        /// </summary>
        /// <param name="lines">The lines to save.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="cts">The token source to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs while saving the file.</exception>
        public static async Task SaveLinesAsync(IEnumerable<string> lines, string path, CancellationTokenSource? cts = null) => await string.Join(Environment.NewLine, lines).SaveAsync(path, cts);

        /// <summary>
        /// Saves a collection of lines to a file.
        /// </summary>
        /// <param name="lines">The lines to save.</param>
        /// <param name="path">The path to the file.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs while saving the file.</exception>
        public static void SaveAsLines(this IEnumerable<string> lines, string path) => SaveLines(lines, path);

        /// <summary>
        /// Asynchronously saves a collection of lines to a file.
        /// </summary>
        /// <param name="lines">The lines to save.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="cts">The token source to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs while saving the file.</exception>
        public static async Task SaveAsLinesAsync(this IEnumerable<string> lines, string path, CancellationTokenSource? cts = null) => await SaveLinesAsync(lines, path, cts);

        /// <summary>
        /// Saves a collection of key-value pairs to a file, each pair on a new line separated by a specified <paramref name="separator"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="pairs">The key-value pairs to save.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="separator">The separator used between keys and values.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs while saving the file.</exception>
        public static void SavePairs<TKey, TValue>(IDictionary<TKey, TValue> pairs, string path, string separator = " = ") => SaveLines(pairs.Select(x => $"{x.Key}{separator}{x.Value}"), path);

        /// <summary>
        /// Asynchronously saves a collection of key-value pairs to a file, each pair on a new line separated by a specified <paramref name="separator"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="pairs">The key-value pairs to save.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="separator">The separator used between keys and values.</param>
        /// <param name="cts">The token source to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs while saving the file.</exception>
        public static async Task SavePairsAsync<TKey, TValue>(IDictionary<TKey, TValue> pairs, string path, string separator = " = ", CancellationTokenSource? cts = null) => await SaveLinesAsync(pairs.Select(x => $"{x.Key}{separator}{x.Value}"), path, cts);

        /// <summary>
        /// Saves a collection of key-value pairs to a file, each pair on a new line separated by a specified <paramref name="separator"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="pairs">The key-value pairs to save.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="separator">The separator used between keys and values.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs while saving the file.</exception>
        public static void SaveAsPairs<TKey, TValue>(this IDictionary<TKey, TValue> pairs, string path, string separator = " = ") => SavePairs(pairs, path, separator);

        /// <summary>
        /// Asynchronously saves a collection of key-value pairs to a file, each pair on a new line separated by a specified <paramref name="separator"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="pairs">The key-value pairs to save.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="separator">The separator used between keys and values.</param>
        /// <param name="cts">The token source to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="path"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs while saving the file.</exception>
        public static async Task SaveAsPairsAsync<TKey, TValue>(this IDictionary<TKey, TValue> pairs, string path, string separator = " = ", CancellationTokenSource? cts = null) => await SavePairsAsync(pairs, path, separator, cts);
        #endregion
    }
}