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
    public class SolidLongIdGen(long defaultId = -1, long startId = 1, int? seed = null) : IIdGenerator<long>
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
        /// <remarks>Based on <see cref="DefaultId"/> property.</remarks>
        public long GetDefaultId() => DefaultId;

        /// <inheritdoc/>
        /// <remarks>Based on <see cref="DefaultId"/> property.</remarks>
        public bool IsDefaultID(long id) => id == DefaultId;

        /// <summary>Generates a new unique identifier using a random long value.</summary>
        /// <remarks>Note: This method may not guarantee uniqueness if the range of values is exhausted.</remarks>
        public long GenerateId() => Random.NextInt64();

        /// <inheritdoc/>
        /// <remarks>The method iterates through the existing IDs in the bank and finds the first available value starting from <see cref="StartId"/>.</remarks>
        public long GenerateIdFor<TData>(IDataBank<long, TData> bank, TData @object) where TData : ModelDso<long>
        {
            return bank.GetAllReadonlyData().Select(x => x.GetId()).FirstAvailableValue(StartId);
        }
    }
}