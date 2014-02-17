using UnityEngine;
using System.Collections;

public class PointText : MonoBehaviour {
    public Color textColor;
    public float duration;
    public int textScore;

	// Use this for initialization
	void Start () {
        GetComponent<TextMesh>().color = textColor;
        GetComponent<TextMesh>().text = textScore.ToString();
        StartCoroutine(Countdown());
	}

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
        yield break;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
