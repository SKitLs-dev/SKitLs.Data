using SKitLs.Data.Banks;
using SKitLs.Data.IO;

namespace SKitLs.Data
{
    /// <summary>
    /// Implementation of <see cref="IDataManager"/> that manages various data banks.
    /// </summary>
    public class DataManager : IDataManager
    {
        /// <inheritdoc/>
        public string DataFolderPath { get; init; }

        private List<DataBankInfo> Notations { get; set; } = [];

        /// <inheritdoc/>
        public IReadOnlyCollection<DataBankInfo> GetNotations() => Notations;

        private List<IDataBank> Banks { get; set; } = [];

        /// <inheritdoc/>
        public IEnumerable<IDataBank> GetBanks() => Banks;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataManager"/> class, setting up the specified data folder.
        /// </summary>
        /// <param name="dataFolderPath">The path to the folder where data will be managed and stored.</param>
        public DataManager(string dataFolderPath = "Resources/Data")
        {
            if (!Directory.Exists(dataFolderPath))
            {
                // TODO Logs
                Console.WriteLine($"{nameof(DataManager)}: {dataFolderPath} not found. Creating...");
                Directory.CreateDirectory(dataFolderPath);
            }

            DataFolderPath = dataFolderPath;
            InitializeDeclarations();
        }

        /// <inheritdoc/>
        /// <exception cref="NullReferenceException">Thrown when a data bank of the specified type is not found.</exception>
        public IDataBank ResolveBank(Type bankType) => Banks.Where(x => x.HoldingType == bankType).FirstOrDefault() ?? throw new NullReferenceException(); //NotDefinedException(bankType);

        /// <inheritdoc/>
        /// <exception cref="NullReferenceException">Thrown when a data bank of the specified type is not found.</exception>
        public IDataBank<TId, TData> ResolveBank<TId, TData>() where TId : notnull, IEquatable<TId>, IComparable<TId> where TData : ModelDso<TId>
            => (IDataBank<TId, TData>?)Banks.Where(x => x.HoldingType == typeof(TData)).FirstOrDefault() ?? throw new NullReferenceException(); // NotDefinedException(typeof(TData));

        /// <inheritdoc/>
        public void Declare<TId, TData>(IDataBank<TId, TData> bank) where TId : notnull, IEquatable<TId>, IComparable<TId> where TData : ModelDso<TId>
        {
            bank.Manager = this;
            var bankInfo = DataBankInfo.OfDataBank(bank);
            bank.OnBankDataUpdated += (cnt) =>
            {
                bankInfo.Count = bank.Count;
            };

            Banks.Add(bank);
            Notations.Add(bankInfo);
        }

        /// <summary>
        /// Initializes data bank declarations. This method is called during the class initialization to set up and declare the necessary data banks.
        /// </summary>
        /// <remarks>
        /// This method can be overridden in derived classes to customize the declaration process for specific data banks.
        /// </remarks>
        public virtual void InitializeDeclarations() { }

        /// <inheritdoc/>
        public void Initialize()
        {
            foreach (var bank in GetBanks())
            {
                bank.Initialize();
            }
        }

        /// <inheritdoc/>
        public async Task InitializeAsync()
        {
            foreach (var bank in GetBanks())
            {
                await bank.InitializeAsync();
            }
        }
    }
}