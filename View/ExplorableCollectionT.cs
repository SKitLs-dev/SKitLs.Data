using SKitLs.Data.View.Filters;

namespace SKitLs.Data.View
{
    public class ExplorableEnum<T> where T : class
    {
        public event EventHandler? FiltersChanged;

        public Func<IEnumerable<T>>? DataRetriever { get; set; }
        public IEnumerable<T> Values { get; set; }
        private IEnumerable<T>? _filtered = null;

        private List<OrderingFilterBase> OrderingFilters { get; set; } = [];
        private OrderingFilterBase? CurrentOrdering => OrderingFilters.Find(x => x.IsEnabled());
        private List<IDataFilter> Filters { get; set; } = [];

        public ExplorableEnum(IEnumerable<T> data)
        {
            Values = data;
            FiltersChanged += (s, e) => _filtered = null;
        }

        public ExplorableEnum(Func<IEnumerable<T>> dataRetriever) : this(dataRetriever?.Invoke() ?? throw new ArgumentNullException(nameof(dataRetriever)))
        {
            DataRetriever = dataRetriever;
        }

        public void SourceObsolete()
        {
            if (DataRetriever is null)
                throw new ArgumentNullException(nameof(DataRetriever));

            Values = DataRetriever.Invoke();
            _filtered = null;
        }

        public void AddFilter(IDataFilter filter)
        {
            filter.SwitcherChanged += (s, e) => FiltersChanged?.Invoke(this, EventArgs.Empty);
            if (filter is OrderingFilterBase ordering)
            {
                ordering.SwitcherChanged += (s, e) =>
                {
                    OrderingFilters.Except([ordering]).ToList().ForEach(x => x.SwitchSilent(0));
                };
                OrderingFilters.Add(ordering);
            }
            else
                Filters.Add(filter);
            FiltersChanged?.Invoke(this, EventArgs.Empty);
        }

        public IEnumerable<T> Explore()
        {
            if (_filtered is null)
            {
                var view = Values;
                foreach (var f in Filters)
                {
                    if (f.IsEnabled())
                    {
                        view = f.Filter(view);
                    }
                }
                _filtered = view;
            }
            if (CurrentOrdering is null)
                return _filtered;
            else
                return CurrentOrdering.Filter(_filtered);
        }
    }
}