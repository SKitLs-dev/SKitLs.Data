//using System.IO.Packaging;
//using System.Xml;

//namespace SKitLs.Data.IO.Tables
//{
//    /// <summary>
//    /// Base class for writing data to Excel files, providing synchronous and asynchronous operations.
//    /// </summary>
//    /// <typeparam name="TData">The type to which each row in the Excel file should be converted.</typeparam>
//    /// <remarks>
//    /// Initializes a new instance of the <see cref="ExcelWriterBase{T}"/> class with the specified parameters.
//    /// </remarks>
//    /// <param name="dataPath">The path to the Excel file.</param>
//    /// <param name="worksheetName">The name of the worksheet within the Excel file.</param>
//    /// <param name="startRow">The starting row index for writing data.</param>
//    /// <param name="startColumn">The starting column index for writing data.</param>
//    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataPath"/> or <paramref name="worksheetName"/> is null.</exception>
//    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startRow"/> or <paramref name="startColumn"/> is less than or equal to 0.</exception>
//    public abstract class ExcelWriterBase<TData>(string dataPath, string worksheetName, int startRow = 1, int startColumn = 1) : TableIOBase("Excel File", dataPath, worksheetName, ",", startRow, startColumn), IDataWriter<TablePartRow> where TData : class
//    {
//        /// <inheritdoc/>
//        public string GetSourceName() => SourceName;

//        /// <summary>
//        /// Converts data of type <typeparamref name="TData"/> into an <see cref="TablePartRow"/>.
//        /// </summary>
//        /// <param name="data">The data of type <typeparamref name="TData"/> to convert.</param>
//        /// <returns>An <see cref="TablePartRow"/> instance representing the converted data.</returns>
//        public abstract TablePartRow Convert(TData data);

//        /// <inheritdoc/>
//        /// <inheritdoc cref="WriteDataListAsync{T}(IEnumerable{T}, CancellationTokenSource?)"/>
//        public virtual bool WriteData<T>(T item) where T : class => WriteDataListAsync([item]).Result;

//        /// <inheritdoc/>
//        /// <inheritdoc cref="WriteDataListAsync(IEnumerable{TablePartRow}, CancellationTokenSource?)"/>
//        public virtual bool WriteData(TablePartRow item) => WriteDataListAsync([item]).Result;

//        /// <inheritdoc/>
//        /// <inheritdoc cref="WriteDataListAsync{T}(IEnumerable{T}, CancellationTokenSource?)"/>
//        public virtual bool WriteDataList<T>(IEnumerable<T> items) where T : class => WriteDataListAsync(items).Result;

//        /// <inheritdoc/>
//        /// <inheritdoc cref="WriteDataListAsync(IEnumerable{TablePartRow}, CancellationTokenSource?)"/>
//        public virtual bool WriteDataList(IEnumerable<TablePartRow> items) => WriteDataListAsync(items).Result;

//        /// <inheritdoc/>
//        /// <inheritdoc cref="WriteDataListAsync{T}(IEnumerable{T}, CancellationTokenSource?)"/>
//        public virtual async Task<bool> WriteDataAsync<T>(T item, CancellationTokenSource? cts = default) where T : class => await WriteDataListAsync([item], cts);

//        /// <inheritdoc/>
//        /// <inheritdoc cref="WriteDataListAsync(IEnumerable{TablePartRow}, CancellationTokenSource?)"/>
//        public virtual async Task<bool> WriteDataAsync(TablePartRow item, CancellationTokenSource? cts) => await WriteDataListAsync([item], cts);

//        /// <inheritdoc/>
//        /// <inheritdoc cref="WriteDataListAsync(IEnumerable{TablePartRow}, CancellationTokenSource?)"/>
//        /// <exception cref="NotSupportedException">Thrown when writing data of type <typeparamref name="TData"/> is not supported.</exception>
//        public virtual async Task<bool> WriteDataListAsync<T>(IEnumerable<T> items, CancellationTokenSource? cts = default) where T : class
//        {
//            if (typeof(T) == typeof(TData))
//            {
//                return await WriteDataListAsync(items.Select(x => Convert((x as TData)!)).ToList(), cts);
//            }
//            else if (typeof(T) == typeof(TablePartRow))
//            {
//                return await WriteDataListAsync(items.Select(x => (x as TablePartRow)!).ToList(), cts);
//            }
//            else
//            {
//                throw new NotSupportedException($"Type {typeof(T).Name} is not supported for data conversion.");
//            }
//        }

//        /// <inheritdoc/>
//        /// <exception cref="IOException">Thrown when an error occurs during the writing process, with detailed context about the issue.</exception>
//        public virtual async Task<bool> WriteDataListAsync(IEnumerable<TablePartRow> items, CancellationTokenSource? cts = default)
//        {
//            cts ??= new();
//            try
//            {
//                var sourceFile = new FileInfo(DataPath);
//                if (!File.Exists(sourceFile.FullName))
//                {
//                    if (CreateNew)
//                    {
//                        File.Create(sourceFile.FullName).Close();
//                    }
//                    else
//                    {
//                        throw new FileNotFoundException($"The file {DataPath} does not exist, and creation of new files is disabled.");
//                    }
//                }

//                await Task.Run(() => {
//                    // Open or create the package
//                    using var package = Package.Open(DataPath, FileMode.Open, FileAccess.ReadWrite);

//                    // Ensure workbook and worksheet parts exist
//                    var workbookXml = EnsureWorkbookXml(package);
//                    var worksheetPath = EnsureWorksheetXml(package, workbookXml, WorksheetName);
//                    EnsureWorkbookRelationships(package, workbookXml);

//                    // Write data to the worksheet
//                    WriteDataToWorksheet(package, worksheetPath, items);
//                }, cts.Token);

//                return true;
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
//                    throw new IOException($"Error on saving data '{SourceName}' ({GetType().Name}) at the file: '{DataPath}'.", ex);
//                }
//                return false;
//            }
//        }

//        /// <summary>
//        /// Ensures that the workbook XML exists in the package and returns it.
//        /// </summary>
//        private static XmlDocument EnsureWorkbookXml(Package package)
//        {
//            var workbookUri = new Uri("/xl/workbook.xml", UriKind.Relative);
//            var workbookContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml";

//            var workbookPart = package.PartExists(workbookUri)
//                ? package.GetPart(workbookUri)
//                : package.CreatePart(workbookUri, workbookContentType);

//            var workbookXml = new XmlDocument();

//            using var stream = workbookPart.GetStream(FileMode.OpenOrCreate, FileAccess.ReadWrite);
//            if (stream.Length == 0)
//            {
//                // Create a new workbook if it doesn't exist
//                workbookXml.LoadXml("<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheets/></workbook>");
//            }
//            else
//            {
//                workbookXml.Load(stream);
//            }
//            workbookXml.Save(stream);

//            return workbookXml;
//        }
//        private static void EnsureWorkbookRelationships(Package package, XmlDocument workbookXml)
//        {
//            var relsUri = new Uri("/xl/_rels/workbook.xml.rels", UriKind.Relative);
//            var relsContentType = "application/vnd.openxmlformats-package.relationships+xml";

//            var relsPart = package.PartExists(relsUri)
//                ? package.GetPart(relsUri)
//                : package.CreatePart(relsUri, relsContentType);

//            var relsXml = new XmlDocument();
//            if (relsPart.GetStream(FileMode.OpenOrCreate, FileAccess.ReadWrite).Length == 0)
//            {
//                relsXml.LoadXml("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"></Relationships>");
//            }
//            else
//            {
//                using var stream = relsPart.GetStream(FileMode.Open, FileAccess.Read);
//                relsXml.Load(stream);
//            }

//            // Проверяем и добавляем ссылки на Worksheets
//            var nsManager = new XmlNamespaceManager(relsXml.NameTable);
//            nsManager.AddNamespace("rel", "http://schemas.openxmlformats.org/package/2006/relationships");
//            nsManager.AddNamespace("ns", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");

//            var sheets = workbookXml.SelectNodes("//ns:sheet", nsManager);
//            foreach (XmlNode sheet in sheets)
//            {
//                var target = sheet.Attributes?["target"]?.Value;
//                if (target == null) continue;

//                var id = sheet.Attributes?["r:id"]?.Value;
//                if (id != null) continue; // Уже есть связь

//                var relNode = relsXml.CreateElement("Relationship", "http://schemas.openxmlformats.org/package/2006/relationships");
//                relNode.SetAttribute("Id", $"rId{sheets.Count}");
//                relNode.SetAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet");
//                relNode.SetAttribute("Target", target);

//                relsXml.DocumentElement?.AppendChild(relNode);
//                sheet.Attributes?.Append(sheet.OwnerDocument?.CreateAttribute("r:id"));
//                sheet.Attributes["r:id"].Value = $"rId{sheets.Count}";
//            }

//            // Записываем обратно
//            using (var stream = relsPart.GetStream(FileMode.Create, FileAccess.Write))
//            {
//                relsXml.Save(stream);
//            }
//        }

//        /// <summary>
//        /// Ensures that the worksheet XML exists and returns its path.
//        /// </summary>
//        private static string EnsureWorksheetXml(Package package, XmlDocument workbookXml, string worksheetName)
//        {
//            var nsManager = new XmlNamespaceManager(workbookXml.NameTable);
//            nsManager.AddNamespace("ns", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");

//            var sheetNode = workbookXml.SelectSingleNode($"//ns:sheet[@name='{worksheetName}']", nsManager);
//            if (sheetNode is not null)
//                return sheetNode.Attributes?["target"]?.Value ?? throw new InvalidOperationException("Invalid worksheet reference.");

//            // Create a new worksheet if it doesn't exist
//            var sheetsNode = workbookXml.SelectSingleNode("//ns:sheets", nsManager) ?? throw new InvalidOperationException("Invalid workbook structure.");
//            var sheetId = sheetsNode.ChildNodes.Count + 1;

//            var worksheetPath = $"/xl/worksheets/sheet{sheetId}.xml";
//            var worksheetNode = workbookXml.CreateElement("sheet", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
//            worksheetNode.SetAttribute("name", worksheetName);
//            worksheetNode.SetAttribute("sheetId", sheetId.ToString());
//            worksheetNode.SetAttribute("target", worksheetPath);

//            sheetsNode.AppendChild(worksheetNode);

//            var worksheetUri = new Uri(worksheetPath, UriKind.Relative);
//            var worksheetContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml";

//            PackagePart worksheetPart;
//            // Проверяем, существует ли уже часть с указанным URI
//            if (package.PartExists(worksheetUri))
//            {
//                worksheetPart = package.GetPart(worksheetUri);
//            }
//            else
//            {
//                worksheetPart = package.CreatePart(worksheetUri, worksheetContentType);
//                using var stream = worksheetPart.GetStream(FileMode.Create, FileAccess.Write);
//                var worksheetXml = new XmlDocument();
//                worksheetXml.LoadXml("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData/></worksheet>");
//                worksheetXml.Save(stream);
//            }

//            return worksheetPath;
//        }

//        /// <summary>
//        /// Writes the given data to the specified worksheet.
//        /// </summary>
//        private static void WriteDataToWorksheet(Package package, string worksheetPath, IEnumerable<TablePartRow> values)
//        {
//            var worksheetPart = package.GetParts()
//                .FirstOrDefault(p => p.Uri.ToString().EndsWith(worksheetPath, StringComparison.OrdinalIgnoreCase))
//                ?? throw new InvalidOperationException("Worksheet part not found.");

//            var worksheetXml = new XmlDocument();
//            using (var stream = worksheetPart.GetStream(FileMode.Open, FileAccess.ReadWrite))
//            {
//                worksheetXml.Load(stream);
//            }

//            var nsManager = new XmlNamespaceManager(worksheetXml.NameTable);
//            nsManager.AddNamespace("ns", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");

//            var sheetDataNode = worksheetXml.SelectSingleNode("//ns:sheetData", nsManager) ?? throw new InvalidOperationException("Invalid worksheet structure.");

//            foreach (var row in values)
//            {
//                var rowNode = worksheetXml.CreateElement("row", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
//                rowNode.SetAttribute("r", row.RowIndex.ToString());

//                for (int i = 0; i < row.Values.Count; i++)
//                {
//                    var columnLetter = (char)('A' + i); // Преобразование индекса в букву
//                    var cellAddress = $"{columnLetter}{row.RowIndex}";

//                    var cellNode = worksheetXml.CreateElement("c", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
//                    cellNode.SetAttribute("r", cellAddress);
//                    var valueNode = worksheetXml.CreateElement("v", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
//                    valueNode.InnerText = row[i];
//                    cellNode.AppendChild(valueNode);
//                    rowNode.AppendChild(cellNode);
//                }

//                sheetDataNode.AppendChild(rowNode);
//            }

//            using (var stream = worksheetPart.GetStream(FileMode.Create, FileAccess.Write))
//            {
//                worksheetXml.Save(stream);
//            }
//        }
//    }
//}