/*
 * ============================================================================
 *  Script   : ReelController.cs
 *  Author   : Niharikaa Agarwal
 *  Project  : Unity Slot Machine Game
 * ----------------------------------------------------------------------------
 *  Brief:
 *  Controls a single slot machine reel. It handles:
 *    - Building a long vertical strip of symbols at runtime
 *    - Clipping the strip so only one symbol is visible (RectMask2D)
 *    - Spinning the strip with smooth acceleration, fast spin and slow stop
 *    - Choosing a random symbol (RNG) for each spin and exposing it
 *      through ResultIndex
 *
 *  GameManager.cs starts the Spin() coroutine on each reel, waits until
 *  IsSpinning is false, then compares the ResultIndex of all three reels.
 *
 *  Hierarchy expected:
 *    Reel (this script + RectMask2D)
 *      -> ReelStrip (empty at edit time, symbols are created at runtime)
 * ============================================================================
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ReelController : MonoBehaviour
{
    #region Inspector References

    [Header("Reel Settings")]
    public Sprite[] symbols;             // All possible symbols for this reel
    public float spinDuration = 2.5f;    // Total time of one spin (seconds)

    #endregion


    #region Public State

    public int ResultIndex { get; private set; }   // Index of the symbol currently shown
    public bool IsSpinning { get; private set; }   // True while the reel is moving

    #endregion


    #region Constants

    private const int TotalSymbols = 50;    
    private const int MiddleIndex = 20;      
    private const int MinTargetIndex = 32;   
    private const int MaxTargetIndex = 39;   

    private const float AccelerationEnd = 0.2f;  
    private const float FastSpinEnd = 0.7f;       
                                                 

    private const float DistanceAfterAcceleration = 0.08f;
    private const float DistanceAfterFastSpin = 0.88f;

    #endregion


    #region Private Fields

    private RectTransform viewport;    
    private RectTransform reelStrip;  
    private Image[] symbolImages;    
    private float symbolHeight;       

    #endregion


    #region Unity Lifecycle

    private IEnumerator Start()
    {
        
        yield return null;
        Canvas.ForceUpdateCanvases();

        viewport = GetComponent<RectTransform>();
        reelStrip = transform.Find("ReelStrip").GetComponent<RectTransform>();

        if (GetComponent<RectMask2D>() == null && GetComponent<Mask>() == null)
            gameObject.AddComponent<RectMask2D>();

        symbolHeight = viewport.rect.height;

        CreateSymbols();
    }

    #endregion


    #region Symbol Setup

    private void CreateSymbols()
    {
        if (symbols == null || symbols.Length == 0)
        {
            Debug.LogError("No symbols assigned to " + gameObject.name);
            return;
        }

        ClearStrip();

        reelStrip.sizeDelta = new Vector2(viewport.rect.width, symbolHeight);
        reelStrip.anchoredPosition = Vector2.zero;

        symbolImages = new Image[TotalSymbols];

        ResultIndex = Random.Range(0, symbols.Length);

        for (int i = 0; i < TotalSymbols; i++)
        {
            GameObject symbolObject = new GameObject(
                "Symbol_" + i,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

            symbolObject.transform.SetParent(reelStrip, false);

            RectTransform rect = symbolObject.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(viewport.rect.width, symbolHeight);

            rect.anchoredPosition = new Vector2(0f, (MiddleIndex - i) * symbolHeight);

            Image image = symbolObject.GetComponent<Image>();
            image.preserveAspect = true;

            image.sprite = (i == MiddleIndex)
                ? symbols[ResultIndex]
                : symbols[Random.Range(0, symbols.Length)];

            symbolImages[i] = image;
        }
    }

    private void ClearStrip()
    {
        for (int i = reelStrip.childCount - 1; i >= 0; i--)
        {
            Destroy(reelStrip.GetChild(i).gameObject);
        }
    }

    private void LayoutSymbols()
    {
        symbolHeight = viewport.rect.height;

        reelStrip.sizeDelta = new Vector2(viewport.rect.width, symbolHeight);
        reelStrip.anchoredPosition = Vector2.zero;

        for (int i = 0; i < TotalSymbols; i++)
        {
            RectTransform rect = symbolImages[i].rectTransform;
            rect.sizeDelta = new Vector2(viewport.rect.width, symbolHeight);
            rect.anchoredPosition = new Vector2(0f, (MiddleIndex - i) * symbolHeight);
        }
    }

    private void RandomizeSymbols()
    {
        for (int i = 0; i < TotalSymbols; i++)
        {
            symbolImages[i].sprite = symbols[Random.Range(0, symbols.Length)];
        }

        symbolImages[MiddleIndex].sprite = symbols[ResultIndex];
    }

    #endregion


    #region Spin Logic

    public IEnumerator Spin()
    {
        if (IsSpinning || symbolImages == null || symbols.Length == 0)
            yield break;

        IsSpinning = true;

        Canvas.ForceUpdateCanvases();
        LayoutSymbols();

        RandomizeSymbols();

        int newResult = Random.Range(0, symbols.Length);
        int targetIndex = Random.Range(MinTargetIndex, MaxTargetIndex);

        symbolImages[targetIndex].sprite = symbols[newResult];

        float startY = 0f;
        float targetY = (targetIndex - MiddleIndex) * symbolHeight;

        float elapsed = 0f;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / spinDuration);
            float movementProgress = GetMovementProgress(progress);

            float currentY = Mathf.Lerp(startY, targetY, movementProgress);
            reelStrip.anchoredPosition = new Vector2(0f, currentY);

            yield return null;
        }

        reelStrip.anchoredPosition = new Vector2(0f, targetY);

        ResultIndex = newResult;

        IsSpinning = false;
    }

    private float GetMovementProgress(float progress)
    {
        if (progress < AccelerationEnd)
        {
            float t = progress / AccelerationEnd;
            return DistanceAfterAcceleration * t * t;
        }

        if (progress < FastSpinEnd)
        {
            float t = (progress - AccelerationEnd) / (FastSpinEnd - AccelerationEnd);
            return Mathf.Lerp(DistanceAfterAcceleration, DistanceAfterFastSpin, t);
        }

        float stopT = (progress - FastSpinEnd) / (1f - FastSpinEnd);
        stopT = Mathf.SmoothStep(0f, 1f, stopT);
        return Mathf.Lerp(DistanceAfterFastSpin, 1f, stopT);
    }

    #endregion
}