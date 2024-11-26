namespace SKitLs.Data.View.Filters
{
    public abstract class SelectingFilter<T> : SelectingFilterBase<T> where T : class
    {
        public SelectingFilter(string id) : base(id, null!)
        {
            Predicate = Selector;
        }

        public abstract bool Selector(T source);
    }
}