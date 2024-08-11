using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{

    [SerializeField] private List<Level> levels;
    [SerializeField] private Transform levelParent;

    private int currentLevel = 0;

    // Start is called before the first frame update
    void Start()
    {
        LoadLevel(currentLevel);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LoadLevel(int levelIndex) {
        foreach(Transform existingLevel in levelParent) {
            Destroy(existingLevel.gameObject);
        }
        Level level = Instantiate(levels[levelIndex], levelParent);
        level.transform.position = Vector3.zero;
        level.OnLevelComplete += OnLevelComplete;
    }

    private void OnLevelComplete(object sender, System.EventArgs e) {
        Debug.Log("LEVEL COMPLETE");
    }
}
