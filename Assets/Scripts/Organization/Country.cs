using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Organization
{
    public class Country
    {
        public string Name { get; set; }

        public Player Player { get; set; }

        public CountryType CountryType { get; set; }

        public List<Province> Provinces { get; set; }
    }
}
