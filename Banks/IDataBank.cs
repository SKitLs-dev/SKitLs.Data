using SKitLs.Data.IO;
using IDataReader = SKitLs.Data.IO.IDataReader;

namespace SKitLs.Data.Banks
{
    /// <summary>
    /// Represents a method that handles bank update events.
    /// </summary>
    public delegate void DataBankUpdatedHandler();

    /// <summary>
    /// Represents a method that handles collection update events, specifying the number of affected rows.
    /// </summary>
    /// <param name="affectedRows">The number of rows affected by the update.</param>
    public delegate void DataBankCollectionUpdatedHandler(int affectedRows);

    /// <summary>
    /// Represents a method that handles collection update events, specifying the updated elements.
    /// </summary>
    /// <typeparam name="TId">The type of the ID of the elements in the collection.</typeparam>
    /// <typeparam name="TData">The type of the data in the collection.</typeparam>
    /// <param name="updatedElements">The elements that were updated.</param>
    public delegate void DataBankCollectionUpdatedHandler<TId, TData>(IEnumerable<TData> updatedElements) where TId : notnull, IEquatable<TId>, IComparable<TId> where TData : ModelDso<TId>;

    /// <summary>
    /// Defines strategies for handling data drops in the bank.
    /// </summary>
    public enum DropStrategy
    {
        /// <summary>
        /// Disables dropping of data.
        /// </summary>
        Disable,

        /// <summary>
        /// Deletes data when dropped.
        /// </summary>
        Delete
    }

    /// <summary>
    /// Defines the interface for a data bank that manages data storage and operations.
    /// </summary>
    public interface IDataBank
    {
        /// <summary>
        /// Gets or sets the data manager that owns and manages the current data bank.
        /// </summary>
        /// <seealso cref="IDataManager"/>
        public IDataManager Manager { get; internal set; }

        /// <summary>
        /// Occurs when the bank's information is updated.
        /// </summary>
        public event DataBankUpdatedHandler? OnBankInfoUpdated;

        /// <summary>
        /// Occurs when the bank's collection data is updated.
        /// </summary>
        public event DataBankCollectionUpdatedHandler? OnBankDataUpdated;

        /// <summary>
        /// Gets the unique identifier of the data bank.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the type of data held by the bank.
        /// </summary>
        public Type HoldingType { get; }

        /// <summary>
        /// Gets or sets the name of the data bank.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets or sets the description of the data bank.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Gets the count of available items in the data bank.
        /// </summary>
        public long Count { get; }

        /// <summary>
        /// Gets the count of all items in the data bank.
        /// </summary>
        public long CountAll => GetAllReadonly().Count;

        /// <summary>
        /// Gets the strategy used for handling data drops.
        /// </summary>
        public DropStrategy DropStrategy { get; }

        /// <summary>
        /// Gets the reader used for reading data to the bank.
        /// </summary>
        public IDataReader? GetReader();

        internal void UpdateReader(IDataReader reader);

        /// <summary>
        /// Gets the writer used for writing data from the bank.
        /// </summary>
        public IDataWriter? GetWriter();

        internal void UpdateWriter(IDataWriter writer);

        /// <summary>
        /// Initializes the data bank synchronously.
        /// </summary>
        public void Initialize();

        /// <summary>
        /// Initializes the data bank asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous initialization operation.</returns>
        public Task InitializeAsync();

        // NEW
        /// <summary>
        /// Builds a new object to be added to the data bank.
        /// </summary>
        /// <returns>A new object instance.</returns>
        public object BuildNewObject();

        // GET
        /// <summary>
        /// Retrieves a read-only list of objects from the data bank.
        /// </summary>
        /// <returns>A read-only list of objects.</returns>
        public IReadOnlyList<object> GetReadonly();

        /// <summary>
        /// Retrieves a read-only list of all objects from the data bank (inc. dropped).
        /// </summary>
        /// <returns>A read-only list of all objects.</returns>
        public IReadOnlyList<object> GetAllReadonly();

        // UPDATE SYNC
        /// <summary>
        /// Adds or updates and saves the specified data to the bank.
        /// </summary>
        /// <typeparam name="T">The type of data to update and save.</typeparam>
        /// <param name="value">The data to add/update and save.</param>
        /// <returns><see langword="true"/> if the data was added; otherwise (if updated), <see langword="false"/>.</returns>
        public bool UpdateSave<T>(T value) where T : class;

        /// <summary>
        /// Adds or updates and saves a collection of data to the bank.
        /// </summary>
        /// <typeparam name="T">The type of data to update and save.</typeparam>
        /// <param name="values">The collection of data to add/update and save.</param>
        /// <returns>The number of elements added.</returns>
        public int UpdateSave<T>(IEnumerable<T> values) where T : class;

        /// <summary>
        /// Drops the specified data from the bank using <see cref="DropStrategy"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to drop.</typeparam>
        /// <param name="value">The data to drop.</param>
        /// <returns><see langword="true"/> if the data was dropped; otherwise <see langword="false"/>.</returns>
        public bool DropSave<T>(T value) where T : class;

        /// <summary>
        /// Drops and removes the specified collection of data from the bank.
        /// </summary>
        /// <typeparam name="T">The type of data in the collection.</typeparam>
        /// <param name="values">The collection of data to remove.</param>
        /// <returns>The number of elements removed.</returns>
        public int DropSave<T>(IEnumerable<T> values) where T : class;

        // UPDATE ASYNC
        /// <summary>
        /// Adds or updates and saves the specified data to the bank asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of data to update and save.</typeparam>
        /// <param name="value">The data to add/update and save.</param>
        /// /// <param name="cts">The <see cref="CancellationTokenSource"/> to observe while waiting for the task to complete.</param>
        /// <returns><see langword="true"/> if the data was added; otherwise (if updated), <see langword="false"/>.</returns>
        public Task<bool> UpdateSaveAsync<T>(T value, CancellationTokenSource? cts = null) where T : class;

        /// <summary>
        /// Adds or updates and saves a collection of data to the bank asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of data to add/update and save.</typeparam>
        /// <param name="values">The collection of data to update and save.</param>
        /// /// <param name="cts">The <see cref="CancellationTokenSource"/> to observe while waiting for the task to complete.</param>
        /// <returns>The number of elements added.</returns>
        public Task<int> UpdateSaveAsync<T>(IEnumerable<T> values, CancellationTokenSource? cts = null) where T : class;

        /// <summary>
        /// Drops the specified data from the bank using <see cref="DropStrategy"/> asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of data to drop.</typeparam>
        /// <param name="value">The data to drop and remove.</param>
        /// /// <param name="cts">The <see cref="CancellationTokenSource"/> to observe while waiting for the task to complete.</param>
        /// <returns><see langword="true"/> if the data was dropped; otherwise <see langword="false"/>.</returns>
        public Task<bool> DropSaveAsync<T>(T value, CancellationTokenSource? cts = null) where T : class;

        /// <summary>
        /// Asynchronously drops and removes the specified collection of data from the bank.
        /// </summary>
        /// <typeparam name="T">The type of data in the collection.</typeparam>
        /// <param name="values">The collection of data to remove.</param>
        /// /// <param name="cts">The <see cref="CancellationTokenSource"/> to observe while waiting for the task to complete.</param>
        /// <returns>The number of elements added.</returns>
        public Task<int> DropSaveAsync<T>(IEnumerable<T> values, CancellationTokenSource? cts = null) where T : class;
    }
}