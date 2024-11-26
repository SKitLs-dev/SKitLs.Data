namespace SKitLs.Data.IO
{
    /// <summary>
    /// Represents the method that handles events related to <see cref="ModelDso{TId}"/> objects.
    /// </summary>
    /// <typeparam name="TId">The type of the ID of the model.</typeparam>
    /// <param name="sender">The <see cref="ModelDso{TId}"/> object that raised the event.</param>
    public delegate void ModelDsoEventHandler<TId>(ModelDso<TId> sender) where TId : notnull, IEquatable<TId>, IComparable<TId>;

    /// <summary>
    /// Represents a model with a specific ID type that supports enabling/disabling and save requests.
    /// </summary>
    /// <typeparam name="TId">The type of the ID of the model.</typeparam>
    public abstract class ModelDso<TId> : IEquatable<ModelDso<TId>> where TId : notnull, IEquatable<TId>, IComparable<TId>
    {
        /// <summary>
        /// Event triggered when the data of the model changes.
        /// </summary>
        public event ModelDsoEventHandler<TId>? DataChanged;

        /// <summary>
        /// Event triggered when a save request is made.
        /// </summary>
        public event ModelDsoEventHandler<TId>? SaveRequested;

        /// <summary>
        /// Gets a value indicating whether the model is enabled.
        /// </summary>
        public bool IsEnabled { get; private set; } = true;

        /// <summary>
        /// Enables the model.
        /// </summary>
        public void Enable() => IsEnabled = true;

        /// <summary>
        /// Disables the model.
        /// </summary>
        public void Disable() => IsEnabled = false;

        /// <summary>
        /// Gets the ID of the model.
        /// </summary>
        /// <returns>The ID of the model.</returns>
        public abstract TId GetId();

        /// <summary>
        /// Sets the ID of the model.
        /// </summary>
        /// <param name="id">The ID to set.</param>
        public abstract void SetId(TId id);

        /// <summary>
        /// Invokes the <see cref="SaveRequested"/> event to request saving the model.
        /// </summary>
        public void Save() => SaveRequested?.Invoke(this);

        /// <summary>
        /// Invokes the <see cref="DataChanged"/> event to notify data has been changed.
        /// </summary>
        public void OnDataChanged() => DataChanged?.Invoke(this);

        /// <inheritdoc/>
        public override int GetHashCode() => GetId().GetHashCode();

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is ModelDso<TId> modelDso)
                return Equals(modelDso);
            return false;
        }

        /// <inheritdoc/>
        public bool Equals(ModelDso<TId>? other)
        {
            if (other?.GetId().Equals(GetId()) ?? false)
                return true;
            return false;
        }
    }
}