namespace SKitLs.Data.View.Filters
{
    public abstract class FilterBase : IDataFilter
    {
        public virtual event EventHandler<bool>? SwitcherChanged;

        public string Id { get; set; }
        public virtual int ValidStates { get; set; }
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
                return false;
        }
    }


    public abstract class FilterBase<T> : FilterBase, IDataFilter<T> where T : class
    {
        public FilterBase(string id) : base(id) { }

        public abstract IEnumerable<T> Filter(IEnumerable<T> data);

        public override IEnumerable<TData> Filter<TData>(IEnumerable<TData> data) where TData : class
        {
            if (typeof(TData) == typeof(T))
            {
                return Filter(data.Select(x => (x as T)!)).Select(x => (x as TData)!);
            }
            else
                throw new NotSupportedException();
        }
    }
}