using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public sealed class Bullet : MonoBehaviour
{
    private Vector2 _direction;                                         // The normalized direction the bullet will travel
    private LineRenderer _lineRenderer;                                 // Renders the bullet as a line
    private CircleCollider2D _collider;                                 // Collider for hit detection

    [SerializeField]
    private float _speed = 12f;                                         // Bullet movement speed (units per second)

    [SerializeField]
    private float _lifetime = 2f;                                       // Time in seconds before the bullet is destroyed

    [SerializeField]
    private float _lineLength = 0.3f;                                   // Length of the bullet's line visual

    [SerializeField]
    private Material _lineMaterial;                                     // Material used for the line renderer

    /// <summary>
    /// Initializes components and sets up the bullet's visual and collider.
    /// </summary>
    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _collider = GetComponent<CircleCollider2D>();

        _collider.isTrigger = true;
        _collider.radius = _lineLength * 0.5f; // reasonable hitbox for a line

        _lineRenderer.positionCount = 2;
        _lineRenderer.useWorldSpace = false;
        _lineRenderer.widthMultiplier = 0.07f;
        _lineRenderer.material = _lineMaterial;

        // Draw a line from (0,0,0) to (0, lineLength, 0) in local space
        _lineRenderer.SetPosition(0, Vector3.zero);
        _lineRenderer.SetPosition(1, Vector3.up * _lineLength);
    }

    /// <summary>
    /// Sets the bullet's direction and schedules its destruction.
    /// </summary>
    /// <param name="direction">The direction in which the bullet will travel.</param>
    public void Initialize(Vector2 direction)
    {
        _direction = direction.normalized;
        Destroy(gameObject, _lifetime);
    }

    /// <summary>
    /// Moves the bullet forward and rotates it to match its direction.
    /// </summary>
    private void Update()
    {
        transform.position += (Vector3)(_direction * _speed * Time.deltaTime);

        // Keep the line pointing in the direction of travel
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Handles collision with asteroids. Destroys both the bullet and the asteroid on contact.
    /// </summary>
    /// <param name="other">The collider the bullet has entered.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Asteroid>() != null) {
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}