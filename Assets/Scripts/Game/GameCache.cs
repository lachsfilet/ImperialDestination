using Assets.Scripts.Map;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Game
{
    public class GameCache
    {
        private GameInfo _currentGame;

        private static GameCache _instance;

        public static GameCache Instance
        {
            get
            {
                return _instance ?? (_instance = new GameCache());
            }
        }

        private GameCache()
        {
            _currentGame = new GameInfo
            {
                Players = new List<Player>(),
                Year = 1815
            };
        }

        public GameInfo CurrentGame
        {
            get
            {
                return _currentGame;
            }
        }

        public bool ContainsMapInfo()
        {
            return _currentGame.MapInfo == null;
        }

        public void SetPlayer(string name, string country, int balance)
        {
            var player = new Player { Name = name, CountryName = country, Balance = balance };
            if (!_currentGame.Players.Any())
                _currentGame.Players.Add(player);
            _currentGame.Players[0] = player;
        }

        public void ReplaceCurrentGame(GameInfo gameInfo)
        {
            _currentGame = gameInfo;
        }

        public void SetSeasonAndYear(Season season, int year)
        {
            _currentGame.Season = season;
            _currentGame.Year = year;
        }

        public void AddMapInfo(MapInfo mapInfo)
        {
            _currentGame.MapInfo = mapInfo;
        }
    }
}
