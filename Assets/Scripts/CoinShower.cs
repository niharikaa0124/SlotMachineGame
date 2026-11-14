/*
 *  Script  : CoinShower.cs
 *  Author  : Niharikaa Agarwal
 *  Brief   : Makes coins rain down from the top of the screen, spread
 *            across the full width, when the player wins. Uses unscaled
 *            time so it still plays while Time.timeScale = 0.
 *            Attach to an empty object that stretches over the Canvas.
 */

using System.Collections;
using UnityEngine;

public class CoinShower : MonoBehaviour
{
    [Header("References")]
    public RectTransform coinPrefab;     
    [Header("Rain Settings")]
    public int coinCount = 60;           
    public float rainDuration = 1.8f;      
    public float minFallSpeed = 600f;     
    public float maxFallSpeed = 1100f;
    public float gravity = 900f;         
    public float sideDrift = 80f;          
    public float maxSpin = 300f;           
    public Vector2 coinScaleRange = new Vector2(0.7f, 1.2f);

    private RectTransform area;

    private void Awake()
    {
        area = GetComponent<RectTransform>();
    }

    public void Play()
    {
        for (int i = 0; i < coinCount; i++)
            StartCoroutine(DropCoin());
    }

    private IEnumerator DropCoin()
    {
        yield return new WaitForSecondsRealtime(Random.Range(0f, rainDuration));

        RectTransform coin = Instantiate(coinPrefab, transform);
        coin.gameObject.SetActive(true);

        coin.anchorMin = coin.anchorMax = new Vector2(0.5f, 0.5f);

        float width = area.rect.width;
        float height = area.rect.height;

        float scale = Random.Range(coinScaleRange.x, coinScaleRange.y);
        coin.localScale = Vector3.one * scale;

        float startX = Random.Range(-width / 2f, width / 2f);
        float startY = height / 2f + coin.rect.height;
        coin.anchoredPosition = new Vector2(startX, startY);

        float fallSpeed = Random.Range(minFallSpeed, maxFallSpeed);
        float drift = Random.Range(-sideDrift, sideDrift);
        float spin = Random.Range(-maxSpin, maxSpin);

        while (coin.anchoredPosition.y > -height / 2f - coin.rect.height)
        {
            float dt = Time.unscaledDeltaTime;

            fallSpeed += gravity * dt;

            coin.anchoredPosition += new Vector2(drift * dt, -fallSpeed * dt);
            coin.Rotate(0f, 0f, spin * dt);

            yield return null;
        }

        Destroy(coin.gameObject);
    }
}