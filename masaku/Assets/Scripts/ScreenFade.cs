using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScreenFade : MonoBehaviour
{
    public static ScreenFade Instance;

    [Header("Fade Settings")]
    public Image fadeImage; 
    
    public TextMeshProUGUI fadeText; 
    public float fadeDuration = 1f; 
    
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
        
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false); 
            fadeText.gameObject.SetActive(false); 
        }
    }
    
    public IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;
        
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
        
        if (fadeText != null)
        {
            fadeText.gameObject.SetActive(true);
        }
    }
    
    public IEnumerator FadeIn()
    {
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

        fadeImage.gameObject.SetActive(false);
        if (fadeText != null)
        {
            fadeText.gameObject.SetActive(false);
        }
    }
    
    public IEnumerator FadeOutAndIn()
    {
        yield return FadeOut();
        yield return new WaitForSeconds(0.5f); 
        yield return FadeIn();
    }
}
