using System;
using System.Linq;
using System.Threading.Tasks;
using static HearthDb.Enums.GameTag;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.API;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Controls;
using System.Windows.Media;
using HearthDb.Enums;
using System.Collections.Generic;

namespace HDTBuddyDisplay
{
    public class BuddyDisplay : IDisposable
    {
        public CardImage CardImage;
        public static MoveCardManager MoveManager;

        public BuddyDisplay()
        {
        }

        private async Task AwaitGameEntity()
        {
            const int maxAttempts = 100;
            const int delayBetweenAttempts = 500;

            for (var i = 0; i < maxAttempts; i++)
            {
                await Task.Delay(delayBetweenAttempts);
                var loadedHeroes = Core.Game.Player.PlayerEntities
                    .Where(x => x.IsHero && (x.HasTag(GameTag.BACON_HERO_CAN_BE_DRAFTED) || x.HasTag(GameTag.BACON_SKIN)))
                    .ToList();

                if (loadedHeroes.Count >= 2 && HeroesSelected(loadedHeroes))
                {
                    break;
                }
            }
        }

        public void InitializeView(int cardDbfId)
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

            CardImage.SetCardIdFromCard(Database.GetCardFromDbfId(cardDbfId, false));
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

            await AwaitGameEntity();

            Entity playerHero = Core.Game.Player.Board.FirstOrDefault(x => x.IsHero);
            if (playerHero == null)
            {
                Log.Error("No player hero found.");
                return;
            }

            var buddyCardId = $"{playerHero.CardId}_Buddy";
            var buddyDbfId = GetDbfidFromCardId(buddyCardId);

            if (buddyDbfId != -1)
            {
                Log.Info("Buddy DbfId found: " + buddyDbfId);
                InitializeView(buddyDbfId);
            }
            else
            {
                Log.Warn("No buddy DbfId found whereas game is already started!");
            }
        }

        private bool HeroesSelected(List<Entity> heroes)
        {
            var playerHero = heroes.FirstOrDefault(x => x.IsControlledBy(Core.Game.Player.Id));
            var opponentHero = heroes.FirstOrDefault(x => !x.IsControlledBy(Core.Game.Player.Id));

            return playerHero != null && opponentHero != null;
        }

        private void LogHeroIds()
        {
            var playerHero = Core.Game.Player.Board.FirstOrDefault(x => x.IsHero);
            var opponentHero = Core.Game.Opponent.Board.FirstOrDefault(x => x.IsHero);

            if (playerHero != null)
            {
                var heroId = playerHero.CardId;
                Log.Info($"Player hero ID is: {heroId}");
            }
            else
            {
                Log.Error("No player hero found.");
            }

            if (opponentHero != null)
            {
                var opponentHeroId = opponentHero.CardId;
                Log.Info($"Opponent hero ID is: {opponentHeroId}");
            }
            else
            {
                Log.Error("No opponent hero found.");
            }
        }

        private int GetDbfidFromCardId(string cardId)
        {
            var card = Hearthstone_Deck_Tracker.Hearthstone.Database.GetCardFromId(cardId);
            Log.Info($"Buddy dbFId: {card?.DbfId ?? -1}");
            return card?.DbfId ?? -1;
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

        public void Dispose()
        {
            ClearCard();
        }
    }
}
