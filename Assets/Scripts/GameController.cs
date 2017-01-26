using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    [SerializeField]
    //private Image falseImage;
    private GameObject falseImage;
    [SerializeField]
    //private Image trueImage;
    private GameObject trueImage;
    [SerializeField]
    private Text secondsCounterText;
    [SerializeField]
    private Text buttonTextToChange;
    [SerializeField]
    private Text ScoreTextDuringGame;
    [SerializeField]
    private Text scoreText;
    [SerializeField]
    private GameObject[] uiToShow;
    [SerializeField]
    private float maxSeconds = 10f;
    [SerializeField]
    private float waitTime = 1f;
    private float secondsPerRound;
    //how much maxSeconds will be reduced, roundRatio*=0.9 after every right-draw-image
    private float roundRatio = 1;
    //waiting after drawing to let the TrailRenderer draw
    private float secondsWait;

    //if we add new patterns in another scene
    private bool needToReloadPatterns = false;

    LineDrawScript lineDrawScript;
    BuildAndCompareScript buildAndCompareScript;

    //current score of right-draw-images
    private int score = 0;
    //user can't draw while the game isn't running
    private bool gameStatus = true;
    //if running the game first time
    private bool firstTime = true;

    void Start ()
    {
        Screen.fullScreen = true;
        lineDrawScript = GetComponent<LineDrawScript>();
        buildAndCompareScript = GetComponent<BuildAndCompareScript>();
        secondsPerRound = maxSeconds;
        secondsCounterText.text = secondsPerRound.ToString("F2");
        scoreText.text = "";
    }
	
	void FixedUpdate ()
    {
        //reloading patterns if needed
        if(needToReloadPatterns)
        {
            buildAndCompareScript.patternsList.Clear();
            buildAndCompareScript.ReadPatternsAndBuildPolygons();
        }
        //if gameStatus is false, user can't draw and seconds aren't reducing
        if (gameStatus)
        {
            secondsPerRound -= Time.deltaTime;
            if (secondsWait > 0) secondsWait -= Time.deltaTime;
            if (secondsWait <= 0)
            {
                trueImage.SetActive(false);
                falseImage.SetActive(false);
                if (lineDrawScript.drawStatus == 2) lineDrawScript.drawStatus = 0;
            }
            if (secondsPerRound <= 0)
            {
                trueImage.SetActive(false);
                falseImage.SetActive(false);
                gameStatus = false;
                secondsPerRound = 0;
                scoreText.text = "Your score: " + score.ToString();
                lineDrawScript.drawStatus = 2;
                for (int i = 0; i < uiToShow.Length; i++)
                    uiToShow[i].SetActive(true);
                score = 0;
                ScoreTextDuringGame.text = "0";
                buttonTextToChange.text = "Начать сначала";
            }
            secondsCounterText.text = secondsPerRound.ToString("F2");
        }
    }

    //use after getting and recognizing every draw-image
    public void Restore(bool recognizeStatus)
    {
        if(recognizeStatus)
        {
            score++;
            ScoreTextDuringGame.text = score.ToString();
            trueImage.SetActive(true);
            roundRatio *= 0.9f;
            secondsPerRound = maxSeconds*roundRatio;
            secondsCounterText.text = secondsPerRound.ToString("F2");
            buildAndCompareScript.ChooseNewPattern();
        } else
        {
            falseImage.SetActive(true);
        }
        secondsWait = waitTime;
    }

    //to start new game
    public void RestoreGameStatus()
    {
        gameStatus = true;
        secondsPerRound = maxSeconds;
        roundRatio = 1;
        secondsCounterText.text = secondsPerRound.ToString("F2");
        scoreText.text = "";
        if (!firstTime)
        {
            buildAndCompareScript.ChooseNewPattern();
            buildAndCompareScript.lastChosenNumber = -1;
        }
        firstTime = false;

    }

    //open scene with adding patterns
    public void OpenAddingPatternScene()
    {
        needToReloadPatterns = true;
        SceneManager.LoadScene("AddingPattern");
        SceneManager.UnloadSceneAsync("MainWindow");
    }
}
