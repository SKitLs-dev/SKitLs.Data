namespace SKitLs.Data.View.Filters
{
    public interface IDataFilter<T> : IDataFilter where T : class
    {
        public IEnumerable<T> Filter(IEnumerable<T> data);
    }
}