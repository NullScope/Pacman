using UnityEngine;
using System.Collections;

public class PowerPellet : Point {

	// Use this for initialization
	new public void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	new public void Update () {
        base.Update();
	}

    /*new void OnTriggerEnter2D(Collider2D coll)
    {
        base.OnTriggerEnter2D(coll);

        if (coll.gameObject.tag == "Player")
        {
            //if collision was with pacman, make the ghosts vulnerable
            gameController.startPowerPellet();
        }
    }*/

    public override void consume()
    {
        base.consume();
        gameController.startPowerPellet();
    }
}
