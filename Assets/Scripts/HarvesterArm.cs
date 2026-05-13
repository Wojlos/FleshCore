using UnityEngine;

public class HarvesterArm : MonoBehaviour
{
    [Tooltip("The saw blade transform (target end of the arm)")]
    public Transform sawTarget;

    [Tooltip("The building/anchor transform (fixed end of the arm)")]
    public Transform buildingAnchor;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (sawTarget == null || buildingAnchor == null) return;

        // Position the arm at the building anchor
        transform.position = buildingAnchor.position;

        // Direction and distance from building to saw
        Vector2 direction = (Vector2)(sawTarget.position - buildingAnchor.position);
        float distance = direction.magnitude;

        // Rotate to face the saw (subtract 90 because the arm extends along local Y)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        // Stretch the tiled sprite to cover the distance along local Y (height)
        spriteRenderer.size = new Vector2(spriteRenderer.size.x, distance);
    }
}
