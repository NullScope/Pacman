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
    public PacTile[,] pacTiles = new PacTile[28, 36];
    public int totalPoints;
    [HideInInspector]
    public Pacman player;
    public Pacman pacmanPrefab;
    [HideInInspector]
    public GhostAI blinky, clyde, inky, pinky;
    public GhostAI blinkyPrefab, clydePrefab, inkyPrefab, pinkyPrefab;
    public GameLevel GameLevelPrefab;
    public float[] PowerPelletTimes;
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
    public Vector2[] symbolPositions;
    public Vector2[] lifePositions;
    public List<GameObject> lifeSprites = new List<GameObject>();
    public Sprite lifeSprite;
    #endregion

    #region Properties

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
        GameLevel gameLevel = null;

        if (GameObject.Find("Game Level(Clone)") == null)
        {
            gameLevel = (GameLevel)Instantiate(GameLevelPrefab, new Vector2(0, 0), GameLevelPrefab.gameObject.transform.rotation);
        }
        else
        {
            gameLevel = GameObject.Find("Game Level(Clone)").GetComponent<GameLevel>();
        }

        level = gameLevel.level++;

        var positionI = 0;

        for (int i = (level >= 5 ? level - 5: 0); i < level; i++)
        {
            var symbolObject = new GameObject("symbol" + i);
            symbolObject.AddComponent("SpriteRenderer");
            symbolObject.GetComponent<SpriteRenderer>().sprite = bonusSymbol.bonusSymbols[(i >= bonusSymbol.bonusSymbols.Length ? bonusSymbol.bonusSymbols.Length - 1 : i)];
            symbolObject.transform.localScale = new Vector3(12.5f, 12.5f, 1);
            symbolObject.transform.position = symbolPositions[positionI];
            positionI++;
        }

        for (int i = 0; i < lifePositions.Length; i++)
        {

            var lifeObject = new GameObject("life" + i);
            lifeObject.AddComponent("SpriteRenderer");
            lifeObject.GetComponent<SpriteRenderer>().sprite = lifeSprite;
            lifeObject.transform.localScale = new Vector3(12.5f, 12.5f, 1);
            lifeObject.transform.position = lifePositions[i];

            lifeSprites.Add(lifeObject);

        }

        StartCoroutine(StartGame(false));
	}

    public IEnumerator StartGame(bool isRestart)
    {
        if (lifes == 0)
        {
            StartCoroutine(Pause(0));
            gameStatusText.text = "GAME OVER";
            gameStatusText.color = Color.red;
            yield return new WaitForSeconds(4f);
            Application.LoadLevel("Pacman");
            yield break;
        }
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
        Destroy(lifeSprites[lifes-1]);
        lifes--;
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

        if (totalPoints == 0)
            Application.LoadLevel("Pacman");
	}

    void updatePowerPellet()
    {
        if (!activePowerPellet)
        {
            return;
        }

        PPtimeLeft -= Time.smoothDeltaTime;

        if (PPtimeLeft <= PowerPelletTimes[(level >= PowerPelletTimes.Length ? PowerPelletTimes.Length - 1 : level)] * 0.2)
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

        modeTimer += Time.smoothDeltaTime;

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
        PPtimeLeft = PowerPelletTimes[(level >= PowerPelletTimes.Length ? PowerPelletTimes.Length - 1 : level)];
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

    public void AddPacTile(int x, int y, PacTile pacTile)
    {
        pacTiles[x, y] = pacTile;
    }

    public PacTile GetPacTile(Vector2 position)
    {
        return PacTiles[(int)position.x, (int)position.y];
    }

    public PacTile GetPacTile(int x, int y)
    {
        return PacTiles[x, y];
    }
}