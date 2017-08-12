using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReproduceControl : MonoBehaviour
{
	public GameObject SlotInFront;
	public Sprite RPsprite;
	public Sprite GreenBar;

	private float maxProduceTime;
	private float pTime;
	private GameObject ReproductionTarget;
	private GameObject RTTemplate;
	private bool startRe;
	private SpriteRenderer TargetSR;


	// Update is called once per frame
	void Update ()
	{
		if (startRe)
			rep ();
	}

	void rep ()
	{
		pTime += Time.deltaTime;
		if (pTime >= maxProduceTime) {
			pTime = 0f;
			// Produce a tower if no tower in front
			if (SlotInFront.transform.childCount == 0) {
				GameObject newTower = (GameObject)Instantiate (ReproductionTarget, SlotInFront.transform);
				newTower.GetComponent<TowerControl> ().HealthBar.GetComponent<SpriteRenderer> ().sprite = GreenBar;
			}
		}
		// Set target Reproduction Bar
		float percent = pTime / maxProduceTime;
		TargetSR.transform.localScale = new Vector3 (percent, TargetSR.transform.localScale.y, TargetSR.transform.localScale.z);
		TargetSR.transform.localPosition = new Vector3 ((1f - percent) * -4.5f, TargetSR.transform.localPosition.y, TargetSR.transform.localPosition.z);

	}

	public void StartReproduce ()
	{
		//First Need to reset everything
		maxProduceTime = 0f;
		pTime = 0f;
		ReproductionTarget = transform.GetChild (0).gameObject;
		// Set up Reproduction Bar
		GameObject targetHB = ReproductionTarget.GetComponent<TowerControl> ().HealthBar;
		TargetSR = targetHB.GetComponent<SpriteRenderer> ();
		TargetSR.sprite = RPsprite;
		targetHB.transform.localScale = new Vector3 (0f, 1f, 1f);
		targetHB.transform.localPosition = new Vector3 (-4.5f, 0, -2f);
		// Set up max Reproduce time
		maxProduceTime = ReproductionTarget.GetComponent<TowerControl> ().repTime;

		startRe = true;
	}
}
