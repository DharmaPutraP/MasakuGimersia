using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScreenFade : MonoBehaviour
{
    public static ScreenFade Instance;

    [Header("Fade Settings")]
    public Image fadeImage; // Black overlay image
    
    public TextMeshProUGUI fadeText; // Text to display during fade
    public float fadeDuration = 1f; // How long fade takes
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Make sure fade image starts transparent and disabled
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false); // Start disabled
            fadeText.gameObject.SetActive(false); // Start disabled
        }
    }
    
    public IEnumerator FadeOut()
    {
        // Fade to black
        if (fadeImage == null) yield break;
        
        // Enable the image to block all clicks
        fadeImage.gameObject.SetActive(true);
        
        float elapsedTime = 0f;
        Color c = fadeImage.color;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        
        c.a = 1f;
        fadeImage.color = c;
        
        // Enable the text during fade out
        if (fadeText != null)
        {
            fadeText.gameObject.SetActive(true);
        }
    }
    
    public IEnumerator FadeIn()
    {
        // Fade from black to clear
        if (fadeImage == null) yield break;
        
        float elapsedTime = 0f;
        Color c = fadeImage.color;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        
        c.a = 0f;
        fadeImage.color = c;

        // Disable the image to allow clicks again
        fadeImage.gameObject.SetActive(false);
        // Disable the text after fade in
        if (fadeText != null)
        {
            fadeText.gameObject.SetActive(false);
        }
    }
    
    public IEnumerator FadeOutAndIn()
    {
        // Fade to black, then back to normal
        yield return FadeOut();
        yield return new WaitForSeconds(0.5f); // Stay black for a moment
        yield return FadeIn();
    }
}
