using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletShot : MonoBehaviour
{

    private enum Direction { Left, Right }
    
    [SerializeField] private float durationShot;

    private LineRenderer line;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float timeStart = float.NegativeInfinity;

    private void Start() {
        line = GetComponent<LineRenderer>();
        timeStart = Time.timeSinceLevelLoad;
        line.positionCount = 2;
        line.SetPosition(0, startPosition);
        line.SetPosition(1, endPosition);
        durationShot = (Mathf.Abs(startPosition.x - endPosition.x) * durationShot) / 64f; // keep it fast regardless of distance
    }

    public void SetUp(Vector3 startPos, Vector3 endPos) {
        startPosition = startPos;
        endPosition = endPos;
    }

    private void Update() {
        if (Time.timeSinceLevelLoad - timeStart > durationShot) {
            Destroy(gameObject);
        } else {
            float progress = (Time.timeSinceLevelLoad - timeStart) / durationShot;
            float newXStart = Mathf.Lerp(startPosition.x, endPosition.x, progress);
            line.SetPosition(0, new Vector3(Mathf.FloorToInt(newXStart), startPosition.y, startPosition.z));
        }
    }


}
