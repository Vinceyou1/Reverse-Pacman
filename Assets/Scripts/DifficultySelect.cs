using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class DifficultySelect : MonoBehaviour
{
    public string difficulty;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerPrefs.SetString("difficulty", difficulty);
        difficulty = "set";
        SceneManager.LoadScene("Pacman", LoadSceneMode.Single);
    }
}
