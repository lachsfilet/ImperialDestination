using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

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
            SceneManager.LoadScene("Map");
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}