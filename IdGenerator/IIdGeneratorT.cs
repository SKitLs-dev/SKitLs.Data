using SKitLs.Data.Banks;
using SKitLs.Data.IO;

namespace SKitLs.Data.IdGenerator
{
    /// <summary>
    /// Represents an interface for generating and managing identifiers of type <typeparamref name="TId"/>.
    /// </summary>
    /// <typeparam name="TId">The type of identifier.</typeparam>
    public interface IIdGenerator<TId> where TId : notnull, IEquatable<TId>, IComparable<TId>
    {
        /// <summary>
        /// Gets the default identifier value for type <typeparamref name="TId"/>.
        /// </summary>
        /// <returns>The default identifier value.</returns>
        public TId GetDefaultId();

        /// <summary>
        /// Checks if the given identifier <paramref name="id"/> is the default identifier value.
        /// </summary>
        /// <param name="id">The identifier to check.</param>
        /// <returns><see langword="true"/> if <paramref name="id"/> is the default identifier; otherwise, <see langword="false"/>.</returns>
        public bool IsDefaultID(TId id);

        /// <summary>
        /// Generates a new unique identifier of type <typeparamref name="TId"/>.
        /// </summary>
        /// <returns>A new unique identifier.</returns>
        public TId GenerateId();

        /// <summary>
        /// Generates a new unique identifier of type <typeparamref name="TId"/> for a specific data bank <paramref name="bank"/>.
        /// </summary>
        /// <typeparam name="TData">The type of data for which the identifier is generated.</typeparam>
        /// <param name="bank">The data bank instance.</param>
        /// <param name="object">The data object for which the identifier is being generated.</param>
        /// <returns>A new unique identifier.</returns>
        /// <remarks>
        /// The identifier generation is specific to the type of data <typeparamref name="TData"/>, which must be of type <see cref="ModelDso{TId}"/>.
        /// </remarks>
        public TId GenerateIdFor<TData>(IDataBank<TId, TData> bank, TData @object) where TData : ModelDso<TId>;
    }
}