using UnityEngine;
using Assets.Scripts.Game;

public class GameControl : MonoBehaviour {

    public static GameControl Instance { get; set; }

    public Map Map { get; set; }

    public Player Player { get; set; }

    // Use this for initialization
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }
}
