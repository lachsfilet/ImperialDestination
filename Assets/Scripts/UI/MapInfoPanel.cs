using Assets.Scripts.Map;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class MapInfoPanel : MonoBehaviour
    {
        public MapMode MapMode;

        public Text TerrainText;
        public Text PositionText;
        public Text ContinentText;
        public Text TileCountText;
        public Text ProvinceText;
        public Text ProvinceCountText;
        public Text CountryText;
        public Text ResourcesText;
        public Text SelectedCountryText;

        private GameObject _mapInfoCache;

        // Start is called before the first frame update
        private void Start()
        {
            _mapInfoCache = new GameObject("MapInfoCache");
            var map = _mapInfoCache.AddComponent<Assets.Scripts.Map.Map>();

            map.MapMode = MapMode;
            map.SelectedCountryText = SelectedCountryText;
            map.TerrainText = TerrainText;
            map.TileCountText = TileCountText;
            map.ProvinceText = ProvinceText;
            map.ProvinceCountText = ProvinceCountText;
            map.ResourcesText = ResourcesText;
            map.PositionText = PositionText;
            map.ContinentText = ContinentText;
            map.CountryText = CountryText;
        }
    }
}