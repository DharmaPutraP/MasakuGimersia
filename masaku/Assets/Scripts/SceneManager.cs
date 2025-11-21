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
    public Button creditTextButton; // Credit text that acts as button

    [Header("Tutorial System")]
    public GameObject tutorialPanel; // Main tutorial panel
    public Image tutorialSlideImage; // Image component to display slides
    public Sprite[] tutorialSlides; // Array of 10 tutorial slide images
    public Button tutorialExitBackButton; // Button that acts as Exit (index 0) or Back (index 1+)
    public Button tutorialNextFinishButton; // Button that acts as Next (index 0 to last-1) or Finish (last)
    public TextMeshProUGUI exitBackButtonText; // Text component for Exit/Back button
    public TextMeshProUGUI nextFinishButtonText; // Text component for Next/Finish button

    private int currentSlideIndex = 0;

    [Header("Credits")]
    public GameObject creditsPanel;
    public Sprite creditSprite;
    public Button creditExitButton;
    
    [Header("Lose Panel")]
    public GameObject losePanel; // Panel shown when player loses
    public Button losePanelCloseButton; // Button to close lose panel
    
    [Header("Audio")]
    public AudioSource audioSource; // AudioSource for sound effects
    public AudioClip winSound; // Sound effect when player wins
    public AudioClip loseSound; // Sound effect when player loses
    
    [Header("Cutscene System")]
    public GameObject cutscenePanel; // Panel for cutscene
    public Image cutsceneBackgroundImage; // Black background image
    public Image cutsceneImage; // Image component to display cutscene sprites
    public Sprite[] cutsceneSprites; // Array of 3 cutscene sprites (opening)
    public Sprite endingCutsceneSprite; // Single ending cutscene sprite
    public float fadeDuration = 1f; // Duration of fade in/out
    public float displayDuration = 2f; // How long each sprite stays visible
    private CanvasGroup cutsceneImageCanvasGroup; // CanvasGroup for the image only, not the background
    public Image backgroundImagePlay;
    
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
    
    IEnumerator PlayCutsceneSequence()
    {
        backgroundImagePlay.enabled = true;
        if (cutscenePanel == null || cutsceneImage == null || cutsceneSprites == null || cutsceneSprites.Length == 0)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
            yield break;
        }
        
        cutscenePanel.SetActive(true);
        
        for (int i = 0; i < cutsceneSprites.Length; i++)
        {
            if (cutsceneSprites[i] != null)
            {
                cutsceneImage.sprite = cutsceneSprites[i];
                
                yield return StartCoroutine(FadeCutscene(0f, 1f, fadeDuration));
                
                yield return new WaitForSeconds(displayDuration);
                
                yield return StartCoroutine(FadeCutscene(1f, 0f, fadeDuration));
            }
        }
        
        cutscenePanel.SetActive(false);
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
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
        
        cutsceneImage.sprite = endingCutsceneSprite;
        
        yield return StartCoroutine(FadeCutscene(0f, 1f, fadeDuration));
        
        yield return new WaitForSeconds(displayDuration + 1f);
        
        yield return StartCoroutine(FadeCutscene(1f, 0f, fadeDuration));
        
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
}
