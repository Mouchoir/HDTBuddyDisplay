using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Plugins;
using System;
using System.Windows.Controls;

namespace HDTBuddyDisplay
{
    public class BuddyDisplayPlugin : IPlugin
    {
        public string Name => "HDTBuddyDisplay";

        public string Description => "Displays your current Battlegrounds buddy on your overlay";

        public string ButtonText => "SETTINGS";

        // Thank you Tignus for HDTAnomalyDisplay ! I used a lot of your code.
        public string Author => "Mouchoir & Tignus";

        public Version Version => new Version(1, 0, 0);

        public MenuItem MenuItem => CreateMenu();

        private MenuItem CreateMenu()
        {
            MenuItem settingsMenuItem = new MenuItem { Header = "Buddy Display Settings" };

            settingsMenuItem.Click += (sender, args) =>
            {
                SettingsView.Flyout.IsOpen = true;
            };

            return settingsMenuItem;
        }
        public BuddyDisplay buddyDisplay;

        public void OnButtonPress() => SettingsView.Flyout.IsOpen = true;

        public void OnLoad()
        {
            buddyDisplay = new BuddyDisplay();
            GameEvents.OnGameStart.Add(buddyDisplay.HandleGameStart);
            GameEvents.OnGameEnd.Add(buddyDisplay.ClearCard);

            // Processing GameStart logic in case plugin was loaded/unloaded after starting a game without restarting HDT
            buddyDisplay.HandleGameStart();
        }

        public void OnUnload()
        {
            Settings.Default.Save();
            buddyDisplay.ClearCard();
            buddyDisplay = null;
        }

        public void OnUpdate()
        {
        }
    }
}
