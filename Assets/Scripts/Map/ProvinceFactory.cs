using Assets.Contracts;
using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using Assets.Contracts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoronoiEngine.Elements;

namespace Assets.Scripts.Map
{
    public class ProvinceFactory
    {
        private readonly IHexMap _map;
        private readonly ICollection<Position> _lines;
        private readonly Func<GameObject, GameObject> _instantiate;
        private readonly GameObject _original;
        private readonly IOrganisationFactory _organisationFactory;

        public ProvinceFactory(IHexMap map, ICollection<Position> lines, Func<GameObject, GameObject> instantiate, GameObject original, IOrganisationFactory organisationFactory)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
            _lines = lines ?? throw new ArgumentNullException(nameof(lines));
            _instantiate = instantiate ?? throw new ArgumentNullException(nameof(instantiate));
            _original = original ?? throw new ArgumentNullException(nameof(original));
            _organisationFactory = organisationFactory ?? throw new ArgumentNullException(nameof(organisationFactory));
        }

        public ICollection<IProvince> CreateProvinces(ICollection<Point> points)
        {
            var sites = points.Select(p => new Position(p.XInt, p.YInt)).ToList();
            var provinces = points.OrderBy(p => p.X * Math.Pow(_map.Height.CountDigits(), 10) + p.Y).Select((point, index) =>
             {
                 var tile = _map.GetTile(point.XInt, point.YInt);
                 if (tile == null)
                     Debug.LogError($"Tile is NULL at X: {point.XInt}, Y: {point.YInt} (X: {point.X}, Y: {point.Y})");
                 var provinceContainer = _instantiate(_original);
                 var province = _organisationFactory.CreateProvince(provinceContainer, $"Region {index}");
                 FillRegion(province, tile, sites);
                 province.DrawBorder(_map);
                 province.ArrangePosition();
                 province.IsWater = true;
                 return province;
             }).Cast<IProvince>().ToList();
            
            var provincelessTiles = _map.Where(t => t.Province == null).ToList();
            foreach(var tile in provincelessTiles)
            {
                var neighbours = _map.GetNeighbours(tile).Where(n=>n.Province!=null);
                if (!neighbours.Any())
                    throw new InvalidOperationException($"Cannot add tile to province - No neighbour owned by province found for {tile}");
                tile.Province = neighbours.First().Province;
            }

            return provinces;
        }

        private void FillRegion(IProvince province, TileBase start, List<Position> sites)
        {
            var lowerBorderDirections = new[] { Direction.Northeast, Direction.West, Direction.Northwest };

            var tileStack = new Stack<TileBase>();
            tileStack.Push(start);

            while (tileStack.Count > 0)
            {
                var tile = tileStack.Pop();
                if (province.HexTiles.Contains(tile))
                    continue;

                if (tile == null)
                    Debug.LogError($"Tile is null, tileStack.Count is {tileStack.Count}, province is {province.Name}");

                if (tile.Position != start.Position && sites.Contains(tile.Position))
                    continue;

                province.AddHexTile(tile);

                // Edge cases
                if (_lines.Contains(tile.Position))
                {
                    if (tile.Position.Equals(start.Position))
                    {
                        var ownedNeighbours = _map.GetNeighboursWithDirection(tile).Where(n => n.Neighbour.Province != null).ToList();
                        var freeNeighbours = _map.GetNeighboursWithDirection(tile).Where(n => n.Neighbour.Province == null).ToList();

                        if (ownedNeighbours.Any())
                        {
                            foreach (var neighbour in freeNeighbours)
                                tileStack.Push(neighbour.Neighbour);
                            continue;
                        }

                        foreach (var neighbour in freeNeighbours.Where(n => lowerBorderDirections.Contains(n.Direction)))
                            tileStack.Push(neighbour.Neighbour);
                        continue;
                    }
                    continue;
                }

                foreach (var neighbour in _map.GetNeighboursWithDirection(tile))
                {
                    if (neighbour.Neighbour == null)
                        Debug.LogError($"Neighbour is null, tile is {tile}, tileStack.Count is {tileStack.Count}, province is {province.Name}");

                    if (neighbour.Neighbour.Province == null)
                        tileStack.Push(neighbour.Neighbour);
                }
            }
        }
    }
}