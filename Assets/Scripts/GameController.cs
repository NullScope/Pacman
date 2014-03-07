using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;

public class GameController : MonoBehaviour
{

    #region Variables
    private float score;
    private float highScore;
    public int level;
    public int lifes;
    private int globalDotCount;
    private int modeCount;
    private float modeTimer;
    private float houseTimer;
    public byte[,] tiles = new byte[28, 36];
    public PacTile[,] pacTiles = new PacTile[28, 36];
    [HideInInspector]
    public Pacman player;
    public Pacman pacmanPrefab;
    [HideInInspector]
    public GhostAI blinky, clyde, inky, pinky;
    public GhostAI blinkyPrefab, clydePrefab, inkyPrefab, pinkyPrefab;
    public float PowerPelletTime;
    private float PPtimeLeft;
    public bool activePowerPellet;
    public bool bGlobalCounter;
    public bool isPaused;
    public BonusSymbol bonusSymbol;
    public TextMesh scoreText;
    public TextMesh highScoreText;
    public TextMesh gameStatusText;
    public TextMesh playerText;
    public PointText pointTextPrefab;
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

    public float Score
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
        StartCoroutine(StartGame(false));
	}

    public IEnumerator StartGame(bool isRestart)
    {
        if (!isRestart)
        {
            StartCoroutine(Pause(4f));
            highScore = PlayerPrefs.GetFloat("Highscore");
            highScoreText.text = highScore.ToString();
            playerText.text = "Player One";
            gameStatusText.text = "Ready!";
            yield return new WaitForSeconds(2f);
            playerText.text = "";
        }
        else
        {
            StartCoroutine(Pause(2f));
            gameStatusText.text = "Ready!";
        }
        
        InstantiatePacman();
        InstantiateGhosts();
        yield return new WaitForSeconds(2f);
        gameStatusText.text = "";
        yield break;
    }

    private void InstantiateGhosts()
    {
        blinky = (GhostAI)Instantiate(blinkyPrefab, new Vector2(14f, -14.5f), blinkyPrefab.gameObject.transform.rotation);
        pinky = (GhostAI)Instantiate(pinkyPrefab, pinkyPrefab.houseTile, pinkyPrefab.gameObject.transform.rotation);
        inky = (GhostAI)Instantiate(inkyPrefab, inkyPrefab.houseTile, inkyPrefab.gameObject.transform.rotation);
        clyde = (GhostAI)Instantiate(clydePrefab, clydePrefab.houseTile, clydePrefab.gameObject.transform.rotation);

        blinky.gameController = this;
        pinky.gameController = this;
        inky.gameController = this;
        clyde.gameController = this;
    }

    private void InstantiatePacman()
    {
        player = (Pacman)Instantiate(pacmanPrefab, pacmanPrefab.startPosition, pacmanPrefab.gameObject.transform.rotation);
        player.gameController = this;
    }

    // Update is called once per frame
    void Update()
    {   
        if (isPaused)
        {
            return;
        }

        updatePowerPellet();
        updateModeTimer();
        updateHouseTimer();
        scoreText.text = Score.ToString();

        if (Score > highScore)
        {
            PlayerPrefs.SetFloat("Highscore", score);
            highScore = Score;
            highScoreText.text = Score.ToString();
        }
	}

    void updatePowerPellet()
    {
        if (!activePowerPellet)
        {
            return;
        }

        PPtimeLeft -= Time.smoothDeltaTime;

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
        houseTimer += Time.smoothDeltaTime;

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

        if (!pinky.isOutside)
        {
            pinky.leaveGhostHouse();
        }
        else
        {
            if (!inky.isOutside)
            {
                inky.leaveGhostHouse();
            }
            else
            {
                if (!clyde.isOutside)
                {
                    clyde.leaveGhostHouse();
                }
            }
        }
    }

    public void UpdateDotCount()
    {
        houseTimer = 0f;

        bonusSymbol.IncreaseDotCount();

        // If the Global Counter is active, use it instead
        if (bGlobalCounter)
        {
            globalDotCount++;

            // Release Pinky from the Ghost House.
            if (globalDotCount == 7)
            {
                if (!pinky.isOutside)
                {
                    pinky.leaveGhostHouse();
                }
            }

            if (globalDotCount == 17)
            {
                if (!inky.isOutside)
                {
                    inky.leaveGhostHouse();
                }
            }

            if (globalDotCount == 32)
            {
                // If Clyde is inside the house, release him AND set the global counter to false.
                if (!clyde.isOutside)
                {
                    clyde.leaveGhostHouse();
                    bGlobalCounter = false;
                }
            }
        }
        else
        {
            if (!pinky.isOutside)
            {
                pinky.IncreaseDotCount();
            }
            else
            {
                if (!inky.isOutside)
                {
                    inky.IncreaseDotCount();
                }
                else
                {
                    if (!clyde.isOutside)
                    {
                        clyde.IncreaseDotCount();
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
        if (!blinky)
        {
            return;
        }

        bGlobalCounter = true;
        globalDotCount = 0;

        // Destroy the game objects.
        Destroy(blinky.gameObject);
        Destroy(clyde.gameObject);
        Destroy(inky.gameObject);
        Destroy(pinky.gameObject);

        // Destroy the scripts.
        Destroy(blinky);
        Destroy(clyde);
        Destroy(inky);
        Destroy(pinky);

        blinky = null;
        clyde = null;
        inky = null;
        pinky = null;

        houseTimer = 0;
        modeTimer = 0;
        modeCount = 0;
    }

    public IEnumerator Pause(float duration)
    {
        isPaused = true;

        if (duration == 0)
        {
            yield break;
        }
        else
        {
            yield return new WaitForSeconds(duration);
            
            isPaused = false;
        }
    }

    public void Resume()
    {
        StopCoroutine("Pause");
        isPaused = false;
    }

    public void changeGhostModes(GhostAI.Modes newMode)
    {
        blinky.setGlobalMode(newMode);
        pinky.setGlobalMode(newMode);
        inky.setGlobalMode(newMode);
        clyde.setGlobalMode(newMode);
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
        blinky.setVulnerability(vuln);
        clyde.setVulnerability(vuln);
        inky.setVulnerability(vuln);
        pinky.setVulnerability(vuln);
    }

    // Sets all ghosts vulnerabilityEnding
    public void setGhostsVulnEnd(bool vulnEnd, bool changeMode)
    {
        blinky.setVulnerabilityEnd(vulnEnd, changeMode);
        clyde.setVulnerabilityEnd(vulnEnd, changeMode);
        inky.setVulnerabilityEnd(vulnEnd, changeMode);
        pinky.setVulnerabilityEnd(vulnEnd, changeMode);
    }

    public void returnGhostsToDefault()
    {
        blinky.returnToDefault();
        clyde.returnToDefault();
        inky.returnToDefault();
        pinky.returnToDefault();
    }

    public void AddTile(int x, int y, byte cost)
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