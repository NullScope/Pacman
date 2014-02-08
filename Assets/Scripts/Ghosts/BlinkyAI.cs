using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlinkyAI : GhostAI {

	// Use this for initialization
	new void Start () {
        base.Start();
	}

	// Update is called once per frame
	new void Update () {
        base.Update();

        targetTile = new Vector2(player.tile.x, player.tile.y * -1);

        /*if (gameController.Tiles.Length == 1008 && !bWorking)
        {
            RequestPathFind(transform.position, new Vector2(14, 26), this, new sbyte[1, 2] { { -1, 0 } }, false);
            bWorking = true;
        }*/
	}
}