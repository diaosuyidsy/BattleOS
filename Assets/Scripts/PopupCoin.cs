using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupCoin : MonoBehaviour
{
	public Text CoinText;
	private Animator animator;

	// Use this for initialization
	void Start ()
	{
		animator = GetComponent<Animator> ();
		AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo (0);
		Destroy (gameObject, clipInfo [0].clip.length);

	}

	public void setText (int coinamount)
	{
		CoinText.text = coinamount.ToString ();
	}
}
