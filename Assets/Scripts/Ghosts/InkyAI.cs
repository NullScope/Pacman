using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InkyAI : GhostAI {

    public GhostAI blinky;

	// Use this for initialization
	new void Start () {
        base.Start();
	}

	// Update is called once per frame
	new void Update () {
        base.Update();

        var distance = new Vector2();
        var offset = new Vector2();

        switch (player.currentDirection)
        {
            case (Pacman.Directions.Up):
                offset = new Vector2(player.tile.x - 2f, -1 * player.tile.y + 2f);
                distance = new Vector2(blinky.tile.x - offset.x, (-1 * blinky.tile.y) - offset.y);
                targetTile = offset - distance;
                break;
            case (Pacman.Directions.Left):
                offset = new Vector2(player.tile.x - 2f, -1 * player.tile.y);
                distance = new Vector2(blinky.tile.x - offset.x, (-1 * blinky.tile.y) - offset.y);
                targetTile = offset - distance;
                break;
            case (Pacman.Directions.Down):
                offset = new Vector2(player.tile.x, -1 * player.tile.y - 2f);
                distance = new Vector2(blinky.tile.x - offset.x, (-1 * blinky.tile.y) - offset.y);
                targetTile = offset - distance;
                break;
            case (Pacman.Directions.Right):
                offset = new Vector2(player.tile.x + 2f, -1 * player.tile.y);
                distance = new Vector2(blinky.tile.x - offset.x, (-1 * blinky.tile.y) - offset.y);
                targetTile = offset - distance;
                break;
        }

        /*if (gameController.Tiles.Length == 1008 && !bWorking)
        {
            RequestPathFind(transform.position, new Vector2(27, 35), this, new sbyte[1, 2] { { 0, 0 } }, true);
            bWorking = true;
        }*/
	}
}