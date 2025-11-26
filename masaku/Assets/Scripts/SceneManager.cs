using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    public Button playButton;
    public Button exitButton;
    public Button tutorialButton;
    public Button creditTextButton; 

    [Header("Tutorial System")]
    public GameObject tutorialPanel; 
    public Image tutorialSlideImage; 
    public Sprite[] tutorialSlides; 
    public Button tutorialExitBackButton; 
    public Button tutorialNextFinishButton; 
    public TextMeshProUGUI exitBackButtonText; 
    public TextMeshProUGUI nextFinishButtonText; 

    private int currentSlideIndex = 0;

    [Header("Credits")]
    public GameObject creditsPanel;
    public Sprite creditSprite;
    public Button creditExitButton;
    
    [Header("Lose Panel")]
    public GameObject losePanel; 
    public Button losePanelCloseButton; 
    
    [Header("Audio")]
    public AudioSource audioSource; 
    public AudioClip winSound; 
    public AudioClip loseSound; 
    
    [Header("Cutscene System")]
    public GameObject cutscenePanel; 
    public Image cutsceneBackgroundImage; 
    public Image cutsceneImage; 
    public Sprite[] cutsceneSprites; 
    public Sprite endingCutsceneSprite; 
    public float fadeDuration = 1f; 
    public float displayDuration = 2f; 
    private CanvasGroup cutsceneImageCanvasGroup; 
    public Image backgroundImagePlay;
    public Button skipCutsceneButton;
    private bool skipCutscene = false;
    
    [Header("Best Time Display")]
    public TextMeshProUGUI bestTimeText;
    
    void Start()
    {
        backgroundImagePlay.enabled = false;
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClicked);
        if (tutorialButton != null)
            tutorialButton.onClick.AddListener(OnTutorialButtonClicked);
        if (creditTextButton != null)
            creditTextButton.onClick.AddListener(OnCreditTextClicked);
        
        if (tutorialExitBackButton != null)
            tutorialExitBackButton.onClick.AddListener(OnTutorialExitBackClicked);
        if (tutorialNextFinishButton != null)
            tutorialNextFinishButton.onClick.AddListener(OnTutorialNextFinishClicked);

        if (creditExitButton != null)
            creditExitButton.onClick.AddListener(OnCreditExitClicked);
        
        if (losePanelCloseButton != null)
            losePanelCloseButton.onClick.AddListener(OnLosePanelCloseClicked);
        
        if (skipCutsceneButton != null)
            skipCutsceneButton.onClick.AddListener(OnSkipCutsceneClicked);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
            
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
        
        if (losePanel != null)
            losePanel.SetActive(false);
        
        if (cutscenePanel != null)
        {
            cutscenePanel.SetActive(false);
            
            if (cutsceneBackgroundImage != null)
            {
                cutsceneBackgroundImage.color = Color.black;
            }
            
            if (cutsceneImage != null)
            {
                cutsceneImageCanvasGroup = cutsceneImage.GetComponent<CanvasGroup>();
                if (cutsceneImageCanvasGroup == null)
                    cutsceneImageCanvasGroup = cutsceneImage.gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        if (skipCutsceneButton != null)
            skipCutsceneButton.gameObject.SetActive(false);
        
        UpdateBestTimeDisplay();
        
        if (PlayerPrefs.GetInt("ShowEndingCutscene", 0) == 1)
        {
            PlayerPrefs.DeleteKey("ShowEndingCutscene");
            StartCoroutine(PlayEndingCutscene());
        }
        
        if (PlayerPrefs.GetInt("ShowLosePanel", 0) == 1)
        {
            PlayerPrefs.DeleteKey("ShowLosePanel");
            if (losePanel != null)
            {
                losePanel.SetActive(true);
                if (audioSource != null && loseSound != null)
                    audioSource.PlayOneShot(loseSound);
            }
        }
            
    }
    
    public void OnPlayButtonClicked()
    {
        StartCoroutine(PlayCutsceneSequence());
    }
    
    public void OnExitButtonClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void OnTutorialButtonClicked()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            currentSlideIndex = 0;
            UpdateTutorialSlide();
        }
    }
    
    public void OnTutorialExitBackClicked()
    {
        if (currentSlideIndex == 0)
        {
            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);
        }
        else
        {
            currentSlideIndex--;
            UpdateTutorialSlide();
        }
    }
    
    public void OnTutorialNextFinishClicked()
    {
        bool isLastSlide = (currentSlideIndex == tutorialSlides.Length - 1);
        
        if (isLastSlide)
        {
            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);
        }
        else
        {
            currentSlideIndex++;
            UpdateTutorialSlide();
        }
    }
    
    void UpdateTutorialSlide()
    {
        if (tutorialSlides == null || tutorialSlides.Length == 0)
        {
            return;
        }
        
        if (tutorialSlideImage != null && currentSlideIndex < tutorialSlides.Length)
        {
            tutorialSlideImage.sprite = tutorialSlides[currentSlideIndex];
        }
        
        bool isFirstSlide = (currentSlideIndex == 0);
        bool isLastSlide = (currentSlideIndex == tutorialSlides.Length - 1);
        
        if (exitBackButtonText != null)
        {
            if (isFirstSlide)
                exitBackButtonText.text = "Keluar";
            else
                exitBackButtonText.text = "Kembali";
        }
        
        if (nextFinishButtonText != null)
        {
            if (isLastSlide)
                nextFinishButtonText.text = "Selesai";
            else
                nextFinishButtonText.text = "Berikutnya";
        }
    }
    
    public void OnCreditTextClicked()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }
    
    public void OnCreditExitClicked()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }
    
    public void OnLosePanelCloseClicked()
    {
        if (losePanel != null)
            losePanel.SetActive(false);
    }
    
    public void OnSkipCutsceneClicked()
    {
        skipCutscene = true;
    }
    
    IEnumerator PlayCutsceneSequence()
    {
        backgroundImagePlay.enabled = true;
        skipCutscene = false;
        
        if (cutscenePanel == null || cutsceneImage == null || cutsceneSprites == null || cutsceneSprites.Length == 0)
        {
            DisableAllLightsBeforeLoad();
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
            yield break;
        }
        
        cutscenePanel.SetActive(true);
        
        if (skipCutsceneButton != null)
            skipCutsceneButton.gameObject.SetActive(true);
        
        for (int i = 0; i < cutsceneSprites.Length; i++)
        {
            if (skipCutscene)
                break;
                
            if (cutsceneSprites[i] != null)
            {
                cutsceneImage.sprite = cutsceneSprites[i];
                
                yield return StartCoroutine(FadeCutscene(0f, 1f, fadeDuration));
                
                if (skipCutscene)
                    break;
                
                yield return new WaitForSeconds(displayDuration);
                
                if (skipCutscene)
                    break;
                
                yield return StartCoroutine(FadeCutscene(1f, 0f, fadeDuration));
            }
        }
        
        if (skipCutsceneButton != null)
            skipCutsceneButton.gameObject.SetActive(false);
        
        cutscenePanel.SetActive(false);
        
        DisableAllLightsBeforeLoad();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
    
    void DisableAllLightsBeforeLoad()
    {
        Light[] allLights = FindObjectsOfType<Light>(true);
        foreach (Light light in allLights)
        {
            // Destroy lights completely to ensure they don't carry over
            if (light != null && light.gameObject != null)
            {
                Destroy(light.gameObject);
            }
        }
    }
    
    IEnumerator FadeCutscene(float startAlpha, float endAlpha, float duration)
    {
        if (cutsceneImageCanvasGroup == null) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            cutsceneImageCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }
        
        cutsceneImageCanvasGroup.alpha = endAlpha;
    }
    
    IEnumerator PlayEndingCutscene()
    {
        skipCutscene = false;
        
        if (cutscenePanel == null || cutsceneImage == null || endingCutsceneSprite == null)
        {
            ShowCreditsPanel();
            yield break;
        }
        if (audioSource != null && winSound != null)
        {
            audioSource.PlayOneShot(winSound);
            StartCoroutine(StopSoundAfterDelay(4f));
        }
        
        cutscenePanel.SetActive(true);
        
        if (skipCutsceneButton != null)
            skipCutsceneButton.gameObject.SetActive(true);
        
        cutsceneImage.sprite = endingCutsceneSprite;
        
        yield return StartCoroutine(FadeCutscene(0f, 1f, fadeDuration));
        
        if (!skipCutscene)
            yield return new WaitForSeconds(displayDuration + 1f);
        
        yield return StartCoroutine(FadeCutscene(1f, 0f, fadeDuration));
        
        if (skipCutsceneButton != null)
            skipCutsceneButton.gameObject.SetActive(false);
        
        cutscenePanel.SetActive(false);
        
        ShowCreditsPanel();
    }
    
    void ShowCreditsPanel()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
        }
    }
    
    IEnumerator StopSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
    
    void UpdateBestTimeDisplay()
    {
        if (bestTimeText != null)
        {
            float bestTime = PlayerPrefs.GetFloat("BestTime", -1f);
            
            if (bestTime > 0 && bestTime != float.MaxValue)
            {
                int minutes = Mathf.FloorToInt(bestTime / 60f);
                int seconds = Mathf.FloorToInt(bestTime % 60f);
                bestTimeText.text = string.Format("Best Time: {0:00}:{1:00}", minutes, seconds);
            }
            else
            {
                bestTimeText.text = "Best Time: --:--";
            }
        }
    }
}
