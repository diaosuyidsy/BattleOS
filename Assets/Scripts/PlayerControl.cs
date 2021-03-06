﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TutorialDesigner;

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
	public bool soulLink = false;
	GameObject leftChain = null;
	GameObject rightChain = null;

	void Update ()
	{
		if (startClicking)
			clickCD += Time.unscaledDeltaTime;
	}

	public void OnBeginDrag (PointerEventData _EventData)
	{
		if (transform.parent.gameObject.tag == "ProductionSlot") {
			return;
		}
		towerBeingDragged = gameObject;
		GameManager.GM.draggingTower = towerBeingDragged;
		StartPosition = transform.position;
		StartParent = transform.parent;
		shadowImage = (GameObject)Instantiate (shadowImagePrefab, Camera.main.ScreenToWorldPoint (Input.mousePosition), Quaternion.identity);
	}

	public void OnDrag (PointerEventData _EventData)
	{
		if (towerBeingDragged == null)
			return;
		if (_EventData.pointerEnter != null && _EventData.pointerEnter != gameObject && _EventData.pointerEnter.tag == "Tower") {
			draggedOver = _EventData.pointerEnter;
			if (GetComponent<TowerControl> ().TI.Equals (draggedOver.GetComponent<TowerControl> ().TI) || GetComponent<TowerControl> ().TI.CanMixMergeWith (draggedOver.GetComponent<TowerControl> ().TI))
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
				if (TryMerge (minSlot)) {
					// Tutorial Design
					EventManager.TriggerEvent ("MergeSuccess");
					// Tutorial Design
					Destroy (gameObject);
				} else {
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

		//Level 5 ablity soul link
		if (soulLink) {
			GetComponent<TowerControl> ().linkSoul ();
		}
		if (GetComponent<TowerControl> ().linkedSoul != null) {
			GetComponent<TowerControl> ().linkedSoul.SendMessage ("linkSoul");
		}
		// If Dragged to ProductionSlot; Create an Image there
		if (minSlot.gameObject.tag == "ProductionSlot") {
			EventManager.TriggerEvent ("DORS");
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
		GameManager.GM.draggingTower = null;
		draggedOver = null;
		//Tutorial Designer
		if (GameManager.GM.Slots [22].transform.childCount > 0) {
			EventManager.TriggerEvent ("TowerInPlace");
		}
	}

	void createTankChain ()
	{
		if (GetComponent<TowerControl> ().TT == TowerType.Tank) {
			if (leftChain != null) {
				Color a = leftChain.GetComponent<SpriteRenderer> ().color;
				a.a = 0f;
				leftChain.GetComponent<SpriteRenderer> ().color = a;
			}
			if (rightChain != null) {
				Color a = leftChain.GetComponent<SpriteRenderer> ().color;
				a.a = 0f;
				leftChain.GetComponent<SpriteRenderer> ().color = a;
			}
			int linktype = GetComponent<TowerControl> ().isTankLinedTogether (null);
			switch (linktype) {
			case 0:
				Debug.Log (0);
				break;
			case 1:
				Debug.Log (1);
				RaycastHit2D[] hits = Physics2D.RaycastAll (transform.position, Vector2.left, 1f);
				foreach (RaycastHit2D hit in hits) {
					if (hit.collider != null && hit.collider.tag == "Chain") {
						Color a = hit.collider.GetComponent<SpriteRenderer> ().color;
						a.a = 1f;
						hit.collider.GetComponent<SpriteRenderer> ().color = a;
						leftChain = hit.collider.gameObject;
						break;
					}
				}
				break;
			case 2:
				Debug.Log (2);
				hits = Physics2D.RaycastAll (transform.position, Vector2.right, 1f);
				foreach (RaycastHit2D hit in hits) {
					if (hit.collider != null && hit.collider.tag == "Chain") {
						Color a = hit.collider.GetComponent<SpriteRenderer> ().color;
						a.a = 1f;
						hit.collider.GetComponent<SpriteRenderer> ().color = a;
						rightChain = hit.collider.gameObject;
						break;
					}
				}
				break;
			case 3:
				Debug.Log (3);
				hits = Physics2D.LinecastAll (new Vector2 (transform.position.x - 1f, transform.position.y), new Vector2 (transform.position.x + 1f, transform.position.y));
				int i = 0;
				foreach (RaycastHit2D hit in hits) {
					if (hit.collider != null && hit.collider.tag == "Chain") {
						Color a = hit.collider.GetComponent<SpriteRenderer> ().color;
						a.a = 1f;
						hit.collider.GetComponent<SpriteRenderer> ().color = a;
						if (i == 0) {
							leftChain = hit.collider.gameObject;
							i++;
						} else {
							rightChain = hit.collider.gameObject;
						}
					}
				}
				break;
			}
		}
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
//			if (gameObject.transform.parent.tag == "ProductionSlot") {
//				gameObject.transform.parent.gameObject.GetComponent<ReproduceControl> ().tryStart ();
//				return;
//			}
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
			//Level 5 soul link ability
			if (GameManager.GM.selectedTower == gameObject && soulLink) {
				GetComponent<TowerControl> ().linkSoul ();
			}
			GameManager.GM.onSelectedTower (GetComponent<TowerControl> ().TowerLevel > 4 && GetComponent<TowerControl> ().TT != TowerType.Missile);
		}
		clickCD = 0f;
	}

	void OnMouseOver ()
	{
		if (!isDraggedOver || transform.parent.tag == "ProductionSlot")
			return;
		transform.localScale = new Vector3 (1f, 1f, 1f);
		int coin = TowerControl.TowerInfoToMergeCoin (GetComponent<TowerControl> ().TI);
		GameManager.GM.ConsumeCoinHolder.GetComponentInChildren<Text> ().text = "-" + GameManager.GM.NumToString (coin);
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
			if (slot == null || !slot.activeSelf)
				continue;
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

	void OnTriggerEnter2D (Collider2D other)
	{
		if (other.tag == "FortifySpell")
			transform.parent.GetComponent<SpriteRenderer> ().color = Color.green;
	}

	void OnTriggerExit2D (Collider2D other)
	{
		if (other.tag == "FortifySpell")
			transform.parent.GetComponent<SpriteRenderer> ().color = Color.white;
	}

	public void setSoulLink (bool y)
	{
		soulLink = y;
	}

	void OnDestroy ()
	{
		if (shadowImage != null) {
			Destroy (shadowImage);
		}
	}
}
