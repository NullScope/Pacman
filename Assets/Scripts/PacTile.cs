using UnityEngine;
using System.Collections;

public class PacTile : MonoBehaviour {
    public GameController gameController;
    
    // Use this for initialization
	public void Start () {
        if (gameController == null)
        {
            GameObject Camera = GameObject.Find("Main Camera");
            gameController = Camera.GetComponent<GameController>();
        }
        gameController.addTile(gameObject.transform.position.x, -1*gameObject.transform.position.y, gameObject);
	}

	// Update is called once per frame
	public void Update () {

	}
}