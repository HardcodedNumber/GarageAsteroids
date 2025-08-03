using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Represents the size classification of an asteroid.
/// </summary>
public enum AsteroidSizeType
{
    Small,
    Medium,
    Large,
}

[RequireComponent(typeof(LineRenderer), typeof(PolygonCollider2D), typeof(Rigidbody2D))]
public sealed class Asteroid : MonoBehaviour
{
    /// <summary>
    /// Event occurs when the asteroid is destroyed
    /// </summary>
    public event OnAsteroidDestroyed AsteroidDestroyed;
    public delegate void OnAsteroidDestroyed(Asteroid asteroid);

    private Rigidbody2D _rigidBody;                                         // Holds a reference to the rigidbody
    private LineRenderer _lineRenderer;                                     // LineRenderer is used to draw the asteroid
    private PolygonCollider2D _collider;                                    // Collider to detect collision
    private int _vertexCount;                                               // Total number of vertices
    private float _radius;                                                  // How wide is the asteroid
    private float _jaggedness;                                              // Current value of how rough the asteroid is
    private float _speed;                                                   // Current speed

    [SerializeField]
    private AsteroidSizeType _size;                                         // Predefined size of the asteroid

    [SerializeField]
    private Material _lineMaterial;                                         // The material to use to draw the line for the asteroid

    [SerializeField]
    private int _minVertexCount = 8;                                        // Min-Max number of vertices to represent an asteroid

    [SerializeField]
    private int _maxVertexCount = 12;

    [SerializeField]
    private float _minRadius = 0.5f;                                        // Min-Max radius to determine how big an asteroid

    [SerializeField]
    private float _maxRadius = 1f;

    [SerializeField, Range(0f, 1f)]                                         // Min-Max roughness of an asteroid
    private float _minJaggedness = 0.4f;

    [SerializeField, Range(0f, 1f)]
    private float _maxJaggedness = 1f;

    [SerializeField]
    private float _minSpeed = 0.5f;                                         // Min-Max how fast the asteroid can move

    [SerializeField]
    private float _maxSpeed = 1f;

    /// <summary>
    /// Getter for the current size of the asteroid
    /// </summary>
    public AsteroidSizeType Size => _size;

    /// <summary>
    /// Since we use <see cref="RequireComponent"/>, we can guarantee that the
    /// Components are available to acquire. Using <see cref="Component.GetComponent{T}()"/> once,
    /// this will save us doing costly lookups throughout the life cycle of the object
    /// </summary>
    public void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _collider = GetComponent<PolygonCollider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();

        // Enforce that there is no gravity and no drag
        _rigidBody.gravityScale = 0f;
        _rigidBody.angularDamping = 0f;
        _rigidBody.linearDamping = 0f;
    }

    private void Update()
    {
        // Update the position of the asteroid based on its current velocity and time
        transform.position += (Vector3)_rigidBody.linearVelocity * Time.deltaTime;

        // If the position causes the asteroid to go off screen, move the asteroid to the opposite side
        ScreenWrapUtility.WrapTransformToScreenBounds(transform, _collider);
    }

    private void OnDestroy()
    {
        // Notify that the asteroid has been destroyed, IFF the player is in a game
        if (gameObject.scene.isLoaded) {
            AsteroidDestroyed?.Invoke(this);
        }
    }

    private void OnDrawGizmos()
    {
        // Since OnDrawGizmos happens in the editor even when the game isn't running,
        // Ensure that we have a rigidbody
        if (_rigidBody != null) {
            Vector3 start = transform.position;
            Vector3 end = start + (Vector3)_rigidBody.linearVelocity.normalized;

            Gizmos.color = Color.yellow;

            // Draw a line from the asteroid's current position to the direction of travel
            Gizmos.DrawLine(start, end);
        }
    }

    /// <summary>
    /// Initialize the asteroid
    /// </summary>
    /// <param name="size">size of the asteroid to create</param>
    /// <param name="direction">direction of travel</param>
    /// <param name="spawnPosition">position to spawn the asteroid</param>
    public void Initialize(
        Vector2 direction,
        Vector2 spawnPosition)
    {
        // Set properties from the serialized field properties
        _vertexCount = Random.Range(_minVertexCount, _maxVertexCount);
        _radius = Random.Range(_minRadius, _maxRadius);
        _jaggedness = Random.Range(_minJaggedness, _maxJaggedness);
        _speed = Random.Range(_minSpeed, _maxSpeed);
        _lineRenderer.material = _lineMaterial;

        GenerateAsteroidShape();

        // Update collider shape
        Vector2[] points = new Vector2[_vertexCount];
        for (int i = 0; i < _vertexCount; i++) {
            points[i] = _lineRenderer.GetPosition(i);
        }
        _collider.points = points;

        // Set velocity
        _rigidBody.linearVelocity = direction.normalized * _speed;

        // update the position to where it should be spawned
        transform.position = spawnPosition;
    }

    /// <summary>
    /// Reset the line renderer
    /// </summary>
    public void ClearRenderer()
    {
        if (_lineRenderer != null) {
            _lineRenderer.positionCount = 0;
        }

        if (_collider != null) {
            _collider.points = new Vector2[0];
        }
    }

    /// <summary>
    /// Update the line renderer to match the configuration 
    /// </summary>
    private void GenerateAsteroidShape()
    {
        _lineRenderer.positionCount = _vertexCount + 1;
        _lineRenderer.loop = true;
        _lineRenderer.useWorldSpace = false;
        _lineRenderer.widthMultiplier = 0.05f;

        for (int i = 0; i < _vertexCount; i++) {
            float angle = (2 * Mathf.PI / _vertexCount) * i;
            float randomRadius = _radius * (1 + Random.Range(-_jaggedness, _jaggedness));
            Vector3 point = new(
                Mathf.Cos(angle) * randomRadius,
                Mathf.Sin(angle) * randomRadius,
                0f
            );
            _lineRenderer.SetPosition(i, point);
        }

        // Close the loop
        _lineRenderer.SetPosition(_vertexCount, _lineRenderer.GetPosition(0));

        // Update the name of the asteroid
        name = $"Asteroid {Size}";
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Calculate the reflection of the velocity based on the collision normal
        ContactPoint2D[] points = new ContactPoint2D[10];
        collision.GetContacts(points);
        Vector2 normal = points[0].normal;

        // Bounce off the surface by flipping our movement direction based on the collision angle
        Vector3 direction = _rigidBody.linearVelocity.normalized;
        _rigidBody.linearVelocity = Vector2.Reflect(direction, normal) * _speed;
    }
}
