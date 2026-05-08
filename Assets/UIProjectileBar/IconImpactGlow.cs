using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class IconImpactGlow : MonoBehaviour
{
    public Image targetImage;
    public Color fallbackGlowColor = new Color(4f, 0f, 0f, 1f);
    public float fadeInDuration = 0.04f;
    public float fadeOutDuration = 0.45f;
    public Vector3 punchScale = new Vector3(1.08f, 1.08f, 1f);

    private RectTransform rectTransform;
    private RectTransform glowRoot;
    private Image innerGlow;
    private Image outerGlow;
    private Image rimGlow;
    private Coroutine glowCoroutine;
    private Vector3 baseScale;

    void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        rectTransform = targetImage.rectTransform;
        baseScale = rectTransform.localScale;
        EnsureGlowImages();
        SetGlowAlpha(0f);
    }

    void LateUpdate()
    {
        if (glowRoot == null)
            return;

        glowRoot.anchorMin = rectTransform.anchorMin;
        glowRoot.anchorMax = rectTransform.anchorMax;
        glowRoot.pivot = rectTransform.pivot;
        glowRoot.anchoredPosition = rectTransform.anchoredPosition;
        glowRoot.sizeDelta = rectTransform.sizeDelta;
        glowRoot.localRotation = rectTransform.localRotation;
    }

    public void Play(Color color)
    {
        if (glowCoroutine != null)
            StopCoroutine(glowCoroutine);

        if (color.a <= 0.001f)
            color.a = 1f;

        glowCoroutine = StartCoroutine(GlowSequence(color));
    }

    public void Play()
    {
        Play(fallbackGlowColor);
    }

    private IEnumerator GlowSequence(Color color)
    {
        EnsureGlowImages();
        ApplyGlowColor(color);

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            SetGlowAlpha(t);
            rectTransform.localScale = Vector3.Lerp(baseScale, Vector3.Scale(baseScale, punchScale), t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            float alpha = 1f - t;
            SetGlowAlpha(alpha);
            rectTransform.localScale = Vector3.Lerp(Vector3.Scale(baseScale, punchScale), baseScale, t);
            yield return null;
        }

        SetGlowAlpha(0f);
        rectTransform.localScale = baseScale;
        glowCoroutine = null;
    }

    private void EnsureGlowImages()
    {
        if (targetImage == null || glowRoot != null)
            return;

        GameObject rootObject = new GameObject("Impact Glow", typeof(RectTransform));
        glowRoot = rootObject.GetComponent<RectTransform>();
        glowRoot.SetParent(rectTransform.parent, false);
        glowRoot.SetSiblingIndex(rectTransform.GetSiblingIndex());

        outerGlow = CreateGlowImage("Outer Glow", new Vector2(1.32f, 1.32f), 0.36f);
        innerGlow = CreateGlowImage("Inner Glow", new Vector2(1.16f, 1.16f), 0.65f);
        rimGlow = CreateGlowImage("Rim Glow", new Vector2(1.04f, 1.04f), 0.95f);
    }

    private Image CreateGlowImage(string name, Vector2 scale, float alpha)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.SetParent(glowRoot, false);
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;
        imageRect.localScale = new Vector3(scale.x, scale.y, 1f);

        Image image = imageObject.GetComponent<Image>();
        image.raycastTarget = false;
        image.sprite = targetImage.sprite;
        image.type = targetImage.type;
        image.preserveAspect = targetImage.preserveAspect;
        image.material = targetImage.material;
        image.color = new Color(1f, 0f, 0f, alpha);
        return image;
    }

    private void ApplyGlowColor(Color color)
    {
        SetImageColor(outerGlow, color, 0.36f);
        SetImageColor(innerGlow, color, 0.65f);
        SetImageColor(rimGlow, Color.Lerp(Color.white, color, 0.9f), 0.95f);
    }

    private void SetGlowAlpha(float alpha)
    {
        SetImageAlpha(outerGlow, alpha * 0.36f);
        SetImageAlpha(innerGlow, alpha * 0.65f);
        SetImageAlpha(rimGlow, alpha * 0.95f);
    }

    private void SetImageColor(Image image, Color color, float alpha)
    {
        if (image == null)
            return;

        color.a = alpha;
        image.color = color;
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
