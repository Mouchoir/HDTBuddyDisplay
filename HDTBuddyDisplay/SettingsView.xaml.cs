using System.Windows;
using System.Windows.Controls;

using MahApps.Metro.Controls;
using Hearthstone_Deck_Tracker;
using System.Collections.Generic;
using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.Logging;
using API = Hearthstone_Deck_Tracker.API;

namespace HDTBuddyDisplay
{
    public partial class SettingsView : UserControl
    {
        private static Flyout _flyout;

        private BuddyDisplay BuddyDisplay;

        private readonly int NorgannonDbfId = 103078;

        public static bool IsUnlocked { get; private set; }


        public static Flyout Flyout
        {
            get
            {
                if (_flyout == null)
                {
                    _flyout = CreateSettingsFlyout();
                }
                return _flyout;
            }
        }

        private static Flyout CreateSettingsFlyout()
        {
            var settings = new Flyout();
            settings.Position = Position.Left;
            Panel.SetZIndex(settings, 100);
            settings.Header = "Settings";
            settings.Content = new SettingsView();
            Core.MainWindow.Flyouts.Items.Add(settings);
            return settings;
        }

        public SettingsView()
        {
            InitializeComponent();
        }
        public IEnumerable<Orientation> OrientationTypes => Enum.GetValues(typeof(Orientation)).Cast<Orientation>();

        private void BtnUnlock_Click(object sender, RoutedEventArgs e)
        {

            if (BuddyDisplay.MoveManager == null && BuddyDisplay == null)
            {
                Log.Info("No ongoing game, create a dummy Norgannon card that can be moved around to save the position. Yes I know it's not a buddy.");
                BuddyDisplay = new BuddyDisplay();
                BuddyDisplay.InitializeView(NorgannonDbfId);
                API.GameEvents.OnGameStart.Add(ClearCardDisplay);
            }

            IsUnlocked = BuddyDisplay.MoveManager.ToggleUILockState();
            if (!IsUnlocked && BuddyDisplay != null)
            {
                BuddyDisplay.ClearCard();
                BuddyDisplay = null;
            }

            BtnUnlock.Content = IsUnlocked ? "Lock overlay" : "Unlock overlay";
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.BuddyCardLeft = 0;
            Settings.Default.BuddyCardTop = 630;
            Settings.Default.BuddyCardScale = 100;
            Settings.Default.Save();
        }

        public void ClearCardDisplay()
        {
            if (BuddyDisplay != null)
            {
                BuddyDisplay.ClearCard();
                BuddyDisplay = null;
            }
        }
    }
}
