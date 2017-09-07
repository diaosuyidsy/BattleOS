using UnityEngine;
using System.Linq;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class WithinDistance : Conditional
{
	public GameObject bossHead;

	public override TaskStatus OnUpdate ()
	{
		RaycastHit2D[] hits = Physics2D.BoxCastAll (bossHead.transform.position, new Vector2 (7f, 0.3f), 0f, Vector2.up);
		if (hits.Any (h => h.collider != null && h.collider.tag == "Tower")) {
			return TaskStatus.Success;

		}
		return TaskStatus.Failure;
	}
}