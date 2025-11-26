using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingBoardEffect : MonoBehaviour
{
    [Header("Cutting Board Prefabs")]
    [Tooltip("Whole carrot prefab before cutting")]
    public GameObject wholeCarrotPrefab;
    [Tooltip("Carrot pieces prefab after cutting")]
    public GameObject carrotPiecesPrefab;
    
    [Tooltip("Whole meat prefab before cutting")]
    public GameObject wholeMeatPrefab;
    [Tooltip("Meat pieces prefab after cutting")]
    public GameObject meatPiecesPrefab;
    
    [Header("Spawn Settings")]
    [Tooltip("Where the ingredient appears on the cutting board")]
    public Transform ingredientSpawnPoint;
    
    [Header("Particle Effect")]
    [Tooltip("Smoke/cutting particle system")]
    public ParticleSystem cuttingParticles;
    
    [Header("Animation Settings")]
    public float ingredientAppearDuration = 0.3f;
    public float cuttingDuration = 1.5f;
    public float piecesDisplayDuration = 0.5f;
    
    private GameObject currentIngredient;
    private bool isAnimating = false;
    
    void Start()
    {
        if (ingredientSpawnPoint == null)
        {
            ingredientSpawnPoint = transform;
        }
        
        if (cuttingParticles != null)
        {
            var emission = cuttingParticles.emission;
            emission.enabled = false;
            cuttingParticles.Stop();
        }
    }
    
    public void PlayCuttingAnimation(CardType cardType)
    {
        if (isAnimating)
        {
            return;
        }
        
        if (cardType == CardType.PotongSayuran)
        {
            StartCoroutine(CuttingAnimationCoroutine(wholeCarrotPrefab, carrotPiecesPrefab));
        }
        else if (cardType == CardType.PotongDaging)
        {
            StartCoroutine(CuttingAnimationCoroutine(wholeMeatPrefab, meatPiecesPrefab));
        }
    }
    
    IEnumerator CuttingAnimationCoroutine(GameObject wholePrefab, GameObject piecesPrefab)
    {
        isAnimating = true;
        
        // 1. Spawn whole ingredient
        if (wholePrefab != null && ingredientSpawnPoint != null)
        {
            currentIngredient = Instantiate(wholePrefab, ingredientSpawnPoint.position, ingredientSpawnPoint.rotation, ingredientSpawnPoint);
            currentIngredient.SetActive(true);
            
            // Pop-in animation
            currentIngredient.transform.localScale = Vector3.zero;
            float elapsedTime = 0f;
            
            while (elapsedTime < ingredientAppearDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / ingredientAppearDuration;
                currentIngredient.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                yield return null;
            }
            currentIngredient.transform.localScale = Vector3.one;
        }
        
        yield return new WaitForSeconds(0.2f);
        
        // 2. Play cutting particles and sound
        if (cuttingParticles != null)
        {
            cuttingParticles.gameObject.SetActive(true);
            cuttingParticles.Clear();
            var emission = cuttingParticles.emission;
            emission.enabled = true;
            var main = cuttingParticles.main;
            main.loop = false;
            cuttingParticles.Play();
        }
        // Wait for cutting animation duration
        yield return new WaitForSeconds(cuttingDuration);
        
        // 3. Stop particles
        if (cuttingParticles != null)
        {
            var emission = cuttingParticles.emission;
            emission.enabled = false;
            cuttingParticles.Stop();
        }
        
        // 4. Replace with pieces
        if (currentIngredient != null)
        {
            Destroy(currentIngredient);
        }
        
        if (piecesPrefab != null && ingredientSpawnPoint != null)
        {
            currentIngredient = Instantiate(piecesPrefab, ingredientSpawnPoint.position, ingredientSpawnPoint.rotation, ingredientSpawnPoint);
            currentIngredient.SetActive(true);
            currentIngredient.transform.localScale = Vector3.one;
        }
        
        // 5. Wait a bit to show the pieces
        yield return new WaitForSeconds(piecesDisplayDuration);
        
        // 6. Fade out pieces
        if (currentIngredient != null)
        {
            float elapsedTime = 0f;
            float fadeDuration = 0.3f;
            Vector3 startScale = currentIngredient.transform.localScale;
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeDuration;
                currentIngredient.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }
            
            Destroy(currentIngredient);
        }
        
        isAnimating = false;
    }
    
    public bool IsAnimating()
    {
        return isAnimating;
    }
    
    public void ClearIngredient()
    {
        if (currentIngredient != null)
        {
            Destroy(currentIngredient);
        }
        
        if (cuttingParticles != null)
        {
            var emission = cuttingParticles.emission;
            emission.enabled = false;
            cuttingParticles.Stop();
        }
        
        isAnimating = false;
    }
}
