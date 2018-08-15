using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.MagicLeap;

public class GameController : MonoBehaviour
{
    public GameObject hazard;
    public Vector3 spawnValues;
    public int hazardCount;
    public float spawnWait;
    public float startWait;
    public float waveWait;
    public Text scoreText;
    public Text restartText;
    public Text gameOverText;

    private int score;
    private bool gameOver;
    private bool restart;
    private AudioSource audioSource;

    // Magic Leap
    private MLInputController mlController;

    /*
     * Setup the Magic Leap controller input.
     */
    void Awake() {
        MLInput.Start();
        MLInput.OnControllerButtonDown += OnButtonDown;
        MLInput.OnControllerButtonUp += OnButtonUp;
        mlController = MLInput.GetController(MLInput.Hand.Left);
    }

    /*
     * Stop listening for Magic Leap input.
     */
    void OnDestroy () {
        MLInput.OnControllerButtonDown -= OnButtonDown;
        MLInput.OnControllerButtonUp -= OnButtonUp;
        MLInput.Stop();
    }

    /*
     * Listen for the Magic Leap bumper being pressed ("tapped").
     */
    void OnButtonDown(byte controller_id, MLInputControllerButton button) {
        if (button == MLInputControllerButton.Bumper) {
        }
    }

    /*
     * Listen for letting go of the Magic Leap controller bumper button.
     */
    void OnButtonUp(byte controller_id, MLInputControllerButton button) {
        if (button == MLInputControllerButton.Bumper) {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }
    }
    void Start()
    {
        gameOver = false;
        restart = false;
        restartText.text = "";
        gameOverText.text = "";
        score = 0;
        UpdateScore();

        audioSource = GetComponent<AudioSource>();
        audioSource.Play();

        StartCoroutine(SpawnWaves());
    }

    void Update()
    {
        // If it's waiting to Restart, then check for the key press. If the key is pressed
        // then re-load the entire scene again.
        if (restart)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                //Application.LoadLevel(Application.loadedLevel);
                SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(startWait);

        while (true)
        {
            for (int i = 0; i < hazardCount; i++)
            {
                Vector3 spawnPosition = new Vector3(Random.Range(-spawnValues.x, spawnValues.x),
                    spawnValues.y, spawnValues.z);
                Quaternion spawnRotation = Quaternion.identity;
                Instantiate(hazard, spawnPosition, spawnRotation);

                yield return new WaitForSeconds(spawnWait);
            }

            yield return new WaitForSeconds(waveWait);

            if (gameOver) 
            {
                restartText.text = "Press 'R' to Restart";
                restart = true;
                break;
            }
        }
    }

    public void AddScore(int newScoreValue)
    {
        score += newScoreValue;
        UpdateScore();
    }

    void UpdateScore() {
        scoreText.text = "Score: " + score;
    }

    public void GameOver() 
    {
        gameOverText.text = "Game Over";
        gameOver = true;            
    }
}
