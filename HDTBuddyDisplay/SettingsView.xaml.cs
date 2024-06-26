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

        private readonly string DummyBuddyCardId = "TB_BaconShop_HERO_16_Buddy";

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
            Flyout settings = new Flyout
            {
                Position = Position.Left
            };
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
                Log.Info("A.F. Kay buddy loaded. All good you can wait 2 turns");
                BuddyDisplay = new BuddyDisplay();
                BuddyDisplay.InitializeView(DummyBuddyCardId);
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
