using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Ghost[] ghosts;
    public Pacman pacman;
    public Transform pellets;

    public Text gameOverText;
    public Text scoreText;
    public Text livesText;
    public Vector2 scroll = Vector2.zero;
    private int lowscore = int.MaxValue;

    public int ghostMultiplier { get; private set; } = 1;
    public int score { get; private set; }
    public int lives { get; private set; }

    private void Start()
    {
        string difficulty = PlayerPrefs.GetString("difficulty");
        NewGame();
        foreach(Ghost g in ghosts){
            if (difficulty == "Hard")
            {
                g.movement.speed = 6f;
            }
            else if (difficulty == "Medium")
            {
                g.movement.speed = 7f;
            }
            else g.movement.speed = 8f;
        }
    }

    private void Update()
    {
        if (lives <= 0 && Input.anyKeyDown) { 
            if(Input.GetKey(KeyCode.Return)){
                SceneManager.LoadScene("Menu", LoadSceneMode.Single);
            }
            else NewGame();
        }
        Ghost newGhost = null;
        foreach (Ghost g in ghosts)
        {
            if (Input.GetKey(g.key) && !g.atHome)
            {
                newGhost = g; break;
            }
        }
        if (newGhost != null)
        {
            foreach (Ghost g in ghosts)
            {
                if (g.Equals(newGhost))
                {
                    g.selected = true;
                }
                else g.selected = false;
            }
        }
    }

    private void NewGame()
    {
        SetScore(0);
        SetLives(3);
        NewRound();
    }

    private void NewRound()
    {
        gameOverText.enabled = false;

        foreach (Transform pellet in pellets) {
            pellet.gameObject.SetActive(true);
        }

        ResetState();
    }

    private void ResetState()
    {
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].ResetState();
        }

        pacman.ResetState();
    }

    private void GameOver()
    {
        if(lives > 0)
        {
            lives = 0;
            gameOverText.text = "You Lose";

        } else
        {
            gameOverText.text = "Game Over";
        }
        if(score < lowscore)
        {
            if(lowscore != int.MaxValue)
            {
                gameOverText.text = "Low Score!";

            }
            lowscore = score;
        }
        
        gameOverText.enabled = true;

        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].gameObject.SetActive(false);
        }

        pacman.gameObject.SetActive(false);
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        livesText.text = "x" + lives.ToString();
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString().PadLeft(2, '0');
    }

    public void PacmanEaten()
    {
        pacman.DeathSequence();

        SetLives(lives - 1);

        if (lives > 0) {
            Invoke(nameof(ResetState), 3f);
        } else {
            GameOver();
        }
    }

    public void GhostEaten(Ghost ghost)
    {
        int points = ghost.points * ghostMultiplier;
        SetScore(score + points);

        ghostMultiplier++;
    }

    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);

        SetScore(score + pellet.points);

        if (!HasRemainingPellets())
        {
            foreach(Ghost g in ghosts) {
                g.gameObject.SetActive(false);
            } 
            pacman.gameObject.SetActive(false);
            Invoke(nameof(GameOver), 3f);
        }
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].frightened.Enable(pellet.duration);
        }

        PelletEaten(pellet);
        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
    }

    private bool HasRemainingPellets()
    {
        foreach (Transform pellet in pellets)
        {
            if (pellet.gameObject.activeSelf) {
                return true;
            }
        }

        return false;
    }

    private void ResetGhostMultiplier()
    {
        ghostMultiplier = 1;
    }


}
