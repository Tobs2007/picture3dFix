using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using ResoniteHotReloadLib;
using System.Collections.Generic;
using System.Reflection;

namespace picture3dFix;
public class picture3dFix : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.0.0";
	public override string Name => "picture3dFix";
	public override string Author => "Tobs";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/tobs2007/picture3dFix/";
	
	const string harmonyId = "com.tobsstuff.picture3dFix";

			




	public override void OnEngineInit() {
		HotReloader.RegisterForHotReload(this);
		init();
	}
	static void init() {
		Harmony harmony = new Harmony(harmonyId);

		// thanks yosh :)
		var og = typeof(PhotoCaptureManager)
			.GetNestedType("<>c__DisplayClass63_0", BindingFlags.Instance | BindingFlags.NonPublic)
			.GetNestedType("<<TakePhoto>b__0>d", BindingFlags.Instance | BindingFlags.NonPublic)
			.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
		Msg("got the function");
		var pfix = typeof(PatchFingerPhoto).GetMethod("postfix", BindingFlags.Static | BindingFlags.Public);

		Msg(og.GetType().ToString());
		Msg("1");
		Msg(pfix.GetType().ToString());
		Msg("prepatch");
		harmony.Patch(og, postfix: pfix);
		Msg("postpatch");
		harmony.PatchAll();
		Msg("initilized picture3dfix");
	}
	static void BeforeHotReload() {

		Harmony harmony = new Harmony(harmonyId);
		harmony.UnpatchAll(harmonyId);
	}

	static void OnHotReload(ResoniteMod modInstance) {
		init();
	}

	[HarmonyPatch(typeof(InteractiveCamera), "SpawnPhoto")]
	class InteractiveCamera_SpawnPhoto_Patch {
		static void Postfix(InteractiveCamera __instance) {

			if (__instance.CameraMode == InteractiveCamera.Mode.CameraStereo) {

				Slot photoRoot = __instance.Slot;
				if (__instance.SpawnPhotoInWorld.Value) {
					photoRoot = __instance.PhotoSpawnPoint;
				}
				List<Slot> children = photoRoot.GetAllChildren();
				Slot img = children[children.Count - 1];
				fixSlot(img);
			}
		}
	}


	// Patch for finger photo
	public static class PatchFingerPhoto {
		public static void postfix() {
			try {
				User user = Engine.Current.WorldManager.FocusedWorld.LocalUser;
				Slot userRoot = user.LocalUserRoot.Slot;
				List<Slot> children = userRoot.GetAllChildren();
				Slot img = children[children.Count - 1];
				if (img.Name == "Photo") {
					fixSlot(img);
				}
			} catch (System.Exception) { }
		}
	}


	

	static void fixSlot(Slot slot) {
		slot.GetComponent<QuadMesh>().DualSided.Value = true;
		slot.GetComponent<UnlitMaterial>().Sidedness.Value = Sidedness.Front;
	}
}
