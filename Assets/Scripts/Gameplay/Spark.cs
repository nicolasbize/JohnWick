using UnityEngine;

public class Spark : MonoBehaviour
{
    public void OnSparkEndFrame() {
        Destroy(gameObject);
    }
}
