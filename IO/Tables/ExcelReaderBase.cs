//using System.Data;
//using System.IO.Packaging;
//using System.Xml;

//namespace SKitLs.Data.IO.Tables
//{
//    // TODO Exceptions
//    // TODO <inheritdoc> exceptions

//    /// <summary>
//    /// Base class for reading data from Excel files, providing synchronous and asynchronous operations.
//    /// </summary>
//    /// <typeparam name="TData">The type to which each row in the Excel file should be converted.</typeparam>
//    /// <remarks>
//    /// Initializes a new instance of the <see cref="ExcelReaderBase{T}"/> class with the specified parameters.
//    /// </remarks>
//    /// <param name="dataPath">The path to the Excel file.</param>
//    /// <param name="worksheetName">The name of the worksheet within the Excel file.</param>
//    /// <param name="startRow">The starting row index for reading data.</param>
//    /// <param name="startColumn">The starting column index for reading data.</param>
//    /// <param name="endColumn">The ending column index for reading data.</param>
//    /// <param name="emptyRowsBreakHit">The maximum number of consecutive empty rows to encounter before stopping.</param>
//    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataPath"/> or <paramref name="worksheetName"/> is null.</exception>
//    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startRow"/>, <paramref name="startColumn"/>, <paramref name="endColumn"/>, or <paramref name="emptyRowsBreakHit"/> is less than or equal to 0.</exception>
//    public abstract class ExcelReaderBase<TData>(string dataPath, string worksheetName, int startRow = 1, int startColumn = 1, int endColumn = 100, int emptyRowsBreakHit = 3) : TableIOBase("Excel File", dataPath, worksheetName, ";", startRow, startColumn, endColumn, emptyRowsBreakHit), IDataReader<TablePartRow> where TData : class
//    {
//        /// <inheritdoc/>
//        public string GetSourceName() => SourceName;

//        /// <summary>
//        /// Converts an <see cref="TablePartRow"/> instance into the desired type <typeparamref name="TData"/>.
//        /// </summary>
//        /// <param name="row">The <see cref="TablePartRow"/> instance to convert.</param>
//        /// <returns>The converted object of type <typeparamref name="TData"/>.</returns>
//        public abstract TData Convert(TablePartRow row);

//        /// <inheritdoc/>
//        /// <inheritdoc cref="ReadDataAsync{T}(CancellationTokenSource?)"/>
//        public virtual IEnumerable<T> ReadData<T>() where T : class => ReadDataAsync<T>().Result;

//        /// <inheritdoc/>
//        /// <inheritdoc cref="ReadDataAsync(CancellationTokenSource?)"/>
//        public virtual IEnumerable<TablePartRow> ReadData() => ReadDataAsync().Result;

//        /// <inheritdoc/>
//        /// <inheritdoc cref="ReadDataAsync(CancellationTokenSource?)"/>
//        /// <exception cref="NotSupportedException">Thrown when writing data of type <typeparamref name="TData"/> is not supported.</exception>
//        public virtual async Task<IEnumerable<T>> ReadDataAsync<T>(CancellationTokenSource? cts = default) where T : class
//        {
//            if (typeof(T) == typeof(string))
//            {
//                var read = await ReadDataAsync(cts);
//                return read.Select(x => (x.Join(DataSep) as T)!);
//            }
//            else if (typeof(T) == typeof(TData))
//            {
//                var read = await ReadDataAsync();
//                return read.Select(x => (Convert(x) as T)!);
//            }
//            else
//            {
//                throw new NotSupportedException($"Type {typeof(T).Name} is not supported for data conversion.");
//            }
//        }

//        /// <inheritdoc/>
//        /// <exception cref="IOException">Thrown when an error occurs during the writing process, with detailed context about the issue.</exception>
//        public virtual async Task<IEnumerable<TablePartRow>> ReadDataAsync(CancellationTokenSource? cts = default)
//        {
//            cts ??= new();
//            var result = new List<TablePartRow>();
//            try
//            {
//                if (!File.Exists(DataPath))
//                {
//                    throw new FileNotFoundException($"The file {DataPath} does not exist.");
//                }

//                await Task.Run(() =>
//                {
//                    using var package = Package.Open(DataPath, FileMode.Open, FileAccess.Read);
//                    var workbook = GetWorkbookXml(package);
//                    var worksheetPath = GetWorksheetPath(workbook, WorksheetName) ?? throw new ArgumentException($"Worksheet {WorksheetName} not found.");

//                    var sheetData = GetSheetDataXml(package, worksheetPath);
//                    ParseSheetData(sheetData, result);

//                }, cts.Token);
//            }
//            catch (OperationCanceledException)
//            {
//                // Handle cancellation
//                throw;
//            }
//            catch (Exception ex)
//            {
//                cts.Cancel();
//                if (!HandleInnerExceptions)
//                {
//                    throw new IOException($"Error on reading data '{SourceName}' ({GetType().Name}) at the file: '{DataPath}'.", ex);
//                }
//            }
//            return result;
//        }

//        /// <summary>
//        /// Extracts the XML document for the workbook part.
//        /// </summary>
//        private static XmlDocument GetWorkbookXml(Package package)
//        {
//            var workbookPart = package.GetParts()
//                .FirstOrDefault(p => p.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")
//                ?? throw new InvalidOperationException("Workbook part not found.");

//            var workbookXml = new XmlDocument();
//            using var stream = workbookPart.GetStream();
//            workbookXml.Load(stream);

//            return workbookXml;
//        }

//        /// <summary>
//        /// Finds the path to the worksheet with the specified name.
//        /// </summary>
//        private static string? GetWorksheetPath(XmlDocument workbookXml, string worksheetName)
//        {
//            var nsManager = new XmlNamespaceManager(workbookXml.NameTable);
//            nsManager.AddNamespace("ns", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");

//            var sheetNode = workbookXml.SelectSingleNode($"//ns:sheet[@name='{worksheetName}']", nsManager);
//            if (sheetNode is null)
//                return null;

//            var relationshipId = sheetNode.Attributes?["r:id"]?.Value;
//            if (relationshipId is null)
//                return null;

//            var relationshipNode = workbookXml.SelectSingleNode($"//ns:Relationships/ns:Relationship[@Id='{relationshipId}']", nsManager);
//            return relationshipNode?.Attributes?["Target"]?.Value;
//        }

//        /// <summary>
//        /// Extracts the XML document for the specified worksheet part.
//        /// </summary>
//        private static XmlDocument GetSheetDataXml(Package package, string worksheetPath)
//        {
//            var worksheetPart = package.GetParts()
//                .FirstOrDefault(p => p.Uri.ToString().EndsWith(worksheetPath, StringComparison.OrdinalIgnoreCase))
//                ?? throw new InvalidOperationException("Worksheet part not found."); ;

//            var sheetDataXml = new XmlDocument();
//            using var stream = worksheetPart.GetStream();
//            sheetDataXml.Load(stream);

//            return sheetDataXml;
//        }

//        /// <summary>
//        /// Parses the sheet data XML and fills the result with rows and columns.
//        /// </summary>
//        private void ParseSheetData(XmlDocument sheetData, List<TablePartRow> result)
//        {
//            var nsManager = new XmlNamespaceManager(sheetData.NameTable);
//            nsManager.AddNamespace("ns", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");

//            var rows = sheetData.SelectNodes("//ns:sheetData/ns:row", nsManager);
//            if (rows is null)
//                return;

//            foreach (XmlNode rowNode in rows)
//            {
//                var rowIndex = int.Parse(rowNode.Attributes?["r"]?.Value ?? "0");
//                var row = new TablePartRow(rowIndex, StartColumn, EndColumn);

//                var columnsCount = 0;
//                var temp = new List<string>();
//                foreach (XmlNode cellNode in rowNode.ChildNodes)
//                {
//                    var cellValue = cellNode.SelectSingleNode("ns:v", nsManager)?.InnerText ?? string.Empty;
//                    temp.Add(cellValue);
//                    columnsCount++;
//                }
//                var endIndex = Math.Min(temp.Count - 1, EndColumn);
//                for (int i = 0; i < endIndex; i++)
//                {
//                    row.Add(temp[i]);
//                }

//                if (!row.IsEmpty())
//                    result.Add(row);
//            }
//        }
//    }
//}