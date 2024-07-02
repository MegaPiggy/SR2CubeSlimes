using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using System.IO;
using MelonLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static CubeSlimes.EntryPoint;
using Object = UnityEngine.Object;
using Il2CppMonomiPark.SlimeRancher.Script.Util;
using Il2CppMonomiPark.SlimeRancher;
using Il2Cpp;

namespace CubeSlimes
{
	public class EntryPoint : MelonMod
	{
		//The Meshes to be replaced
		internal static readonly string[] replaceMeshes = {
			"slime_default",
			"slime_default_LOD1",
			"slime_default_LOD2",
			"slime_default_LOD3",
			"slime_default_v4_LOD0",
			"slime_default_v4_LOD1",
			"slime_default_v4_LOD2",
			"slime_default_v4_LOD3",
			"saber_exotic_LOD1",
			"saber_exotic_LOD2",
			"slime_exotic_default_LOD1",
			"slime_exotic_default_LOD2",
			"slime_exotic_default_LOD3",
			"slime_exotic_gold_LOD1",
			"slime_exotic_gold_LOD2",
			"slime_exotic_gold_LOD3",
			"lucky_exotic_bib_LOD1",
			"lucky_exotic_bib_LOD2",
#if COIN
			"luckyexotic_coin_LOD0",
			"luckycat_coin_LOD0",
			"luckycat_coin_LOD1",
#endif
			"slime_face_LOD0",
			"slime_face_LOD1",
			"slime_face_LOD2",
			"slime_face_LOD3",
			"slime_saber_LOD1",
			"slime_saber_LOD2",
			"saber_exotic_LOD1",
			"saber_exotic_LOD2",
			"slime_puddle_LOD1",
			"mosaic_LOD1",
			"mosaic_LOD2",
			"mosaic_LOD3",
			"mosaic_exotic_shards_LOD1",
			"rocks_ball",
			"rock_exotic_ball_LOD0",
			"slime_tarr",
			"slime_tarr_bite",
			"slime_gold",
			"slime_quicksilver",
			"slime_saber",
		};

		internal static System.Reflection.Assembly execAssembly = System.Reflection.Assembly.GetExecutingAssembly();
		internal static Transform DisabledParent;
		internal static byte[] cubeslimeBuffer = GetAsset("cubeslime");
		internal static AssetBundle cubeslime = AssetBundle.LoadFromMemory(cubeslimeBuffer);

		public static byte[] GetAsset(string path)
		{
			var stream = execAssembly.GetManifestResourceStream($"{execAssembly.GetName().Name}.{path}");
			byte[] data = new byte[stream.Length];
			stream.Read(data, 0, data.Length);
			return data;
		}

		public static Mesh CubeMesh => cubeslime.LoadFromObject<MeshFilter>("assets/cube_slime.obj").sharedMesh;

		public static T GetOrDefault<T>(string name) where T : UnityEngine.Object => Resources.FindObjectsOfTypeAll<T>().FirstOrDefault(x => x.name == name);

		public override void OnInitializeMelon()
		{
			SystemContext.IsModded = true;
			MelonLogger.Msg("Initializing");
			DisabledParent = new GameObject("DisabledParent").transform;
			DisabledParent.gameObject.SetActive(false);
			UnityEngine.Object.DontDestroyOnLoad(DisabledParent.gameObject);
			DisabledParent.gameObject.hideFlags |= HideFlags.HideAndDontSave;
			Action<Scene, LoadSceneMode> OnSceneLoaded = (scene, loadSceneMode) =>
			{
				CubifyEverything(scene);
			};
			SceneManager.sceneLoaded += OnSceneLoaded;
			CubifyEverything(SceneManager.GetActiveScene());
		}

		public static void CubifyEverything(Scene scene)
		{
			foreach (MeshFilter meshFilter in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<MeshFilter>()))
				meshFilter.Cubify();

			foreach (SkinnedMeshRenderer skinnedMeshRenderer in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<SkinnedMeshRenderer>()))
				skinnedMeshRenderer.Cubify();

			MeshFilter[] filters = Resources.FindObjectsOfTypeAll<MeshFilter>();
			foreach (MeshFilter meshFilter in filters)
				meshFilter.Cubify();

			SkinnedMeshRenderer[] skinned = Resources.FindObjectsOfTypeAll<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinned)
				skinnedMeshRenderer.Cubify();

			SlimeAppearanceElement[] elements = Resources.FindObjectsOfTypeAll<SlimeAppearanceElement>();
			foreach (SlimeAppearanceElement element in elements)
			{
				element.Cubify();
				string name = element.name.RemoveClone();
				string lowerName = name.ToLower();
				if (lowerName.EndsWith("body") || lowerName.EndsWith("body_v4") || lowerName.EndsWith("face"))
				{
					for (int index = 0; index <= 3; index++)
					{
						SlimeAppearanceObject lod = element.Prefabs.ElementAtOrDefault(index);
						if (lod != null)
						{
							lod.Cubify();
						}
					}
					SlimeAppearanceObject lod3 = element.Prefabs.ElementAtOrDefault(3);
					if (lod3 == null || !lod3.name.EndsWith("3"))
						continue;
					else
					{
						lod3.IgnoreLODIndex = true;
					}
					if (name.StartsWith("QuickSilver") || name.StartsWith("Saucer"))
						element.Prefabs = new SlimeAppearanceObject[4] { element.Prefabs.ElementAtOrDefault(0), element.Prefabs.ElementAtOrDefault(1), element.Prefabs.ElementAtOrDefault(2), lod3 };
					else if (name.StartsWith("Gold"))
						element.Prefabs = new SlimeAppearanceObject[5] { element.Prefabs.ElementAtOrDefault(1), element.Prefabs.ElementAtOrDefault(2), lod3, element.Prefabs.ElementAtOrDefault(4), element.Prefabs.ElementAtOrDefault(5) };
					else
						element.Prefabs = new SlimeAppearanceObject[3] { element.Prefabs.ElementAtOrDefault(1), element.Prefabs.ElementAtOrDefault(2), lod3 };
				}
			}
		}

	}


	[HarmonyPatch(typeof(TimeDirector), nameof(TimeDirector.Update))]
	internal static class LODQuality1
	{
		public static void Postfix()
		{
			QualitySettings.skinWeights = SkinWeights.TwoBones;
			QualitySettings.lodBias = 0.5f;
			QualitySettings.maximumLODLevel = 1;
		}
	}

	[HarmonyPatch(typeof(SavedProfile), nameof(SavedProfile.PushOptions))]
	internal static class LODQuality2
	{
		public static void Postfix()
		{
			QualitySettings.skinWeights = SkinWeights.TwoBones;
			QualitySettings.lodBias = 0.5f;
			QualitySettings.maximumLODLevel = 1;
		}
	}
}