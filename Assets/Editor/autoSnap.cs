using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
[ExecuteInEditMode]
[System.Serializable]
public class autoSnap : EditorWindow
{
    [SerializeField]
    static private Vector3 prevPosition;
    [SerializeField]
    static private bool doSnap = true;
    [SerializeField]
    static private float snapValue = 1;
    
    [MenuItem("Edit/Auto Snap %_l")]

    static void Init()
    {
        EditorWindow window = (autoSnap)EditorWindow.GetWindow(typeof(autoSnap));
        window.maxSize = new Vector2(200, 100);
        EditorApplication.update = Update;
    }

    public void OnGUI()
    {
        doSnap = EditorGUILayout.Toggle("Auto Snap", doSnap);
        snapValue = EditorGUILayout.FloatField("Snap Value", snapValue);
    }

    static public void Update()
    {
        if (doSnap
        && Selection.transforms.Length > 0
        && Selection.transforms[0].position != prevPosition)
        {
            Snap();
            prevPosition = Selection.transforms[0].position;
        }
    }

    static private void Snap()
    {
        foreach (var transform in Selection.transforms)
        {
            var t = transform.transform.position;
            t.x = Round(t.x);
            t.y = Round(t.y);
            t.z = Round(t.z);
            transform.transform.position = t;
        }
    }

    static private float Round(float input)
    {
        return snapValue * Mathf.Round((input / snapValue));
    }
}