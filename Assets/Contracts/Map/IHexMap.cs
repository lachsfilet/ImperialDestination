using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Contracts.Map
{
    public interface IHexMap : IEnumerable<TileBase>
    {
        int Height { get; }

        int Width { get; }

        TileBase GetTile(Position position);

        IEnumerable<TileBase> GetNeighbours(TileBase tile, bool reverse = false);

        IEnumerable<TilePair> GetNeighboursWithDirection(TileBase tile);

        TilePair GetNextNeighbourWithDirection(TileBase hexTile, TileBase currentNeighbour);

        TilePair GetPairWithDirection(TileBase hexTile, TileBase neighbour);

        void AddTile(int x, int y, TileBase tile);

        IEnumerable<Position> DrawLine(Position start, Position end);
    }
}
