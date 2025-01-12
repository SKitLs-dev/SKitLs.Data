using SKitLs.Data.Banks;
using SKitLs.Data.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKitLs.Data.IdGenerator
{
    /// <summary>
    /// Represents the arguments of an event triggered when the allowable limits of free IDs for <see cref="RandomLongIdGen"/> are nearing exhaustion.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="LimitsExpiringArgs"/> class.
    /// </remarks>
    /// <param name="dataBank">The related <see cref="IDataBank"/>.</param>
    /// <param name="current">The current count of IDs used.</param>
    /// <param name="max">The maximum count of IDs available.</param>
    public class LimitsExpiringArgs(IDataBank dataBank, long current, long max) : EventArgs
    {
        /// <summary>
        /// The related <see cref="IDataBank"/>.
        /// </summary>
        public IDataBank DataBank { get; set; } = dataBank;

        /// <summary>
        /// Gets the current count of IDs used.
        /// </summary>
        public long CurrentCount { get; init; } = current;

        /// <summary>
        /// Gets the maximum count of IDs available.
        /// </summary>
        public long MaxCount { get; init; } = max;

        /// <summary>
        /// Gets the number of IDs still available.
        /// </summary>
        public long Available => MaxCount - CurrentCount;
    }

    /// <summary>
    /// An ID generator that creates random IDs within a specified range <c>[<see cref="FromId"/>; <see cref="ToId"/>)</c>.
    /// Once a threshold defined by <see cref="StopRandomOn"/> is reached, switches to <see cref="SolidLongIdGen"/> mode for better efficiency and continuous ID assignment.
    /// If the <see cref="ExpiresOn"/> threshold is reached, an error is thrown.
    /// </summary>
    /// <remarks>Initializes a new instance of the <see cref="RandomLongIdGen"/> class.</remarks>
    /// <param name="defaultId">The default ID value.</param>
    /// <param name="fromId">The starting range for ID generation.</param>
    /// <param name="toId">The ending range for ID generation.</param>
    /// <param name="notifyOnFullness">The threshold for triggering the <see cref="LimitsExceeding"/> event.</param>
    /// <param name="seed">An optional seed for the random number generator.</param>
    public class RandomLongIdGen(long defaultId = -1, long fromId = 10000, long toId = 100000, double notifyOnFullness = 0.6d, int? seed = null) : IIdGenerator<long>
    {
        /// <summary>
        /// Occurs when the limits of available IDs are nearing exhaustion.
        /// </summary>
        /// <remarks>
        /// Related threshold: <see cref="NotifyOnFullness"/>.
        /// </remarks>
        public event EventHandler<LimitsExpiringArgs>? LimitsExceeding;

        /// <summary>
        /// Gets or sets the random number generator instance.
        /// </summary>
        private Random Random { get; set; } = seed is not null ? new Random(seed.Value) : new Random();

        /// <summary>
        /// Gets the default identifier value (-1 by default).
        /// </summary>
        private long DefaultId { get; set; } = defaultId;

        /// <summary>
        /// Gets or sets the starting range for ID generation.
        /// </summary>
        private long FromId { get; set; } = fromId;

        /// <summary>
        /// Gets or sets the ending range for ID generation.
        /// </summary>
        private long ToId { get; set; } = toId;
        
        /// <summary>
        /// Gets the total available IDs based on the range [<see cref="FromId"/>; <see cref="ToId"/>).
        /// </summary>
        private long Available => ToId - FromId;

        /// <summary>
        /// Gets or sets the fullness threshold for triggering the <see cref="LimitsExceeding"/> event.
        /// </summary>
        public double NotifyOnFullness { get; set; } = notifyOnFullness;

        /// <summary>
        /// Gets or sets the threshold for total ID capacity after which an exception is thrown.
        /// </summary>
        /// <remarks>
        /// 97% by default. Avoid using bigger values.
        /// </remarks>
        public double ExpiresOn { get; set; } = 0.97d;

        /// <summary>
        /// Gets or sets the threshold at which random ID generation stops, and the generator switches to <see cref="SolidLongIdGen"/>.
        /// </summary>
        public double StopRandomOn { get; set; } = 0.65d;

        /// <inheritdoc/>
        /// <remarks>Based on <see cref="DefaultId"/> property.</remarks>
        public long GetDefaultId() => DefaultId;

        /// <inheritdoc/>
        /// <remarks>Based on <see cref="DefaultId"/> property.</remarks>
        public bool IsDefaultID(long id) => id == DefaultId;

        /// <summary>Generates a new unique identifier using a random long value.</summary>
        /// <remarks>Note: This method may not guarantee uniqueness if the range of values is exhausted.</remarks>
        public long GenerateId() => Random.NextInt64(FromId, ToId);

        /// <inheritdoc/>
        /// <remarks>
        /// The method generates IDs randomly within the specified range until <see cref="StopRandomOn"/> is reached.
        /// Beyond this point, the generator switches to <see cref="SolidLongIdGen"/> mode.
        /// If the limit defined by <see cref="ExpiresOn"/> is reached, an exception is thrown.
        /// </remarks>
        public long GenerateIdFor<TData>(IDataBank<long, TData> bank, TData @object) where TData : ModelDso<long>
        {
            if (bank.Count >= Math.Floor(ExpiresOn * Available))
            {
                throw new OverflowException($"{bank.HoldingType.Name} DataBank fullness exceeds available limits");
            }

            if (bank.Count >= NotifyOnFullness * Available)
            {
                LimitsExceeding?.Invoke(this, new LimitsExpiringArgs(bank, bank.Count, Available));
            }

            var id = Random.NextInt64(FromId, ToId);
            if (bank.Count >= StopRandomOn * Available)
            {
                var solid = new SolidLongIdGen(DefaultId, FromId, seed);
                id = solid.GenerateIdFor(bank, @object);
            }
            else
            {
                while (bank.TryGetValue(id) is not null)
                {
                    id = Random.NextInt64(FromId, ToId);
                }
            }
            return id;
        }
    }
}