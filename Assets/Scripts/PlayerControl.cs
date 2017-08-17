using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
	bool startClicking = false;
	float clickCD = 0f;

	void Update ()
	{
		if (startClicking)
			clickCD += Time.deltaTime;
	}

	public void OnBeginDrag (PointerEventData _EventData)
	{
//		if (Engaged)
//			return;
//		if (transform.parent.gameObject.tag == "ProductionSlot" && transform.parent.gameObject.GetComponent<ReproduceControl> ().isReproducing ())
//			return;
		if (transform.parent.gameObject.tag == "ProductionSlot") {
			return;
		}
		towerBeingDragged = gameObject;
		StartPosition = transform.position;
		StartParent = transform.parent;
		shadowImage = (GameObject)Instantiate (shadowImagePrefab, Camera.main.ScreenToWorldPoint (Input.mousePosition), Quaternion.identity);
	}

	public void OnDrag (PointerEventData _EventData)
	{
//		if (Engaged) {
//			if (shadowImage != null)
//				Destroy (shadowImage);
//			return;
//		}
		if (towerBeingDragged == null)
			return;
		if (_EventData.pointerEnter != gameObject && _EventData.pointerEnter.tag == "Tower") {
			draggedOver = _EventData.pointerEnter;
			if (GetComponent<TowerControl> ().TI.Equals (draggedOver.GetComponent<TowerControl> ().TI))
				draggedOver.GetComponent<PlayerControl> ().isDraggedOver = true;
		}
		if (shadowImage != null)
			shadowImage.transform.position = new Vector3 (Camera.main.ScreenToWorldPoint (Input.mousePosition).x, Camera.main.ScreenToWorldPoint (Input.mousePosition).y, transform.position.z);
	}

	public void OnEndDrag (PointerEventData _EventData)
	{
		if (draggedOver != null) {
			draggedOver.GetComponent<PlayerControl> ().isDraggedOver = false;
			draggedOver.transform.localScale = new Vector3 (0.5f, 0.5f, 1f);
		}
		if (towerBeingDragged == null)
			return;
//		if (Engaged) {
//			Destroy (shadowImage);
//			return;
//		}
		Destroy (shadowImage);
		Transform minSlot = FindNearestSlot ();
		// If minSlot is not startParent, and minSlot has occupant
		// TryMerge
		// Else is just a simple Position change
		if (minSlot != StartParent && minSlot.childCount > 0) {
			if (minSlot.tag == "ProductionSlot") {
				minSlot.GetComponent<ReproduceControl> ().StopReproduce ();
				GameObject child = minSlot.GetChild (0).gameObject;
				child.transform.SetParent (null);
				Destroy (child);
			}
			if (minSlot.gameObject.tag != "ProductionSlot") {
				if (TryMerge (minSlot))
					Destroy (gameObject);
				else {
					minSlot.GetChild (0).parent = StartParent;
					transform.parent = minSlot;
					StartParent.transform.GetChild (0).localPosition = Vector3.zero;
				}
			}
			transform.localPosition = Vector3.zero;
		} else {
			transform.parent = minSlot;
			transform.localPosition = Vector3.zero;
		}
		// If Dragged to ProductionSlot; Create an Image there
		if (minSlot.gameObject.tag == "ProductionSlot") {
			transform.parent = StartParent;
			transform.localPosition = Vector3.zero;
			if (!minSlot.gameObject.GetComponent<ReproduceControl> ().Locked) {
				GameObject TowerImage = Instantiate (gameObject, minSlot.position, Quaternion.identity, minSlot.transform);
				SpriteRenderer[] sprites = TowerImage.GetComponent<TowerControl> ().EnterRepoSprites;
				foreach (SpriteRenderer sprite in sprites) {
					Color b = sprite.color;
					b.a = 0.5f;
					sprite.color = b;	
				}
				minSlot.gameObject.GetComponent<ReproduceControl> ().StartReproduce ();
			}

		} 
		GameManager.GM.ConsumeCoinHolder.SetActive (false);
		towerBeingDragged = null;
		draggedOver = null;
	}

	void OnMouseDown ()
	{
		startClicking = true;
	}

	void OnMouseUp ()
	{
		startClicking = false;
		if (clickCD <= 0.1f) {
			// It's a click
			if (gameObject.transform.parent.tag == "ProductionSlot") {
				gameObject.transform.parent.gameObject.GetComponent<ReproduceControl> ().tryStart ();
				return;
			}
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
			GameManager.GM.onSelectedTower ();
		}
		clickCD = 0f;
	}

	void OnMouseOver ()
	{
		if (!isDraggedOver || transform.parent.tag == "ProductionSlot")
			return;
		transform.localScale = new Vector3 (1f, 1f, 1f);
		int coin = TowerControl.TowerInfoToMergeCoin (GetComponent<TowerControl> ().TI);
		GameManager.GM.ConsumeCoinHolder.GetComponentInChildren<Text> ().text = "-" + coin.ToString ();
		GameManager.GM.ConsumeCoinHolder.SetActive (true);
	}

	void OnMouseExit ()
	{
		if (!isDraggedOver)
			return;
		transform.localScale = new Vector3 (0.5f, 0.5f, 1f);
		GameManager.GM.ConsumeCoinHolder.SetActive (false);
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
		float minDis = Vector2.Distance (StartPosition, Camera.main.ScreenToWorldPoint (Input.mousePosition));
		Transform minSlot = StartParent;
		foreach (GameObject slot in GameManager.GM.Slots) {
			float tempDis = Vector2.Distance (Camera.main.ScreenToWorldPoint (Input.mousePosition), slot.transform.position);
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
