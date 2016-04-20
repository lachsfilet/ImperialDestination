using UnityEngine;
using System.Collections;

public class MainCamera : MonoBehaviour {

    public float Speed = 30;

    public int ScrollArea = 20;

    private Vector3 _movement;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        var h = Input.GetAxisRaw("Horizontal");
        var v = Input.GetAxisRaw("Vertical");

        if (Input.mousePosition.x < ScrollArea)
            h = -1;
        if (Input.mousePosition.x > Screen.width - ScrollArea)
            h = 1;
        if (Input.mousePosition.y < ScrollArea)
            v = -1;
        if (Input.mousePosition.y > Screen.height - ScrollArea)
            v = 1;
        
        _movement.Set(h, v, v);
        _movement = _movement.normalized * Speed * Time.deltaTime;
        transform.Translate(_movement);
    }
}
