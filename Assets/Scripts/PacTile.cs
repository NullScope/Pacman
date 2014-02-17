using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PacTile : MonoBehaviour {
    public GameController gameController;
    public byte cost;
    //public bool isIntersection, allowUpwards, allowBottom, allowLeft, allowRight;
    public List<GhostAI.Directions> allowedDirections = new List<GhostAI.Directions>();
    
    // Use this for initialization
	public void Start () {
        if (gameController == null)
        {
            GameObject Camera = GameObject.Find("Main Camera");
            gameController = Camera.GetComponent<GameController>();
        }
        gameController.AddTile((int)gameObject.transform.position.x, (int)(-1 * gameObject.transform.position.y), cost);
        gameController.AddPacTile((int)gameObject.transform.position.x, (int)(-1 * gameObject.transform.position.y), this);
	}

	// Update is called once per frame
	public void Update () {

	}
}