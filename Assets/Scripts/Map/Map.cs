using System.Linq;
using Assets.Scripts.Organization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Map
{
    public class Map : MonoBehaviour
    {
        private Tile _lastHovered;
        private Tile _selectedTile;
        private Country _selectedCountry;

        public MapMode MapMode { get; set; }
        public MapInfo MapInfo { get; set; }

        public Text TerrainText { get; set; }
        public Text PositionText { get; set; }
        public Text ContinentText { get; set; }
        public Text TileCountText { get; set; }
        public Text ProvinceText { get; set; }
        public Text ProvinceCountText { get; set; }
        public Text CountryText { get; set; }
        public Text ResourcesText { get; set; }
        public Text SelectedCountryText { get; set; }

        // Update is called once per frame
        void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit))
                return;

            var hexTile = hit.collider.gameObject;
            var tile = hexTile.GetComponent<Tile>();

            if (Input.GetMouseButtonDown(0))
            {
                if (MapMode == MapMode.Overview)
                {
                    if (tile.Province == null)
                        return;

                    var country = tile.Province.Owner;
                    if (country.CountryType == CountryType.Minor)
                        return;

                    if (_selectedCountry != null && _selectedCountry != country)
                    {
                        _selectedCountry.Provinces.ForEach(p =>
                        {
                            p.HexTiles.ToList().ForEach(
                                t =>
                                {
                                    t.Deselect();
                                });
                        });
                    }
                    if (country == _selectedCountry)
                        return;

                    var color = country.Color.gamma;
                    country.Provinces.ForEach(p =>
                    {
                        p.HexTiles.ToList().ForEach(
                            t =>
                            {
                                t.Select(color);
                            });
                    });
                    _selectedCountry = country;
                    SelectedCountryText.text = country.Name;
                    return;
                }

                if (_selectedTile != null)
                    _selectedTile.Deselect();
                tile.Select();
                _selectedTile = tile;
                TerrainText.text = _selectedTile.TileTerrainType.ToString();
                PositionText.text = string.Format("x: {0}, y: {1}", _selectedTile.Position.X, _selectedTile.Position.Y);
                ContinentText.text = tile.transform.parent != null ? tile.transform.parent.name : "None";
                TileCountText.text = tile.transform.parent != null ? tile.transform.parent.childCount.ToString() : "None";
                ProvinceText.text = tile.Province != null ? tile.Province.Name : "None";
                ProvinceCountText.text = tile.Province != null ? tile.Province.HexTiles.Count().ToString() : "None";
                CountryText.text = tile.Province != null && tile.Province.Owner != null ? tile.Province.Owner.Name : "None";
                ResourcesText.text = string.Join(",", tile.Resources.Select(r => r.Name).ToArray());
                return;
            }

            if (MapMode == MapMode.Overview)
                return;

            if (_lastHovered != null && tile != _lastHovered)
            {
                _lastHovered.Leave();
            }
            tile.Hover();
            _lastHovered = tile;
        }
    }
}