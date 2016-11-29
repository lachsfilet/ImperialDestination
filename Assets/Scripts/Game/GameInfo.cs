using Assets.Scripts.Map;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Game
{
    [Serializable]
    public class GameInfo
    {
        public MapInfo MapInfo { get; set; }

        public int Year { get; set; }

        public Season Season { get; set; }

        public IList<Player> Players { get; set; }
    }
}
