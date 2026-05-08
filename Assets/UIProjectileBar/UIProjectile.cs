using System.Collections;
using UnityEngine;

public class UIProjectile : MonoBehaviour
{
    public TrailRenderer trail;
    public ParticleSystem sparks;
    public float speed = 2f;
    public float arcHeight = 2f; // Controls how high the curve swoops
    public float arcHeightRandomness = 0.1f;
    public float sideOffsetRandomness = 0.1f;

    public void Fire(Vector3 startPos, Transform target, Color glowColor, System.Action onHit = null)
    {
        // Vector3 camOffset = Camera.main.transform.forward * -0.5f;
        transform.position = startPos; // + camOffset;
        
        trail.Clear();
        trail.emitting = true;

        trail.startColor = glowColor;
        trail.endColor = new Color(glowColor.r, glowColor.g, glowColor.b, 0f);

        var main = sparks.main;
        main.startColor = glowColor;

        sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        float shotArcHeight = arcHeight + Random.Range(-arcHeightRandomness, arcHeightRandomness);
        float shotSideOffset = Random.Range(-sideOffsetRandomness, sideOffsetRandomness);

        StartCoroutine(MoveToTarget(startPos, target, shotArcHeight, shotSideOffset, onHit));
    }

    IEnumerator MoveToTarget(Vector3 start, Transform target, float shotArcHeight, float shotSideOffset, System.Action onHit)
    {
        float t = 0f;
        Vector3 launchDirection = target != null ? target.position - start : Vector3.right;
        Vector3 sideDirection = Vector3.Cross(launchDirection.normalized, Vector3.forward);
        if (sideDirection.sqrMagnitude < 0.001f)
            sideDirection = Vector3.right;
        
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            if (target == null) break;

            // Vector3 end = target.position + camOffset;
            Vector3 end = target.position;
            // Calculate a Bezier curve for a nice arcing swoop
            Vector3 mid = Vector3.Lerp(start, end, 0.5f);
            mid.y += shotArcHeight;
            mid += sideDirection.normalized * shotSideOffset;

            Vector3 m1 = Vector3.Lerp(start, mid, t);
            Vector3 m2 = Vector3.Lerp(mid, end, t);
            transform.position = Vector3.Lerp(m1, m2, t);

            yield return null;
        }

        // --- IMPACT! ---
        if (target != null)
            onHit?.Invoke();
        
        // 1. Stop drawing the trail
        trail.emitting = false;
        
        // 2. Fire the burst explosion!
        sparks.Play();
        
        // 3. Wait for the trail and particles to fade before destroying the object
        float trailFadeTime = trail.time;
        float sparkLifetime = sparks.main.startLifetime.constantMax;
        float destroyDelay = Mathf.Max(trailFadeTime, sparkLifetime);
        
        Destroy(gameObject, destroyDelay);
    }
}
