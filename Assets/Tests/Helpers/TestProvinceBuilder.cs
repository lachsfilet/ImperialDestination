using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.TestUtilities
{
    public class TestProvinceBuilder
    {
        private int _height;
        private int _width;
        private readonly IHexMap _map;

        private TestProvinceBuilder(IHexMap map)
        {
            _map = map;
            _height = 3;
            _width = 3;
        }

        public static TestProvinceBuilder New(IHexMap map)
            => new TestProvinceBuilder(map);

        public TestProvinceBuilder WithHeight(int height)
        {
            _height = height;
            return this;
        }

        public TestProvinceBuilder WithWidth(int width)
        {
            _width = width;
            return this;
        }
        
        public IList<Mock<IProvince>> Build()
        {
            var provinces = Enumerable.Range(0, _height * _width).Select(
                n => new Mock<IProvince>())
                .ToList();

            for (var i = 0; i < _height; i++)
            {
                for (var j = 0; j < _width; j++)
                {
                    var index = j + i * _width;
                    var province = provinces[index];
                    province.Setup(m => m.Name).Returns($"Province {index}");
                    province.SetupProperty(p => p.IsWater);
                    province.Object.IsWater = true;
                    province.SetupProperty(p => p.Owner);
                    province.Setup(m => m.GetNeighbours(_map)).Returns(() =>
                    {
                        var list = new List<IProvince>();
                        // Top left -> 0
                        if (index % _width > 0 && index >= _width)
                            list.Add(provinces[index - _width - 1].Object);
                        // Top -> 1
                        if (index >= _width)
                            list.Add(provinces[index - _width].Object);
                        // Top right -> 2
                        if (index >= _width && index % _width < _width - 1)
                            list.Add(provinces[index - _width + 1].Object);
                        // Left -> 3
                        if (index % _width > 0)
                            list.Add(provinces[index - 1].Object);
                        // Right -> 4
                        if (index % _width < _width - 1)
                            list.Add(provinces[index + 1].Object);
                        // Bottom left -> 5
                        if (index + _width < provinces.Count && index % _width > 0)
                            list.Add(provinces[index + _width - 1].Object);
                        // Bottom -> 6
                        if (index + _width < provinces.Count)
                            list.Add(provinces[index + _width].Object);
                        // Bottom right -> 7
                        if (index + _width < provinces.Count && index % _width < _width - 1)
                            list.Add(provinces[index + _width + 1].Object);
                        return list;
                    });
                }
            }
            return provinces;
        }
    }
}
