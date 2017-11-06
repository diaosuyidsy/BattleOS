using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FortifyControl : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public static FortifyControl FC;
	public GameObject Progression;
	public GameObject SpellSprite;
	public GameObject BlurBorder;
	public GameObject ProgressBarHolder;
	public GameObject ActiveRange;
	public float maxProduceTime;
	public GAui SpellPanel;
	public Text SpellName;
	public Text SpellIntro;
	public Text SpellFunction;
	// Used to identiy action of dragging spell
	// Not to Slow down time if doing so
	public bool draggingSpell = false;

	bool gameStarted = false;
	bool startRe = false;
	bool startClicking = false;
	bool SpellReady = false;
	float clickCD = 0f;
	float pTime;
	Color spriteColor;
	bool isIntroVisible = false;

	void Awake ()
	{
		FC = this;
	}

	// Use this for initialization
	void Start ()
	{
		spriteColor = SpellSprite.GetComponent<SpriteRenderer> ().color;
	}
		
	// Update is called once per frame
	void Update ()
	{
		if (startClicking)
			clickCD += Time.deltaTime;
		if (!SpellReady && gameStarted) {
			rep ();
		}
	}

	public void StartGame ()
	{
		gameStarted = true;
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
		if (startRe)
			return;
		ProgressBarHolder.SetActive (true);

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
			IntroEnter ();
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
		draggingSpell = true;
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
		draggingSpell = false;
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
