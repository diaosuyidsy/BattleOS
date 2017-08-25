using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralTouch : MonoBehaviour
{
	public GameObject TimeSlowText;
	public GameObject TimeFastText;
	public GameObject TimeNormText;

	private Vector3 fp;
	//First touch position
	private Vector3 lp;
	//Last touch position
	private float dragDistance;
	//minimum distance for a swipe to be registered

	void Start ()
	{
		dragDistance = Screen.height * 10f / 100f; //dragDistance is 15% height of the screen
	}

	void Update ()
	{
		if (Input.GetMouseButtonDown (0)) { //check for the first touch
			fp = Input.mousePosition;
			lp = Input.mousePosition;
		} else if (Input.GetMouseButton (0)) { // update the last position based on where they moved
			lp = Input.mousePosition;
			if (GameManager.GM.draggingTower != null) {
				fp = Input.mousePosition;
			}
		} else if (Input.GetMouseButtonUp (0)) { //check if the finger is removed from the screen
			lp = Input.mousePosition;  //last touch position. Ommitted if you use list

			//Check if drag distance is greater than 20% of the screen height
			if (Mathf.Abs (lp.x - fp.x) > dragDistance || Mathf.Abs (lp.y - fp.y) > dragDistance) {//It's a drag
				//check if the drag is vertical or horizontal
				if (Mathf.Abs (lp.x - fp.x) > Mathf.Abs (lp.y - fp.y)) {   //If the horizontal movement is greater than the vertical movement...
					if ((lp.x > fp.x)) {  //If the movement was to the right)//Right swipe
//						Debug.Log ("Right Swipe");
						fastTime ();
					} else {   //Left swipe
//						Debug.Log ("Left Swipe");
						slowTime ();
					}
				}
			}
		}
	}

	public void slowTime ()
	{
		if (Mathf.Approximately (Time.timeScale, 1f)) {
			Time.timeScale = 0.05f;
			StartCoroutine (TextEmerge (TimeSlowText));
		}
		if (Mathf.Approximately (Time.timeScale, 1.5f)) {
			Time.timeScale = 1f;
			StartCoroutine (TextEmerge (TimeNormText));
		}
	}

	public void fastTime ()
	{
		if (Mathf.Approximately (Time.timeScale, 1f)) {
			Time.timeScale = 1.5f;
			StartCoroutine (TextEmerge (TimeFastText));
		}
		if (Mathf.Approximately (Time.timeScale, 0.05f)) {
			Time.timeScale = 1f;
			StartCoroutine (TextEmerge (TimeNormText));

		}
	}

	IEnumerator TextEmerge (GameObject text)
	{
		text.SetActive (true);
		yield return new WaitForSecondsRealtime (1f);
		text.SetActive (false);
	}
}
