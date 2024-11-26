using SKitLs.Data.IdGenerator;
using SKitLs.Data.IO;

namespace SKitLs.Data.Banks
{
    /// <summary>
    /// Represents a data bank that manages data with specific ID and data types.
    /// </summary>
    /// <typeparam name="TId">The type of the ID of the elements in the bank.</typeparam>
    /// <typeparam name="TData">The type of the data in the bank.</typeparam>
    /// <remarks>
    /// This class provides methods for initializing, reading, writing, and managing data entities within the data bank. 
    /// It inherits from <see cref="DataBankBase{TId, TData}"/> and implements the necessary interfaces for data management.
    /// </remarks>
    public class DataBank<TId, TData> : DataBankBase<TId, TData> where TId : notnull, IEquatable<TId>, IComparable<TId> where TData : ModelDso<TId>
    {
        /// <summary>
        /// Gets or sets the ID generator for the data bank.
        /// </summary>
        public IIdGenerator<TId>? IdGenerator { get; init; }

        /// <summary>
        /// Gets or sets the data reader for the data bank.
        /// </summary>
        public IDataReader? Reader { get; internal set; }

        /// <summary>
        /// Gets or sets the data writer for the data bank.
        /// </summary>
        public IDataWriter? Writer { get; internal set; }

        /// <summary>
        /// Gets or sets the function to generate a new instance of TData.
        /// </summary>
        public Func<TData> NewInstanceGenerator { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataBank{TId, TData}"/> class with the specified name, description, and drop strategy.
        /// </summary>
        /// <param name="name">The name of the data bank.</param>
        /// <param name="description">The description of the data bank.</param>
        /// <param name="dropStrategy">The strategy to use when dropping data.</param>
        public DataBank(string? name = null, string? description = null, DropStrategy dropStrategy = DropStrategy.Disable)
        {
            Name = name ?? typeof(TData).Name.ToLower();
            Description = description ?? "No more info";
            DropStrategy = dropStrategy;
            NewInstanceGenerator ??= ActivatorGenerator;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataBank{TId, TData}"/> class with the specified parameters.
        /// </summary>
        /// <param name="notation">The data bank notation containing name and description.</param>
        /// <param name="reader">The data reader to use for the data bank.</param>
        /// <param name="writer">The data writer to use for the data bank.</param>
        /// <param name="idGenerator">The ID generator for the data bank.</param>
        /// <param name="instanceGenerator">Optional: The function to generate a new instance of TData.</param>
        /// <param name="dropStrategy">The strategy to use when dropping data.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="notation"/> holding type does not match the type of <typeparamref name="TData"/>.</exception>
        public DataBank(DataBankInfo notation, IDataReader<TData> reader, IDataWriter<TData> writer, IIdGenerator<TId>? idGenerator, Func<TData>? instanceGenerator = null, DropStrategy dropStrategy = DropStrategy.Disable) : this(notation.Name, notation.Description, dropStrategy)
        {
            if (notation.InType != typeof(TData))
                throw new ArgumentException($"Notation holding type mismatch in bank '{Id}'", nameof(notation));

            Reader = reader;
            Writer = writer;
            NewInstanceGenerator = instanceGenerator ?? ActivatorGenerator;
            IdGenerator = idGenerator;
        }

        // TODO Exception
        private static TData ActivatorGenerator() => Activator.CreateInstance<TData>() ?? throw new NullReferenceException();

        /// <inheritdoc/>
        public override TData BuildNewData()
        {
            if (IdGenerator is null)
                throw new NullReferenceException(nameof(IdGenerator));

            var @new = NewInstanceGenerator.Invoke();
            @new.SetId(IdGenerator.GetDefaultId());
            return @new;
        }

        /// <inheritdoc/>
        public override IDataReader? GetReader() => Reader;

        /// <inheritdoc/>
        public override void UpdateReader(IDataReader reader) => Reader = reader;

        /// <inheritdoc/>
        public override IDataWriter? GetWriter() => Writer;

        /// <inheritdoc/>
        public override void UpdateWriter(IDataWriter writer) => Writer = writer;

        /// <inheritdoc/>
        public override IIdGenerator<TId>? GetIdGenerator() => IdGenerator;
    }
}