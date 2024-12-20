namespace SKitLs.Data.View
{
    /// <summary>
    /// Represents a generic data filter interface that extends <see cref="IDataFilter"/> for a specific data type.
    /// </summary>
    /// <typeparam name="T">The type of data to filter.</typeparam>
    /// <seealso cref="IDataFilter" />
    public interface IDataFilter<T> : IDataFilter where T : class
    {
        /// <summary>
        /// Filters the specified collection of data of type <typeparamref name="T"/> based on filter criteria.
        /// </summary>
        /// <param name="data">The collection of data to filter.</param>
        /// <returns>The filtered collection of data of type <typeparamref name="T"/>.</returns>
        public IEnumerable<T> Filter(IEnumerable<T> data);
    }
}