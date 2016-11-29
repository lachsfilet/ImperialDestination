using System.Collections.Generic;

namespace Assets.Scripts.Map
{
    public class SectorFactory
    {
        public IEnumerable<Sector> CreateSectors(int height, int width, int count)
        {
            var countY = count / 2;
            for (var i = 0;  i < 2; i++)
            {
                var sectorHeight = height / 2;
                var sectorWidth = width / countY;
                for (var j = 0; j < countY; j++)
                {
                    var bottom = i * sectorHeight;
                    var left = j * sectorWidth;
                    var top = bottom + sectorHeight - 1;
                    var right = left + sectorWidth - 1;
                    var sector = new Sector(top, right, bottom, left);
                    yield return sector;
                }
            }
        }
    }
}
