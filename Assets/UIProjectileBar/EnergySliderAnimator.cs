using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class EnergySliderAnimator : MonoBehaviour
{
    public Slider slider;
    public float slideDuration = 0.28f;
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public Color edgeCoreColor = Color.white;
    public Color edgeGlowColor = new Color(0.45f, 0.95f, 1f, 0.65f);
    public Vector2 edgeCoreSize = new Vector2(14f, 20f);
    public Vector2 edgeGlowSize = new Vector2(36f, 28f);
    public int sparkCount = 9;
    public Vector2 sparkSizeRange = new Vector2(2f, 5f);
    public Vector2 sparkSpread = new Vector2(18f, 26f);

    private RectTransform edgeRoot;
    private Image edgeCore;
    private Image edgeGlow;
    private Image[] sparks;
    private Vector2[] sparkSeeds;
    private Coroutine slideCoroutine;
    private float targetValue;
    private Sprite glowSprite;
    private Sprite coreSprite;
    private Sprite sparkSprite;

    void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        targetValue = slider.value;
        EnsureEdgeEffect();
        SetEdgeVisible(false, 0f);
    }

    public void AddValue(float amount)
    {
        if (amount <= 0f)
            return;

        float baseValue = slideCoroutine == null ? slider.value : targetValue;
        AnimateTo(Mathf.Clamp(baseValue + amount, slider.minValue, slider.maxValue));
    }

    public void ResetValue()
    {
        AnimateTo(slider.minValue);
        SetEdgeVisible(false, 0f);
    }

    private void AnimateTo(float newValue)
    {
        targetValue = newValue;

        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        slideCoroutine = StartCoroutine(SlideToTarget(slider.value, targetValue));
    }

    private IEnumerator SlideToTarget(float startValue, float endValue)
    {
        bool increasing = endValue > startValue;
        float elapsed = 0f;

        if (increasing)
            SetEdgeVisible(true, 1f);
        else
            SetEdgeVisible(false, 0f);

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            float eased = slideCurve.Evaluate(t);
            slider.value = Mathf.Lerp(startValue, endValue, eased);

            if (increasing)
                UpdateEdgeEffect(t);

            yield return null;
        }

        slider.value = endValue;

        if (increasing)
            yield return FadeEdgeOut();

        slideCoroutine = null;
    }

    private IEnumerator FadeEdgeOut()
    {
        float elapsed = 0f;
        const float fadeDuration = 0.12f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            SetEdgeVisible(true, alpha);
            yield return null;
        }

        SetEdgeVisible(false, 0f);
    }

    private void EnsureEdgeEffect()
    {
        if (slider.fillRect == null || edgeRoot != null)
            return;

        GameObject rootObject = new GameObject("Fill Edge Effect", typeof(RectTransform));
        edgeRoot = rootObject.GetComponent<RectTransform>();
        edgeRoot.SetParent(slider.fillRect, false);
        edgeRoot.anchorMin = new Vector2(1f, 0.5f);
        edgeRoot.anchorMax = new Vector2(1f, 0.5f);
        edgeRoot.pivot = new Vector2(0.5f, 0.5f);
        edgeRoot.anchoredPosition = Vector2.zero;
        edgeRoot.sizeDelta = edgeGlowSize;

        glowSprite = CreateSoftSprite("SliderEdgeGlow", 96, 72, 0.85f, 1.15f, 2.4f);
        coreSprite = CreateSoftSprite("SliderEdgeCore", 64, 64, 1f, 1f, 2.2f);
        sparkSprite = CreateSoftSprite("SliderEdgeSpark", 32, 32, 1f, 1f, 2.4f);

        edgeGlow = CreateEdgeImage("Glow", edgeRoot, edgeGlowColor, edgeGlowSize, glowSprite);
        edgeCore = CreateEdgeImage("Core", edgeRoot, edgeCoreColor, edgeCoreSize, coreSprite);
        CreateSparks();
    }

    private Image CreateEdgeImage(string name, RectTransform parent, Color color, Vector2 size, Sprite sprite)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;

        Image image = imageObject.GetComponent<Image>();
        image.raycastTarget = false;
        image.sprite = sprite;
        image.color = color;
        return image;
    }

    private void CreateSparks()
    {
        sparks = new Image[Mathf.Max(0, sparkCount)];
        sparkSeeds = new Vector2[sparks.Length];

        for (int i = 0; i < sparks.Length; i++)
        {
            float u = sparks.Length <= 1 ? 0f : i / (float)(sparks.Length - 1);
            float size = Mathf.Lerp(sparkSizeRange.x, sparkSizeRange.y, Mathf.Repeat(u * 2.37f, 1f));
            Image spark = CreateEdgeImage("Spark " + i, edgeRoot, Color.white, new Vector2(size, size), sparkSprite);
            sparks[i] = spark;
            sparkSeeds[i] = new Vector2(Mathf.Repeat(u * 5.19f + 0.21f, 1f), Mathf.Repeat(u * 3.61f + 0.43f, 1f));
        }
    }

    private Sprite CreateSoftSprite(string name, int width, int height, float xWeight, float yWeight, float power)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.name = name + " Texture";
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            float v = ((y + 0.5f) / height) * 2f - 1f;
            for (int x = 0; x < width; x++)
            {
                float u = ((x + 0.5f) / width) * 2f - 1f;
                float distance = Mathf.Sqrt(u * u * xWeight + v * v * yWeight);
                float alpha = Mathf.Pow(Mathf.Clamp01(1f - distance), power);
                pixels[y * width + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = name;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    private void UpdateEdgeEffect(float t)
    {
        if (edgeRoot == null)
            return;

        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 70f);
        float flare = 1f - t;
        edgeRoot.localScale = Vector3.one * Mathf.Lerp(0.95f, 1.12f, pulse * flare);
        edgeRoot.anchoredPosition = new Vector2(Mathf.Lerp(-2f, 1.5f, pulse), 0f);
        UpdateSparks(t, pulse);
        SetEdgeVisible(true, Mathf.Lerp(0.45f, 1f, pulse) * Mathf.Lerp(1f, 0.55f, t));
    }

    private void UpdateSparks(float t, float pulse)
    {
        if (sparks == null)
            return;

        float fade = 1f - t;
        for (int i = 0; i < sparks.Length; i++)
        {
            Image spark = sparks[i];
            if (spark == null)
                continue;

            RectTransform rect = spark.rectTransform;
            Vector2 seed = sparkSeeds[i];
            float phase = Time.time * Mathf.Lerp(18f, 34f, seed.x) + seed.y * 6.28318f;
            float x = Mathf.Lerp(-sparkSpread.x, sparkSpread.x * 0.25f, seed.x) + Mathf.Sin(phase) * 3f;
            float y = Mathf.Lerp(-sparkSpread.y * 0.5f, sparkSpread.y * 0.5f, seed.y) + Mathf.Cos(phase * 1.37f) * 4f;
            rect.anchoredPosition = new Vector2(x * fade, y);
            rect.localScale = Vector3.one * Mathf.Lerp(0.6f, 1.35f, pulse * fade);
            SetImageAlpha(spark, Mathf.Lerp(0.2f, 1f, pulse) * fade);
        }
    }

    private void SetEdgeVisible(bool visible, float alpha)
    {
        if (edgeRoot != null)
            edgeRoot.gameObject.SetActive(visible);

        SetImageAlpha(edgeGlow, alpha * edgeGlowColor.a);
        SetImageAlpha(edgeCore, alpha * edgeCoreColor.a);

        if (sparks == null)
            return;

        for (int i = 0; i < sparks.Length; i++)
            SetImageAlpha(sparks[i], alpha);
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
