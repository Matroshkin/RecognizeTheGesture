using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;

public class LineDrawScript : MonoBehaviour {

    public int drawStatus = 0; //0 - no click, 1 - drawing right now, 2 - draw ended, can't draw again

    private BuildAndCompareScript buildAndCompareScript;

    void Start()
    {
        buildAndCompareScript = GetComponent<BuildAndCompareScript>();
    }
 
	void FixedUpdate ()
    {
        AddToMas();
	}

    //on click adds mouseposition to polygon of draw-image
    private void AddToMas()
    {
        if ((Input.GetMouseButton(0)))
        {
            if (drawStatus == 0)
                drawStatus = 1;
            if ((Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width) &&
                (Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height) &&
                (drawStatus == 1))
            {
                //get mouseposition and moving gameobject to that position to trigger Trail Renderer
                float rayDistance;
                Plane gameObjectPlane = new Plane(Camera.main.transform.forward * -1, this.transform.position);
                Ray gameObjectRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (gameObjectPlane.Raycast(gameObjectRay, out rayDistance))
                    this.transform.position = gameObjectRay.GetPoint(rayDistance);
                //add new point to draw-iamge
                buildAndCompareScript.pointsList.Add(new BuildAndCompareScript.Point((int)Input.mousePosition.x, (int)Input.mousePosition.y));
            }
        }
        else
        if (drawStatus == 1)
        {
            drawStatus = 2;
            buildAndCompareScript.Recognize();
        }
    }   
}


