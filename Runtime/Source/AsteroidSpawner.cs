using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns asteroids based on an interval
/// </summary>
public sealed class AsteroidSpawner : MonoBehaviour
{
    private List<Asteroid> _asteroids;                                  // Holds the current collection of asteroids during play
    private float _timer;                                               // Timer to indicate when to spawn the asteroids

    [Header("Asteroid Prefabs")]
    [SerializeField]
    private GameObject _largeAsteroidPrefab;                            // Prefab for large asteroids

    [SerializeField]
    private GameObject _mediumAsteroidPrefab;                           // Prefab for medium asteroids

    [SerializeField]
    private GameObject _smallAstroidPrefab;                             // Prefab for small asteroids

    [Header("Spawn Timing")]
    [SerializeField]
    private float _minSpawnInterval = 2.0f;                             // Minimum time between spawns

    [SerializeField]
    private float _maxSpawnInterval = 10.0f;                            // Maximum time between spawns

    [SerializeField]
    private float _spawnAcceleration = 0.95f;                           // Each spawn, interval is multiplied by this to speed up spawning

    [SerializeField]
    private float _minIntervalClamp = 0.5f;                             // Never go below this interval

    /// <summary>
    /// Initialize the asteroid list.
    /// </summary>
    private void Awake()
    {
        _asteroids = new();
    }

    /// <summary>
    /// Schedule the first asteroid spawn.
    /// </summary>
    private void Start()
    {
        ScheduleNextSpawn();
    }

    /// <summary>
    /// Handles the spawn timer and triggers asteroid spawning when timer elapses.
    /// </summary>
    private void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0f) {
            SpawnAsteroidOffScreen();
            ScheduleNextSpawn();
        }
    }

    /// <summary>
    /// Clean up all asteroids and detach event handlers when this spawner is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (_asteroids.Count > 0) {
            for (int i = _asteroids.Count - 1; i >= 0; i--) {
                Asteroid asteroid = _asteroids[i];

                if (asteroid != null) {
                    asteroid.AsteroidDestroyed -= OnAsteroidDestroyed;

                    Destroy(asteroid);
                    asteroid = null;
                }
            }

            _asteroids.Clear();
        }
    }

    /// <summary>
    /// Randomly determines the next spawn interval, applies acceleration, and clamps to minimum.
    /// </summary>
    private void ScheduleNextSpawn()
    {
        _timer = Random.Range(_minSpawnInterval, _maxSpawnInterval);
        _timer = Mathf.Max(_timer * _spawnAcceleration, _minIntervalClamp);
    }

    /// <summary>
    /// Spawns a new asteroid just off the edge of the screen, aimed toward a random point on screen.
    /// Ensures asteroids do not spawn too close to each other.
    /// </summary>
    private void SpawnAsteroidOffScreen()
    {
        Camera cam = Camera.main;

        // Choose a random edge (0=left, 1=right, 2=top, 3=bottom)
        int edge = Random.Range(0, 4);
        Vector2 spawnPos = Vector2.zero;

        float camZ = -cam.transform.position.z;
        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0, 0, camZ));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1, 1, camZ));

        // Determine spawn position just outside the chosen edge
        switch (edge) {
            case 0: // Left
                spawnPos = new Vector2(min.x - 1f, Random.Range(min.y, max.y));
                break;
            case 1: // Right
                spawnPos = new Vector2(max.x + 1f, Random.Range(min.y, max.y));
                break;
            case 2: // Top
                spawnPos = new Vector2(Random.Range(min.x, max.x), max.y + 1f);
                break;
            case 3: // Bottom
                spawnPos = new Vector2(Random.Range(min.x, max.x), min.y - 1f);
                break;
        }

        // Pick a random point on the screen to aim the asteroid toward
        Vector3 randomScreenPoint = cam.ViewportToWorldPoint(new Vector3(Random.value, Random.value, 0));
        Vector2 direction = ((Vector2)randomScreenPoint - spawnPos).normalized;

        // If an asteroid is at or near the spawn position, move spawnPos further out in the same direction
        const float minSeparation = 3.0f;
        foreach (Asteroid asteroid in _asteroids) {
            if (Vector2.Distance(spawnPos, asteroid.transform.position) < minSeparation) {
                // Move spawnPos further out along the direction vector
                spawnPos += direction * minSeparation;
            }
        }

        // Choose random weighted size [Large:60%, Medium:30%, Small:10%]
        float rand = Random.value;
        AsteroidSizeType size;

        if (rand < 0.6f)
            size = AsteroidSizeType.Large;
        else if (rand < 0.9f)
            size = AsteroidSizeType.Medium;
        else
            size = AsteroidSizeType.Small;

        InstantiateAsteroid(size, spawnPos, direction);
    }

    /// <summary>
    /// Instantiates an asteroid of the given size at the specified position and direction.
    /// Registers for destruction events and tracks the asteroid.
    /// </summary>
    /// <param name="size">The size of the asteroid to spawn.</param>
    /// <param name="position">The world position to spawn the asteroid.</param>
    /// <param name="direction">The direction the asteroid will travel.</param>
    private void InstantiateAsteroid(AsteroidSizeType size, Vector2 position, Vector2 direction)
    {
        GameObject prefab = size switch {
            AsteroidSizeType.Large => _largeAsteroidPrefab,
            AsteroidSizeType.Medium => _mediumAsteroidPrefab,
            AsteroidSizeType.Small => _smallAstroidPrefab,
            _ => _largeAsteroidPrefab
        };

        GameObject asteroidObj = Instantiate(prefab, position, Quaternion.identity);
        Asteroid asteroid = asteroidObj.GetComponent<Asteroid>();
        asteroid.Initialize(direction, position);

        asteroid.AsteroidDestroyed += OnAsteroidDestroyed;
        _asteroids.Add(asteroid);
    }

    /// <summary>
    /// Handles asteroid destruction event.
    /// Updates score, removes asteroid from tracking, and spawns child asteroids if needed.
    /// </summary>
    /// <param name="asteroid">The asteroid that was destroyed.</param>
    private void OnAsteroidDestroyed(Asteroid asteroid)
    {
        asteroid.AsteroidDestroyed -= OnAsteroidDestroyed;
        _asteroids.Remove(asteroid);

        // Update score based on the destroyed asteroid
        int sizeType = (int)asteroid.Size;
        GameManager.Instance.AddScore(((int)AsteroidSizeType.Large + 1 - sizeType) * 100);

        // Ignore creating any new asteroids if we are at the smallest size
        if (asteroid.Size == AsteroidSizeType.Small) {
            return;
        }

        // Create new asteroids based on the current size
        int numAsteroids = sizeType + 1;
        AsteroidSizeType newSize = asteroid.Size == AsteroidSizeType.Large ? AsteroidSizeType.Medium : AsteroidSizeType.Small;

        for (int i = 0; i < numAsteroids; ++i) {
            float distance = Random.Range(0.5f, 1.5f);
            float angle = (2 * Mathf.PI * i) / numAsteroids; // Evenly spaced angles
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
            Vector2 newPos = (Vector2)asteroid.transform.position + offset;

            Vector2 direction = offset.normalized; // Move away from the center

            InstantiateAsteroid(newSize, newPos, direction);
        }
    }
}