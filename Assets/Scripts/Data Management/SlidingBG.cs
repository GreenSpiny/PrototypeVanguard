using UnityEngine;

public class SlidingBG : MonoBehaviour
{
    [SerializeField] Vector2 slideVector;
    [SerializeField] float slideDistance;
    private Vector3 startPosition;

    Vector2 offset;

    private void Awake()
    {
        startPosition = transform.localPosition;
        offset = Vector2.zero;
    }

    private void Update()
    {
        offset.x += slideVector.x * Time.deltaTime;
        offset.y += slideVector.y * Time.deltaTime;
        transform.localPosition = startPosition + new Vector3(offset.x % slideDistance, offset.y % slideDistance);
    }
}
