using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InkyAI : GhostAI {
	
	// Use this for initialization
	new void Start () {
        base.Start();
	}

	// Update is called once per frame
	new void Update () {
        base.Update();

        if (gameController.Tiles.Length == 1008 && !bWorking)
        {
            RequestPathFind(transform.position, new Vector2(27, 35), this, new sbyte[1, 2] { { 0, 0 } }, true);
            bWorking = true;
        }
	}
}