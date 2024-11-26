using SKitLs.Data.Banks;
using SKitLs.Data.IO;
using SKitLs.Utils.Extensions.Lists;

namespace SKitLs.Data.IdGenerator
{
    /// <summary>
    /// Implements an identifier generator for <see cref="long"/> type identifiers with uninterrupted values support.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="SolidLongIdGen"/> class with optional default values.
    /// </remarks>
    /// <param name="defaultId">The default identifier value.</param>
    /// <param name="startId">The starting identifier value for generation.</param>
    /// <param name="seed">Optional seed value for random number generation.</param>
    public class SolidLongIdGen(long defaultId = -1, int startId = 1, int? seed = null) : IIdGenerator<long>
    {
        private Random Random { get; set; } = seed is not null ? new Random(seed.Value) : new Random();

        /// <summary>
        /// Gets the default identifier value (-1 by default).
        /// </summary>
        private long DefaultId { get; set; } = defaultId;

        /// <summary>
        /// Gets the starting identifier value for generation (1 by default).
        /// </summary>
        private long StartId { get; set; } = startId;

        /// <inheritdoc/>
        public long GetDefaultId() => DefaultId;

        /// <inheritdoc/>
        public bool IsDefaultID(long id) => id == DefaultId;

        /// <inheritdoc/>
        public long GenerateId() => Random.NextInt64();

        /// <inheritdoc/>
        public long GenerateIdFor<TData>(IDataBank<long, TData> bank, TData @object) where TData : ModelDso<long>
        {
            return bank.GetAllReadonlyData().Select(x => x.GetId()).FirstAvailableValue(StartId);
        }
    }
}