using SKitLs.Data.Banks;
using SKitLs.Data.IO;

namespace SKitLs.Data.IdGenerator
{
    /// <summary>
    /// Implements an identifier generator for <see cref="Guid"/> type identifiers.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GuidIdGenerator"/> class.
    /// </remarks>
    public class GuidIdGenerator : IIdGenerator<Guid>
    {
        /// <inheritdoc/>
        public Guid GetDefaultId() => Guid.Empty;

        /// <inheritdoc/>
        public bool IsDefaultID(Guid id) => id == Guid.Empty;

        /// <inheritdoc/>
        public Guid GenerateId() => Guid.NewGuid();

        /// <inheritdoc/>
        public Guid GenerateIdFor<TData>(IDataBank<Guid, TData> bank, TData @object) where TData : ModelDso<Guid>
        {
            Guid id;
            do
            {
                id = GenerateId();
            } while (bank.TryGetValue(id) is not null);
            return id;
        }
    }
}