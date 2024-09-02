using UnityEngine;

public class TimeHelper : MonoBehaviour {

    [SerializeField] private float durationTimeStop = 0.1f;

    private float timeSinceTimeStop = float.NegativeInfinity;

    public static TimeHelper Instance;

    private void Awake() {
        Instance = this;
    }

    public void StopTime() {
        timeSinceTimeStop = Time.realtimeSinceStartup;
        Time.timeScale = 0f;
    }


    private void Update() {
        if (Time.timeScale == 0f && (Time.realtimeSinceStartup - timeSinceTimeStop > durationTimeStop)) {
            Time.timeScale = 1f;
        }
    }


}
