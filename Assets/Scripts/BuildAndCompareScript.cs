using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class BuildAndCompareScript : MonoBehaviour {

    //point (x,y) in the space
    public struct Point
    {
        public int x;
        public int y;

        public Point(int newX, int newY)
        {
            x = newX;
            y = newY;
        }
    }

    //"polygons" of points
    //for draw-image
    public List<Point> pointsList = new List<Point>();
    //for all pattern-images
    public List<List<Point>> patternsList = new List<List<Point>>();
    //for all pattern-images to show in right corner as an example to draw for user
    private List<List<Point>> patternsListToExample = new List<List<Point>>();
    
    //which pattern-image to draw, random [0 , patternsList.Count)
    private int chosenNumber;
    //the same figure can't be chosen twice in a roe
    public int lastChosenNumber = -1;

    //if true - draw-image is true, false - draw-image is false
    private bool recognizeStatus = true;

    [Header("Recognize settings:")]
    [SerializeField]
    private int fillWidth = 10;
    [SerializeField]
    private float proportionsMaxDifference = 1.4f;
    [SerializeField]
    private float minPercentOfRight = 70f;
    [SerializeField]
    private float maxPercentOfMiss = 40f;

    LineDrawScript lineDrawScript;
    GameController gameController;
    LineRenderer lineRenderer;

    void Start()
    {
        lineDrawScript = GetComponent<LineDrawScript>();
        gameController = GetComponent<GameController>();
        lineRenderer = GetComponent<LineRenderer>();
        ReadPatternsAndBuildPolygons();
        ChooseNewPattern();
    }

    //full recognize
    public void Recognize()
    {
        //array for draw-image
        int[,] pointArray = BuildArray(pointsList);
        //array for pattern-image
        int[,] patternArray = BuildArray(patternsList[chosenNumber]);
        ReScalePatternImage(pointArray, ref patternArray);
        if (recognizeStatus)
        {
            ReScaleDrawImage(ref pointArray, patternArray);
            FillWithThickness(ref pointArray, fillWidth);
            FillWithThickness(ref patternArray, fillWidth);
            float percentRight = 0;
            float percentMiss = 0;
            CompareDrawAndPattern(pointArray, patternArray, ref percentRight, ref percentMiss);
            if (percentRight < minPercentOfRight) recognizeStatus = false;
            if (percentMiss > maxPercentOfMiss) recognizeStatus = false;
        }
        //tell gameController if draw-image is true or not
        gameController.Restore(recognizeStatus);
        //we can add new patterns between rounds so we clear pointsList in the end of round
        pointsList.Clear();
    }

    //build 2D-arrays for polygon of dots
    int[,] BuildArray(List<Point> list)
    {
        int[,] pointArr;
        int xMin, xMax, yMin, yMax;
        //get bounds of array
        xMin = Screen.width + 1;
        yMin = Screen.height + 1;
        xMax = 0;
        yMax = 0;
        foreach (Point point in list)
        {
            if (point.x > xMax) xMax = point.x;
            if (point.x < xMin) xMin = point.x;
            if (point.y > yMax) yMax = point.y;
            if (point.y < yMin) yMin = point.y;
        }
        //build new array with "2" in points where polygon has dots
        pointArr = new int[xMax - xMin + 1, yMax - yMin + 1];
        foreach (Point point in list)
        {
            pointArr[point.x - xMin, point.y - yMin] = 2;
        }
        return pointArr;
    }

    //rescale pattern-image to proportions of the draw-image, adding "free" space in 
    void ReScalePatternImage(int[,] pointArr, ref int[,] patternArr)
    {
        //proportions of draw-image and pattern-image
        float ratioPoint, ratioPattern;
        bool recognize = true;
        ratioPoint = (float)pointArr.GetLength(0) / pointArr.GetLength(1);
        ratioPattern = (float)patternArr.GetLength(0) / patternArr.GetLength(1);
        //if the difference of the proportions is bigger, than number -> draw-image is false
        if (Mathf.Max(ratioPoint, ratioPattern) / Mathf.Min(ratioPoint, ratioPattern) > proportionsMaxDifference)
            recognize = false; else
        //checking the difference of the proportions
        if (ratioPoint > ratioPattern)
        {
            //how much free space in X we need to add
            int dx = (int)(ratioPoint * patternArr.GetLength(1) - patternArr.GetLength(0));
            if (dx % 2 != 0) dx += 1; //crutch? not sure. but we really need it to be even
                                      //because we add the same amount of free space 
                                      //on the left and on the right side of the array.
                                      //creating new array with new free space and adding old data about pattern-image
            //creating new array with free space
            int[,] newPatternArr = new int[dx + patternArr.GetLength(0), patternArr.GetLength(1)];
            for (int i = 0; i < patternArr.GetLength(0); i++)
                for (int j = 0; j < patternArr.GetLength(1); j++)
                {
                    newPatternArr[i + dx / 2, j] = patternArr[i, j];
                }
            patternArr = newPatternArr;

        }
        else
        //do not check ratiopoint = ratioPattern, because in that case everything is OK
        if (ratioPoint < ratioPattern)
        {
            //how much free space in Y we need to add
            int dy = (int)(patternArr.GetLength(0) / ratioPoint - patternArr.GetLength(1));
            if (dy % 2 != 0) dy += 1; //aaaaaaand again, sorry
            //creating new array with free space
            int[,] newPatternArr = new int[patternArr.GetLength(0), dy + patternArr.GetLength(1)];
            for (int i = 0; i < patternArr.GetLength(0); i++)
                for (int j = 0; j < patternArr.GetLength(1); j++)
                    newPatternArr[i, j + dy / 2] = patternArr[i, j];
            patternArr = newPatternArr;
        }
        //if recognize is false, algorithm will stop and draw-image will be recognized as false
        recognizeStatus = recognize; //i think using bool instead of void would be bad idea
    }

    //rescale draw-image to get two 2d-arrays of same size
    void ReScaleDrawImage(ref int[,] pointArr, int[,] patternArr)
    {
        int[,] newPointArr = new int[patternArr.GetLength(0), patternArr.GetLength(1)];
        for (int i = 0; i < pointArr.GetLength(0); i++)
            for (int j = 0; j < pointArr.GetLength(1); j++)
            {
                if (pointArr[i, j] == 2)
                    newPointArr[(int)((i) * patternArr.GetLength(0) / pointArr.GetLength(0)),
                                (int)((j) * patternArr.GetLength(1) / pointArr.GetLength(1))]
                                = 2;
            }
        pointArr = newPointArr;
    }

    //fill arrays with fillWidth of the lines (because we have lines with thickness 1)
    void FillWithThickness(ref int[,] arr, int width)
    {
        int[,] newArr = new int[2 * width + arr.GetLength(0), 2 * width + arr.GetLength(1)];
        for(int i = 0; i < arr.GetLength(0); i++)
            for(int j = 0; j < arr.GetLength(1); j++)
                if(arr[i,j] == 2)
                {
                    for(int k = i - width; k <= i + width; k++)
                        for (int l = j - width; l <= j + width; l++)
                            newArr[k + width, l + width] = 1;   
                }
        arr = newArr;
    }

    //compare arrays
    void CompareDrawAndPattern(int[,] pointArr, int[,] patternArr, ref float percentTrue, ref float percentMiss)
    {
        int rightCount = 0, falseCount = 0; //rightCount - amount of points that are filled in both images; falseCount - filled in pattern-image, not filled in draw-image
        int missCount = 0, pointCount = 0; //missCount - amount of points that are filled in draw-image, not filled in pattern-image; pointCount - amount of points filled in draw-image
        for(int i = 0; i < pointArr.GetLength(0); i++)
            for(int j = 0; j < pointArr.GetLength(1); j++)
            {
            
                if (patternArr[i, j] == 1)
                {
                    if (pointArr[i, j] == 1)
                    {
                        rightCount++;
                        pointCount++;
                    }
                    else
                    {
                        falseCount++;
                    }
                }
                else
                {
                    if (pointArr[i, j] == 1)
                    {
                        missCount++;
                        pointCount++;
                    }
                }   
            }
        percentTrue = rightCount * 100 / (rightCount + falseCount);
        percentMiss = missCount * 100 / pointCount;

    }
    
    //read pattern-lines from .txt file and building polygons
    public void ReadPatternsAndBuildPolygons()
    {
        string dataPath = Application.streamingAssetsPath + "/patterns.txt";
        StreamReader theReader = new StreamReader(dataPath, Encoding.Default);
        string line;
        using (theReader)
        {
            do
            {
                line = theReader.ReadLine();
                if (line != null)
                {
                    string[] stringsToInt = line.Split(' ');
                    int[] numbers = new int[stringsToInt.Length];
                    for (int i = 0; i < stringsToInt.Length; i++)
                    {
                        numbers[i] = System.Int32.Parse(stringsToInt[i]);
                    }
                    List<Point> newPointList = new List<Point>();
                    List<Point> newPointListToDraw = new List<Point>();
                    //in .txt files we have only vertexes, but we need lines between them too, so we count them
                    for (int i = 0; i < numbers.Length - 2; i += 2)
                    {
                        newPointListToDraw.Add(new Point(numbers[i], numbers[i + 1]));
                        newPointList.Add(new Point(numbers[i], numbers[i + 1]));
                        for (int j = Mathf.Min(numbers[i], numbers[i + 2]) + 1; j < Mathf.Max(numbers[i], numbers[i + 2]); j++)
                        {
                            newPointList.Add(new Point(j, (int)((j - numbers[i]) * (numbers[i + 3] - numbers[i + 1]) / (numbers[i + 2] - numbers[i]) + numbers[i + 1])));
                        }
                        for (int j = Mathf.Min(numbers[i + 1], numbers[i + 3]) + 1; j < Mathf.Max(numbers[i + 1], numbers[i + 3]); j++)
                        {
                            newPointList.Add(new Point((int)((j - numbers[i + 1]) * (numbers[i + 2] - numbers[i]) / (numbers[i + 3] - numbers[i + 1]) + numbers[i]), j));
                        }
                    }
                    newPointListToDraw.Add(new Point(numbers[numbers.Length - 2], numbers[numbers.Length - 1]));
                    newPointList.Add(new Point(numbers[numbers.Length - 2], numbers[numbers.Length - 1]));
                    patternsList.Add(newPointList);
                    patternsListToExample.Add(newPointListToDraw);
                }
            } while (line != null);
            theReader.Close();
        }
    }

    //choosing new pattern-image for user to draw after completing previous
    public void ChooseNewPattern()
    {
        do
        {
            chosenNumber = Random.Range(0, patternsList.Count);
        } while (lastChosenNumber == chosenNumber);
        lastChosenNumber = chosenNumber;
        DrawExample(chosenNumber);
    }

    //drawing example to draw for user in upper right corner
    private void DrawExample(int chosen)
    {
        float dx = 4.16f;
        float dy = 2.5f;
        lineRenderer.numPositions = patternsListToExample[chosen].Count;
        int rendererLineCount = 0;
        int xMin, xMax, yMin, yMax;
        //getting bounds of pattern-array
        xMin = Screen.width + 1;
        yMin = Screen.height + 1;
        xMax = 0;
        yMax = 0;
        foreach (Point point in patternsListToExample[chosen])
        {
            if (point.x > xMax) xMax = point.x;
            if (point.x < xMin) xMin = point.x;
            if (point.y > yMax) yMax = point.y;
            if (point.y < yMin) yMin = point.y;
        }
        foreach (Point point in patternsListToExample[chosen])
        {
            //placing points in LineRenderer
            if ((xMax - xMin) > (yMax - yMin))
                lineRenderer.SetPosition(rendererLineCount, new Vector3((dx+(float)(point.x - xMin) * 2 / (xMax - xMin+1)), dy+((float)(point.y - yMin) * 2 / (xMax - xMin+1))));
            else
                lineRenderer.SetPosition(rendererLineCount, new Vector3((dx+(float)(point.x - xMin) * 2 / (yMax - yMin+1)), dy+((float)(point.y - yMin) * 2 / (yMax - yMin+1))));
            rendererLineCount++;
        }
    }
}
