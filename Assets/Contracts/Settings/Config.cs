using System.Collections.Generic;

namespace Assets.Contracts.Settings
{
    public class Config
    {
        public IList<ResourceModel> Resources { get; set; }
        
        public IList<string> MajorCountryNames { get; set; }

        public IList<string> MinorCountryNames { get; set; }
    }
}