using System;
using UnityEngine;

// Token: 0x02000010 RID: 16
[AddComponentMenu("Dynamic Bone/Dynamic Bone Plane Collider")]
public class DynamicBonePlaneCollider : DynamicBoneColliderBase
{
	// Token: 0x06000064 RID: 100 RVA: 0x000049F7 File Offset: 0x00002BF7
	private void OnValidate()
	{
	}

	// Token: 0x06000066 RID: 102 RVA: 0x00004AC8 File Offset: 0x00002CC8
	private void OnDrawGizmosSelected()
	{
		if (!base.enabled)
		{
			return;
		}
		if (this.m_Bound == DynamicBoneColliderBase.Bound.Outside)
		{
			Gizmos.color = Color.yellow;
		}
		else
		{
			Gizmos.color = Color.magenta;
		}
		Vector3 b = Vector3.up;
		switch (this.m_Direction)
		{
		case DynamicBoneColliderBase.Direction.X:
			b = base.transform.right;
			break;
		case DynamicBoneColliderBase.Direction.Y:
			b = base.transform.up;
			break;
		case DynamicBoneColliderBase.Direction.Z:
			b = base.transform.forward;
			break;
		}
		Vector3 vector = base.transform.TransformPoint(this.m_Center);
		Gizmos.DrawLine(vector, vector + b);
	}
}
