using System;
using UnityEngine;

// Token: 0x0200000F RID: 15
public class DynamicBoneColliderBase : MonoBehaviour
{

	// Token: 0x04000046 RID: 70
	public DynamicBoneColliderBase.Direction m_Direction = DynamicBoneColliderBase.Direction.Y;

	// Token: 0x04000047 RID: 71
	public Vector3 m_Center = Vector3.zero;

	// Token: 0x04000048 RID: 72
	public DynamicBoneColliderBase.Bound m_Bound;

	// Token: 0x02000325 RID: 805
	public enum Direction
	{
		// Token: 0x04001D7C RID: 7548
		X,
		// Token: 0x04001D7D RID: 7549
		Y,
		// Token: 0x04001D7E RID: 7550
		Z
	}

	// Token: 0x02000326 RID: 806
	public enum Bound
	{
		// Token: 0x04001D80 RID: 7552
		Outside,
		// Token: 0x04001D81 RID: 7553
		Inside
	}
}
