using UnityEngine;
using System.Collections;

public class Point : PacTile {
    public Sprite backgroundSprite;
    public int points;
    public int framePauses;
    public bool isConsumed;

	// Use this for initialization
	new public void Start () {
        base.Start();

        gameController.totalPoints++;
	}

	// Update is called once per frame
	new public void Update () {
        base.Update();
        
	}

    /*public void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Player")
        {
            //if collision was with pacman, increase score
            consume();
        }
    }*/

    public virtual void consume()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = backgroundSprite;
        gameController.Score += points;
        gameController.UpdateDotCount();
        gameController.player.framePauses = framePauses;
        gameController.totalPoints--;
        isConsumed = true;
    }
}