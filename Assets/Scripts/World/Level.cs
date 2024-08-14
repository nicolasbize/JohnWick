using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public event EventHandler OnStartTransition;

    [SerializeField] private AudioClip levelMusic;

    private Checkpoint nextCheckpoint = null;
    private Queue<Checkpoint> checkpointQueue = new Queue<Checkpoint>();
    private CameraFollow mainCamera;
    private AudioSource audioSource;
    private bool isFadingOutMusic = false;

    private void Start() {
        mainCamera = Camera.main.GetComponent<CameraFollow>();
        mainCamera.OnPositionChange += Camera_OnPositionChange;
        foreach(Checkpoint checkpoint in GetComponentsInChildren<Checkpoint>()) {
            checkpointQueue.Enqueue(checkpoint);
        }
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = levelMusic;
        audioSource.Play();
        ActivateNextCheckpoint(false);
    }

    private void ActivateNextCheckpoint(bool flashGoIndicator = true) {
        if (checkpointQueue.Count > 0){
            nextCheckpoint = checkpointQueue.Dequeue();
            nextCheckpoint.OnComplete += OnCompleteCheckpoint;
            mainCamera.Unlock();
            if (nextCheckpoint.IsTransitionCheckpoint) {
                OnStartTransition?.Invoke(this, EventArgs.Empty);
            } else {
                if (flashGoIndicator) {
                    UI.Instance.NotifyGoGoGo();
                }
            }
        } else {
            nextCheckpoint = null;
            FadeOutMusic(2, 0);
        }
    }

    public IEnumerator FadeOutMusic(float duration, float targetVolume) {
        float currentTime = 0;
        float start = audioSource.volume;
        while (currentTime < duration) {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        yield break;
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


}
