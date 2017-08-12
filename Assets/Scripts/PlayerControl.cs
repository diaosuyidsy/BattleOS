using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControl : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public static GameObject towerBeingDragged;
	public GameObject shadowImagePrefab;
	public bool Engaged = false;
	public bool isDraggedOver = false;
	public GameObject towerRangeImage;

	Vector3 StartPosition;
	Transform StartParent;
	GameObject shadowImage;
	GameObject draggedOver;

	public void OnBeginDrag (PointerEventData _EventData)
	{
		if (Engaged)
			return;
		towerBeingDragged = gameObject;
		StartPosition = transform.position;
		StartParent = transform.parent;
		shadowImage = (GameObject)Instantiate (shadowImagePrefab, Camera.main.ScreenToWorldPoint (Input.mousePosition), Quaternion.identity);
	}

	public void OnDrag (PointerEventData _EventData)
	{
		if (Engaged) {
			Destroy (shadowImage);
			return;
		}
		if (_EventData.pointerEnter != gameObject && _EventData.pointerEnter.tag == "Tower") {
			draggedOver = _EventData.pointerEnter;
			if (GetComponent<TowerControl> ().TI.Equals (draggedOver.GetComponent<TowerControl> ().TI))
				draggedOver.GetComponent<PlayerControl> ().isDraggedOver = true;
		}
		shadowImage.transform.position = new Vector3 (Camera.main.ScreenToWorldPoint (Input.mousePosition).x, Camera.main.ScreenToWorldPoint (Input.mousePosition).y, transform.position.z);
	}

	public void OnEndDrag (PointerEventData _EventData)
	{
		if (draggedOver != null) {
			draggedOver.GetComponent<PlayerControl> ().isDraggedOver = false;
			draggedOver.transform.localScale = new Vector3 (0.5f, 0.5f, 1f);
		}
		if (Engaged) {
			Destroy (shadowImage);
			return;
		}
		Destroy (shadowImage);
		Transform minSlot = FindNearestSlot ();
		// If minSlot is not startParent, and minSlot has occupant
		// TryMerge
		// Else is just a simple Position change
		if (minSlot != StartParent && minSlot.childCount > 0) {
			if (TryMerge (minSlot))
				Destroy (gameObject);
			else {
				transform.parent = StartParent;
			}
			transform.localPosition = Vector3.zero;
		} else {
			transform.parent = minSlot;
			transform.localPosition = Vector3.zero;
			if (minSlot.gameObject.tag == "ProductionSlot") {
				minSlot.gameObject.SendMessage ("StartReproduce");
			}
		}

		towerBeingDragged = null;
		draggedOver = null;
	}

	void OnMouseDown ()
	{
		if (GameManager.GM.selectedTower == null) {
			GameManager.GM.selectedTower = gameObject;
			towerRangeImage.SetActive (true);
		} else if (GameManager.GM.selectedTower == gameObject) {
			GameManager.GM.selectedTower = null;
			towerRangeImage.SetActive (false);
		} else {
			GameManager.GM.selectedTower.GetComponent<PlayerControl> ().towerRangeImage.SetActive (false);
			GameManager.GM.selectedTower = gameObject;
			towerRangeImage.SetActive (true);
		}

	}

	void OnMouseEnter ()
	{
		if (!isDraggedOver)
			return;
		transform.localScale = new Vector3 (1f, 1f, 1f);
	}

	void OnMouseOver ()
	{
		if (!isDraggedOver)
			return;
		transform.localScale = new Vector3 (1f, 1f, 1f);
	}

	void OnMouseExit ()
	{
		if (!isDraggedOver)
			return;
		transform.localScale = new Vector3 (0.5f, 0.5f, 1f);
		isDraggedOver = false;
	}

	public void PreMerge ()
	{
		if (TryMerge (FindNearestSlot ())) {
			gameObject.transform.localScale = new Vector3 (2, 2);
		}
	}

	Transform FindNearestSlot ()
	{
		float minDis = Vector3.Distance (StartPosition, Camera.main.ScreenToWorldPoint (Input.mousePosition));
		Transform minSlot = StartParent;
		foreach (GameObject slot in GameManager.GM.Slots) {
			float tempDis = Vector3.Distance (Camera.main.ScreenToWorldPoint (Input.mousePosition), slot.transform.position);
			if (tempDis <= minDis) {
				minDis = tempDis;
				minSlot = slot.transform;
			}
		}
		return minSlot;
	}

	// Return the result of the merge
	bool TryMerge (Transform MergedParent)
	{
		TowerInfo thisTI = GetComponent<TowerControl> ().TI;
		return MergedParent.GetChild (0).gameObject.GetComponent<TowerControl> ().AbsorbOtherTower (thisTI);

	}
}
