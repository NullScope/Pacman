using UnityEngine;
using System.Collections;

public class ClydeAI : GhostAI {
	
	// Use this for initialization
	new void Start () {
        base.Start();
    }

    // Update is called once per frame
	new void Update () {
        base.Update();

        if (Euclidean(this.tile, player.tile) >= 8)
        {
            targetTile = new Vector2(player.tile.x, -1 * player.tile.y);
        }
        else
        {
            targetTile = scatterTile;
        }

        /*if (gameController.Tiles.Length == 1008 && !bWorking)
        {
            RequestPathFind(transform.position, new Vector2(0, 35), this, new sbyte[1, 2] { { 0, 0 } }, true);
            bWorking = true;
        }*/
    }
}