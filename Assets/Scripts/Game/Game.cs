using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Assets.Scripts.Game
{
    public class Game : MonoBehaviour
    {
        public Text YearSeasonText;

        public Text BalaneText;

        public int CurrentTurn;

        public int CurrentYear;

        public Season CurrentSeason;

        public List<Player> Players;

        // Use this for initialization
        void Start()
        {
            if(!GameCache.Instance.IsEmpty())
            {
                CurrentSeason = GameCache.Instance.CurrentGame.Season;
                CurrentYear = GameCache.Instance.CurrentGame.Year;
            }
            SetInfoTexts();
        }
        
        public void EndTurn()
        {
            CurrentTurn++;
            
            if(CurrentSeason == Season.Winter)
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
            if(YearSeasonText != null)
                YearSeasonText.text = string.Format("{0}, {1}", CurrentSeason, CurrentYear);
        }
    }
}
