using Scripts.Data;
using Scripts.UI.GameScreens;
using UnityEngine;

namespace Scripts.Infrastructure
{
    /// <summary>
    /// Bootstraps and wires up UI screens with their respective ViewModels and services.
    /// </summary>
    public class UIScreensBootstrapper : MonoBehaviour
    {
        [Header("Assign screens in the inspector")]
        /// <summary>
        /// Reference to the Daily Rewards screen UI component.
        /// </summary>
        public DailyRewardsScreen dailyRewardsScreen;


        /// <summary>
        /// Initializes services, creates ViewModels, and wires them to the screens.
        /// Called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {

            // ServiceLocator.Initialize();

            // Check for required screens
            if (dailyRewardsScreen == null)
            {
                Debug.LogError($"{nameof(UIScreensBootstrapper)}: Daily Rewards Screen is not assigned in the Inspector!");
                return;
            }

            // Create ViewModels using shared services

            var dailyRewardsDataService = ServiceLocator.Instance.DailyRewardsDataService;
            if (dailyRewardsDataService == null)
            {
                Debug.LogError($"{nameof(UIScreensBootstrapper)}: DailyRewardsDataService not found in ServiceLocator!");
                return;
            }

            var currencyService = ServiceLocator.Instance.CurrencyService;
            if (currencyService == null)
            {
                Debug.LogError($"{nameof(UIScreensBootstrapper)}: CurrencyService not found in ServiceLocator!");
                return;
            }

            var dailyRewardsVM = new DailyRewardsViewModel(
                dailyRewardsDataService,
                currencyService);

            dailyRewardsScreen.SetViewModel(dailyRewardsVM);
        }
    }
}
