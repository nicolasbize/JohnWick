using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private CameraFollow mainCamera;
    [SerializeField] private List<Checkpoint> checkpoints;

    private Checkpoint nextCheckpoint = null;
    private Queue<Checkpoint> checkpointQueue = new Queue<Checkpoint>();

    private void Start() {
        mainCamera.OnPositionChange += Camera_OnPositionChange;
        foreach(Checkpoint checkpoint in GetComponentsInChildren<Checkpoint>(true)) {
            checkpointQueue.Enqueue(checkpoint);
        }
        ActivateNextCheckpoint();
    }

    private void ActivateNextCheckpoint() {
        if (checkpointQueue.Count > 0){
            nextCheckpoint = checkpointQueue.Dequeue();
            nextCheckpoint.OnComplete += OnCompleteCheckpoint;
        } else {
            Debug.Log("completed level");
        }
    }

    private void OnCompleteCheckpoint(object sender, System.EventArgs e) {
        ActivateNextCheckpoint();
    }

    private void Camera_OnPositionChange(object sender, System.EventArgs e) {
        if (nextCheckpoint != null && !mainCamera.IsLocked && 
            mainCamera.transform.position.x >= nextCheckpoint.CameraLockTargetX) {
            // lock camera in place
            Vector3 prevPosition = mainCamera.transform.position;
            mainCamera.transform.position = new Vector3(nextCheckpoint.CameraLockTargetX, prevPosition.y, prevPosition.z);
            mainCamera.LockInPlace();
            nextCheckpoint.Run();
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (nextCheckpoint == null) {
            nextCheckpoint = new Checkpoint();
        }
    }
}
