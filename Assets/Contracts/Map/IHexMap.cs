using System.Collections.Generic;

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

        IEnumerable<TileBase> GetTilesOfTerrainType(TileTerrainType terrainType);
        TileBase GetTile(int x, int y);
    }
}