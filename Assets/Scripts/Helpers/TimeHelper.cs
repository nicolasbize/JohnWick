using UnityEngine;

public class TimeHelper : MonoBehaviour {

    [SerializeField] private float durationTimeStop = 0.1f;

    private float timeSinceTimeStop = float.NegativeInfinity;
    private bool timeHalted = false;

    public static TimeHelper Instance;

    private void Awake() {
        Instance = this;
    }

    public void StopTime() {
        timeSinceTimeStop = Time.realtimeSinceStartup;
        Time.timeScale = 0f;
        timeHalted = true;
    }


    private void Update() {
        if (timeHalted && (Time.realtimeSinceStartup - timeSinceTimeStop > durationTimeStop)) {
            Time.timeScale = 1f;
            timeHalted = false;
        }
    }


}
