using Assets.Scripts.Game;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.CountrySelection
{
    public class CountrySelection : MonoBehaviour
    {
        public void StartGame()
        {
            if (!GameCache.Instance.CurrentGame.Players.Any())
                return;

            GameCache.Instance.SetAiPlayers(20000);
            SceneManager.LoadScene("VoronoiTest");
        }
    }
}