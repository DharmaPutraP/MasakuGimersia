using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkSmokeEffect : MonoBehaviour
{
    [Header("Particle System")]
    public ParticleSystem smokeParticles;
    
    [Header("Settings")]
    public float emissionRateWalking = 30f;
    public float emissionRateIdle = 0f;
    public bool useAnimatorParameter = false;
    public string walkingParameterName = "IsWalking";
    
    private ParticleSystem.EmissionModule emission;
    private Animator animator;
    private bool isWalking = false;
    private bool hasAnimatorParameter = false;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        
        if (smokeParticles == null)
        {
            smokeParticles = GetComponentInChildren<ParticleSystem>();
        }
        
        if (smokeParticles != null)
        {
            emission = smokeParticles.emission;
            emission.rateOverTime = emissionRateIdle;
            
            // Ensure particle system is properly configured
            var main = smokeParticles.main;
            main.playOnAwake = false;
            
            // Check if animator has the walking parameter
            if (useAnimatorParameter && animator != null)
            {
                foreach (AnimatorControllerParameter param in animator.parameters)
                {
                    if (param.name == walkingParameterName)
                    {
                        hasAnimatorParameter = true;
                        break;
                    }
                }
                
                if (!hasAnimatorParameter)
                {
                    Debug.LogWarning("Animator parameter '" + walkingParameterName + "' not found on " + gameObject.name + ". Use SetWalkingState() method instead.");
                }
            }
            
            Debug.Log("WalkSmokeEffect initialized on " + gameObject.name);
        }
        else
        {
            Debug.LogWarning("No ParticleSystem assigned or found on " + gameObject.name);
        }
    }
    
    void Update()
    {
        if (useAnimatorParameter && hasAnimatorParameter && animator != null && smokeParticles != null)
        {
            bool currentlyWalking = animator.GetBool(walkingParameterName);
            
            if (currentlyWalking != isWalking)
            {
                isWalking = currentlyWalking;
                UpdateSmokeEffect();
            }
        }
    }
    
    void UpdateSmokeEffect()
    {
        if (smokeParticles == null) return;
        
        if (isWalking)
        {
            emission.rateOverTime = emissionRateWalking;
            if (!smokeParticles.isPlaying)
            {
                smokeParticles.Play();
            }
            Debug.Log(gameObject.name + " started walking - smoke ON");
        }
        else
        {
            emission.rateOverTime = emissionRateIdle;
            Debug.Log(gameObject.name + " stopped walking - smoke OFF");
        }
    }
    
    // Public method to manually control the effect
    public void SetWalkingState(bool walking)
    {
        if (smokeParticles != null && isWalking != walking)
        {
            isWalking = walking;
            UpdateSmokeEffect();
        }
    }
}
