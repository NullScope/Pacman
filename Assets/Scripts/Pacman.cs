﻿using System;
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
            GameObject Camera = GameObject.Find("Main Camera");
            gameController = Camera.GetComponent<GameController>();
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
    //also check if Pacman auto-movement has hit a wall
    private void updateAxis()
    {
        if (!isMoving)
        {
            if (Input.GetAxisRaw("Horizontal") != 0
             && gameController.GetTile(tile.x + Input.GetAxisRaw("Horizontal"), tile.y) != 0
             && gameController.GetTile(tile.x + Input.GetAxisRaw("Horizontal"), tile.y) != 2)
            {
                input.x = Input.GetAxisRaw("Horizontal");
                input.y = 0;
            }

            if (Input.GetAxisRaw("Vertical") != 0
                && gameController.GetTile(tile.x, tile.y - Input.GetAxisRaw("Vertical")) != 0
                && gameController.GetTile(tile.x, tile.y - Input.GetAxisRaw("Vertical")) != 2)
            {
                input.x = 0;
                input.y = Input.GetAxisRaw("Vertical");
            }

            if (input.x != 0
                && (gameController.GetTile(tile.x + input.x, tile.y) == 0
                || gameController.GetTile(tile.x + input.x, tile.y) == 2))
            {
                input.x = 0;
            }
                

            if (input.y != 0
                && (gameController.GetTile(tile.x, tile.y - input.y) == 0
                || gameController.GetTile(tile.x, tile.y - input.y) == 2))
            {
                input.y = 0;
            }
        }
    }

    // For some reason tile coordinates do not round down immediately,
    // it takes 1-2 frames to round down.
    public void UpdatePosition()
    {
        tile = new Vector2((int)transform.position.x, (int)Math.Abs(transform.position.y));
    }

    public void UpdateDotCollision()
    {
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
