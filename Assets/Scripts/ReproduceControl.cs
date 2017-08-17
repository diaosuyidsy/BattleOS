using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReproduceControl : MonoBehaviour
{
	public GameObject SlotInFront;
	public Sprite RPsprite;
	public Sprite GreenBar;
	public bool Locked = true;
	public GameObject ProductionStarterPrefab;
	public int productionCoin;

	private float maxProduceTime;
	private float pTime;
	private GameObject ReproductionTarget;
	private bool startRe;
	private SpriteRenderer TargetSR;
	private int siblingIndex;

	void Start ()
	{
		siblingIndex = transform.GetSiblingIndex ();
	}

	// Update is called once per frame
	void Update ()
	{
		if (startRe)
			rep ();
	}

	void rep ()
	{
		if (ReproductionTarget == null)
			return;
		pTime += Time.deltaTime;
		if (pTime >= maxProduceTime) {
			pTime = 0f;
			// Produce a tower if no tower in front
			if (SlotInFront.transform.childCount == 0) {
				GameObject newTower = (GameObject)Instantiate (ReproductionTarget, SlotInFront.transform);
				newTower.GetComponent<TowerControl> ().HealthBar.GetComponent<SpriteRenderer> ().sprite = GreenBar;
				//set range image to false
				newTower.transform.GetChild (0).gameObject.SetActive (false);
				newTower.GetComponent<PlayerControl> ().Engaged = false;
				newTower.GetComponent<TowerControl> ().TowerSpriteAndAnimation.GetComponent<SpriteRenderer> ().color = Color.white;

				SpriteRenderer[] sprites = newTower.GetComponentsInChildren<SpriteRenderer> ();
				foreach (SpriteRenderer sprite in sprites) {
					Color b = sprite.color;
					b.a = 1f;
					sprite.color = b;
				}
				GameManager.GM.ProductionStarters [siblingIndex].SetActive (true);
				GameManager.GM.AddCoin (0);
				startRe = false;
				ReproductionTarget.GetComponent<PlayerControl> ().Engaged = false;
			}
		}
		// Set target Reproduction Bar
		float percent = pTime / maxProduceTime;
		TargetSR.transform.localScale = new Vector3 (percent, TargetSR.transform.localScale.y, TargetSR.transform.localScale.z);
		TargetSR.transform.localPosition = new Vector3 ((1f - percent) * -4.5f, TargetSR.transform.localPosition.y, TargetSR.transform.localPosition.z);

	}

	public void StopReproduce ()
	{
		startRe = false;
		ReproductionTarget = null;
	}

	public void StartReproduce ()
	{
		if (Locked)
			return;
		//First Need to reset everything
		maxProduceTime = 0f;
		pTime = 0f;
		ReproductionTarget = transform.GetChild (0).gameObject;

		ReproductionTarget.GetComponent<TowerControl> ().setFunctioning (true);
		ReproductionTarget.transform.GetChild (0).gameObject.SetActive (false);
		// Set up Reproduction Bar
		GameObject targetHB = ReproductionTarget.GetComponent<TowerControl> ().HealthBar;
		TargetSR = targetHB.GetComponent<SpriteRenderer> ();
		TargetSR.sprite = RPsprite;
		targetHB.transform.localScale = new Vector3 (0f, 1f, 1f);
		targetHB.transform.localPosition = new Vector3 (-4.5f, 0, -2f);
		// Set up max Reproduce time
		maxProduceTime = ReproductionTarget.GetComponent<TowerControl> ().thisTowerToRepTime ();
		// Set up need coin for production
		productionCoin = ReproductionTarget.GetComponent<TowerControl> ().thisTowerToRepCoin ();
		// Active the starter
		GameManager.GM.ProductionStarters [siblingIndex].transform.GetChild (1).GetComponent<Text> ().text = productionCoin.ToString ();

		GameManager.GM.ProductionStarters [siblingIndex].SetActive (true);
		GameManager.GM.AddCoin (0);
	}

	public void tryStart ()
	{
		// If coin not sufficient, no
		if (!GameManager.GM.hasEnoughCoin (productionCoin))
			return;
		// If prior slot has a tower, no
		if (SlotInFront.transform.childCount != 0)
			return;

		//Else start!
		GameManager.GM.ProductionStarters [siblingIndex].SetActive (false);
		GameManager.GM.AddCoin (-productionCoin);
		startRe = true;
		ReproductionTarget.GetComponent<PlayerControl> ().Engaged = true;

	}

	public bool isReproducing ()
	{
		return startRe;
	}
}
