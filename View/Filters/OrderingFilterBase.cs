namespace SKitLs.Data.View.Filters
{
    public abstract class OrderingFilterBase : FilterBase
    {
        protected OrderingFilterBase(string id, int validStates) : base(id, validStates) { }
    }

    public class PropOrder : OrderingFilterBase
    {
        public string PropertyName { get; set; }
        public Type Type { get; set; }

        public PropOrder(string propertyName, Type type) : base($"prop.{type.Name}.{propertyName}", 3)
        {
            PropertyName = propertyName;
            Type = type;
        }

        public override IEnumerable<TData> Filter<TData>(IEnumerable<TData> data) where TData : class
        {
            var prop = Type.GetProperty(PropertyName) ?? throw new Exception();
            if (State == 1)
                return data.OrderBy(prop.GetValue);
            else if (State == 2)
                return data.OrderByDescending(prop.GetValue);
            else
                return data;
        }
    }
}