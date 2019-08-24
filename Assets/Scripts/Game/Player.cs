using Assets.Contracts.Research;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Game
{
    [Serializable]
    public class Player
    {
        public bool IsHuman { get; set; }

        public string Name { get; set; }

        public string CountryName { get; set; }

        public int Balance { get; set; }

        public IList<ITechnology> Technologies { get; set; }
    }
}
