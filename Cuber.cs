using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CubeSlimes
{
	internal static class Cuber
	{
		public static void Cubify(this SlimeAppearance appearance)
		{
			if (appearance == null)
				return;

			if (appearance.Structures == null)
				return;

			foreach (SlimeAppearanceStructure structure in appearance.Structures)
				structure.Cubify();
		}
		public static void Cubify(this SlimeAppearanceStructure structure)
		{
			if (structure == null)
				return;

			if (structure.Element == null)
				return;

			structure.Element.Cubify();
		}
		public static void Cubify(this SlimeAppearanceElement element)
		{
			if (element == null)
				return;

			if (element.Prefabs == null)
				return;

			foreach (SlimeAppearanceObject prefab in element.Prefabs)
			{
				if (prefab == null)
					continue;
				prefab.Cubify();
			}
		}
		public static void Cubify(this SlimeAppearanceObject prefab)
		{
			if (prefab == null)
				return;
			prefab.gameObject.Cubify();
		}
		public static void Cubify(this GameObject gameObject)
		{
			if (gameObject == null)
				return;
			foreach (MeshFilter meshFilter in gameObject.GetComponentsInChildren<MeshFilter>())
			{
				if (meshFilter == null)
					continue;
				meshFilter.Cubify();
			}
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				if (skinnedMeshRenderer == null)
					continue;
				skinnedMeshRenderer.Cubify();
			}
		}
		public static void Cubify(this MeshFilter meshFilter)
		{
			if (meshFilter == null)
				return;
			if (EntryPoint.replaceMeshes.Contains(meshFilter.name.RemoveClone()) && EntryPoint.CubeMesh != null)
			{
				if (meshFilter.sharedMesh != EntryPoint.CubeMesh)
					meshFilter.sharedMesh = EntryPoint.CubeMesh;
				if (meshFilter.mesh != EntryPoint.CubeMesh)
					meshFilter.mesh = EntryPoint.CubeMesh;
			}
		}
		public static void Cubify(this SkinnedMeshRenderer skinnedMeshRenderer)
		{
			if (skinnedMeshRenderer == null)
				return;
			if (EntryPoint.replaceMeshes.Contains(skinnedMeshRenderer.name.RemoveClone()) && EntryPoint.CubeMesh != null)
			{
				if (skinnedMeshRenderer.sharedMesh != EntryPoint.CubeMesh)
					skinnedMeshRenderer.sharedMesh = EntryPoint.CubeMesh;
			}
		}
	}
}
