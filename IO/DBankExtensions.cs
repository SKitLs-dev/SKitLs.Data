using SKitLs.Data.Banks;
using SKitLs.Data.IdGenerator;
using SKitLs.Data.IO.Json;
using System.Text.Json;

namespace SKitLs.Data.IO
{
    /// <summary>
    /// Provides extension methods for configuring data banks with specific input/output (IO) mechanisms.
    /// </summary>
    /// <remarks>
    /// This class is marked as <see cref="ObsoleteAttribute"/> and is currently in the Beta stage. The extension methods allow
    /// data banks to be configured with JSON-based IO mechanisms, including both single-file and split-file approaches.
    /// </remarks>
    /// <seealso cref="IDataBank{TId, TData}"/>
    public static class DBankExtensions
    {
        /// <summary>
        /// Creates a data bank of type <typeparamref name="TData"/> with a default JSON-based Reader/Writer in the manager's folder (<see cref="IDataManager.DataFolderPath"/>).
        /// The bank uses the provided <paramref name="idGenerator"/> to generate unique identifiers.
        /// <para/>
        /// The <c>NewInstanceGenerator</c> is implicitly set to use the default constructor of <typeparamref name="TData"/> (requires a parameterless constructor).
        /// </summary>
        /// <typeparam name="TId">The type of the identifier, which must implement <see cref="IEquatable{T}"/> and <see cref="IComparable{T}"/>.</typeparam>
        /// <typeparam name="TData">The type of data stored in the bank, which must inherit from <see cref="ModelDso{TId}"/>.</typeparam>
        /// <param name="manager">The <see cref="IDataManager"/> instance managing the data bank.</param>
        /// <param name="idGenerator">The generator used to create unique IDs for data entries.</param>
        /// <param name="dropStrategy">The strategy for handling data drops, with the default being <see cref="DropStrategy.Disable"/>.</param>
        /// <param name="instanceGenerator">Optional: The function to generate a new instance of TData.</param>
        /// <param name="options">The JSON serialization options used for serialization.</param>
        /// <returns>An instance of <see cref="IDataBank{TId, TData}"/> configured with JSON-based reader/writer.</returns>
        /// <seealso cref="JsonSingleReader{TData}"/>
        /// <seealso cref="JsonSingleWriter{TData, TId}"/>
        public static IDataBank<TId, TData> JsonBank<TId, TData>(this IDataManager manager, IIdGenerator<TId> idGenerator, Func<TData>? instanceGenerator = null, DropStrategy dropStrategy = DropStrategy.Disable, JsonSerializerOptions? options = null) where TId : notnull, IEquatable<TId>, IComparable<TId> where TData : ModelDso<TId>
        {
            var dbName = typeof(TData).Name;
            var path = Path.Combine(manager.DataFolderPath, HotIO.FitJsonPath(dbName));
            var bank = new DataBank<TId, TData>(dbName, null, dropStrategy, instanceGenerator)
            {
                Reader = new JsonSingleReader<TData>(path, true, options),
                Writer = new JsonSingleWriter<TId, TData>(path, true, options),
                IdGenerator = idGenerator,
            };
            manager.Declare(bank);
            return bank;
        }

        /// <summary>
        /// Creates a data bank of type <typeparamref name="TData"/> with a split-file JSON-based Reader/Writer in the manager's folder (<see cref="IDataManager.DataFolderPath"/>).
        /// The bank uses the provided <paramref name="idGenerator"/> to generate unique identifiers.
        /// <para/>
        /// The <c>NewInstanceGenerator</c> is implicitly set to use the default constructor of <typeparamref name="TData"/> (requires a parameterless constructor).
        /// </summary>
        /// <typeparam name="TId">The type of the identifier, which must implement <see cref="IEquatable{T}"/> and <see cref="IComparable{T}"/>.</typeparam>
        /// <typeparam name="TData">The type of data stored in the bank, which must inherit from <see cref="ModelDso{TId}"/>.</typeparam>
        /// <param name="manager">The <see cref="IDataManager"/> instance managing the data bank.</param>
        /// <param name="idGenerator">The generator used to create unique IDs for data entries.</param>
        /// <param name="dropStrategy">The strategy for handling data drops, with the default being <see cref="DropStrategy.Disable"/>.</param>
        /// <param name="instanceGenerator">Optional: The function to generate a new instance of TData.</param>
        /// <param name="options">The JSON serialization options used for serialization.</param>
        /// <returns>An instance of <see cref="IDataBank{TId, TData}"/> configured with split-file JSON-based reader/writer.</returns>
        /// <seealso cref="JsonSplitReader{TData}"/>
        /// <seealso cref="JsonSplitWriter{TData, TId}"/>
        public static IDataBank<TId, TData> JsonSplitBank<TId, TData>(this IDataManager manager, IIdGenerator<TId> idGenerator, Func<TData>? instanceGenerator = null, DropStrategy dropStrategy = DropStrategy.Disable, JsonSerializerOptions? options = null) where TId : notnull, IEquatable<TId>, IComparable<TId> where TData : ModelDso<TId>
        {
            var dbName = typeof(TData).Name;
            var path = Path.Combine(manager.DataFolderPath, dbName);
            var bank = new DataBank<TId, TData>(dbName, null, dropStrategy, instanceGenerator)
            {
                Reader = new JsonSplitReader<TData>(path, true, options),
                Writer = new JsonSplitWriter<TId, TData>(path, true, options),
                IdGenerator = idGenerator,
            };
            manager.Declare(bank);
            return bank;
        }
    }
}