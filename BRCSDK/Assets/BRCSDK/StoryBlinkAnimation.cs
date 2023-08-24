using System;
using UnityEngine;

namespace Reptile
{
	public class StoryBlinkAnimation : MonoBehaviour
	{
		public SkinnedMeshRenderer mainRenderer;

		public Mesh characterMesh;

		private float blinkTimer = 4.6f;

		private float blinkDuration = 0.2f;

		private bool blink;
	}
}