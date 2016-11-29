using System.Collections.Generic;

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
        }

        public GameInfo CurrentGame
        {
            get
            {
                return _currentGame;
            }
        }

        public bool IsEmpty()
        {
            return _currentGame == null;
        }

        public void SetPlayer(string name, string country)
        {
            _currentGame = _currentGame ?? CreateGameInfo();
            if (_currentGame.Players == null)
                _currentGame.Players = new List<Player>();
            _currentGame.Players.Add(new Player { Name = name, CountryName = country });
        }

        public void ReplaceCurrentGame(GameInfo gameInfo)
        {
            _currentGame = gameInfo;
        }

        public void SetSeasonAndYear(Season season, int year)
        {
            _currentGame = _currentGame ?? CreateGameInfo();
            _currentGame.Season = season;
            _currentGame.Year = year;
        }

        private GameInfo CreateGameInfo()
        {
            _currentGame = new GameInfo
            {
                Players = new List<Player>()
            };
            return _currentGame;
        }
    }
}
