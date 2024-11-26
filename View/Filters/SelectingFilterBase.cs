namespace SKitLs.Data.View.Filters
{
    public class SelectingFilterBase<T> : FilterBase<T> where T : class
    {
        public Func<T, bool> Predicate { get; set; }

        public SelectingFilterBase(string id, Func<T, bool> predicate) : base(id)
        {
            Predicate = predicate;
        }

        public override IEnumerable<T> Filter(IEnumerable<T> data)
        {
            return data.Where(Predicate);
        }
    }
}