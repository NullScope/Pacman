using UnityEngine;
using System.Collections;

public class BonusSymbol : MonoBehaviour {

    public GameController gameController;
    public Vector2 tile;
    public bool hasSpawned;
    public Sprite[] bonusSymbols;
    public int[] bonusPoints;
    public Color textColor;
    private int dotCount;
    

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        UpdateDotCount();
        UpdatePosition();
	}

    void UpdateDotCount()
    {
        if (dotCount == 70 || dotCount == 170)
            StartCoroutine(spawnBonus());
    }

    public void UpdatePosition()
    {
        tile = new Vector2((int)transform.position.x, (int)System.Math.Abs(transform.position.y));
    }

    public void IncreaseDotCount()
    {
        dotCount++;
    }

    IEnumerator spawnBonus()
    {
        hasSpawned = true;

        // The last index will be used after the level surpasses the array length. 
        if (gameController.level >= bonusSymbols.Length - 1)
            gameObject.GetComponent<SpriteRenderer>().sprite = bonusSymbols[bonusSymbols.Length - 1];
        else
            gameObject.GetComponent<SpriteRenderer>().sprite = bonusSymbols[gameController.level - 1];

        yield return new WaitForSeconds(Random.Range(9,10));
        destroyBonus();
    }

    public void consume()
    {
        var points = 0;
        PointText text;
        if (gameController.level >= bonusPoints.Length)
        {
            gameController.Score += bonusPoints[bonusPoints.Length - 1];
            points = bonusPoints[bonusPoints.Length - 1];
        }
        else
        {
            gameController.Score += bonusPoints[gameController.level];
            points = bonusPoints[gameController.level];
        }

        text = (PointText)Instantiate(gameController.pointTextPrefab, transform.position, gameController.pointTextPrefab.transform.rotation);
        text.duration = 2f;
        text.textScore = points;
        text.textColor = textColor;
        
        destroyBonus();
    }

    void destroyBonus()
    {
        StopAllCoroutines();
        hasSpawned = false;
        gameObject.GetComponent<SpriteRenderer>().sprite = null;
    }
}
