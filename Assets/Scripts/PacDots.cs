using UnityEngine;
using System.Collections;

public class PacDots : Point {

	// Use this for initialization
	new void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	new void Update () {
        base.Update();
	}

    new void OnTriggerEnter2D(Collider2D coll)
    {
        base.OnTriggerEnter2D(coll);
        if (coll.gameObject.tag == "Player")
        {
            //if collision was with pacman, increase score

        }
    }
}
