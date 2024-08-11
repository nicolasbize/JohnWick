using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public event EventHandler OnLevelComplete;
    
    [SerializeField] private List<Checkpoint> checkpoints;

    private Checkpoint nextCheckpoint = null;
    private Queue<Checkpoint> checkpointQueue = new Queue<Checkpoint>();
    private CameraFollow mainCamera;

    private void Start() {
        mainCamera = Camera.main.GetComponent<CameraFollow>();
        mainCamera.OnPositionChange += Camera_OnPositionChange;
        foreach(Checkpoint checkpoint in GetComponentsInChildren<Checkpoint>()) {
            checkpointQueue.Enqueue(checkpoint);
        }
        ActivateNextCheckpoint(false);
    }

    private void ActivateNextCheckpoint(bool flashGoIndicator = true) {
        if (checkpointQueue.Count > 0){
            nextCheckpoint = checkpointQueue.Dequeue();
            nextCheckpoint.OnComplete += OnCompleteCheckpoint;
            mainCamera.Unlock();
            if (flashGoIndicator) {
                UI.Instance.NotifyGoGoGo();
            }
        } else {
            OnLevelComplete?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnCompleteCheckpoint(object sender, System.EventArgs e) {
        ((Checkpoint) sender).OnComplete -= OnCompleteCheckpoint;
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
