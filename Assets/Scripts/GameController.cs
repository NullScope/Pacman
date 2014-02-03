using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;

public class GameController : MonoBehaviour
{

    #region Variables
    private int score;
    private int level;
    public int lifes;
    private int modeCount;
    private float modeTimer;
    private byte[,] tiles = new byte[28, 36];
    private PacTile[,] pacTiles = new PacTile[28, 36];
    public Pacman player;
    public GhostAI Blinky, Clyde, Inky, Pinky;
    private GhostAI.Modes currentMode;
    public float PowerPellet;
    private float PPtimeLeft;
    private bool bPowerPellet;
    #endregion

    #region Properties
    public byte[,] Tiles
    {
        get
        {
            return tiles;
        }
       
    }

    public PacTile[,] PacTiles
    {
        get
        {
            return pacTiles;
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
    #endregion

    // Use this for initialization
	void Start () 
    {
        currentMode = GhostAI.Modes.Scatter;
        //Increase level
        level++;   
	}

    // Update is called once per frame
    void Update()
    {   
        updatePowerPellet();
        updateModeTimer();
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

    void updateModeTimer()
    {
        modeTimer += Time.deltaTime;

        switch (modeCount)
        {
            case(0):
                if (level >= 5)
                {
                    if (modeTimer >= 5f)
                    {
                        changeGhostModes(GhostAI.Modes.Chase);
                    }
                }
                else
                {
                    if (modeTimer >= 7f)
                    {
                        changeGhostModes(GhostAI.Modes.Chase);
                    }
                }
                break;
            case(1):
                break;
            case(2):
                break;
            case(3):
                break;
            case(4):
                break;
            case(5):
                break;
            case(6):
                break;
            case(7):
                break;
        }
    }

    public void changeGhostModes(GhostAI.Modes newMode)
    {

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

    public void addTile(int x, int y, byte cost)
    {
        tiles[x, y] = cost;
    }

    public void AddPacTile(int x, int y, PacTile pacTile)
    {
        pacTiles[x, y] = pacTile;
    }

    public PacTile GetPacTile(int x, int y)
    {
        return PacTiles[x, y];
    }

    public PacTile GetPacTile(Vector2 position)
    {
        return PacTiles[(int)position.x, (int)position.y];
    }

    public byte getTile(float x, float y)
    {
        return tiles[(int)x, (int)y];
    }
}