namespace SKitLs.Data.View
{
    public abstract class FilterBase<T> : FilterBase, IDataFilter<T> where T : class
    {
        public FilterBase(string id) : base(id) { }

        public abstract IEnumerable<T> Filter(IEnumerable<T> data);

        public override IEnumerable<TData> Filter<TData>(IEnumerable<TData> data) where TData : class
        {
            if (typeof(TData).IsAssignableTo(typeof(T)))
            {
                return Filter(data.Select(x => (x as T)!)).Select(x => (x as TData)!);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}