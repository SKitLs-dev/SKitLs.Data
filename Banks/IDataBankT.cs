using SKitLs.Data.IdGenerator;
using SKitLs.Data.IO;

namespace SKitLs.Data.Banks
{
    /// <summary>
    /// Interface representing a data bank that manages data with specific ID and data types.
    /// </summary>
    /// <typeparam name="TId">The type of the ID of the elements in the bank.</typeparam>
    /// <typeparam name="TData">The type of the data in the bank.</typeparam>
    public interface IDataBank<TId, TData> : IDataBank, IEnumerable<TData> where TId : notnull, IEquatable<TId>, IComparable<TId> where TData : ModelDso<TId>
    {
        /// <summary>
        /// Occurs when new data is added to the bank.
        /// </summary>
        public event DataBankCollectionUpdatedHandler<TId, TData>? OnBankDataAdded;

        /// <summary>
        /// Occurs when data is removed from the bank.
        /// </summary>
        public event DataBankCollectionUpdatedHandler<TId, TData>? OnBankDataDropped;

        // NEW
        /// <summary>
        /// Builds a new instance of the data type managed by the bank.
        /// </summary>
        /// <returns>A new instance of <typeparamref name="TData"/>.</returns>
        public TData BuildNewData();

        /// <summary>
        /// Gets the id generator used for keeping ids constraints.
        /// </summary>
        public IIdGenerator<TId>? GetIdGenerator();

        // GET
        /// <summary>
        /// Retrieves a read-only list of data from the bank.
        /// </summary>
        /// <returns>A read-only list of <typeparamref name="TData"/>.</returns>
        public IReadOnlyList<TData> GetReadonlyData();

        /// <summary>
        /// Retrieves a read-only list of all objects from the data bank (inc. dropped).
        /// </summary>
        /// <returns>A read-only list of all objects.</returns>
        public IReadOnlyList<TData> GetAllReadonlyData();

        /// <summary>
        /// Tries to retrieve data from the bank by its ID.
        /// </summary>
        /// <param name="id">The ID of the data to retrieve.</param>
        /// <returns>The data with the specified ID, or <see langword="null"/> if not found.</returns>
        public TData? TryGetValue(TId id);

        /// <summary>
        /// Tries to retrieve data from the bank that matches the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate to match data.</param>
        /// <returns>The data that matches the predicate, or <see langword="null"/> if no match is found.</returns>
        public TData? TryGetValue(Func<TData, bool> predicate);

        /// <summary>
        /// Retrieves data from the bank by its ID.
        /// </summary>
        /// <param name="id">The ID of the data to retrieve.</param>
        /// <returns>The data with the specified ID.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the specified ID is not found.</exception>
        public TData GetValue(TId id);

        /// <summary>
        /// Retrieves data from the bank that matches the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate to match data.</param>
        /// <returns>The data that matches the predicate.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no data matches the predicate.</exception>
        public TData GetValue(Func<TData, bool> predicate);

        // UPDATE SYNC
        /// <summary>
        /// Adds or updates and saves the specified data to the bank synchronously.
        /// </summary>
        /// <param name="value">The data to add/update and save.</param>
        /// <returns><see langword="true"/> if the data was added; otherwise (if updated), <see langword="false"/>.</returns>
        public bool UpdateSave(TData value);

        /// <summary>
        /// Adds or updates and saves a collection of data to the bank synchronously.
        /// </summary>
        /// <param name="values">The collection of data to add/update and save.</param>
        /// <returns>The number of elements added.</returns>
        public int UpdateSave(IEnumerable<TData> values);

        /// <summary>
        /// Drops the specified data from the bank using <see cref="IDataBank.DropStrategy"/> synchronously.
        /// </summary>
        /// <param name="value">The data to drop and remove.</param>
        /// <returns><see langword="true"/> if the data was dropped; otherwise <see langword="false"/>.</returns>
        public bool DropSave(TData value);

        /// <summary>
        /// Drops and removes the specified collection of data from the bank.
        /// </summary>
        /// <param name="values">The collection of data to remove.</param>
        /// <returns>The number of elements removed.</returns>
        public int DropSave(IEnumerable<TData> values);

        // UPDATE ASYNC
        /// <summary>
        /// Adds or updates and saves the specified data to the bank asynchronously.
        /// </summary>
        /// <param name="value">The data to add/update and save.</param>
        /// /// <param name="cts">The <see cref="CancellationTokenSource"/> to observe while waiting for the task to complete.</param>
        /// <returns><see langword="true"/> if the data was added; otherwise (if updated), <see langword="false"/>.</returns>
        public Task<bool> UpdateSaveAsync(TData value, CancellationTokenSource? cts = null);

        /// <summary>
        /// Updates and saves a collection of data to the bank asynchronously.
        /// </summary>
        /// <param name="values">The collection of data to add/update and save.</param>
        /// /// <param name="cts">The <see cref="CancellationTokenSource"/> to observe while waiting for the task to complete.</param>
        /// <returns>The number of elements added.</returns>
        public Task<int> UpdateSaveAsync(IEnumerable<TData> values, CancellationTokenSource? cts = null);

        /// <summary>
        /// Drops the specified data from the bank asynchronously.
        /// </summary>
        /// <param name="value">The data to drop and remove.</param>
        /// /// <param name="cts">The <see cref="CancellationTokenSource"/> to observe while waiting for the task to complete.</param>
        /// <returns><see langword="true"/> if the data was dropped; otherwise <see langword="false"/>.</returns>
        public Task<bool> DropSaveAsync(TData value, CancellationTokenSource? cts = null);

        /// <summary>
        /// Asynchronously drops and removes the specified collection of data from the bank.
        /// </summary>
        /// <param name="values">The collection of data to remove.</param>
        /// /// <param name="cts">The <see cref="CancellationTokenSource"/> to observe while waiting for the task to complete.</param>
        /// <returns>The number of elements removed.</returns>
        public Task<int> DropSaveAsync(IEnumerable<TData> values, CancellationTokenSource? cts = null);

        // SELECTORS
        /// <summary>
        /// Drops data from the bank that matches the specified <paramref name="predicate"/> synchronously.
        /// </summary>
        /// <param name="predicate">The predicate to match data to drop.</param>
        /// <returns>The number of elements removed.</returns>
        public int DropSaveWhere(Func<TData, bool> predicate);

        /// <summary>
        /// Drops data from the bank that matches the specified <paramref name="predicate"/> asynchronously.
        /// </summary>
        /// <param name="predicate">The predicate to match data to drop.</param>
        /// /// <param name="cts">The <see cref="CancellationTokenSource"/> to observe while waiting for the task to complete.</param>
        /// <returns>The number of elements removed.</returns>
        public Task<int> DropSaveWhereAsync(Func<TData, bool> predicate, CancellationTokenSource? cts = null);
    }
}