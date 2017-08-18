using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FortifyControl : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public GameObject Progression;
	public GameObject SpellSprite;
	public GameObject BlurBorder;
	public GameObject ProgressBarHolder;
	public GameObject ActiveRange;
	public int coinNeeded;
	public Text coinText;
	public float maxProduceTime;
	public GAui SpellPanel;
	public Text SpellName;
	public Text SpellIntro;
	public Text SpellFunction;

	bool startRe = false;
	bool startClicking = false;
	bool SpellReady = false;
	float clickCD = 0f;
	float pTime;
	Color spriteColor;
	bool isIntroVisible = false;

	// Use this for initialization
	void Start ()
	{
		coinText.text = coinNeeded.ToString ();
		if (!GameManager.GM.hasEnoughCoin_Plain (coinNeeded))
			coinText.color = Color.red;
		spriteColor = SpellSprite.GetComponent<SpriteRenderer> ().color;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (startClicking)
			clickCD += Time.deltaTime;
		if (startRe) {
			rep ();
		}
	}

	void rep ()
	{
		pTime += Time.deltaTime;
		if (pTime >= maxProduceTime) {
			pTime = 0f;
			SpellReady = true;
			BlurBorder.SetActive (true);
			SpellSprite.GetComponent<SpriteRenderer> ().color = Color.white;
			ProgressBarHolder.SetActive (false);
			startRe = false;
		}
		float percent = pTime / maxProduceTime;
		Progression.transform.localScale = new Vector3 (percent, Progression.transform.localScale.y, Progression.transform.localScale.z);
		Progression.transform.localPosition = new Vector3 ((1f - percent) * -4.5f, Progression.transform.localPosition.y, Progression.transform.localPosition.z);

	}

	void postCast ()
	{
		BlurBorder.SetActive (false);
		SpellSprite.GetComponent<SpriteRenderer> ().color = spriteColor;
		ProgressBarHolder.SetActive (true);
		ActiveRange.transform.localPosition = Vector3.zero;
		ActiveRange.SetActive (false);
		SpellReady = false;
		GameManager.GM.SpellStarter.SetActive (true);
		isIntroVisible = true;
		IntroEnter ();
	}

	void Cast ()
	{
		Collider2D[] hits = Physics2D.OverlapBoxAll (Camera.main.ScreenToWorldPoint (Input.mousePosition), new Vector2 (2.43f, 2.43f), 0f);
		foreach (Collider2D hit in hits) {
			if (hit != null && hit.tag == "Tower") {
				hit.gameObject.GetComponent<TowerControl> ().TempBuff (1.25f, 1.25f, 1.25f, 10f);
			}
		}
		postCast ();
	}

	void tryStart ()
	{
		if (!GameManager.GM.hasEnoughCoin (coinNeeded))
			return;
		if (startRe)
			return;
		ProgressBarHolder.SetActive (true);
		GameManager.GM.SpellStarter.SetActive (false);
		GameManager.GM.AddCoin (-coinNeeded);

		startRe = true;
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
			if (!SpellReady && !startRe)
				tryStart ();
			else {
				IntroEnter ();
			}
		}
		clickCD = 0f;
	}

	void IntroEnter ()
	{
		isIntroVisible = !isIntroVisible;
		if (isIntroVisible) {
			SpellName.text = "Boost(Spell)";
			SpellIntro.text = "Drag to incentivize Towers in an area";
			SpellFunction.text = "+25% Attack Power, Armor and Attack Speed Bonus";
			SpellPanel.MoveIn (GUIAnimSystem.eGUIMove.SelfAndChildren);
		} else {
			SpellPanel.MoveOut (GUIAnimSystem.eGUIMove.SelfAndChildren);
		}
	}

	public void OnBeginDrag (PointerEventData _EventData)
	{
		if (!SpellReady)
			return;
		ActiveRange.SetActive (true);
		ActiveRange.transform.localPosition = Vector3.zero;
	}

	public void OnDrag (PointerEventData _EventData)
	{
		if (!SpellReady)
			return;
		ActiveRange.transform.position = new Vector3 (Camera.main.ScreenToWorldPoint (Input.mousePosition).x, Camera.main.ScreenToWorldPoint (Input.mousePosition).y, transform.position.z);
	}

	public void OnEndDrag (PointerEventData _EventData)
	{
		if (!SpellReady)
			return;
		Vector3 mouseP = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		if (Vector2.Distance (mouseP, transform.position) <= 1f || Mathf.Abs (mouseP.y - transform.position.y) <= 1f) {
			//Cancel Spell
			ActiveRange.transform.localPosition = Vector3.zero;
			ActiveRange.SetActive (false);
		} else {
			Cast ();
		}
	}
}
