using System;
using UnityEngine;

// Token: 0x0200000E RID: 14
[AddComponentMenu("Dynamic Bone/Dynamic Bone Collider")]
public class DynamicBoneCollider : DynamicBoneColliderBase
{
	// Token: 0x0600005A RID: 90 RVA: 0x00004458 File Offset: 0x00002658
	private void OnValidate()
	{
		this.m_Radius = Mathf.Max(this.m_Radius, 0f);
		this.m_Height = Mathf.Max(this.m_Height, 0f);
	}

	// Token: 0x06000060 RID: 96 RVA: 0x000048A8 File Offset: 0x00002AA8
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
		float radius = this.m_Radius * Mathf.Abs(base.transform.lossyScale.x);
		float num = this.m_Height * 0.5f - this.m_Radius;
		if (num <= 0f)
		{
			Gizmos.DrawWireSphere(base.transform.TransformPoint(this.m_Center), radius);
			return;
		}
		Vector3 center = this.m_Center;
		Vector3 center2 = this.m_Center;
		switch (this.m_Direction)
		{
		case DynamicBoneColliderBase.Direction.X:
			center.x -= num;
			center2.x += num;
			break;
		case DynamicBoneColliderBase.Direction.Y:
			center.y -= num;
			center2.y += num;
			break;
		case DynamicBoneColliderBase.Direction.Z:
			center.z -= num;
			center2.z += num;
			break;
		}
		Gizmos.DrawWireSphere(base.transform.TransformPoint(center), radius);
		Gizmos.DrawWireSphere(base.transform.TransformPoint(center2), radius);
	}

	// Token: 0x04000044 RID: 68
	public float m_Radius = 0.5f;

	// Token: 0x04000045 RID: 69
	public float m_Height;
}
