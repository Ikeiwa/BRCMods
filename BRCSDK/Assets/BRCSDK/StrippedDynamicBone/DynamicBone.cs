using System;
using System.Collections.Generic;
using Reptile;
using UnityEngine;

// Token: 0x0200000D RID: 13
[AddComponentMenu("Dynamic Bone/Dynamic Bone")]
public class DynamicBone : MonoBehaviour
{
	// Token: 0x0600003F RID: 63 RVA: 0x00003224 File Offset: 0x00001424
	private void Start()
	{
	}

	// Token: 0x06000040 RID: 64 RVA: 0x0000324E File Offset: 0x0000144E
	private void OnCoreUpdatePaused()
	{
	}

	// Token: 0x06000041 RID: 65 RVA: 0x00003257 File Offset: 0x00001457
	private void OnCoreUpdateUnPaused()
	{
	}

	// Token: 0x06000042 RID: 66 RVA: 0x00003260 File Offset: 0x00001460
	private void OnDestroy()
	{
	}

	// Token: 0x06000043 RID: 67 RVA: 0x00003284 File Offset: 0x00001484
	private void FixedUpdate()
	{
	}

	// Token: 0x06000044 RID: 68 RVA: 0x00003295 File Offset: 0x00001495
	private void Update()
	{
	}

	// Token: 0x06000045 RID: 69 RVA: 0x000032A8 File Offset: 0x000014A8
	private void LateUpdate()
	{
	}

	// Token: 0x06000046 RID: 70 RVA: 0x000032ED File Offset: 0x000014ED
	private void PreUpdate()
	{
	}

	// Token: 0x06000047 RID: 71 RVA: 0x00003314 File Offset: 0x00001514
	private void CheckDistance()
	{
	}

	// Token: 0x06000048 RID: 72 RVA: 0x0000339E File Offset: 0x0000159E
	private void OnEnable()
	{
	}

	// Token: 0x06000049 RID: 73 RVA: 0x000033A6 File Offset: 0x000015A6
	private void OnDisable()
	{
	}

	// Token: 0x0600004A RID: 74 RVA: 0x000033B0 File Offset: 0x000015B0
	private void OnValidate()
	{
		this.m_UpdateRate = Mathf.Max(this.m_UpdateRate, 0f);
		this.m_Damping = Mathf.Clamp01(this.m_Damping);
		this.m_Elasticity = Mathf.Clamp01(this.m_Elasticity);
		this.m_Stiffness = Mathf.Clamp01(this.m_Stiffness);
		this.m_Inert = Mathf.Clamp01(this.m_Inert);
		this.m_Radius = Mathf.Max(this.m_Radius, 0f);
		if (Application.isEditor && Application.isPlaying)
		{
			this.InitTransforms();
			this.SetupParticles();
		}
	}

	// Token: 0x0600004B RID: 75 RVA: 0x00003448 File Offset: 0x00001648
	private void OnDrawGizmosSelected()
	{
		if (!base.enabled || this.m_Root == null)
		{
			return;
		}
		if (Application.isEditor && !Application.isPlaying && base.transform.hasChanged)
		{
			this.InitTransforms();
			this.SetupParticles();
		}
		Gizmos.color = Color.white;
		for (int i = 0; i < this.m_Particles.Count; i++)
		{
			DynamicBone.Particle particle = this.m_Particles[i];
			if (particle.m_ParentIndex >= 0)
			{
				DynamicBone.Particle particle2 = this.m_Particles[particle.m_ParentIndex];
				Gizmos.DrawLine(particle.m_Position, particle2.m_Position);
			}
			if (particle.m_Radius > 0f)
			{
				Gizmos.DrawWireSphere(particle.m_Position, particle.m_Radius * this.m_ObjectScale);
			}
		}
	}

	// Token: 0x0600004C RID: 76 RVA: 0x00003511 File Offset: 0x00001711
	public void SetWeight(float w)
	{
		if (this.m_Weight != w)
		{
			if (w == 0f)
			{
				this.InitTransforms();
			}
			else if (this.m_Weight == 0f)
			{
				this.ResetParticlesPosition();
			}
			this.m_Weight = w;
		}
	}

	// Token: 0x0600004D RID: 77 RVA: 0x00003546 File Offset: 0x00001746
	public float GetWeight()
	{
		return this.m_Weight;
	}

	// Token: 0x0600004E RID: 78 RVA: 0x00003550 File Offset: 0x00001750
	private void UpdateDynamicBones(float t)
	{
	}

	// Token: 0x0600004F RID: 79 RVA: 0x00003678 File Offset: 0x00001878
	private void SetupParticles()
	{
		this.m_Particles.Clear();
		if (this.m_Root == null)
		{
			return;
		}
		this.m_LocalGravity = this.m_Root.InverseTransformDirection(this.m_Gravity);
		this.m_ObjectScale = Mathf.Abs(base.transform.lossyScale.x);
		this.m_ObjectPrevPosition = base.transform.position;
		this.m_ObjectMove = Vector3.zero;
		this.m_BoneTotalLength = 0f;
		this.AppendParticles(this.m_Root, -1, 0f);
		this.UpdateParameters();
	}

	// Token: 0x06000050 RID: 80 RVA: 0x00003710 File Offset: 0x00001910
	private void AppendParticles(Transform b, int parentIndex, float boneLength)
	{
		DynamicBone.Particle particle = new DynamicBone.Particle();
		particle.m_Transform = b;
		particle.m_ParentIndex = parentIndex;
		if (b != null)
		{
			particle.m_Position = (particle.m_PrevPosition = b.position);
			particle.m_InitLocalPosition = b.localPosition;
			particle.m_InitLocalRotation = b.localRotation;
		}
		else
		{
			Transform transform = this.m_Particles[parentIndex].m_Transform;
			if (this.m_EndLength > 0f)
			{
				Transform parent = transform.parent;
				if (parent != null)
				{
					particle.m_EndOffset = transform.InverseTransformPoint(transform.position * 2f - parent.position) * this.m_EndLength;
				}
				else
				{
					particle.m_EndOffset = new Vector3(this.m_EndLength, 0f, 0f);
				}
			}
			else
			{
				particle.m_EndOffset = transform.InverseTransformPoint(base.transform.TransformDirection(this.m_EndOffset) + transform.position);
			}
			particle.m_Position = (particle.m_PrevPosition = transform.TransformPoint(particle.m_EndOffset));
		}
		if (parentIndex >= 0)
		{
			boneLength += (this.m_Particles[parentIndex].m_Transform.position - particle.m_Position).magnitude;
			particle.m_BoneLength = boneLength;
			this.m_BoneTotalLength = Mathf.Max(this.m_BoneTotalLength, boneLength);
		}
		int count = this.m_Particles.Count;
		this.m_Particles.Add(particle);
		if (b != null)
		{
			for (int i = 0; i < b.childCount; i++)
			{
				bool flag = false;
				if (this.m_Exclusions != null)
				{
					for (int j = 0; j < this.m_Exclusions.Count; j++)
					{
						if (this.m_Exclusions[j] == b.GetChild(i))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					this.AppendParticles(b.GetChild(i), count, boneLength);
				}
				else if (this.m_EndLength > 0f || this.m_EndOffset != Vector3.zero)
				{
					this.AppendParticles(null, count, boneLength);
				}
			}
			if (b.childCount == 0 && (this.m_EndLength > 0f || this.m_EndOffset != Vector3.zero))
			{
				this.AppendParticles(null, count, boneLength);
			}
		}
	}

	// Token: 0x06000051 RID: 81 RVA: 0x00003970 File Offset: 0x00001B70
	public void UpdateParameters()
	{
		if (this.m_Root == null)
		{
			return;
		}
		this.m_LocalGravity = this.m_Root.InverseTransformDirection(this.m_Gravity);
		for (int i = 0; i < this.m_Particles.Count; i++)
		{
			DynamicBone.Particle particle = this.m_Particles[i];
			particle.m_Damping = this.m_Damping;
			particle.m_Elasticity = this.m_Elasticity;
			particle.m_Stiffness = this.m_Stiffness;
			particle.m_Inert = this.m_Inert;
			particle.m_Radius = this.m_Radius;
			if (this.m_BoneTotalLength > 0f)
			{
				float time = particle.m_BoneLength / this.m_BoneTotalLength;
				if (this.m_DampingDistrib != null && this.m_DampingDistrib.keys.Length != 0)
				{
					particle.m_Damping *= this.m_DampingDistrib.Evaluate(time);
				}
				if (this.m_ElasticityDistrib != null && this.m_ElasticityDistrib.keys.Length != 0)
				{
					particle.m_Elasticity *= this.m_ElasticityDistrib.Evaluate(time);
				}
				if (this.m_StiffnessDistrib != null && this.m_StiffnessDistrib.keys.Length != 0)
				{
					particle.m_Stiffness *= this.m_StiffnessDistrib.Evaluate(time);
				}
				if (this.m_InertDistrib != null && this.m_InertDistrib.keys.Length != 0)
				{
					particle.m_Inert *= this.m_InertDistrib.Evaluate(time);
				}
				if (this.m_RadiusDistrib != null && this.m_RadiusDistrib.keys.Length != 0)
				{
					particle.m_Radius *= this.m_RadiusDistrib.Evaluate(time);
				}
			}
			particle.m_Damping = Mathf.Clamp01(particle.m_Damping);
			particle.m_Elasticity = Mathf.Clamp01(particle.m_Elasticity);
			particle.m_Stiffness = Mathf.Clamp01(particle.m_Stiffness);
			particle.m_Inert = Mathf.Clamp01(particle.m_Inert);
			particle.m_Radius = Mathf.Max(particle.m_Radius, 0f);
		}
	}

	// Token: 0x06000052 RID: 82 RVA: 0x00003B6C File Offset: 0x00001D6C
	private void InitTransforms()
	{
		for (int i = 0; i < this.m_Particles.Count; i++)
		{
			DynamicBone.Particle particle = this.m_Particles[i];
			if (particle.m_Transform != null)
			{
				particle.m_Transform.localPosition = particle.m_InitLocalPosition;
				particle.m_Transform.localRotation = particle.m_InitLocalRotation;
			}
		}
	}

	// Token: 0x06000053 RID: 83 RVA: 0x00003BCC File Offset: 0x00001DCC
	private void ResetParticlesPosition()
	{
		for (int i = 0; i < this.m_Particles.Count; i++)
		{
			DynamicBone.Particle particle = this.m_Particles[i];
			if (particle.m_Transform != null)
			{
				particle.m_Position = (particle.m_PrevPosition = particle.m_Transform.position);
			}
			else
			{
				Transform transform = this.m_Particles[particle.m_ParentIndex].m_Transform;
				particle.m_Position = (particle.m_PrevPosition = transform.TransformPoint(particle.m_EndOffset));
			}
		}
		this.m_ObjectPrevPosition = base.transform.position;
	}

	// Token: 0x04000023 RID: 35
	[Tooltip("The root of the transform hierarchy to apply physics.")]
	public Transform m_Root;

	// Token: 0x04000024 RID: 36
	[Tooltip("Internal physics simulation rate.")]
	public float m_UpdateRate = 60f;

	// Token: 0x04000025 RID: 37
	public DynamicBone.UpdateMode m_UpdateMode;

	// Token: 0x04000026 RID: 38
	[Tooltip("How much the bones slowed down.")]
	[Range(0f, 1f)]
	public float m_Damping = 0.1f;

	// Token: 0x04000027 RID: 39
	public AnimationCurve m_DampingDistrib;

	// Token: 0x04000028 RID: 40
	[Tooltip("How much the force applied to return each bone to original orientation.")]
	[Range(0f, 1f)]
	public float m_Elasticity = 0.1f;

	// Token: 0x04000029 RID: 41
	public AnimationCurve m_ElasticityDistrib;

	// Token: 0x0400002A RID: 42
	[Tooltip("How much bone's original orientation are preserved.")]
	[Range(0f, 1f)]
	public float m_Stiffness = 0.1f;

	// Token: 0x0400002B RID: 43
	public AnimationCurve m_StiffnessDistrib;

	// Token: 0x0400002C RID: 44
	[Tooltip("How much character's position change is ignored in physics simulation.")]
	[Range(0f, 1f)]
	public float m_Inert;

	// Token: 0x0400002D RID: 45
	public AnimationCurve m_InertDistrib;

	// Token: 0x0400002E RID: 46
	[Tooltip("Each bone can be a sphere to collide with colliders. Radius describe sphere's size.")]
	public float m_Radius;

	// Token: 0x0400002F RID: 47
	public AnimationCurve m_RadiusDistrib;

	// Token: 0x04000030 RID: 48
	[Tooltip("If End Length is not zero, an extra bone is generated at the end of transform hierarchy.")]
	public float m_EndLength;

	// Token: 0x04000031 RID: 49
	[Tooltip("If End Offset is not zero, an extra bone is generated at the end of transform hierarchy.")]
	public Vector3 m_EndOffset = Vector3.zero;

	// Token: 0x04000032 RID: 50
	[Tooltip("The force apply to bones. Partial force apply to character's initial pose is cancelled out.")]
	public Vector3 m_Gravity = Vector3.zero;

	// Token: 0x04000033 RID: 51
	[Tooltip("The force apply to bones.")]
	public Vector3 m_Force = Vector3.zero;

	// Token: 0x04000034 RID: 52
	[Tooltip("Collider objects interact with the bones.")]
	public List<DynamicBoneColliderBase> m_Colliders;

	// Token: 0x04000035 RID: 53
	[Tooltip("Bones exclude from physics simulation.")]
	public List<Transform> m_Exclusions;

	// Token: 0x04000036 RID: 54
	private bool paused;

	// Token: 0x04000037 RID: 55
	[Tooltip("Constrain bones to move on specified plane.")]
	public DynamicBone.FreezeAxis m_FreezeAxis;

	// Token: 0x04000038 RID: 56
	[Tooltip("Disable physics simulation automatically if character is far from camera or player.")]
	private bool m_DisableAtADistance = true;

	// Token: 0x04000039 RID: 57
	public Transform m_ReferenceObject;

	// Token: 0x0400003A RID: 58
	public float m_DistanceToObject = 20f;

	// Token: 0x0400003B RID: 59
	private Vector3 m_LocalGravity = Vector3.zero;

	// Token: 0x0400003C RID: 60
	private Vector3 m_ObjectMove = Vector3.zero;

	// Token: 0x0400003D RID: 61
	private Vector3 m_ObjectPrevPosition = Vector3.zero;

	// Token: 0x0400003E RID: 62
	private float m_BoneTotalLength;

	// Token: 0x0400003F RID: 63
	private float m_ObjectScale = 1f;

	// Token: 0x04000040 RID: 64
	private float m_Time;

	// Token: 0x04000041 RID: 65
	private float m_Weight = 1f;

	// Token: 0x04000042 RID: 66
	private bool m_DistantDisabled;

	// Token: 0x04000043 RID: 67
	private List<DynamicBone.Particle> m_Particles = new List<DynamicBone.Particle>();

	// Token: 0x02000322 RID: 802
	public enum UpdateMode
	{
		// Token: 0x04001D66 RID: 7526
		Normal,
		// Token: 0x04001D67 RID: 7527
		AnimatePhysics,
		// Token: 0x04001D68 RID: 7528
		UnscaledTime
	}

	// Token: 0x02000323 RID: 803
	public enum FreezeAxis
	{
		// Token: 0x04001D6A RID: 7530
		None,
		// Token: 0x04001D6B RID: 7531
		X,
		// Token: 0x04001D6C RID: 7532
		Y,
		// Token: 0x04001D6D RID: 7533
		Z
	}

	// Token: 0x02000324 RID: 804
	private class Particle
	{
		// Token: 0x04001D6E RID: 7534
		public Transform m_Transform;

		// Token: 0x04001D6F RID: 7535
		public int m_ParentIndex = -1;

		// Token: 0x04001D70 RID: 7536
		public float m_Damping;

		// Token: 0x04001D71 RID: 7537
		public float m_Elasticity;

		// Token: 0x04001D72 RID: 7538
		public float m_Stiffness;

		// Token: 0x04001D73 RID: 7539
		public float m_Inert;

		// Token: 0x04001D74 RID: 7540
		public float m_Radius;

		// Token: 0x04001D75 RID: 7541
		public float m_BoneLength;

		// Token: 0x04001D76 RID: 7542
		public Vector3 m_Position = Vector3.zero;

		// Token: 0x04001D77 RID: 7543
		public Vector3 m_PrevPosition = Vector3.zero;

		// Token: 0x04001D78 RID: 7544
		public Vector3 m_EndOffset = Vector3.zero;

		// Token: 0x04001D79 RID: 7545
		public Vector3 m_InitLocalPosition = Vector3.zero;

		// Token: 0x04001D7A RID: 7546
		public Quaternion m_InitLocalRotation = Quaternion.identity;
	}
}
