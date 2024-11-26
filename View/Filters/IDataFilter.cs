namespace SKitLs.Data.View.Filters
{
    public interface IDataFilter : IEquatable<IDataFilter>
    {
        public event EventHandler<bool>? SwitcherChanged;

        public void Switch(int? value = null);
        public void SwitchSilent(int? value = null);

        public bool IsEnabled();

        public IEnumerable<TData> Filter<TData>(IEnumerable<TData> data) where TData : class;
    }
}