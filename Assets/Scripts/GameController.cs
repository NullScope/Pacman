using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
    private int score;
    private GameObject[,] tiles = new GameObject[28, 36];
    
    public GameObject[,] Tiles
    {
        get
        {
            return tiles;
        }
    }

    public int Score
    {
        get 
        {
            return score;
        }
        set
        {
            score = value;
        }
    }
    
    public Pacman player;
    public GhostAI Blinky, Clyde, Inky, Pinky;
    public int lifes;
    public float PowerPellet;
    
    private float PPtimeLeft;
    private bool bPowerPellet;

	// Use this for initialization
	void Start () {
        
	}

    // Update is called once per frame
    void Update()
    {
        //print(player.tileX + " | " + player.tileY);
        updatePowerPellet();
	}

    public void CollisionWithGhost(GhostAI ghost)
    {
        if (bPowerPellet)
        {
            ghost.Death();
        }
        else
        {
            player.Death();
        }
    }

    void updatePowerPellet()
    {
        if (bPowerPellet == true)
        {

            PPtimeLeft -= Time.deltaTime;

            if (PPtimeLeft <= PowerPellet * 0.2)
            {
                setGhostsVulnEnd(true);
            }

            if (PPtimeLeft <= 0)
            {
                endPowerPellet();

            }
        }
    }

    public void startPowerPellet()
    {
        // Set bPowerPellet to true so Update can countdown the time left
        bPowerPellet = true;
        PPtimeLeft = PowerPellet;
        setGhostsVuln(true);
    }

    public void endPowerPellet()
    {
        // Set bPowerPellet to false so Update stops counting the time left
        bPowerPellet = false;
        setGhostsVulnEnd(false);
    }

    // Sets all ghosts vulnerability
    public void setGhostsVuln(bool vuln)
    {
        Blinky.setVulnerability(vuln);
        Clyde.setVulnerability(vuln);
        Inky.setVulnerability(vuln);
        Pinky.setVulnerability(vuln);
    }

    // Sets all ghosts vulnerabilityEnding
    public void setGhostsVulnEnd(bool vulnEnd)
    {
        Blinky.setVulnerabilityEnd(vulnEnd);
        Clyde.setVulnerabilityEnd(vulnEnd);
        Inky.setVulnerabilityEnd(vulnEnd);
        Pinky.setVulnerabilityEnd(vulnEnd);
    }

    public void returnGhostsToDefault()
    {
        Blinky.returnToDefault();
        Clyde.returnToDefault();
        Inky.returnToDefault();
        Pinky.returnToDefault();
    }

    public void addTile(float x, float y, GameObject tile)
    {
        tiles[(int)x, (int)y] = tile;
    }

    public GameObject getTile(int x, int y)
    {
        return tiles[x, y];
    }
}