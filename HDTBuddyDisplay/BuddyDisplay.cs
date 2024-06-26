using HearthDb.Enums;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using HSCard = Hearthstone_Deck_Tracker.Hearthstone.Card;

using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.RegularExpressions;
using ControlzEx.Standard;

namespace HDTBuddyDisplay
{
    public class BuddyDisplay
    {
        public CardImage CardImage;
        public static MoveCardManager MoveManager;

        public BuddyDisplay()
        {
        }
        public async Task<String> AwaitHeroSelection()
        {
            const int maxAttempts = 100;
            const int delayBetweenAttempts = 500;

            for (int i = 0; i < maxAttempts; i++)
            {
                await Task.Delay(delayBetweenAttempts);

                // Heroes list is available
                List<Entity> loadedHeroes = Core.Game.Player.PlayerEntities
                    .Where(x => x.IsHero && (x.HasTag(GameTag.BACON_HERO_CAN_BE_DRAFTED) || x.HasTag(GameTag.BACON_SKIN)))
                    .ToList();

                // Hero has been selected
                Entity playerHero = Core.Game.Player.Board.FirstOrDefault(x => x.IsPlayer && x.IsHero);

                String heroCardId = playerHero?.CardId ?? String.Empty;

                if (loadedHeroes.Count >= 2 && !String.IsNullOrEmpty(heroCardId))
                {
                    return heroCardId;
                }
            }
            return String.Empty;
        }

        public void InitializeView(String cardId)
        {
            if (CardImage == null)
            {
                CardImage = new CardImage();

                Core.OverlayCanvas.Children.Add(CardImage);
                Canvas.SetTop(CardImage, Settings.Default.BuddyCardTop);
                Canvas.SetLeft(CardImage, Settings.Default.BuddyCardLeft);
                CardImage.Visibility = System.Windows.Visibility.Visible;

                MoveManager = new MoveCardManager(CardImage, SettingsView.IsUnlocked);
                Settings.Default.PropertyChanged += SettingsChanged;
                SettingsChanged(null, null);
            }

            HSCard card = Database.GetCardFromId(cardId);
            card.BaconCard = true;  // Ensure we are getting the Battlegrounds version
            CardImage.SetCardIdFromCard(card);
        }

        private void SettingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            CardImage.RenderTransform = new ScaleTransform(Settings.Default.BuddyCardScale / 100, Settings.Default.BuddyCardScale / 100);
            Canvas.SetTop(CardImage, Settings.Default.BuddyCardTop);
            Canvas.SetLeft(CardImage, Settings.Default.BuddyCardLeft);
        }

        public async void HandleGameStart()
        {
            if (Core.Game.CurrentGameMode != GameMode.Battlegrounds)
                return;

            // Wait until the game starts
            String heroCardId = await AwaitHeroSelection();

            if (String.IsNullOrEmpty(heroCardId))
            {
                Log.Error("No player hero found. Possible timeout at game load.");
                return;
            }

            String cleanHeroId = CleanHeroId(heroCardId);
            String buddyCardId = $"{cleanHeroId}_Buddy";

            InitializeView(buddyCardId);
        }

        private string CleanHeroId(string heroId)
        {
            string cleanHero;
            string skinPattern = @"^(.*?)_SKIN_";

            Match heroHasSkin = Regex.Match(heroId, skinPattern);

            if (heroHasSkin.Success)
            {
                cleanHero = heroHasSkin.Groups[1].Value;
            }
            else
            {
                // If "_SKIN_" is not found, keep the original string
                cleanHero = heroId;
            }

            // Deal with transformations
            return BattlegroundsUtils.GetOriginalHeroId(cleanHero);
        }

        public void ClearCard()
        {
            if (CardImage != null)
            {
                CardImage.SetCardIdFromCard(null);
                Core.OverlayCanvas.Children.Remove(CardImage);
                CardImage = null;
            }

            if (MoveManager != null)
            {
                Log.Info("Destroying the MoveManager...");
                MoveManager.Dispose();
                MoveManager = null;
            }

            Settings.Default.PropertyChanged -= SettingsChanged;
        }
    }
}
