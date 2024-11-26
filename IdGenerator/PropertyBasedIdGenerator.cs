using SKitLs.Data.Banks;
using SKitLs.Data.IO;

namespace SKitLs.Data.IdGenerator
{
    /// <summary>
    /// A base class for generating identifiers based on a property of an object rather than generating new unique IDs.
    /// </summary>
    /// <typeparam name="TId">The type of the identifier, which must implement <see cref="IEquatable{T}"/> and <see cref="IComparable{T}"/>.</typeparam>
    /// <typeparam name="T">The type of data object, which must inherit from <see cref="ModelDso{TId}"/>.</typeparam>
    /// <remarks>
    /// This class does not generate new IDs but relies on properties of the existing object to determine the ID.
    /// The <see cref="GenerateId()"/> method is unsupported and will throw a <see cref="NotSupportedException"/>.
    /// </remarks>
    public abstract class PropertyBasedIdGenerator<TId, T> : IIdGenerator<TId> where TId : notnull, IEquatable<TId>, IComparable<TId> where T : ModelDso<TId>
    {
        /// <inheritdoc/>
        public abstract TId GetDefaultId();

        /// <inheritdoc/>
        public bool IsDefaultID(TId id) => GetDefaultId().Equals(id);

        /// <inheritdoc/>
        /// <remarks><b>The <see cref="PropertyBasedIdGenerator{TId, T}"/> does not support creating new unique identifiers.</b></remarks>
        public virtual TId GenerateId() => throw new NotSupportedException();

        /// <inheritdoc/>
        /// <remarks>
        /// This method tries to cast the bank and object to the expected types. If casting fails, a <see cref="NotSupportedException"/> is thrown.
        /// </remarks>
        /// <exception cref="NotImplementedException">Thrown if the provided bank or object type is not compatible with the expected types.</exception>
        public TId GenerateIdFor<TData>(IDataBank<TId, TData> bank, TData @object) where TData : ModelDso<TId>
        {
            if (bank is IDataBank<TId, T> banka && @object is T obj)
                return GenerateIdFor(banka, obj);

            throw new NotSupportedException("The data bank or object type does not match the expected types.");
        }

        /// <summary>
        /// Abstract method that generates an identifier for a specific object within a data bank.
        /// </summary>
        /// <param name="bank">The data bank containing the object.</param>
        /// <param name="object">The object for which the identifier is being generated.</param>
        /// <returns>The identifier for the specified object.</returns>
        /// <remarks>
        /// This method should be implemented in derived classes to generate an identifier based on a specific property of the object.
        /// </remarks>
        public abstract TId GenerateIdFor(IDataBank<TId, T> bank, T @object);
    }
}