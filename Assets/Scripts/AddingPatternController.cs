using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;

public class AddingPatternController : MonoBehaviour {

    //point (x,y) in the space
    struct Point
    {
        public int x;
        public int y;
        public Point(int newX, int newY)
        {
            x = newX;
            y = newY;
        }
    }

    //polygon of points
    private List<Point> patternList = new List<Point>();

    //to skip one click after clicking buttons
    private bool skipOne = false;

    LineRenderer lineRenderer;
   

	void Start ()
    {
        lineRenderer = GetComponent<LineRenderer>();
	}
	
	void Update ()
    {
		if(Input.GetMouseButtonUp(0))
        {
            if (skipOne)
                skipOne = false;
            else
            {
                //adding new points into polygon
                patternList.Add(new Point((int)(Input.mousePosition.x / 10), (int)(Input.mousePosition.y / 10)));
                lineRenderer.numPositions++;
                lineRenderer.SetPosition(lineRenderer.numPositions - 1, new Vector3((Input.mousePosition.x * 6.66f * 2 / Screen.width - 6.66f), (Input.mousePosition.y * 5f * 2 / Screen.height - 5f)));
            }
        }
	}

    //open main window
    public void OpenMainWindowScene()
    {
        SceneManager.LoadScene("MainWindow");
        SceneManager.UnloadSceneAsync("AddingPattern");
    }

    //delete pattern from the screen
    public void ClearScreen()
    {
        patternList.Clear();
        lineRenderer.numPositions = 0;
        skipOne = true;
    }

    //add pattern to .txt file
    public void AddPattern()
    {
        if (patternList.Count > 0)
        {
            string dataPath = Application.streamingAssetsPath + "/patterns.txt";
            StreamWriter theWriter = new StreamWriter(dataPath, true);
            theWriter.WriteLine();
            theWriter.Write(patternList[0].x.ToString() + ' ' + patternList[0].y.ToString());
            for (int i = 1; i < patternList.Count; i++) 
            {
                theWriter.Write(' ' + patternList[i].x.ToString() + ' ' + patternList[i].y.ToString());
            }
            patternList.Clear();
            lineRenderer.numPositions = 0;
            theWriter.Close();
        }
        skipOne = true;
    }
}
