using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer), typeof(PolygonCollider2D))]
public sealed class PlayerShip : MonoBehaviour
{
    private LineRenderer _lineRenderer;                                 // Renders the ship outline
    private PolygonCollider2D _collider;                                // Collider for ship collision detection
    private Vector2 _velocity;                                          // Current velocity of the ship

    [Header("Ship Shape")]
    [SerializeField]
    private float _shipWidth = 0.5f;                                   // Width of the ship (local space units)

    [SerializeField]
    private float _shipHeight = 0.8f;                                  // Height of the ship (local space units)

    [SerializeField]
    private Material _lineMaterial;                                    // Material for the ship's line renderer

    [Header("Movement")]
    [SerializeField]
    private float _thrust = 5f;                                        // Acceleration force applied when thrusting

    [SerializeField]
    private float _turnSpeed = 180f;                                   // Rotation speed in degrees per second

    [SerializeField]
    private float _maxSpeed = 10f;                                     // Maximum speed the ship can reach

    [Header("Bullets")]
    [SerializeField]
    private Bullet _bulletPrefab;                                      // Prefab for the bullet to shoot

    public Vector2 Velocity { get => _velocity; }                      // Public getter for the ship's velocity

    /// <summary>
    /// Initializes components and draws the ship shape.
    /// </summary>
    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();

        _collider = GetComponent<PolygonCollider2D>();
        _collider.isTrigger = true;

        DrawShip();
    }

    /// <summary>
    /// Redraws the ship shape in the editor when values change.
    /// </summary>
    private void OnValidate()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _collider = GetComponent<PolygonCollider2D>();

        DrawShip();
    }

    /// <summary>
    /// Draws the ship as a triangle using the line renderer and updates the collider shape.
    /// </summary>
    private void DrawShip()
    {
        if (_lineRenderer == null) {
            return;
        }

        _lineRenderer.positionCount = 4;
        _lineRenderer.loop = true;
        _lineRenderer.useWorldSpace = false;
        _lineRenderer.widthMultiplier = 0.05f;
        _lineRenderer.material = _lineMaterial;

        Vector2[] points = new Vector2[4];
        points[0] = new(0, _shipHeight / 2f); // Top
        points[1] = new(-_shipWidth / 2f, -_shipHeight / 2f); // Bottom left
        points[2] = new(_shipWidth / 2f, -_shipHeight / 2f); // Bottom right
        points[3] = points[0]; // Close the loop

        for (int i = 0; i < points.Length; i++) {
            _lineRenderer.SetPosition(i, new Vector3(points[i].x, points[i].y, 0));
        }

        _collider.points = points;
    }

    /// <summary>
    /// Handles player input for movement, rotation, shooting, and screen wrapping.
    /// </summary>
    private void Update()
    {
        if (!Application.isPlaying) {
            return;
        }

        // Rotation input
        float turn = 0f;

        if (Input.GetKey(KeyCode.A)) {
            turn += 1f;
        }

        if (Input.GetKey(KeyCode.D)) {
            turn -= 1f;
        }

        transform.Rotate(0, 0, turn * _turnSpeed * Time.deltaTime);

        // Thrust input
        float thrustInput = 0f;
        if (Input.GetKey(KeyCode.W)) {
            thrustInput += 1f;
        }
        if (Input.GetKey(KeyCode.S)) {
            thrustInput -= 1f;
        }

        if (Mathf.Abs(thrustInput) > 0.01f) {
            Vector2 direction = transform.up;
            _velocity += direction * _thrust * thrustInput * Time.deltaTime;
            _velocity = Vector2.ClampMagnitude(_velocity, _maxSpeed);
        }

        // Move the ship
        transform.position += (Vector3)_velocity * Time.deltaTime;

        // Wrap the ship around the screen edges
        ScreenWrapUtility.WrapTransformToScreenBounds(transform, _collider);
        
        // Shoot bullets (Space key)
        if (Input.GetKeyDown(KeyCode.Space)) {
            Vector3 spawnPos = transform.position + transform.up * (_shipHeight / 2f + 0.1f);
            Bullet bullet = Instantiate(_bulletPrefab, spawnPos, transform.rotation);
            bullet.Initialize(transform.up);
        }
    }

    /// <summary>
    /// Handles collision with asteroids and triggers game over.
    /// </summary>
    /// <param name="collision">The collider the ship has entered.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Asteroid>() != null) {
            GameManager.Instance.GameOver();
        }
    }
}