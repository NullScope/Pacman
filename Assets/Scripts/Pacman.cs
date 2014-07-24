using System;
using UnityEngine;
using System.Collections;

public class Pacman : MonoBehaviour {

    public enum Directions { Up, Left, Down, Right }

    #region Variables
    public GameController gameController;
    private Animator anim;

    public int framePauses;

    public Directions currentDirection;
    public Directions startDirection;

    public Vector2 tile;
    public Vector2 startPosition;

    public bool isAlive;
    private bool isFirstMove;

    private float moveSpeed;
    public float maxSpeed;
    public float[] defaultSpeedPercentages;
    public float[] frightSpeedPercentages;

    private bool isMoving = false;
    private Vector2 input;
    #endregion

	// Use this for initialization
	void Start () {
        isAlive = true;
        isFirstMove = true;

        anim = GetComponent<Animator>();

        UpdatePosition();

        if (gameController == null)
        {
            gameController = GameObject.Find("Main Camera").GetComponent<GameController>();
        }

        switch (startDirection)
        {
            case(Directions.Up):
                input.y = -1;
                break;
            case (Directions.Left):
                input.x = -1;
                break;
            case (Directions.Down):
                input.y = 1;
                break;
            case (Directions.Right):
                input.x = 1;
                break;
        }
	}

    // Update is called once per frame
    void Update()
    {

        if (gameController.isPaused)
        {
            anim.speed = 0;
            return;
        }
        else
        {
            anim.speed = 1;
        }

        UpdatePosition();
        UpdateDotCollision();

        if (isAlive == true)
        {
            updateAxis();
            if (!isMoving && input != Vector2.zero)
            {
                StartCoroutine(move());
            }
        }

        // Always stop Running animation after Pacman moves a tile.
        if (input == Vector2.zero)
        {
            anim.SetBool("Running", false);
        }
	}

    //Kills Pacman and plays death animation
    public void Death()
    {
        isAlive = false;
        transform.Rotate(new Vector3(0, 0, -transform.eulerAngles.z), Space.World);
        anim.SetBool("Running", false);
        anim.SetBool("Dead", true);
        input = Vector2.zero;
    }

    //Sets Pacman position to respawn
    void Respawn()
    {
        /*transform.position = startPosition;
        isAlive = true;
        isFirstMove = true;
        anim.SetBool("Dead", false);*/
        gameController.StartCoroutine(gameController.StartGame(true));
        Destroy(this.gameObject);
        Destroy(this);
    }

    void destroyGhosts()
    {
        gameController.PacmanDeath();
    }

    void playDeathBeep()
    {

    }

    public IEnumerator move()
    {
        float newZ, t = 0;
        Vector2 endPosition, startPosition;
        isMoving = true;
        var doingDiagonal = false;

        if (tile.x < 0)
        {
            transform.position = new Vector2(gameController.pacTiles.GetLength(0) + 0.5f, transform.position.y);
        }
        else
        {
            if (tile.x >= gameController.pacTiles.GetLength(0))
            {
                transform.position = new Vector2(-0.5f, transform.position.y);
            }
        }

        if (tile.y < 0)
        {
            transform.position = new Vector2(transform.position.x, -1 * gameController.pacTiles.GetLength(1)+0.5f);
        }
        else
        {
            if (tile.y >= gameController.pacTiles.GetLength(1))
            {
                transform.position = new Vector2(transform.position.x, 0.5f);
            }
        }

        startPosition = transform.position;
        
        // If its the first time Pacman moving from the respawn point,
        // Pacman needs to move 0.5f instead of 1.0f because of the respawn location
        if (isFirstMove){
            endPosition = new Vector2(startPosition.x + System.Math.Sign(input.x) - 0.5f * input.x,
                                      startPosition.y + System.Math.Sign(input.y));
        }else{
            endPosition = new Vector2(startPosition.x + System.Math.Sign(input.x),
                                      startPosition.y + System.Math.Sign(input.y));
        }

        // Calculate rotation.
        if (input.x != 0)
        {
            newZ = input.x * 180;
            if (newZ == -180) newZ = 0;
        }else
        {
            newZ = input.y * -90;
        }

        currentDirection = VectorToDirection(startPosition, endPosition);

        UpdateMoveSpeed();

        while (t < 1f)
        {
            if (!doingDiagonal
                && tile.x == Mathf.Floor(endPosition.x)
                && tile.y == Mathf.Floor(Mathf.Abs(endPosition.y))
                && input.x != 0 
                && Input.GetAxisRaw("Vertical") != 0
                && gameController.GetPacTile(new Vector2(Mathf.Floor(endPosition.x), Mathf.Floor(Math.Abs(endPosition.y)) - Input.GetAxisRaw("Vertical"))).cost != 0
                && gameController.GetPacTile(new Vector2(Mathf.Floor(endPosition.x), Mathf.Floor(Math.Abs(endPosition.y)) - Input.GetAxisRaw("Vertical"))).cost != 2)
            {
                currentDirection = VectorToDirection(startPosition, endPosition);
                endPosition.y = endPosition.y + Input.GetAxisRaw("Vertical");
                doingDiagonal = true;
            }
            else if (!doingDiagonal
                    && tile.x == Mathf.Floor(endPosition.x)
                    && tile.y == Mathf.Floor(Mathf.Abs(endPosition.y))
                    && input.y != 0
                    && Input.GetAxisRaw("Horizontal") != 0
                    && gameController.GetPacTile(new Vector2(Mathf.Floor(endPosition.x) + Input.GetAxisRaw("Horizontal"), Mathf.Floor(Math.Abs(endPosition.y)))).cost != 0
                    && gameController.GetPacTile(new Vector2(Mathf.Floor(endPosition.x) + Input.GetAxisRaw("Horizontal"), Mathf.Floor(Math.Abs(endPosition.y)))).cost != 2)
            {
                currentDirection = VectorToDirection(startPosition, endPosition);
                endPosition.x = endPosition.x + Input.GetAxisRaw("Horizontal");
                doingDiagonal = true;
            }

            if (isFirstMove)
                t += (moveSpeed * 2) * Time.smoothDeltaTime;
            else
                t += moveSpeed * Time.smoothDeltaTime;


            while (framePauses != 0)
            {
                framePauses--;
                yield return 2;
            }

            transform.position = Vector2.Lerp(startPosition, endPosition, t);

            //Rotate Pacman to face the current direction
            transform.Rotate(new Vector3(0, 0, -transform.eulerAngles.z), Space.World);
            transform.Rotate(new Vector3(0, 0, newZ), Space.World);

            anim.SetBool("Running", true);
            
            if (t >= 1f)
                break;
            else
                yield return 1;
        }

        isMoving = false;
        isFirstMove = false;
        yield break;
    }

    //Check if the player pressed any axis button, ensure it's a valid move and update input accordingly
    //also check if Pacman auto-movement has hit a wall.
    private void updateAxis()
    {
        if(isMoving)
        {
            return; 
        }

        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            // If pacman is outside of the maze, prevents out of bounds errors.
            if (tile.x + Input.GetAxisRaw("Horizontal") < 0 || tile.x + Input.GetAxisRaw("Horizontal") >= gameController.pacTiles.GetLength(0))
            {
                input.x = Input.GetAxisRaw("Horizontal");
                input.y = 0;
            }
            else
            {
                if (gameController.GetPacTile(new Vector2(tile.x + Input.GetAxisRaw("Horizontal"), tile.y)).cost != 0
                   && gameController.GetPacTile(new Vector2(tile.x + Input.GetAxisRaw("Horizontal"), tile.y)).cost != 2)
                {
                    input.x = Input.GetAxisRaw("Horizontal");
                    input.y = 0;
                }
            }
                
        }

        if (Input.GetAxisRaw("Vertical") != 0)
        {
            // If pacman is outside of the maze, prevents out of bounds errors.
            if (tile.y + Input.GetAxisRaw("Vertical") < 0 || tile.y + Input.GetAxisRaw("Vertical") >= gameController.pacTiles.GetLength(1))
            {
                input.x = 0;
                input.y = Input.GetAxisRaw("Vertical");
            }
            else
            {
                if (gameController.GetPacTile(new Vector2(tile.x, tile.y - Input.GetAxisRaw("Vertical"))).cost != 0
                    && gameController.GetPacTile(new Vector2(tile.x, tile.y - Input.GetAxisRaw("Vertical"))).cost != 2)
                {
                    input.x = 0;
                    input.y = Input.GetAxisRaw("Vertical");
                }
            }
                
        }


        if (input.x != 0)
        {
            // If pacman is outside of the maze, prevents out of bounds errors
            if (tile.x > 0 && tile.x < gameController.pacTiles.GetLength(0) - 1)
            {
                if (gameController.GetPacTile(new Vector2(tile.x + input.x, tile.y)).cost == 0 || gameController.GetPacTile(new Vector2(tile.x + input.x, tile.y)).cost == 2)
                {
                    input.x = 0;
                }
            }
        }


        if (input.y != 0)
        {
            // If pacman is outside of the maze, prevents out of bounds errors
            if (tile.y > 0 && tile.y < gameController.pacTiles.GetLength(1) - 1)
            {
                if (gameController.GetPacTile(new Vector2(tile.x, tile.y - input.y)).cost == 0 || gameController.GetPacTile(new Vector2(tile.x, tile.y - input.y)).cost == 2)
                {
                    input.y = 0;
                }
            }
        }
    }

    // For some reason tile coordinates do not round down immediately,
    // it takes 1-2 frames to round down.
    public void UpdatePosition()
    {
        
        tile = new Vector2(Mathf.Floor(transform.position.x), Mathf.Floor(Math.Abs(transform.position.y)));

        // Current Center Tile.
        Debug.DrawLine(new Vector3(tile.x, -1 * tile.y), new Vector3(tile.x + 1, -1 * tile.y), new Color(130, 0, 255, 255), 0.0f, false);
        Debug.DrawLine(new Vector3(tile.x, -1 * tile.y - 1), new Vector3(tile.x + 1, -1 * tile.y - 1), new Color(130, 0, 255, 255), 0.0f, false);
        Debug.DrawLine(new Vector3(tile.x, -1 * tile.y), new Vector3(tile.x, -1 * tile.y - 1), new Color(130, 0, 255, 255), 0.0f, false);
        Debug.DrawLine(new Vector3(tile.x + 1, -1 * tile.y), new Vector3(tile.x + 1, -1 * tile.y - 1), new Color(130, 0, 255, 255), 0.0f, false);

        if (tile.x + input.x > 0 && tile.x + input.x < gameController.pacTiles.GetLength(0) - 1 && tile.y - input.y > 0 && tile.y + input.y < gameController.pacTiles.GetLength(1) - 1)
        {
            PacTile targetTile = gameController.GetPacTile(new Vector2(tile.x + input.x, tile.y - input.y));
            
        }
    }

    public void UpdateDotCollision()
    {
        if (tile.x <= 0 || tile.x >= gameController.pacTiles.GetLength(0) || tile.y <= 0 || tile.y >= gameController.pacTiles.GetLength(1))
        {
            return;
        }

        if (gameController.GetPacTile(tile).tag == "PacDot" || gameController.GetPacTile(tile).tag == "Power Pellet")
        {
            Point point = (Point)gameController.GetPacTile(tile);
            
            if (!point.isConsumed)
                point.consume();
        }

        // Check for collision with the Bonus Symbol
        if (tile == gameController.bonusSymbol.tile && gameController.bonusSymbol.hasSpawned)
            gameController.bonusSymbol.consume();
    }

    void UpdateMoveSpeed()
    {
        // The last index will be used after the level surpasses the array length. 
        if (!gameController.activePowerPellet)
            if (gameController.level > defaultSpeedPercentages.Length - 1)
                moveSpeed = maxSpeed * (defaultSpeedPercentages[defaultSpeedPercentages.Length - 1] / 100);
            else
                moveSpeed = maxSpeed * (defaultSpeedPercentages[gameController.level] / 100);
        else
            if (gameController.level > frightSpeedPercentages.Length - 1)
                moveSpeed = maxSpeed * (frightSpeedPercentages[frightSpeedPercentages.Length - 1] / 100);
            else
                moveSpeed = maxSpeed * (frightSpeedPercentages[gameController.level] / 100);
    }

    public Directions VectorToDirection(Vector2 startPoint, Vector2 endPoint)
    {
        Vector2 direction = startPoint - endPoint;

        // If Direction is Left.
        if (direction.x == 1f)
        {
            return Directions.Left;
        }

        // If Direction is Right.
        if (direction.x == -1f)
        {
            return Directions.Right;
        }

        // If Direction is Down.
        if (direction.y == 1f)
        {
            return Directions.Down;
        }

        // If Direction is Up
        if (direction.y == -1f)
        {
            return Directions.Up;
        }

        return Directions.Up;
    }

}
