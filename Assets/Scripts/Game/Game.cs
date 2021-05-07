using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Game
{
    public class Game : MonoBehaviour
    {
        public Text CountryText;

        public Text YearSeasonText;

        public Text BalanceText;

        public int CurrentTurn;

        public int CurrentYear;

        public Season CurrentSeason;

        public Player Player;

        // Use this for initialization
        private void Start()
        {
            if (!GameCache.Instance.ContainsMapInfo)
            {
                CurrentSeason = GameCache.Instance.CurrentGame.Season;
                CurrentYear = GameCache.Instance.CurrentGame.Year;
            }
            Player = GameCache.Instance.CurrentGame.Players.Single(x => x.IsHuman);

            SetInfoTexts();
        }

        public void EndTurn()
        {
            CurrentTurn++;

            if (CurrentSeason == Season.Winter)
            {
                CurrentSeason = Season.Spring;
                CurrentYear++;
            }
            else
                CurrentSeason++;

            GameCache.Instance.SetSeasonAndYear(CurrentSeason, CurrentYear);
            SetInfoTexts();
        }

        private void SetInfoTexts()
        {
            if (CountryText != null)
                CountryText.text = Player.CountryName;

            if (YearSeasonText != null)
                YearSeasonText.text = string.Format("{0}, {1}", CurrentSeason, CurrentYear);

            if (BalanceText != null)
                BalanceText.text = Player.Balance.ToString();
        }
    }
}