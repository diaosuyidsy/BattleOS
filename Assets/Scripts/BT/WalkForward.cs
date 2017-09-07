using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class WalkForward : Action
{
	public float speed = 0f;

	public override TaskStatus OnUpdate ()
	{
		transform.Translate (Vector3.down * Time.deltaTime * speed, Space.World);
		return TaskStatus.Success;
	}
}