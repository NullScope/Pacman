﻿using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class DebugTiles : MonoBehaviour {
    PacTile tile;
	// Use this for initialization
	void Init () {
        
	}
	
	// Update is called once per frame
	void Update () {
        if (tile == null)
        {
            tile = gameObject.GetComponent<PacTile>();
        }

        if (tile.isIntersection)
        {
            Debug.DrawLine(new Vector3(gameObject.transform.position.x, gameObject.transform.position.y), new Vector3(gameObject.transform.position.x + 1, gameObject.transform.position.y), Color.green, 0.1f);
            Debug.DrawLine(new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 1), new Vector3(gameObject.transform.position.x + 1, gameObject.transform.position.y - 1), Color.green, 0.1f);
            Debug.DrawLine(new Vector3(gameObject.transform.position.x, gameObject.transform.position.y), new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 1), Color.green, 0.1f);
            Debug.DrawLine(new Vector3(gameObject.transform.position.x + 1, gameObject.transform.position.y), new Vector3(gameObject.transform.position.x + 1, gameObject.transform.position.y - 1), Color.green, 0.1f);

            if (tile.allowUpwards)
            {
                Debug.DrawLine(new Vector3(gameObject.transform.position.x + 0.5f, gameObject.transform.position.y - 0.5f), new Vector3(gameObject.transform.position.x + 0.5f, gameObject.transform.position.y + 0.5f), Color.red, 0.1f);
            }

            if (tile.allowBottom)
            {
                Debug.DrawLine(new Vector3(gameObject.transform.position.x+0.5f, gameObject.transform.position.y-0.5f), new Vector3(gameObject.transform.position.x + 0.5f, gameObject.transform.position.y - 1.5f), Color.red, 0.1f);
            }

            if (tile.allowLeft)
            {
                Debug.DrawLine(new Vector3(gameObject.transform.position.x+0.5f, gameObject.transform.position.y - 0.5f), new Vector3(gameObject.transform.position.x - 0.5f, gameObject.transform.position.y - 0.5f), Color.red);
            }

            if (tile.allowRight)
            {
                Debug.DrawLine(new Vector3(gameObject.transform.position.x + 0.5f, gameObject.transform.position.y - 0.5f), new Vector3(gameObject.transform.position.x + 1.5f, gameObject.transform.position.y - 0.5f), Color.red);
            }
            
        }
        
	}
}
