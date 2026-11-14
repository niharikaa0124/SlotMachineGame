/*
 * ============================================================================
 *  Script   : GameManager.cs
 *  Author   : Niharikaa Agarwal
 *  Project  : Unity Slot Machine Game
 * ----------------------------------------------------------------------------
 *  Brief:
 *  Controls the main flow of the slot machine game. It handles:
 *    - Spinning the three reels one after another
 *    - Checking the result (win when all three symbols match)
 *    - Calculating payouts and updating the balance
 *    - Animating the balance counting up after a win
 *    - Showing the Win panel (with coin rain) and the Game Over panel
 *    - Playing sound effects (spin button, spinning loop, win, coin drop,
 *      balance counting, lose, continue)
 *    - Playing background music that loops all the time. Its volume fades
 *      down only while the Win panel is open, then returns to normal.
 *
 *  Works together with ReelController.cs, which animates each reel and
 *  decides which symbol it stops on, and CoinShower.cs, which makes the
 *  coins rain down on a win.
 * ============================================================================
 */

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region Inspector References

    [Header("Reels")]
    public ReelController[] reels;

    [Header("UI Text")]
    public TMP_Text balanceText;
    public TMP_Text winText;
    public TMP_Text gameOverText;

    [Header("Buttons")]
    public UnityEngine.UI.Button spinButton;
    public UnityEngine.UI.Button continueButton;
    public UnityEngine.UI.Button retryButton;

    [Header("Panels")]
    public GameObject winPanel;
    public GameObject gameOverPanel;

    [Header("Effects")]
    public CoinShower coinShower;

    [Header("Balance Animation")]
    public float countDuration = 1.5f;   

    #endregion


    #region Audio

    [Header("Audio Sources")]
    public AudioSource sfxSource;        
    public AudioSource spinLoopSource;   
    public AudioSource countSource;      
    public AudioSource musicSource;     

    [Header("Audio Clips")]
    public AudioClip spinButtonClip;     
    public AudioClip continueButtonClip; 
    public AudioClip spinLoopClip;       
    public AudioClip winClip;            
    public AudioClip coinDropClip;       
    public AudioClip countLoopClip;      
    public AudioClip loseClip;           
    public AudioClip musicClip;         

    [Header("Background Music Volume")]
    [Range(0f, 1f)] public float musicNormalVolume = 0.6f; 
    [Range(0f, 1f)] public float musicDuckedVolume = 0.15f; 
    public float musicFadeTime = 0.5f;                      

    #endregion


    #region Game Settings

    private const int StartingBalance = 1000;
    private const int BetAmount = 50;
    private const float ReelStartDelay = 0.2f; 

    private readonly int[] payouts = { 2, 3, 5, 10 };

    #endregion


    #region Game State

    private int balance = StartingBalance;
    private bool isSpinning = false;
    private Coroutine balanceRoutine;     
    private Coroutine musicFadeRoutine;   

    #endregion


    #region Unity Lifecycle

    private void Start()
    {
        Time.timeScale = 1f;

        winPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        spinButton.interactable = true;

        SetupLoopAudio(spinLoopSource, spinLoopClip);
        SetupLoopAudio(countSource, countLoopClip);
        SetupMusic();

        UpdateBalanceUI();
    }

    #endregion


    #region Spin Logic


    public void SpinAll()
    {
        
        TryStartMusic();

        
        if (isSpinning || balance <= 0 || balance < BetAmount)
            return;

        PlaySfx(spinButtonClip);

        balance -= BetAmount;
        UpdateBalanceUI();

        spinButton.interactable = false;

        StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        isSpinning = true;
        spinButton.interactable = false;

        StartLoop(spinLoopSource, spinLoopClip);

        for (int i = 0; i < reels.Length; i++)
        {
            StartCoroutine(reels[i].Spin());

            if (i < reels.Length - 1)
                yield return new WaitForSeconds(ReelStartDelay);
        }

        while (AnyReelSpinning())
        {
            yield return null;
        }

        StopLoop(spinLoopSource);

        CheckWin();

        isSpinning = false;
    }

    private bool AnyReelSpinning()
    {
        foreach (ReelController reel in reels)
        {
            if (reel.IsSpinning)
                return true;
        }

        return false;
    }

    #endregion


    #region Win / Lose Logic

    private void CheckWin()
    {
        bool isWinner =
            reels[0].ResultIndex == reels[1].ResultIndex &&
            reels[1].ResultIndex == reels[2].ResultIndex;

        if (isWinner)
        {
            ShowWin();
        }
        else
        {
            UpdateBalanceUI();

            if (balance < BetAmount)
                ShowGameOver();
            else
                spinButton.interactable = true;
        }
    }

    private void ShowWin()
    {
        int symbolIndex = reels[0].ResultIndex;
        int reward = BetAmount * payouts[symbolIndex];

        int oldBalance = balance;
        balance += reward;

        winText.text = "JACKPOT!\nYOU WON " + reward + " COINS!";

        FadeMusic(musicDuckedVolume);

        PlaySfx(winClip);
        PlaySfx(coinDropClip);

        if (coinShower != null)
            coinShower.Play();

        AnimateBalance(oldBalance, balance);

        winPanel.SetActive(true);
        spinButton.interactable = false;

        Time.timeScale = 0f; 
    }

    private void ShowGameOver()
    {
        gameOverText.text = "POINTS FINISHED!\nRETRY?";

        FadeMusic(musicDuckedVolume);

        PlaySfx(loseClip);

        gameOverPanel.SetActive(true);
        spinButton.interactable = false;

        Time.timeScale = 0f; 
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;

        balance = StartingBalance;
        UpdateBalanceUI();

        FadeMusic(musicNormalVolume);

        gameOverPanel.SetActive(false);
        spinButton.interactable = true;

        isSpinning = false;
    }

    #endregion


    #region Button Callbacks

    public void ContinueGame()
    {
        PlaySfx(continueButtonClip);

        UpdateBalanceUI();

        FadeMusic(musicNormalVolume);

        Time.timeScale = 1f;

        winPanel.SetActive(false);
        spinButton.interactable = true;
    }

    #endregion


    #region Audio Helpers

    private void SetupLoopAudio(AudioSource source, AudioClip clip)
    {
        if (source == null)
            return;

        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
    }

    private void StartLoop(AudioSource source, AudioClip clip)
    {
        if (source != null && clip != null)
            source.Play();
    }

    private void StopLoop(AudioSource source)
    {
        if (source != null)
            source.Stop();
    }

    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    private void SetupMusic()
    {
        if (musicClip == null)
        {
            Debug.LogWarning("GameManager: Music Clip is empty. Drag your music file into it.");
            return;
        }

        AudioListener.pause = false;
        AudioListener.volume = 1f;

        if (FindFirstObjectByType<AudioListener>() == null)
            Debug.LogWarning("GameManager: No Audio Listener in the scene. Add one to the Main Camera.");

        if (musicSource != null)
        {
            musicSource.playOnAwake = false;
            musicSource.Stop();
        }

        if (musicNormalVolume < 0.05f)
        {
            Debug.LogWarning("GameManager: Music Normal Volume was 0, using 0.6 instead.");
            musicNormalVolume = 0.6f;
        }

        GameObject musicObject = new GameObject("BackgroundMusic");
        musicObject.transform.SetParent(transform);
        musicSource = musicObject.AddComponent<AudioSource>();

        if (musicClip.loadState == AudioDataLoadState.Unloaded)
            musicClip.LoadAudioData();

        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.mute = false;
        musicSource.spatialBlend = 0f;
        musicSource.priority = 0;                  
        musicSource.ignoreListenerPause = true;
        musicSource.volume = musicNormalVolume;
        musicSource.Play();

        Debug.Log("Music: playing=" + musicSource.isPlaying +
                  ", volume=" + musicSource.volume +
                  ", clip=" + musicClip.name +
                  ", length=" + musicClip.length + "s");
    }

    private void TryStartMusic()
    {
        if (musicSource != null && musicClip != null && !musicSource.isPlaying)
            musicSource.Play();
    }

    private void FadeMusic(float targetVolume)
    {
        if (musicSource == null)
            return;

        if (musicFadeRoutine != null)
            StopCoroutine(musicFadeRoutine);

        musicFadeRoutine = StartCoroutine(FadeMusicRoutine(targetVolume));
    }

    private IEnumerator FadeMusicRoutine(float targetVolume)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < musicFadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / musicFadeTime);
            yield return null;
        }

        musicSource.volume = targetVolume;
        musicFadeRoutine = null;
    }

    #endregion


    #region UI Helpers

    private void UpdateBalanceUI()
    {
        if (balanceRoutine != null)
        {
            StopCoroutine(balanceRoutine);
            balanceRoutine = null;
        }

        StopLoop(countSource);

        balanceText.text = "BALANCE: " + balance;
    }

    private void AnimateBalance(int from, int to)
    {
        if (balanceRoutine != null)
            StopCoroutine(balanceRoutine);

        balanceRoutine = StartCoroutine(CountBalanceRoutine(from, to));
    }

    private IEnumerator CountBalanceRoutine(int from, int to)
    {
        StartLoop(countSource, countLoopClip);

        float elapsed = 0f;

        while (elapsed < countDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / countDuration);
            t = 1f - (1f - t) * (1f - t);

            int shown = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
            balanceText.text = "BALANCE: " + shown;

            yield return null;
        }

        balanceText.text = "BALANCE: " + to;

        StopLoop(countSource);
        balanceRoutine = null;
    }

    #endregion
}