using UnityEngine;
using System.Collections;

public class countdown : MonoBehaviour {
    float timer = 1;
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0)
        {
            guiText.text = timer.ToString("F0");
        }
        else
        {
            guiText.text = "TIME OVER\nPress X to restart";
            if (Input.GetKeyDown("x"))
            { // reload the same level
                Application.LoadLevel(Application.loadedLevel);
            }
        }
    }
}