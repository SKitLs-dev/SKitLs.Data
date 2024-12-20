namespace SKitLs.Data.View
{
    /// <summary>
    /// Provides a base implementation of the <see cref="IDataFilter"/> interface.
    /// </summary>
    /// <seealso cref="IDataFilter"/>
    public abstract class FilterBase : IDataFilter
    {
        /// <inheritdoc />
        public virtual event EventHandler<bool>? SwitcherChanged;

        /// <summary>
        /// Gets or sets the unique identifier for the filter instance.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the number of valid states for the filter.
        /// </summary>
        public virtual int ValidStates { get; set; }

        /// <summary>
        /// Gets or sets the current state of the filter.
        /// </summary>
        public virtual int State { get; set; } = 0;

        public FilterBase(string id, int validStates = 2)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            ValidStates = validStates;
        }

        public virtual bool IsEnabled() => State != 0;
        public virtual void Switch(int? value = null)
        {
            SwitchSilent(value);
            SwitcherChanged?.Invoke(this, IsEnabled());
        }
        public virtual void SwitchSilent(int? value = null)
        {
            State = value is not null ? value.Value : (State + 1) % ValidStates;
        }
        public abstract IEnumerable<TData> Filter<TData>(IEnumerable<TData> data) where TData : class;

        public virtual bool Equals(IDataFilter? other)
        {
            if (other is FilterBase filter)
            {
                return filter.Id == Id;
            }
            else
            {
                return false;
            }
        }
    }
}