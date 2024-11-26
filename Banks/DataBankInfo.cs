using SKitLs.Data.IO;

namespace SKitLs.Data.Banks
{
    /// <summary>
    /// Represents information about a data bank, including the count of items, the type of data, and optional name and description.
    /// </summary>
    /// <param name="count">The number of items in the data bank.</param>
    /// <param name="inType">The type of data stored in the data bank.</param>
    /// <param name="name">Optional: The name of the data bank. If not provided, a default name based on the data type will be used.</param>
    /// <param name="description">Optional: The description of the data bank.</param>
    public class DataBankInfo(long count, Type inType, string? name = null, string? description = null)
    {
        /// <summary>
        /// Gets or sets the number of items in the data bank.
        /// </summary>
        public long Count { get; set; } = count;

        /// <summary>
        /// Gets or sets the type of data stored in the data bank.
        /// </summary>
        public Type InType { get; set; } = inType;

        /// <summary>
        /// Gets or sets the name of the data bank.
        /// </summary>
        public string Name { get; set; } = name ?? $"Bank '{inType.Name}'";

        /// <summary>
        /// Gets or sets the description of the data bank.
        /// </summary>
        public string? Description { get; set; } = description;

        /// <summary>
        /// Creates a <see cref="DataBankInfo"/> instance from an existing <see cref="IDataBank{TId, TData}"/> instance.
        /// </summary>
        /// <typeparam name="TId">The type of the ID of the elements in the bank.</typeparam>
        /// <typeparam name="TData">The type of the data in the bank.</typeparam>
        /// <param name="bank">The data bank instance from which to create the information.</param>
        /// <returns>A new <see cref="DataBankInfo"/> instance containing information from the specified data bank.</returns>
        internal static DataBankInfo OfDataBank<TId, TData>(IDataBank<TId, TData> bank) where TId : notnull, IEquatable<TId>, IComparable<TId> where TData : ModelDso<TId>
        {
            return new(bank.Count, typeof(TData), bank.Name, bank.Description);
        }
    }
}