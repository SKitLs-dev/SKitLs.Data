using SKitLs.Data.IdGenerator;
using SKitLs.Data.IO;
using System.Collections;

namespace SKitLs.Data.Banks
{
    // TODO Exceptions
    /// <summary>
    /// Abstract base class representing a data bank that manages data with specific ID and data types.
    /// </summary>
    /// <typeparam name="TId">The type of the ID of the elements in the bank.</typeparam>
    /// <typeparam name="TData">The type of the data in the bank.</typeparam>
    /// <remarks>
    /// This class provides the base implementation for a data bank, including methods for initializing, updating, and dropping data.
    /// Derived classes should implement specific data retrieval mechanisms.
    /// </remarks>
    public abstract class DataBankBase<TId, TData> : IDataBank<TId, TData> where TId : notnull, IEquatable<TId>, IComparable<TId> where TData : ModelDso<TId>
    {
        private IDataManager? _dm;
        /// <inheritdoc/>
        public IDataManager DataManager
        {
            get => _dm ?? throw new NullReferenceException();
            set => _dm = value;
        }

        /// <inheritdoc/>
        public event DataBankUpdatedHandler? OnBankInfoUpdated;

        /// <inheritdoc/>
        public event DataBankCollectionUpdatedHandler? OnBankDataUpdated;

        /// <inheritdoc/>
        public event DataBankCollectionUpdatedHandler<TId, TData>? OnBankDataAdded;

        /// <inheritdoc/>
        public event DataBankCollectionUpdatedHandler<TId, TData>? OnBankDataDropped;

        /// <inheritdoc/>
        public string Id => HoldingType.Name.ToLower();

        /// <inheritdoc/>
        public string? Name { get; set; }

        /// <inheritdoc/>
        public string? Description { get; set; }

        /// <inheritdoc/>
        public Type HoldingType => typeof(TData);

        /// <inheritdoc/>
        public long Count => GetReadonly().Count;

        /// <inheritdoc/>
        public long CountAll => GetAllReadonly().Count;

        /// <summary>
        /// 
        /// </summary>
        protected SortedList<TId, TData> Data { get; set; } = [];

        /// <inheritdoc/>
        public DropStrategy DropStrategy { get; init; }

        /// <inheritdoc/>
        public abstract IDataReader? GetReader();

        /// <inheritdoc/>
        public abstract void UpdateReader(IDataReader reader);

        /// <inheritdoc/>
        public abstract IDataWriter? GetWriter();

        /// <inheritdoc/>
        public abstract void UpdateWriter(IDataWriter writer);

        /// <inheritdoc/>
        public abstract IIdGenerator<TId>? GetIdGenerator();

        /// <summary>
        /// 
        /// </summary>
        protected Exception NullReader => new NullReferenceException($"Null Reader in DataBank '{Id}'");
        /// <summary>
        /// 
        /// </summary>
        protected Exception NullWriter => new NullReferenceException($"Null Writer in DataBank '{Id}'");
        /// <summary>
        /// 
        /// </summary>
        protected Exception NullIdGen => new NullReferenceException($"Null Id Generator in DataBank '{Id}'");
        /// <summary>
        /// 
        /// </summary>
        protected Exception NoElement => new KeyNotFoundException($"Element not found");
        /// <summary>
        /// 
        /// </summary>
        protected Exception NotSupported(Type attempt, Type target) => new NotSupportedException($"Cannot handle values of a type '{attempt.Name}' in '{target.Name}' typed bank ('{Id}').");

        /// <summary>
        /// Initializes a new instance of the <see cref="DataBankBase{TId, TData}"/> class.
        /// </summary>
        public DataBankBase() { }

        // INITIALIZE
        /// <inheritdoc/>
        /// <exception cref="NullReferenceException">Thrown when the <see cref="GetReader"/> is <see langword="null"/>.</exception>
        public void Initialize()
        {
            var reader = GetReader() ?? throw NullReader;
            var read = reader.ReadData<TData>();
            foreach (var item in read)
            {
                Data.Add(item.GetId(), item);
            }
            UpdateSave(read);
        }

        /// <inheritdoc/>
        /// <exception cref="NullReferenceException">Thrown when the <see cref="GetReader"/> is <see langword="null"/>.</exception>
        public async Task InitializeAsync()
        {
            var reader = GetReader() ?? throw NullReader;
            var read = await reader.ReadDataAsync<TData>();
            foreach (var item in read)
            {
                Data.Add(item.GetId(), item);
            }
            UpdateSave(read);
        }

        // NEW
        /// <inheritdoc/>
        /// <inheritdoc cref="BuildNewData"/>
        public object BuildNewObject() => BuildNewData();

        /// <inheritdoc/>
        public abstract TData BuildNewData();

        // IO
        /// <inheritdoc/>
        /// <inheritdoc cref="IDataWriter{T}.WriteData(T)"/>
        /// <exception cref="NullReferenceException">Thrown when the <see cref="GetWriter"/> is <see langword="null"/>.</exception>
        private void SaveObject(TData @object) => (GetWriter() ?? throw NullWriter).WriteData(@object);

        /// <inheritdoc/>
        /// <inheritdoc cref="IDataWriter{T}.WriteDataList(IEnumerable{T})"/>
        /// <exception cref="NullReferenceException">Thrown when the <see cref="GetWriter"/> is <see langword="null"/>.</exception>
        private void DeleteObjects(IEnumerable<TData> objects) => (GetWriter() ?? throw NullWriter).DeleteDataList(objects);
        /// <inheritdoc/>
        /// <inheritdoc cref="IDataWriter{T}.WriteDataList(IEnumerable{T})"/>
        /// <exception cref="NullReferenceException">Thrown when the <see cref="GetWriter"/> is <see langword="null"/>.</exception>
        private async Task DeleteObjectsAsync(IEnumerable<TData> objects) => await (GetWriter() ?? throw NullWriter).DeleteDataListAsync(objects);

        /// <inheritdoc/>
        /// <inheritdoc cref="IDataWriter{T}.WriteDataList(IEnumerable{T})"/>
        /// <exception cref="NullReferenceException">Thrown when the <see cref="GetWriter"/> is <see langword="null"/>.</exception>
        private void SaveObjects(IEnumerable<TData> objects) => (GetWriter() ?? throw NullWriter).WriteDataList(objects);

        /// <inheritdoc/>
        /// <inheritdoc cref="IDataWriter{T}.WriteDataAsync(T, CancellationTokenSource?)"/>
        /// <exception cref="NullReferenceException">Thrown when the <see cref="GetWriter"/> is <see langword="null"/>.</exception>
        private async Task SaveObjectAsync(TData @object, CancellationTokenSource? cts = null)
        {
            var writer = GetWriter() ?? throw NullWriter;
            await writer.WriteDataAsync(@object, cts);
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="IDataWriter{T}.WriteDataListAsync(IEnumerable{T}, CancellationTokenSource?)"/>
        /// <exception cref="NullReferenceException">Thrown when the <see cref="GetWriter"/> is <see langword="null"/>.</exception>
        private async Task SaveObjectsAsync(IEnumerable<TData> objects, CancellationTokenSource? cts = null)
        {
            var writer = GetWriter() ?? throw NullWriter;
            await writer.WriteDataListAsync(objects, cts);
        }

        // GET
        /// <inheritdoc/>
        public IEnumerator<TData> GetEnumerator() => Data.Values.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public IReadOnlyList<object> GetReadonly() => GetReadonlyData();

        /// <inheritdoc/>
        public IReadOnlyList<TData> GetReadonlyData() => [.. Data.Values.Where(x => x.IsEnabled)];

        /// <inheritdoc/>
        public IReadOnlyList<object> GetAllReadonly() => GetAllReadonlyData();

        /// <inheritdoc/>
        public IReadOnlyList<TData> GetAllReadonlyData() => [.. Data.Values];

        /// <inheritdoc/>
        public TData? TryGetValue(TId id)
        {
            Data.TryGetValue(id, out var value);
            return value;
        }

        /// <inheritdoc/>
        public TData? TryGetValue(Func<TData, bool> predicate) => Data.Values.FirstOrDefault(predicate);

        /// <inheritdoc/>
        /// <inheritdoc cref="TryGetValue(TId)"/>
        /// <exception cref="KeyNotFoundException">Thrown when the specified ID is not found.</exception>
        public TData GetValue(TId id) => TryGetValue(id) ?? throw NoElement;

        /// <inheritdoc/>
        /// <inheritdoc cref="TryGetValue(Func{TData, bool})"/>
        /// <exception cref="KeyNotFoundException">Thrown when no data matches the predicate.</exception>
        public TData GetValue(Func<TData, bool> predicate) => TryGetValue(predicate) ?? throw NoElement;

        /// <inheritdoc/>
        /// <inheritdoc cref="UpdateSave(TData)"/>
        public bool UpdateSave<T>(T value) where T : class
        {
            if (typeof(T) == typeof(TData))
                return UpdateSave((value as TData)!);
            else
                throw NotSupported(typeof(T), typeof(TData));
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="SaveObject(TData)"/>
        /// <exception cref="NullReferenceException">Thrown when the <see cref="GetIdGenerator"/> is <see langword="null"/>.</exception>
        public bool UpdateSave(TData value)
        {
            var gen = GetIdGenerator() ?? throw NullIdGen;
            var @new = false;

            if (gen.IsDefaultID(value.GetId()))
            {
                Fit(value, gen);
                Data.Add(value.GetId(), value);
                @new = true;
            }

            SaveObject(value);
            OnBankDataUpdated?.Invoke(1);
            OnBankDataAdded?.Invoke([value]);
            return @new;
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="UpdateSave(IEnumerable{TData})"/>
        /// <exception cref="NotSupportedException">Thrown when the specified type <typeparamref name="T"/> is not supported.</exception>
        public int UpdateSave<T>(IEnumerable<T> values) where T : class
        {
            if (typeof(T) == typeof(TData) || values.FirstOrDefault()?.GetType() == typeof(TData))
                return UpdateSave(values.Select(x => (x as TData)!));
            else
                throw NotSupported(typeof(T), typeof(TData));
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="SaveObjects(IEnumerable{TData})"/>
        /// <exception cref="NullReferenceException">Thrown when the <see cref="GetIdGenerator"/> is <see langword="null"/>.</exception>
        public int UpdateSave(IEnumerable<TData> values)
        {
            var gen = GetIdGenerator() ?? throw NullIdGen;
            var @new = 0;

            foreach (var value in values)
            {
                if (gen.IsDefaultID(value.GetId()))
                {
                    Fit(value, gen);
                    Data.Add(value.GetId(), value);
                    @new++;
                }
            }

            SaveObjects(values);
            OnBankDataUpdated?.Invoke(values.Count());
            OnBankDataAdded?.Invoke(values);
            return @new;
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="DropSave(TData)"/>
        /// <exception cref="NotSupportedException">Thrown when the specified type <typeparamref name="T"/> is not supported.</exception>
        public bool DropSave<T>(T value) where T : class
        {
            if (value.GetType() == typeof(TData))
                return DropSave((value as TData)!);
            else
                throw NotSupported(typeof(T), typeof(TData));
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="SaveObject(TData)"/>
        public bool DropSave(TData value)
        {
            value.Disable();
            if (DropStrategy == DropStrategy.Disable)
            {
                SaveObject(value);
            }
            else //if (DropStrategy == DropStrategy.Delete)
            {
                try
                {
                    Data.Remove(value.GetId());
                    DeleteObjects([value]);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            OnBankDataUpdated?.Invoke(1);
            OnBankDataDropped?.Invoke([value]);
            return true;
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="DropSave(IEnumerable{TData})"/>
        /// <exception cref="NotSupportedException">Thrown when the specified type <typeparamref name="T"/> is not supported.</exception>
        public int DropSave<T>(IEnumerable<T> values) where T : class
        {
            if (typeof(T) == typeof(TData) || values.FirstOrDefault()?.GetType() == typeof(TData))
                return DropSave(values.Select(x => (x as TData)!));
            else
                throw NotSupported(typeof(T), typeof(TData));
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="SaveObjects(IEnumerable{TData})"/>
        public int DropSave(IEnumerable<TData> values)
        {
            var affected = new List<TData>();

            foreach (var value in values)
            {
                value.Disable();
                if (DropStrategy == DropStrategy.Delete)
                {
                    try
                    {
                        Data.Remove(value.GetId());
                        affected.Add(value);
                    }
                    catch (Exception) { }
                }
            }

            if (DropStrategy == DropStrategy.Disable)
            {
                SaveObjects(affected);
            }
            else
            {
                DeleteObjects(affected);
            }
            OnBankDataUpdated?.Invoke(affected.Count);
            OnBankDataDropped?.Invoke(affected);
            return affected.Count;
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="UpdateSaveAsync(TData, CancellationTokenSource)"/>
        /// <exception cref="NotSupportedException">Thrown when the specified type <typeparamref name="T"/> is not supported.</exception>
        public async Task<bool> UpdateSaveAsync<T>(T value, CancellationTokenSource? cts = null) where T : class
        {
            if (typeof(T) == typeof(TData))
                return await UpdateSaveAsync((value as TData)!, cts);
            else
                throw NotSupported(typeof(T), typeof(TData));
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="SaveObjectAsync(TData, CancellationTokenSource?)"/>
        /// <exception cref="NullReferenceException">Thrown when the <see cref="GetIdGenerator"/> is <see langword="null"/>.</exception>
        public async Task<bool> UpdateSaveAsync(TData value, CancellationTokenSource? cts = null)
        {
            var gen = GetIdGenerator() ?? throw NullIdGen;
            var @new = false;

            if (gen.IsDefaultID(value.GetId()))
            {
                Fit(value, gen);
                Data.Add(value.GetId(), value);
                @new = true;
            }

            await SaveObjectAsync(value, cts);
            OnBankDataUpdated?.Invoke(1);
            OnBankDataAdded?.Invoke([value]);
            return @new;
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="UpdateSaveAsync(IEnumerable{TData}, CancellationTokenSource?)"/>
        /// <exception cref="NotSupportedException">Thrown when the specified type <typeparamref name="T"/> is not supported.</exception>
        public async Task<int> UpdateSaveAsync<T>(IEnumerable<T> values, CancellationTokenSource? cts = null) where T : class
        {
            if (typeof(T) == typeof(TData) || values.FirstOrDefault()?.GetType() == typeof(TData))
                return await UpdateSaveAsync(values.Select(x => (x as TData)!), cts);
            else
                throw NotSupported(typeof(T), typeof(TData));
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="SaveObjectsAsync(IEnumerable{TData}, CancellationTokenSource?)"/>
        /// <exception cref="NullReferenceException">Thrown when the <see cref="GetIdGenerator"/> is <see langword="null"/>.</exception>
        public async Task<int> UpdateSaveAsync(IEnumerable<TData> values, CancellationTokenSource? cts = null)
        {
            var gen = GetIdGenerator() ?? throw NullIdGen;
            var @new = 0;

            foreach (var value in values)
            {
                if (gen.IsDefaultID(value.GetId()))
                {
                    Fit(value, gen);
                    Data.Add(value.GetId(), value);
                    @new++;
                }
            }

            await SaveObjectsAsync(values, cts);
            OnBankDataUpdated?.Invoke(values.Count());
            OnBankDataAdded?.Invoke(values);
            return @new;
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="DropSaveAsync(TData, CancellationTokenSource?)"/>
        /// <exception cref="NotSupportedException">Thrown when the specified type <typeparamref name="T"/> is not supported.</exception>
        public async Task<bool> DropSaveAsync<T>(T value, CancellationTokenSource? cts = null) where T : class
        {
            if (value.GetType() == typeof(TData))
                return await DropSaveAsync((value as TData)!, cts);
            else
                throw NotSupported(typeof(T), typeof(TData));
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="SaveObjectAsync(TData, CancellationTokenSource?)"/>
        public async Task<bool> DropSaveAsync(TData value, CancellationTokenSource? cts = null)
        {
            value.Disable();
            if (DropStrategy == DropStrategy.Disable)
            {
                await SaveObjectsAsync([value], cts);
            }
            else
            {
                try
                {
                    Data.Remove(value.GetId());
                    await DeleteObjectsAsync([value]);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            OnBankDataUpdated?.Invoke(1);
            OnBankDataDropped?.Invoke([value]);
            return true;
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="DropSaveAsync(IEnumerable{TData}, CancellationTokenSource?)"/>
        /// <exception cref="NotSupportedException">Thrown when the specified type <typeparamref name="T"/> is not supported.</exception>
        public async Task<int> DropSaveAsync<T>(IEnumerable<T> values, CancellationTokenSource? cts = null) where T : class
        {
            if (typeof(T) == typeof(TData) || values.FirstOrDefault()?.GetType() == typeof(TData))
                return await DropSaveAsync(values.Select(x => (x as TData)!), cts);
            else
                throw NotSupported(typeof(T), typeof(TData));
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="SaveObjectsAsync(IEnumerable{TData}, CancellationTokenSource?)"/>
        public async Task<int> DropSaveAsync(IEnumerable<TData> values, CancellationTokenSource? cts = null)
        {
            var affected = new List<TData>();

            foreach (var value in values)
            {
                value.Disable();
                if (DropStrategy == DropStrategy.Delete)
                {
                    try
                    {
                        Data.Remove(value.GetId());
                        affected.Add(value);
                    }
                    catch (Exception) { }
                }
            }

            if (DropStrategy == DropStrategy.Disable)
            {
                await SaveObjectsAsync(affected);
            }
            else
            {
                await DeleteObjectsAsync(affected);
            }
            OnBankDataUpdated?.Invoke(affected.Count);
            OnBankDataDropped?.Invoke(affected);
            return affected.Count;
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="DropSave(IEnumerable{TData})"/>
        public int DropSaveWhere(Func<TData, bool> predicate) => DropSave(GetReadonlyData().Where(predicate));

        /// <inheritdoc/>
        /// <inheritdoc cref="DropSaveAsync(IEnumerable{TData}, CancellationTokenSource?)"/>
        public async Task<int> DropSaveWhereAsync(Func<TData, bool> predicate, CancellationTokenSource? cts = null) => await DropSaveAsync(GetReadonlyData().Where(predicate), cts);

        /// <summary>
        /// Updates data's id using <see cref="GetIdGenerator"/> and subscribing for updates.
        /// </summary>
        /// <param name="data">Data to fit.</param>
        /// <param name="gen">The IdGenerator.</param>
        /// <returns>Fitted data.</returns>
        private void Fit(TData data, IIdGenerator<TId> gen)
        {
            data.SaveRequested += (sender) =>
            {
                if (sender is TData data)
                    SaveObject(data);
                else throw NotSupported(sender.GetType(), typeof(TData));
            };

            data.SetId(gen.GenerateIdFor(this, data));
        }
    }
}