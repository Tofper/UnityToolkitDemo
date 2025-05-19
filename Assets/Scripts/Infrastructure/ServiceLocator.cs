using Scripts.Data;

namespace Scripts.Infrastructure
{
    // Central service locator for shared services
    public class ServiceLocator
    {
        // Private static instance
        private static ServiceLocator _instance;

        // Public static property to access the instance
        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServiceLocator();
                    _instance.Initialize(); // Initialize services when instance is first accessed
                }
                return _instance;
            }
        }

        // Private constructor to prevent external instantiation
        private ServiceLocator()
        {
            // Initialization logic in Initialize method
        }

        public CurrencyService CurrencyService { get; private set; }
        public DailyRewardsDataService DailyRewardsDataService { get; private set; }

        // Non-static Initialize method
        private void Initialize()
        {
            CurrencyService = new CurrencyService();
            DailyRewardsDataService = new DailyRewardsDataService();
        }
    }
}
