namespace SKitLs.Data.View
{
    /// <summary>
    /// Represents a data filter interface that supports enabling, disabling, and filtering data.
    /// </summary>
    /// <seealso cref="System.IEquatable{IDataFilter}" />
    public interface IDataFilter : IEquatable<IDataFilter>
    {
        /// <summary>
        /// Occurs when the switcher state changes.
        /// </summary>
        public event EventHandler<bool>? SwitcherChanged;

        /// <summary>
        /// Toggles the switcher state with an optional value.
        /// </summary>
        /// <param name="value">The optional value to set for the switcher.</param>
        public void Switch(int? value = null);


        /// <summary>
        /// Toggles the switcher state silently with an optional value, without raising events.
        /// </summary>
        /// <param name="value">The optional value to set for the switcher.</param>
        public void SwitchSilent(int? value = null);

        /// <summary>
        /// Determines whether the switcher is currently enabled.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if enabled; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsEnabled();

        /// <summary>
        /// Filters the specified data collection based on filter criteria.
        /// </summary>
        /// <typeparam name="TData">The type of data to filter.</typeparam>
        /// <param name="data">The collection of data to filter.</param>
        /// <returns>The filtered collection of data.</returns>
        public IEnumerable<TData> Filter<TData>(IEnumerable<TData> data) where TData : class;
    }
}