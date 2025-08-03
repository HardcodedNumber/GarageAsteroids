using UnityEngine;

public static class ScreenWrapUtility
{
    /// <summary>
    /// Wraps the object's position around the screen bounds, using the collider's bounds.
    /// </summary>
    /// <param name="transform">The object's transform.</param>
    /// <param name="collider">The collider whose bounds to use.</param>
    public static void WrapTransformToScreenBounds(Transform transform, Collider2D collider)
    {
        if (transform == null || collider == null) {
            return;
        }

        Bounds bounds = collider.bounds;
        Camera cam = Camera.main;
        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
        Vector3 pos = transform.position;
        bool wrapped = false;
        float halfWidth = bounds.extents.x;
        float halfHeight = bounds.extents.y;

        // Only wrap if the entire object is out of bounds
        if (pos.x < min.x - halfWidth) {
            pos.x = max.x + halfWidth;
            wrapped = true;
        }
        else if (pos.x > max.x + halfWidth) {
            pos.x = min.x - halfWidth;
            wrapped = true;
        }
 
        if (pos.y < min.y - halfHeight) {
            pos.y = max.y + halfHeight;
            wrapped = true;
        }
        else if (pos.y > max.y + halfHeight) {
            pos.y = min.y - halfHeight;
            wrapped = true;
        }

        if (wrapped) {
            transform.position = pos;
        }
    }
}