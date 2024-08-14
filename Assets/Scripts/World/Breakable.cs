using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    [SerializeField] private Transform cleanObjectSprite;
    [SerializeField] private Transform brokenObjectSprite;
    [SerializeField] private Transform bottomFixedSprite;
    [SerializeField] private Pickable pickablePrefab;

    private bool broken = false;
    private Vector2 precisePosition;
    private float height = 5; // start top off
    private Vector2 velocity = Vector2.zero;
    private float dzHeight = 2f;
    private float gravity = 10f;
    private float intensity = 20f;
    private int nbBouncesLeft = 3;

    // Start is called before the first frame update
    void Start()
    {
        precisePosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (broken) {
            dzHeight -= gravity * Time.deltaTime;
            height += dzHeight;
            precisePosition += velocity * Time.deltaTime;
            brokenObjectSprite.transform.position = new Vector3(Mathf.FloorToInt(precisePosition.x), Mathf.FloorToInt(precisePosition.y + height), Mathf.FloorToInt(precisePosition.y));
            transform.gameObject.layer = LayerMask.NameToLayer("Pickable");
            if (height <= 0 && nbBouncesLeft > 0) {
                dzHeight = nbBouncesLeft / 2;
                nbBouncesLeft -= 1;
                brokenObjectSprite.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, .5f + nbBouncesLeft * 0.2f);
            } else if (height <= 0 && nbBouncesLeft <= 0) {
                broken = false;
                height = 0;
                Destroy(gameObject);
            }

        }
    }

    public void Break(Vector2 hitPosition) {
        if (!broken) {
            broken = true;
            cleanObjectSprite.GetComponent<SpriteRenderer>().enabled = false;
            bottomFixedSprite.GetComponent<SpriteRenderer>().enabled = true;
            brokenObjectSprite.GetComponent<SpriteRenderer>().enabled = true;
            velocity = new Vector2(transform.position.x - hitPosition.x, 0).normalized * intensity;
            if (pickablePrefab != null) {
                Pickable pickable = Instantiate<Pickable>(pickablePrefab, transform.parent);
                pickable.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y);
            }
        }
    }
}
