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
    public int level;
    public int lifes;
    private int globalDotCount;
    private int modeCount;
    private float modeTimer;
    private float houseTimer;
    private byte[,] tiles = new byte[28, 36];
    private PacTile[,] pacTiles = new PacTile[28, 36];
    public Pacman player;
    public GhostAI Blinky, Clyde, Inky, Pinky;
    public GhostAI.Modes startMode;
    private GhostAI.Modes currentMode;
    public float PowerPelletTime;
    private float PPtimeLeft;
    public bool activePowerPellet;
    public bool bGlobalCounter;
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
        currentMode = startMode;
	}

    // Update is called once per frame
    void Update()
    {   
        updatePowerPellet();
        updateModeTimer();
        updateHouseTimer();
	}

    void updatePowerPellet()
    {
        if (!activePowerPellet)
        {
            return;
        }

        PPtimeLeft -= Time.deltaTime;

        if (PPtimeLeft <= PowerPelletTime * 0.2)
        {
            setGhostsVulnEnd(true, false);
        }

        if (PPtimeLeft <= 0)
        {
            endPowerPellet();

        }
    }

    void updateHouseTimer()
    {
        houseTimer += Time.deltaTime;

        if (level >= 5)
        {
            if (houseTimer < 3f)
            {
                return;
            }
        }
        else
        {
            if (houseTimer < 4f)
            {
                return;
            }
        }

        houseTimer = 0;

        if (!Pinky.isOutside)
        {
            Pinky.leaveGhostHouse();
        }
        else
        {
            if (!Inky.isOutside)
            {
                Inky.leaveGhostHouse();
            }
            else
            {
                if (!Clyde.isOutside)
                {
                    Clyde.leaveGhostHouse();
                }
            }
        }
    }

    public void updateDotCounter()
    {
        houseTimer = 0f;
        // If the Global Counter is active, use it instead
        if (bGlobalCounter)
        {
            globalDotCount++;

            // Release Pinky from the Ghost House.
            if (globalDotCount == 7)
            {
                if (!Pinky.isOutside)
                {
                    Pinky.leaveGhostHouse();
                }
            }

            if (globalDotCount == 17)
            {
                if (!Inky.isOutside)
                {
                    Inky.leaveGhostHouse();
                }
            }

            if (globalDotCount == 32)
            {
                // If Clyde is inside the house, release him AND set the global counter to false.
                if (!Clyde.isOutside)
                {
                    Clyde.leaveGhostHouse();
                    bGlobalCounter = false;
                }
            }
        }
        else
        {
            if (!Pinky.isOutside)
            {
                Pinky.IncreaseDotCount();
            }
            else
            {
                if (!Inky.isOutside)
                {
                    Inky.IncreaseDotCount();
                }
                else
                {
                    if (!Clyde.isOutside)
                    {
                        Clyde.IncreaseDotCount();
                    }
                }
            }
        }
    }

    void updateModeTimer()
    {
        if (activePowerPellet)
        {
            return;
        }

        modeTimer += Time.deltaTime;
        print("TIMER: " + modeTimer);

        switch (modeCount)
        {
            case(0):
                if (level >= 5)
                {
                    if (modeTimer >= 5f)
                    {
                        changeGhostModes(GhostAI.Modes.Chase);
                        modeTimer = 0;
                        modeCount++;
                    }
                }
                else
                {
                    if (modeTimer >= 7f)
                    {
                        changeGhostModes(GhostAI.Modes.Chase);
                        modeTimer = 0;
                        modeCount++;
                    }
                }
                break;
            case(1):
                if (modeTimer >= 20f)
                {
                    changeGhostModes(GhostAI.Modes.Scatter);
                    modeTimer = 0;
                    modeCount++;
                }
                break;
            case(2):
                if (level >= 5)
                {
                    if (modeTimer >= 5f)
                    {
                        changeGhostModes(GhostAI.Modes.Chase);
                        modeTimer = 0;
                        modeCount++;
                    }
                }
                else
                {
                    if (modeTimer >= 7f)
                    {
                        changeGhostModes(GhostAI.Modes.Chase);
                        modeTimer = 0;
                        modeCount++;
                    }
                }
                break;
            case(3):
                if (modeTimer >= 20f)
                {
                    changeGhostModes(GhostAI.Modes.Scatter);
                    modeTimer = 0;
                    modeCount++;
                }
                break;
            case(4):
                if (modeTimer >= 5f)
                {
                    changeGhostModes(GhostAI.Modes.Chase);
                    modeTimer = 0;
                    modeCount++;
                }
                break;
            case(5):
                if (level >= 2 && level <= 4)
                {
                    if (modeTimer >= 1033f)
                    {
                        changeGhostModes(GhostAI.Modes.Scatter);
                        modeTimer = 0;
                        modeCount++;
                    }
                }
                else
                {
                    if (level >= 5)
                    {
                        if (modeTimer >= 1037f)
                        {
                            changeGhostModes(GhostAI.Modes.Scatter);
                            modeTimer = 0;
                            modeCount++;
                        }
                    }
                    else
                    {
                        if (modeTimer >= 20f)
                        {
                            changeGhostModes(GhostAI.Modes.Scatter);
                            modeTimer = 0;
                            modeCount++;
                        }
                    }

                }
                break;
            case(6):
                if (level >= 2)
                {
                    if (modeTimer >= 1f/60f)
                    {
                        changeGhostModes(GhostAI.Modes.Scatter);
                        modeTimer = 0;
                        modeCount++;
                    }
                }
                else
                {
                    if (modeTimer >= 5f)
                    {
                        changeGhostModes(GhostAI.Modes.Scatter);
                        modeTimer = 0;
                        modeCount++;
                    }
                }
                break;
            case(7):
                // Infinite
                break;
        }
    }

    // If Pacman dies, reset the ghosts back to the original position and set game flags
    public void PacmanDeath()
    {
        bGlobalCounter = true;
        globalDotCount = 0;
    }

    public void changeGhostModes(GhostAI.Modes newMode)
    {
        Blinky.setGlobalMode(newMode);
        Pinky.setGlobalMode(newMode);
        Inky.setGlobalMode(newMode);
        Clyde.setGlobalMode(newMode);
    }

    public void startPowerPellet()
    {
        // Set bPowerPellet to true so Update can countdown the time left
        activePowerPellet = true;
        PPtimeLeft = PowerPelletTime;
        setGhostsVuln(true);
        setGhostsVulnEnd(false, false);
    }

    public void endPowerPellet()
    {
        // Set bPowerPellet to false so Update stops counting the time left
        activePowerPellet = false;
        setGhostsVulnEnd(false, true);
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
    public void setGhostsVulnEnd(bool vulnEnd, bool changeMode)
    {
        Blinky.setVulnerabilityEnd(vulnEnd, changeMode);
        Clyde.setVulnerabilityEnd(vulnEnd, changeMode);
        Inky.setVulnerabilityEnd(vulnEnd, changeMode);
        Pinky.setVulnerabilityEnd(vulnEnd, changeMode);
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

    public byte GetTile(float x, float y)
    {
        return tiles[(int)x, (int)y];
    }
}