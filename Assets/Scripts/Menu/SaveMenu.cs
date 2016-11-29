using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Assets.Scripts.Game;

namespace Assets.Scripts.Menu
{
    public class SaveMenu : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        public void Save()
        {
            var saveGame = string.Format("{0}_{1}_{2}.json",
                GameCache.Instance.CurrentGame.Players.First().CountryName,
                GameCache.Instance.CurrentGame.Year,
                GameCache.Instance.CurrentGame.Season);

            var filePath = EditorUtility.SaveFilePanel("Save current game", "", saveGame, "json");
            if (string.IsNullOrEmpty(filePath))
                return;

            var gameInfo = GameCache.Instance.CurrentGame;

            var json = JsonConvert.SerializeObject(gameInfo, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            File.WriteAllText(filePath, json);
        }
    }
}