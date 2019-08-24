using System.Collections.Generic;

namespace Assets.Contracts.Organization
{
    public interface ICountry
    {
        string Name { get; set; }

        List<IProvince> Provinces { get; set; }
    }
}