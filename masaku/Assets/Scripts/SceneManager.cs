using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public Canvas MenuCanvas;
    public Canvas TutorialCanvas;

    void Start()
    {
        // Pastikan MenuCanvas aktif dan TutorialCanvas nonaktif saat start
        if (MenuCanvas != null)
            MenuCanvas.enabled = true;
        if (TutorialCanvas != null)
            TutorialCanvas.enabled = false;
    }

    void Update()
    {
        
    }

    // Fungsi untuk tombol Mainkan
    public void OnPlayButtonPressed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }

    // Fungsi untuk tombol Keluar
    public void OnExitButtonPressed()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Fungsi untuk tombol Tutorial
    public void OnTutorialButtonPressed()
    {
        if (MenuCanvas != null)
            MenuCanvas.enabled = false;
        if (TutorialCanvas != null)
            TutorialCanvas.enabled = true;
    }

    // Fungsi untuk kembali ke menu dari tutorial
    public void OnBackToMenuButtonPressed()
    {
        if (MenuCanvas != null)
            MenuCanvas.enabled = true;
        if (TutorialCanvas != null)
            TutorialCanvas.enabled = false;
    }
}