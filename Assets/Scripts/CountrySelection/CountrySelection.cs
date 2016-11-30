﻿using Assets.Scripts.Game;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.CountrySelection
{
    public class CountrySelection : MonoBehaviour
    {
        public void StartGame()
        {
            if (GameCache.Instance.CurrentGame.Players.Any())
                SceneManager.LoadScene("Map");
        }
    }
}