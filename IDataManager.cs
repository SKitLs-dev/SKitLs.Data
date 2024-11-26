using SKitLs.Data.Banks;
using SKitLs.Data.IO;

namespace SKitLs.Data
{
    /// <summary>
    /// Interface representing a data manager that handles various data banks.
    /// </summary>
    public interface IDataManager
    {
        /// <summary>
        /// Gets the data folder path.
        /// </summary>
        public string DataFolderPath { get; }

        /// <summary>
        /// Retrieves a list of notations for all data banks.
        /// </summary>
        /// <returns>A list of <see cref="DataBankInfo"/> objects representing the notations.</returns>
        public IReadOnlyCollection<DataBankInfo> GetNotations();

        /// <summary>
        /// Retrieves a list of all data banks.
        /// </summary>
        /// <returns>A list of <see cref="IDataBank"/> objects representing the data banks.</returns>
        public IEnumerable<IDataBank> GetBanks();

        /// <summary>
        /// Resolves a data bank by its type.
        /// </summary>
        /// <param name="bankType">The type of the data bank to resolve.</param>
        /// <returns>The resolved <see cref="IDataBank"/>.</returns>
        public IDataBank ResolveBank(Type bankType);

        /// <summary>
        /// Resolves a data bank by the type of data it holds.
        /// </summary>
        /// <typeparam name="TData">The type of data the bank holds.</typeparam>
        /// <returns>The resolved <see cref="IDataBank"/>.</returns>
        public IDataBank ResolveBank<TData>() => ResolveBank(typeof(TData));

        /// <summary>
        /// Resolves a data bank by the type of its ID and the type of data it holds.
        /// </summary>
        /// <typeparam name="TId">The type of the data bank's ID.</typeparam>
        /// <typeparam name="TData">The type of data the bank holds.</typeparam>
        /// <returns>The resolved <see cref="IDataBank{TId, TData}"/>.</returns>
        public IDataBank<TId, TData> ResolveBank<TId, TData>() where TId : notnull, IEquatable<TId>, IComparable<TId> where TData : ModelDso<TId>;

        /// <summary>
        /// Declares a new data bank to the manager.
        /// </summary>
        /// <typeparam name="TId">The type of the data bank's ID.</typeparam>
        /// <typeparam name="TData">The type of data the bank holds.</typeparam>
        /// <param name="bank">The data bank to add.</param>
        public void Declare<TId, TData>(IDataBank<TId, TData> bank) where TId : notnull, IEquatable<TId>, IComparable<TId> where TData : ModelDso<TId>;

        /// <summary>
        /// Initializes all data banks synchronously.
        /// </summary>
        public void Initialize();

        /// <summary>
        /// Initializes all data banks asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task InitializeAsync();
    }
}