using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;
using Assets.Scripts.Game;

namespace Assets.Scripts.Menu
{
    public class MainMenu : MonoBehaviour
    {

        public Text newGameText;
        public Text exitText;

        private Button _newGameButton;
        private Button _exitButton;

        // Use this for initialization
        void Start()
        {
            _newGameButton = newGameText.GetComponent<Button>();
            _exitButton = exitText.GetComponent<Button>();
        }

        public void StartGame()
        {
            SceneManager.LoadScene("CountrySelection");
        }

        public void LoadGame()
        {
            var path = EditorUtility.OpenFilePanel("Load savegame", "", "json");
            var jsonContent = File.ReadAllText(path);
            var gameInfo = JsonConvert.DeserializeObject<GameInfo>(jsonContent, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            GameCache.Instance.ReplaceCurrentGame(gameInfo);

            SceneManager.LoadScene("Map");
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}