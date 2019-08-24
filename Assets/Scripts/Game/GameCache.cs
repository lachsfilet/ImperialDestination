using Assets.Contracts.Research;
using Assets.Scripts.Map;
using Assets.Scripts.Research;
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
            var player = new Player {
                IsHuman = true,
                Name = name,
                CountryName = country,
                Balance = balance,
                Technologies = new List<ITechnology>
                {
                    new Railway { IsInvented = true }
                }
            };
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

        public void SetMapInfo(MapInfo mapInfo)
        {
            _currentGame.MapInfo = mapInfo;
        }

        public void SetCountryNames(IEnumerable<string> countryNames)
        {
            _currentGame.CountryNames = countryNames.ToList();
        }

        public void SetAiPlayers(int balance)
        {
            if (_currentGame.CountryNames == null)
                return;

            foreach(var country in _currentGame.CountryNames)
            {
                if (!_currentGame.Players.Any(p => p.CountryName == country))
                    _currentGame.Players.Add(
                        new Player
                        {
                            IsHuman = false,
                            Name = string.Format("AI {0}", country),
                            CountryName = country,
                            Balance = balance,
                            Technologies = new List<ITechnology>
                            {
                                new Railway { IsInvented = true }
                            }
                        }
                    );                        
            }
        }
    }
}
