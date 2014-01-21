using UnityEngine;
using System.Collections;

public class PacTile : MonoBehaviour {
    public GameController gameController;
    public byte cost;
    public bool isIntersection, allowUpwards, allowBottom, allowLeft, allowRight;
    
    // Use this for initialization
	public void Start () {
        if (gameController == null)
        {
            GameObject Camera = GameObject.Find("Main Camera");
            gameController = Camera.GetComponent<GameController>();
        }
        gameController.addTile((int)gameObject.transform.position.x, (int)(-1*gameObject.transform.position.y), cost);
	}

	// Update is called once per frame
	public void Update () {

	}
}