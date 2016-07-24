using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.CountrySelection
{
    public class CountrySelection : MonoBehaviour
    {
        public void StartGame()
        {
            SceneManager.LoadScene("Map");
        }
    }
}