using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{

	public GameObject[] NeedActivates;
	public GAui playButton;
	public GAui TitleText;
	//Tutorial Design
	public GameObject bouncingCirclePrefab;

	public void StartGame ()
	{
		playButton.MoveOut (GUIAnimSystem.eGUIMove.SelfAndChildren);
		TitleText.MoveOut (GUIAnimSystem.eGUIMove.SelfAndChildren);
		StartCoroutine (activateInactives ());
	}

	IEnumerator activateInactives ()
	{
		foreach (GameObject na in NeedActivates) {
			na.SetActive (true);
			yield return new WaitForSeconds (0.03f);
		}
	}

	//Tutorial Design

	public void StartTutorial ()
	{
		playButton.MoveOut (GUIAnimSystem.eGUIMove.SelfAndChildren);
		TitleText.MoveOut (GUIAnimSystem.eGUIMove.SelfAndChildren);
		StartCoroutine (activateNormalSlots ());
	}

	IEnumerator activateNormalSlots ()
	{
		for (int i = 0; i < 35; i++) {
			NeedActivates [i].SetActive (true);
			yield return new WaitForSeconds (0.03f);
		}
	}

	public void ReproductionSlotEnter ()
	{
		StartCoroutine (reproduceEnter ());
	}

	IEnumerator reproduceEnter ()
	{
		for (int i = 35; i < NeedActivates.Length; i++) {
			NeedActivates [i].SetActive (true);
			yield return new WaitForSeconds (0.03f);
		}
	}

	// Really badly written methond, but not in the mood to change it.
	public void GenerateTwoBouncingCircle ()
	{
		GameObject[] alltowers = GameObject.FindGameObjectsWithTag ("Tower");
		int Heal = 0, Tank = 0, DPS = 0;
		foreach (GameObject tower in alltowers) {
			if (tower.transform.parent.tag == "ProductionSlot") {
				continue;
			}
			if (tower.GetComponent<TowerControl> ().TT == TowerType.Heal) {
				Heal++;
			} else if (tower.GetComponent<TowerControl> ().TT == TowerType.Range) {
				DPS++;
			} else {
				Tank++;
			}
		}
		if (Heal == 2) {
			foreach (GameObject tower in alltowers) {
				if (tower.transform.parent.tag == "ProductionSlot") {
					continue;
				}
				if (tower.GetComponent<TowerControl> ().TT == TowerType.Heal) {
					Instantiate (bouncingCirclePrefab, tower.transform.position, Quaternion.identity);
				}
			}
		}
		if (DPS == 2) {
			foreach (GameObject tower in alltowers) {
				if (tower.transform.parent.tag == "ProductionSlot") {
					continue;
				}
				if (tower.GetComponent<TowerControl> ().TT == TowerType.Range) {
					Instantiate (bouncingCirclePrefab, tower.transform.position, Quaternion.identity);
				}
			}
		}

		if (Tank == 2) {
			foreach (GameObject tower in alltowers) {
				if (tower.transform.parent.tag == "ProductionSlot") {
					continue;
				}
				if (tower.GetComponent<TowerControl> ().TT == TowerType.Tank) {
					Instantiate (bouncingCirclePrefab, tower.transform.position, Quaternion.identity);
				}
			}
		}
	}

	public void DestroyBouncingcircles ()
	{
		GameObject[] temp = GameObject.FindGameObjectsWithTag ("BouncingCircle");
		foreach (GameObject go in temp) {
			Destroy (go);
		}
	}
}
