using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class WalkUntilTime : Conditional
{
	public float timeWalking;
	public float walkingSpeed;

	public float timeSinceBeginning;

	public override void OnStart ()
	{
		timeSinceBeginning = 0f;
	}

	public override TaskStatus OnUpdate ()
	{
		if (timeSinceBeginning <= timeWalking)
			return TaskStatus.Failure;
		else {
			timeSinceBeginning += Time.deltaTime;
			transform.Translate (Vector3.down * Time.deltaTime * walkingSpeed, Space.World);
			return TaskStatus.Success;
		}
	}
}