using UnityEngine;

/// <summary>
/// Renders and animates a field of stars with parallax and fading effects.
/// Stars are randomly positioned, sized, and faded in/out for a dynamic background.
/// </summary>
public sealed class Starfield : MonoBehaviour
{
    private Star[] _stars;                                              // Array holding all star objects in the field
    private Transform _cameraTransform;                                 // Reference to the main camera's transform

    [SerializeField]
    private PlayerShip _player;                                         // Reference to the player ship for parallax effect

    [SerializeField]
    private int _starCount = 100;                                       // Number of stars to generate

    [SerializeField]
    private Vector2 _fieldSize = new(30f, 20f);                         // Size of the starfield area (width, height)

    [SerializeField]
    private float _minStarSize = 0.03f;                                 // Minimum scale for a star

    [SerializeField]
    private float _maxStarSize = 0.12f;                                 // Maximum scale for a star

    [SerializeField]
    private float _minFadeTime = 1f;                                    // Minimum time for a star to fade in/out

    [SerializeField]
    private float _maxFadeTime = 3f;                                    // Maximum time for a star to fade in/out

    [SerializeField]
    private Sprite[] _starSprites;                                      // Array of possible sprites for stars

    [SerializeField]
    private Color _starColor = Color.white;                             // Base color for all stars

    [SerializeField]
    private float _parallax = 0.2f;                                     // How much stars move relative to player (parallax factor)

    /// <summary>
    /// Caches the camera transform for use in star repositioning.
    /// </summary>
    private void Awake()
    {
        _cameraTransform = Camera.main.transform;
    }

    /// <summary>
    /// Initializes the starfield by creating and configuring all star GameObjects.
    /// </summary>
    private void Start()
    {
        _stars = new Star[_starCount];

        for (int i = 0; i < _starCount; i++) {
            GameObject starGO = new("Star");
            starGO.transform.parent = transform;

            SpriteRenderer spriteRenderer = starGO.AddComponent<SpriteRenderer>();
            
            int randomSpriteIndex = Random.Range(0, _starSprites.Length);
            spriteRenderer.sprite = _starSprites[randomSpriteIndex];
            spriteRenderer.color = new Color(_starColor.r, _starColor.g, _starColor.b, 1f); // Start visible

            float size = Random.Range(_minStarSize, _maxStarSize);
            starGO.transform.localScale = Vector3.one * size;

            Vector3 pos = new(
                Random.Range(-_fieldSize.x / 2f, _fieldSize.x / 2f),
                Random.Range(-_fieldSize.y / 2f, _fieldSize.y / 2f)
            );
            starGO.transform.localPosition = pos;

            _stars[i] = new Star {
                go = starGO,
                renderer = spriteRenderer,
                fadeTime = Random.Range(_minFadeTime, _maxFadeTime),
                fadeTimer = Random.Range(0f, 1f),
                fadingIn = Random.value > 0.5f,
                basePosition = pos,
                parallaxFactor = Random.Range(0.5f, 1.5f) * _parallax
            };
        }
    }

    /// <summary>
    /// Updates star positions for parallax, animates fading, and handles star respawning.
    /// </summary>
    private void Update()
    {
        Vector2 playerDelta = Vector2.zero;

        // Use the ship's velocity for parallax
        if (_player != null) {
            Vector2 velocity = _player.Velocity;
            playerDelta = -velocity * Time.deltaTime;
        }

        foreach (Star star in _stars) {
            // Parallax movement
            Vector2 parallaxOffset = playerDelta * star.parallaxFactor;
            star.go.transform.localPosition += (Vector3)parallaxOffset;

            // Fade in/out animation
            star.fadeTimer += Time.deltaTime;

            float t = Mathf.Clamp01(star.fadeTimer / star.fadeTime);
            float alpha = star.fadingIn ? t : 1f - t;
            Color color = star.renderer.color;

            color.a = alpha;
            star.renderer.color = color;

            if (star.fadeTimer >= star.fadeTime) {
                star.fadingIn = !star.fadingIn;
                star.fadeTimer = 0f;

                if (!star.fadingIn) {
                    // Reposition and randomize star when it fades out
                    star.basePosition = new Vector3(
                        Random.Range(-_fieldSize.x / 2f, _fieldSize.x / 2f),
                        Random.Range(-_fieldSize.y / 2f, _fieldSize.y / 2f)
                    ) + 
                    _cameraTransform.localPosition;
                    star.go.transform.localPosition = star.basePosition;

                    int randomSpriteIndex = Random.Range(0, _starSprites.Length);
                    star.renderer.sprite = _starSprites[randomSpriteIndex];
                }
            }
        }
    }

    /// <summary>
    /// Represents a single star in the starfield, with properties for animation and parallax.
    /// </summary>
    private sealed class Star
    {
        public GameObject go;                                           // The GameObject representing the star
        public SpriteRenderer renderer;                                 // SpriteRenderer for the star's visual
        public float fadeTime;                                          // Duration of the fade in/out animation
        public float fadeTimer;                                         // Current timer for fading
        public bool fadingIn;                                           // Whether the star is currently fading in
        public Vector2 basePosition;                                    // The original position of the star (for parallax)
        public float parallaxFactor;                                    // How much this star is affected by parallax
    }
}