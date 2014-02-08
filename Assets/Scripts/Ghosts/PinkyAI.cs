using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PinkyAI : GhostAI {
	
	// Use this for initialization
	new void Start () {
        base.Start();
	}

	// Update is called once per frame
	new void Update () {
        base.Update();

        switch (player.currentDirection)
        {
            case(Pacman.Directions.Up):
                targetTile = new Vector2(player.tile.x - 4f, -1 * player.tile.y + 4f);
                break;
            case (Pacman.Directions.Left):
                targetTile = new Vector2(player.tile.x - 4f, -1 * player.tile.y);
                break;
            case (Pacman.Directions.Down):
                targetTile = new Vector2(player.tile.x, -1 * player.tile.y - 4f);
                break;
            case (Pacman.Directions.Right):
                targetTile = new Vector2(player.tile.x + 4f, -1 * player.tile.y);
                break;
        }

        /*if (gameController.Tiles.Length == 1008 && !bWorking)
        {
            RequestPathFind(transform.position, new Vector2(2, 0), this, new sbyte[1, 2] { { 1, 0 } }, true);
            bWorking = true;
        }*/
	}
}