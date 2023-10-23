using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Extensions
{
	public static T LoadFromObject<T>(this AssetBundle bundle, string name) where T : Object => ((Il2CppObjectBase)bundle.LoadAsset(name)).Cast<GameObject>().GetComponentInChildren<T>();

	public static void GenerateBoneData(this SlimeAppearanceApplicator slimePrefab, SlimeAppearanceObject bodyApp, float scale = 1)
	{
		var mesh = bodyApp.GetComponent<SkinnedMeshRenderer>().sharedMesh;
		bodyApp.AttachedBones = new SlimeAppearance.SlimeBone[] { SlimeAppearance.SlimeBone.Core, SlimeAppearance.SlimeBone.JiggleRight, SlimeAppearance.SlimeBone.JiggleLeft, SlimeAppearance.SlimeBone.JiggleTop, SlimeAppearance.SlimeBone.JiggleBottom, SlimeAppearance.SlimeBone.JiggleFront, SlimeAppearance.SlimeBone.JiggleBack };
		var v = mesh.vertices;
		var max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
		var min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		var sum = Vector3.zero;
		for (int i = 0; i < v.Length; i++)
		{
			sum += v[i];
			if (v[i].x > max.x)
				max.x = v[i].x;
			if (v[i].x < min.x)
				min.x = v[i].x;
			if (v[i].y > max.y)
				max.y = v[i].y;
			if (v[i].y < min.y)
				min.y = v[i].y;
			if (v[i].z > max.z)
				max.z = v[i].z;
			if (v[i].z < min.z)
				min.z = v[i].z;
		}
		var center = sum / v.Length;
		var dis = 0f;
		foreach (var ver in v)
			dis += (ver - center).magnitude;
		dis /= v.Length;
		var b = new BoneWeight[v.Length];
		for (int i = 0; i < v.Length; i++)
		{
			var r = v[i] - center;
			var o = Mathf.Clamp01((r.magnitude - (dis / 4)) / (dis / 2));
			b[i] = new BoneWeight();
			if (o == 0)
				b[i].weight0 = 1;
			else
			{
				b[i].weight0 = 1 - o;
				b[i].boneIndex1 = r.x >= 0 ? 1 : 2;
				b[i].boneIndex2 = r.y >= 0 ? 3 : 4;
				b[i].boneIndex3 = r.z >= 0 ? 5 : 6;
				var n = r.Abs();
				var s = n.ToArray().Sum();
				b[i].weight1 = n.x / s * o;
				b[i].weight2 = n.y / s * o;
				b[i].weight3 = n.z / s * o;
			}
			b[i].weight0 *= scale;
			b[i].weight1 *= scale;
			b[i].weight2 *= scale;
			b[i].weight3 *= scale;
		}
		mesh.boneWeights = b;

		var p = new Matrix4x4[bodyApp.AttachedBones.Length];
		for (int i = 0; i < bodyApp.AttachedBones.Length; i++)
			p[i] = slimePrefab.Bones.First((x) => x.Bone == bodyApp.AttachedBones[i]).BoneObject.transform.worldToLocalMatrix * slimePrefab.Bones.First((x) => x.Bone == SlimeAppearance.SlimeBone.Root).BoneObject.transform.localToWorldMatrix;
		mesh.bindposes = p;
	}

	public static float[] ToArray(this Vector3 value) => new float[] { value.x, value.y, value.z };
	public static Vector3 Abs(this Vector3 value) => new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));

	public static string RemoveClone(this string str) => str.Replace("(Clone)", "").Trim();

	public static T Instantiate<T>(this T obj) where T : Object
	{
		T @new = Object.Instantiate(obj);
		@new.name = obj.name;
		return @new;
	}

	public static void Log(this object @this) => MelonLoader.MelonLogger.Msg(@this);
}