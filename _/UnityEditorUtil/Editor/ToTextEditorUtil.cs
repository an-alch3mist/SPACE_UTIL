// Claude 4.5 Sonnet - Enhanced Version //
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Linq;
using System.IO;

using SPACE_UTIL;

namespace SPACE_UnityEditor
{
	/// <summary>
	/// Enhanced Unity Editor script that adds hierarchy copying for both Scene GameObjects and Project assets.
	/// - Scene Hierarchy: Right-click GameObject → "Copy Hierarchy as Text"
	/// - Project Assets: Right-click any asset → "Copy Asset Hierarchy as Text"
	/// Includes detailed metadata for meshes, prefabs, audio clips, animations, etc.
	/// </summary>
	public static class CopyAsText
	{
		// Adjustable indentation settings
		private const string branch_char = "├ "; // ──
		private const string last_branch_char = "└ "; // ──
		private const string vertical_line = "│ ";
		private const string empty_indent_space = "  ";

		// Configurable ignore list for project hierarchy
		private static readonly string[] IGNORED_FOLDERS = new string[]
		{
			".git",
			".vs",
			".vscode",
			"Library",
			"Temp",
			"Logs",
			"obj",
			"Builds",
			".idea",
			"node_modules"
		};

		private static readonly string[] IGNORED_FILES = new string[]
		{
			".gitignore",
			".gitattributes",
			".DS_Store",
			"Thumbs.db",
			".meta"  // Already handled separately, but listed for completeness
		};

		// Component abbreviation system - maps component patterns to short codes
		// Format: "code" -> new string[] { "Component1", "Component2", ... }
		private static readonly System.Collections.Generic.Dictionary<string, string[]> COMPONENT_ABBREVIATIONS =
			new System.Collections.Generic.Dictionary<string, string[]>()
		{
			// Common rendering combinations
			{ "dmc", new[] { "MeshFilter", "MeshRenderer" } },  // Default Mesh Components
			{ "smc", new[] { "SkinnedMeshRenderer" } },         // Skinned Mesh Component
			
			// Physics
			{ "rb", new[] { "Rigidbody" } },
			{ "rb2d", new[] { "Rigidbody2D" } },
			
			// Colliders
			{ "bc", new[] { "BoxCollider" } },
			{ "sc", new[] { "SphereCollider" } },
			{ "cc", new[] { "CapsuleCollider" } },
			{ "mc", new[] { "MeshCollider" } },
			{ "bc2d", new[] { "BoxCollider2D" } },
			{ "cc2d", new[] { "CircleCollider2D" } },
			
			// Animation
			{ "anim", new[] { "Animator" } },
			{ "animclip", new[] { "Animation" } },
			
			// Audio
			{ "asrc", new[] { "AudioSource" } },
			{ "alstn", new[] { "AudioListener" } },
			
			// Rendering
			{ "cam", new[] { "Camera" } },
			{ "lgt", new[] { "Light" } },
			{ "canvas", new[] { "Canvas" } },
			{ "cr", new[] { "CanvasRenderer", } },
			{ "sr", new[] { "ScrollRect", } },
			{ "tmp", new[] { "TextMeshProUGUI", "TextMeshPro" } },
			
			// Common UI combinations
			{ "btnO", new[] { "Button", "Image", "Outline" } },
			{ "btn", new[] { "Button", "Image" } },
			{ "img", new[] { "Image" } },
			{ "autoFitH", new[] { "HorizontalLayoutGroup", "ContentSizeFitter" } },
			{ "autoFitV", new[] { "VerticalLayoutGroup", "ContentSizeFitter" } },
			{ "autoFit", new[] { "ContentSizeFitter" } },
			
			// Particles
			{ "ps", new[] { "ParticleSystem" } },
			{ "psr", new[] { "ParticleSystemRenderer" } },
			
			// Other
			{ "tr", new[] { "TrailRenderer" } },
			{ "lr", new[] { "LineRenderer" } },
		};

		// Toggle for using abbreviations (can be disabled for full component names)
		private const bool USE_COMPONENT_ABBREVIATIONS = true;
		private const bool USE_PREFAB_SCALE_SHORTHAND = true;  // Use "1.0" instead of "(1.00,1.00,1.00)" for uniform scales
		private const bool USE_ASSET_TYPE_ABBREVIATIONS = true;  // Abbreviate asset type names
		private const bool USE_UNIFORM_MESH_BOUNDS_SHORTHAND = true;  // Use "0.04u" instead of "0.04×0.04×0.04"

		// Asset type abbreviations
		private static readonly System.Collections.Generic.Dictionary<string, string> ASSET_TYPE_ABBREVIATIONS =
			new System.Collections.Generic.Dictionary<string, string>()
		{
			{ "Mesh", "mesh" },
			{ "Material", "mat" },
			{ "Prefab", "pf" },
			{ "Texture", "tex" },
			{ "AnimClip", "anim" },
			{ "Audio", "audio" },
			{ "Script", "cs" },
			{ "Scene", "scene" },
			{ "TextAsset", "txt" },
		};

		// Shader name abbreviations (common Unity shaders)
		private static readonly System.Collections.Generic.Dictionary<string, string> SHADER_ABBREVIATIONS =
			new System.Collections.Generic.Dictionary<string, string>()
		{
			{ "Universal Render Pipeline/Lit", "URP/Lit" },
			{ "Universal Render Pipeline/Unlit", "URP/Unlit" },
			{ "Universal Render Pipeline/Simple Lit", "URP/SimpleLit" },
			{ "Universal Render Pipeline/Baked Lit", "URP/BakedLit" },
			{ "Universal Render Pipeline/Particles/Lit", "URP/Particles/Lit" },
			{ "Universal Render Pipeline/Particles/Unlit", "URP/Particles/Unlit" },
			{ "Standard", "std" },
			{ "Standard (Specular setup)", "Std/Spec" },
			{ "Autodesk Interactive", "AutodeskInt" },
			{ "Unlit/Color", "Unlit/Col" },
			{ "Unlit/Texture", "Unlit/Tex" },
			{ "Unlit/Transparent", "Unlit/Trans" },
			{ "Legacy Shaders/Diffuse", "Legacy/Diff" },
			{ "Sprites/Default", "Sprite" }
		};

		#region Scene Hierarchy (GameObject)

		[MenuItem("GameObject/Copy Hierarchy as Text", false, 0)]
		private static void CopyHierarchyAsText()
		{
			GameObject selected = Selection.activeGameObject;

			if (selected == null)
			{
				Debug.Log("[CopyHierarchy] No GameObject selected!".colorTag("yellow"));
				return;
			}

			// Build hierarchy string
			string hierarchyStr = BuildSceneHierarchyString(selected);

			// Copy to clipboard
			EditorGUIUtility.systemCopyBuffer = hierarchyStr;

			// Log to file using UTIL.cs LOG system
			try
			{
				LOG.AddLog(hierarchyStr, syntaxType: "sceneGameObject-hierarchy");
				Debug.Log($"[CopyHierarchy] Copied hierarchy of '{selected.name}' to clipboard and saved to LOG.md".colorTag("lime"));
			}
			catch (System.Exception e)
			{
				Debug.Log($"[CopyHierarchy] Failed to log hierarchy: {e.Message}".colorTag("red"));
			}
		}

		[MenuItem("GameObject/Copy Hierarchy as Text", true)]
		private static bool ValidateCopyHierarchyAsText()
		{
			return Selection.activeGameObject != null;
		}

		/// <summary>
		/// Builds a formatted scene hierarchy string with GameObject metadata.
		/// </summary>
		private static string BuildSceneHierarchyString(GameObject root)
		{
			StringBuilder sb = new StringBuilder();

			// Add legend at the top if abbreviations are enabled
			if (USE_COMPONENT_ABBREVIATIONS)
			{
				sb.AppendLine("=== Component Abbreviations ===");
				foreach (var kvp in COMPONENT_ABBREVIATIONS)
				{
					sb.AppendLine($"{kvp.Key} = {string.Join(" | ", kvp.Value)}");
				}
				sb.AppendLine("================================");
				sb.AppendLine();
			}
			if (USE_ASSET_TYPE_ABBREVIATIONS == true)
			{
				sb.AppendLine("=== Asset Type Abbreviations ===");
				foreach (var kvp in ASSET_TYPE_ABBREVIATIONS)
					sb.AppendLine($"{kvp.Value} = {kvp.Key}");
				sb.AppendLine("================================");
				sb.AppendLine();
			}

			// Root level with metadata
			string rootMeta = GetGameObjectMetadata(root);
			sb.AppendLine($"./{root.name}/{rootMeta}");

			// Process children
			Transform rootTransform = root.transform;
			int childCount = rootTransform.childCount;

			for (int i = 0; i < childCount; i++)
			{
				bool isLastChild = (i == childCount - 1);
				Transform child = rootTransform.GetChild(i);
				BuildSceneHierarchyRecursive(child, "", isLastChild, sb);
			}

			return sb.ToString();
		}

		private static void BuildSceneHierarchyRecursive(Transform current, string prefix, bool isLast, StringBuilder sb)
		{
			string branchChar = isLast ? last_branch_char : branch_char;
			string metadata = GetGameObjectMetadata(current.gameObject);

			sb.AppendLine($"{prefix}{branchChar}{current.name} {metadata}");

			string childPrefix = prefix + (isLast ? empty_indent_space : vertical_line);

			int childCount = current.childCount;
			for (int i = 0; i < childCount; i++)
			{
				bool isLastChild = (i == childCount - 1);
				Transform child = current.GetChild(i);
				BuildSceneHierarchyRecursive(child, childPrefix, isLastChild, sb);
			}
		}

		/// <summary>
		/// Gets metadata for a GameObject: localScale and component names.
		/// </summary>
		private static string GetGameObjectMetadata(GameObject go)
		{
			Vector3 scale = go.transform.localScale;
			string scaleStr = FormatScale(scale);

			Component[] components = go.GetComponents<Component>();
			var componentNames = components
				.Where(c => c != null && !(c is Transform))
				.Select(c => c.GetType().Name)
				.ToList();

			string componentsStr = AbbreviateComponents(componentNames);

			return $"({scaleStr} | {componentsStr})";
		}

		/// <summary>
		/// Formats scale with shorthand for uniform scales.
		/// </summary>
		private static string FormatScale(Vector3 scale)
		{
			if (!USE_PREFAB_SCALE_SHORTHAND)
				return $"scale:({scale.x:F2},{scale.y:F2},{scale.z:F2})";

			// Check if scale is uniform (all components equal within small epsilon)
			bool isUniform = Mathf.Approximately(scale.x, scale.y) &&
							 Mathf.Approximately(scale.y, scale.z);

			if (isUniform)
			{
				return $"scale:{scale.x:F1}";
			}
			else
			{
				return $"scale:({scale.x:F1},{scale.y:F1},{scale.z:F1})";
			}
		}

		/// <summary>
		/// Abbreviates component names using the abbreviation dictionary.
		/// </summary>
		private static string AbbreviateComponents(System.Collections.Generic.List<string> componentNames)
		{
			if (componentNames.Count == 0)
				return "no components";

			if (!USE_COMPONENT_ABBREVIATIONS)
				return string.Join(", ", componentNames);

			var abbreviated = new System.Collections.Generic.List<string>();
			var remaining = new System.Collections.Generic.List<string>(componentNames);

			// Try to match component patterns
			foreach (var kvp in COMPONENT_ABBREVIATIONS)
			{
				bool allMatch = kvp.Value.All(comp => remaining.Contains(comp));

				if (allMatch)
				{
					abbreviated.Add(kvp.Key);
					foreach (string comp in kvp.Value)
					{
						remaining.Remove(comp);
					}
				}
			}

			// Add remaining unmatched components
			abbreviated.AddRange(remaining);

			return abbreviated.Count > 0 ? string.Join(", ", abbreviated) : "no components";
		}

		#endregion

		#region Project Hierarchy (Assets)

		[MenuItem("Assets/Copy Asset Hierarchy as Text", false, 20)]
		private static void CopyAssetHierarchyAsText()
		{
			Object selected = Selection.activeObject;

			if (selected == null)
			{
				Debug.Log("[CopyHierarchy] No asset selected!".colorTag("yellow"));
				return;
			}

			string assetPath = AssetDatabase.GetAssetPath(selected);
			if (string.IsNullOrEmpty(assetPath))
			{
				Debug.Log("[CopyHierarchy] Selected object is not an asset!".colorTag("yellow"));
				return;
			}

			// Build asset hierarchy string
			string hierarchyStr = BuildAssetHierarchyString(assetPath, selected);

			// Copy to clipboard
			EditorGUIUtility.systemCopyBuffer = hierarchyStr;

			// Log to file
			try
			{
				LOG.AddLog(hierarchyStr, syntaxType: "projectFolder-hierarchy");
				Debug.Log($"[CopyHierarchy] Copied asset hierarchy of '{selected.name}' to clipboard and saved to LOG.md".colorTag("lime"));
			}
			catch (System.Exception e)
			{
				Debug.Log($"[CopyHierarchy] Failed to log asset hierarchy: {e.Message}".colorTag("red"));
			}
		}

		[MenuItem("Assets/Copy Asset Hierarchy as Text", true)]
		private static bool ValidateCopyAssetHierarchyAsText()
		{
			return Selection.activeObject != null;
		}

		/// <summary>
		/// Builds asset hierarchy for folders and complex assets (FBX, Prefabs, etc.)
		/// </summary>
		private static string BuildAssetHierarchyString(string assetPath, Object rootAsset)
		{
			StringBuilder sb = new StringBuilder();

			// Add legend at the top if abbreviations are enabled
			if (USE_COMPONENT_ABBREVIATIONS)
			{
				sb.AppendLine("=== Component Abbreviations ===");
				foreach (var kvp in COMPONENT_ABBREVIATIONS)
				{
					sb.AppendLine($"{kvp.Key} = {string.Join(" | ", kvp.Value)}");
				}
				sb.AppendLine("================================");
				sb.AppendLine();
			}
			if (USE_ASSET_TYPE_ABBREVIATIONS == true)
			{
				sb.AppendLine("=== Asset Type Abbreviations ===");
				foreach (var kvp in ASSET_TYPE_ABBREVIATIONS)
					sb.AppendLine($"{kvp.Value} = {kvp.Key}");
				sb.AppendLine("================================");
				sb.AppendLine();
			}

			// Check if it's a folder
			if (AssetDatabase.IsValidFolder(assetPath))
			{
				sb.AppendLine($"./{Path.GetFileName(assetPath)}/");
				BuildFolderHierarchy(assetPath, "", sb);
			}
			else
			{
				// Single asset - show with sub-assets
				string metadata = GetAssetMetadata(rootAsset, assetPath);
				sb.AppendLine($"./{rootAsset.name} {metadata}");

				// Load all sub-assets (for FBX, prefabs, etc.)
				Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);

				for (int i = 0; i < subAssets.Length; i++)
				{
					bool isLast = (i == subAssets.Length - 1);
					string branchChar = isLast ? last_branch_char : branch_char;
					string subMeta = GetAssetMetadata(subAssets[i], assetPath);

					sb.AppendLine($"{branchChar}{subAssets[i].name} {subMeta}");
				}

				// For prefabs, also show internal GameObject hierarchy
				if (rootAsset is GameObject prefabRoot)
				{
					sb.AppendLine($"{branch_char}[Prefab Contents]");
					BuildPrefabHierarchy(prefabRoot, vertical_line, sb);
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Recursively builds folder hierarchy, excluding .meta files and ignored folders/files.
		/// </summary>
		private static void BuildFolderHierarchy(string folderPath, string prefix, StringBuilder sb)
		{
			string[] entries = Directory.GetFileSystemEntries(folderPath);
			var filtered = entries
				.Where(e => !e.EndsWith(".meta"))
				.Where(e => !ShouldIgnoreEntry(e))
				.ToArray();

			for (int i = 0; i < filtered.Length; i++)
			{
				bool isLast = (i == filtered.Length - 1);
				string branchChar = isLast ? last_branch_char : branch_char;
				string entryName = Path.GetFileName(filtered[i]);

				if (Directory.Exists(filtered[i]))
				{
					// Subfolder
					sb.AppendLine($"{prefix}{branchChar}{entryName}/");
					string childPrefix = prefix + (isLast ? empty_indent_space : vertical_line);
					BuildFolderHierarchy(filtered[i], childPrefix, sb);
				}
				else
				{
					// File
					Object asset = AssetDatabase.LoadAssetAtPath<Object>(filtered[i]);
					string metadata = asset != null ? GetAssetMetadata(asset, filtered[i]) : "";
					sb.AppendLine($"{prefix}{branchChar}{entryName} {metadata}");

					// Show sub-assets (meshes, animations in FBX)
					if (asset != null)
					{
						Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(filtered[i]);
						string childPrefix = prefix + (isLast ? empty_indent_space : vertical_line);

						for (int j = 0; j < subAssets.Length; j++)
						{
							bool isLastSub = (j == subAssets.Length - 1);
							string subBranch = isLastSub ? last_branch_char : branch_char;
							string subMeta = GetAssetMetadata(subAssets[j], filtered[i]);
							sb.AppendLine($"{childPrefix}{subBranch}{subAssets[j].name} {subMeta}");
						}
					}
				}
			}
		}

		/// <summary>
		/// Checks if a file or folder should be ignored based on the ignore lists.
		/// </summary>
		private static bool ShouldIgnoreEntry(string path)
		{
			string name = Path.GetFileName(path);

			// Check if it's a folder in ignore list
			if (Directory.Exists(path))
			{
				foreach (string ignoredFolder in IGNORED_FOLDERS)
				{
					if (name.Equals(ignoredFolder, System.StringComparison.OrdinalIgnoreCase))
						return true;
				}
			}
			else
			{
				// Check if it's a file in ignore list
				foreach (string ignoredFile in IGNORED_FILES)
				{
					if (name.Equals(ignoredFile, System.StringComparison.OrdinalIgnoreCase))
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Builds hierarchy for prefab internal structure.
		/// </summary>
		private static void BuildPrefabHierarchy(GameObject prefabRoot, string prefix, StringBuilder sb)
		{
			Transform rootTransform = prefabRoot.transform;
			int childCount = rootTransform.childCount;

			for (int i = 0; i < childCount; i++)
			{
				bool isLast = (i == childCount - 1);
				Transform child = rootTransform.GetChild(i);
				BuildPrefabHierarchyRecursive(child, prefix, isLast, sb);
			}
		}

		private static void BuildPrefabHierarchyRecursive(Transform current, string prefix, bool isLast, StringBuilder sb)
		{
			string branchChar = isLast ? last_branch_char : branch_char;
			string metadata = GetGameObjectMetadata(current.gameObject);

			sb.AppendLine($"{prefix}{branchChar}{current.name} {metadata}");

			string childPrefix = prefix + (isLast ? empty_indent_space : vertical_line);

			int childCount = current.childCount;
			for (int i = 0; i < childCount; i++)
			{
				bool isLastChild = (i == childCount - 1);
				Transform child = current.GetChild(i);
				BuildPrefabHierarchyRecursive(child, childPrefix, isLastChild, sb);
			}
		}

		/// <summary>
		/// Gets detailed metadata based on asset type.
		/// </summary>
		private static string GetAssetMetadata(Object asset, string assetPath)
		{
			if (asset == null) return "(unknown)";

			// Mesh
			if (asset is Mesh mesh)
			{
				Bounds bounds = mesh.bounds;
				string boundsStr = FormatBounds(bounds.size);
				string typeStr = AbbreviateAssetType("Mesh");
				return $"({typeStr} | {boundsStr} | v:{mesh.vertexCount})";
			}

			// Animation Clip
			if (asset is AnimationClip clip)
			{
				string typeStr = AbbreviateAssetType("AnimClip");
				return $"({typeStr} | {clip.length:F2}s | {clip.frameRate:F0}fps)";
			}

			// Audio Clip
			if (asset is AudioClip audio)
			{
				string typeStr = AbbreviateAssetType("Audio");
				return $"({typeStr} | {audio.length:F2}s | {audio.channels}ch)";
			}

			// Texture
			if (asset is Texture2D tex)
			{
				string typeStr = AbbreviateAssetType("Texture");
				return $"({typeStr} | {tex.width}×{tex.height} | {tex.format})";
			}

			// Material
			if (asset is Material mat)
			{
				string typeStr = AbbreviateAssetType("Material");
				string shaderName = AbbreviateShaderName(mat.shader.name);
				return $"({typeStr} | {shaderName})";
			}

			// Prefab (GameObject)
			if (asset is GameObject go)
			{
				Vector3 scale = go.transform.localScale;
				string scaleStr = FormatScale(scale);

				Component[] components = go.GetComponents<Component>();
				var componentNames = components
					.Where(c => c != null && !(c is Transform))
					.Select(c => c.GetType().Name)
					.ToList();

				string componentsStr = AbbreviateComponents(componentNames);
				string typeStr = AbbreviateAssetType("Prefab");

				return $"({typeStr} | {scaleStr} | {componentsStr})";
			}

			// Script/MonoScript
			if (asset is MonoScript script)
			{
				string typeStr = AbbreviateAssetType("Script");
				return $"({typeStr} | {script.GetClass()?.Name ?? "unknown"})";
			}

			// TextAsset (plain text / .txt / .md / etc)
			if (asset is TextAsset textAsset)
			{
				string typeStr = AbbreviateAssetType("TextAsset");
				// show length or first N chars if you like, or show extension:
				string ext = Path.GetExtension(assetPath);
				if (string.IsNullOrEmpty(ext)) ext = ".txt";
				// return $"({typeStr} | {ext} | {textAsset.text?.Length ?? 0} chars)";
				return $"({typeStr})";
			}


			// Scene
			if (assetPath.EndsWith(".unity"))
			{
				string typeStr = AbbreviateAssetType("Scene");
				return $"({typeStr})";
			}

			// Generic fallback
			return $"({asset.GetType().Name})";
		}

		/// <summary>
		/// Formats bounds with shorthand for uniform sizes.
		/// </summary>
		private static string FormatBounds(Vector3 size)
		{
			if (!USE_UNIFORM_MESH_BOUNDS_SHORTHAND)
				return $"bounds:{size.x:F2}×{size.y:F2}×{size.z:F2}";

			// Check if bounds are uniform (cube-like)
			bool isUniform = Mathf.Approximately(size.x, size.y) &&
							 Mathf.Approximately(size.y, size.z);

			if (isUniform)
			{
				return $"bounds:{size.x:F2}u";
			}
			else
			{
				return $"bounds:{size.x:F2}×{size.y:F2}×{size.z:F2}";
			}
		}

		/// <summary>
		/// Abbreviates asset type names.
		/// </summary>
		private static string AbbreviateAssetType(string typeName)
		{
			if (!USE_ASSET_TYPE_ABBREVIATIONS)
				return typeName;

			return ASSET_TYPE_ABBREVIATIONS.ContainsKey(typeName)
				? ASSET_TYPE_ABBREVIATIONS[typeName]
				: typeName;
		}

		/// <summary>
		/// Abbreviates shader names.
		/// </summary>
		private static string AbbreviateShaderName(string shaderName)
		{
			if (!USE_ASSET_TYPE_ABBREVIATIONS)
				return shaderName;

			return SHADER_ABBREVIATIONS.ContainsKey(shaderName)
				? SHADER_ABBREVIATIONS[shaderName]
				: shaderName;
		}

		#endregion

		#region Animator Controller Hierarchy

		[MenuItem("Assets/Copy Animator Controller Hierarchy", false, 21)]
		private static void CopyAnimatorControllerHierarchy()
		{
			Object selected = Selection.activeObject;

			if (selected == null || !(selected is UnityEditor.Animations.AnimatorController))
			{
				Debug.Log("[CopyHierarchy] No Animator Controller selected!".colorTag("yellow"));
				return;
			}

			UnityEditor.Animations.AnimatorController controller = selected as UnityEditor.Animations.AnimatorController;
			string hierarchyStr = BuildAnimatorControllerString(controller);

			// Copy to clipboard
			EditorGUIUtility.systemCopyBuffer = hierarchyStr;

			// Log to file
			try
			{
				LOG.AddLog(hierarchyStr, syntaxType: "animatorController-hierarchy");
				Debug.Log($"[CopyHierarchy] Copied Animator Controller '{controller.name}' to clipboard and saved to LOG.md".colorTag("lime"));
			}
			catch (System.Exception e)
			{
				Debug.Log($"[CopyHierarchy] Failed to log animator controller: {e.Message}".colorTag("red"));
			}
		}

		[MenuItem("Assets/Copy Animator Controller Hierarchy", true)]
		private static bool ValidateCopyAnimatorControllerHierarchy()
		{
			return Selection.activeObject is UnityEditor.Animations.AnimatorController;
		}

		/// <summary>
		/// Builds a formatted string representation of an Animator Controller.
		/// </summary>
		private static string BuildAnimatorControllerString(UnityEditor.Animations.AnimatorController controller)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine($"=== Animator Controller: {controller.name} ===");
			sb.AppendLine();

			// Parameters
			sb.AppendLine("Parameters:");
			if (controller.parameters.Length == 0)
			{
				sb.AppendLine("  (none)");
			}
			else
			{
				foreach (var param in controller.parameters)
				{
					string typeStr = GetParameterTypeString(param.type);
					string defaultVal = GetParameterDefaultValue(param);
					sb.AppendLine($"  {param.name} ({typeStr}) = {defaultVal}");
				}
			}
			sb.AppendLine();

			// Layers
			sb.AppendLine($"Layers ({controller.layers.Length}):");
			for (int i = 0; i < controller.layers.Length; i++)
			{
				var layer = controller.layers[i];
				sb.AppendLine($"{branch_char}Layer {i}: {layer.name}");
				sb.AppendLine($"{vertical_line}  Weight: {layer.defaultWeight:F2} | Blending: {layer.blendingMode} | IK: {layer.iKPass} | Sync: {(layer.syncedLayerIndex >= 0 ? $"Layer {layer.syncedLayerIndex}" : "None")}");
				sb.AppendLine();

				BuildStateMachineHierarchy(layer.stateMachine, vertical_line, sb);
				sb.AppendLine();
			}

			return sb.ToString();
		}

		/// <summary>
		/// Recursively builds state machine hierarchy with states and transitions.
		/// </summary>
		private static void BuildStateMachineHierarchy(UnityEditor.Animations.AnimatorStateMachine stateMachine, string prefix, StringBuilder sb)
		{
			// Entry transitions
			if (stateMachine.entryTransitions.Length > 0)
			{
				sb.AppendLine($"{prefix}Entry:");
				foreach (var trans in stateMachine.entryTransitions)
				{
					string conditionsStr = GetTransitionConditions(trans.conditions);
					string destName = trans.destinationState != null ? trans.destinationState.name : trans.destinationStateMachine?.name ?? "Unknown";
					sb.AppendLine($"{prefix}  {branch_char}[{conditionsStr}] → {destName}");
				}
			}

			// Default state indicator
			if (stateMachine.defaultState != null)
			{
				//sb.AppendLine($"{prefix}Entry → {stateMachine.defaultState.name}(The Default State)");
				sb.AppendLine($"{prefix}Entry:");
				sb.AppendLine($"{prefix}  {last_branch_char}({"default transition"}) → {stateMachine.defaultState.name}(The Default State)");
			}

			// Any State transitions
			if (stateMachine.anyStateTransitions.Length > 0)
			{
				sb.AppendLine($"{prefix}Any State:");
				int transI = 0;
				foreach (var trans in stateMachine.anyStateTransitions)
				{
					string transInfo = GetTransitionInfo(trans);
					string destName = trans.destinationState != null ? trans.destinationState.name : trans.destinationStateMachine?.name ?? "Unknown";
					string _branchChar = (transI == stateMachine.anyStateTransitions.Length - 1) ? last_branch_char : branch_char;
					sb.AppendLine($"{prefix}  {_branchChar}{transInfo} → {destName}");
					transI += 1;
				}
			}


			// States
			sb.AppendLine($"{prefix}States Info ({stateMachine.states.Length}):");
			for (int i = 0; i < stateMachine.states.Length; i++)
			{
				var childState = stateMachine.states[i];
				var state = childState.state;
				bool isLast = (i == stateMachine.states.Length - 1);
				string branchChar = isLast ? last_branch_char : branch_char;

				// Check if motion is a BlendTree
				string motionInfo = GetMotionInfo(state.motion);
				string stateInfo = $"{state.name} | {motionInfo} | Speed: {state.speed:F2}x";
				if (state == stateMachine.defaultState)
					stateInfo += " [DEFAULT]";

				sb.AppendLine($"{prefix}{branchChar}{stateInfo}");

				string childPrefix = prefix + (isLast ? empty_indent_space : vertical_line);
				// If it's a blend tree, show its structure
				if (state.motion is UnityEditor.Animations.BlendTree blendTree)
				{
					BuildBlendTreeHierarchy(blendTree, childPrefix + "  ", sb);
				}

				// Transitions from this state
				if (state.transitions.Length > 0)
				{
					for (int j = 0; j < state.transitions.Length; j++)
					{
						var trans = state.transitions[j];
						bool isLastTrans = (j == state.transitions.Length - 1);
						string transBranch = isLastTrans ? last_branch_char : branch_char;

						string transInfo = GetTransitionInfo(trans);
						string destName = trans.isExit ? "Exit" :
							(trans.destinationState != null ? trans.destinationState.name :
							trans.destinationStateMachine?.name ?? "Unknown");

						sb.AppendLine($"{childPrefix}{transBranch}{transInfo} → {destName}");
					}
				}
			}

			// Sub-state machines
			if (stateMachine.stateMachines.Length > 0)
			{
				sb.AppendLine($"{prefix}Sub-State Machines ({stateMachine.stateMachines.Length}):");
				for (int i = 0; i < stateMachine.stateMachines.Length; i++)
				{
					var childSM = stateMachine.stateMachines[i];
					bool isLast = (i == stateMachine.stateMachines.Length - 1);
					string branchChar = isLast ? last_branch_char : branch_char;

					sb.AppendLine($"{prefix}{branchChar}[StateMachine] {childSM.stateMachine.name}");
					string childPrefix = prefix + (isLast ? empty_indent_space : vertical_line);
					BuildStateMachineHierarchy(childSM.stateMachine, childPrefix + "  ", sb);
				}
			}
		}

		/// <summary>
		/// Gets motion information, detecting blend trees.
		/// </summary>
		private static string GetMotionInfo(Motion motion)
		{
			if (motion == null)
				return "Motion: (no motion)";

			if (motion is UnityEditor.Animations.BlendTree blendTree)
			{
				string blendType = GetBlendTreeTypeString(blendTree.blendType);
				return $"BlendTree: {motion.name} ({blendType})";
			}

			return $"Motion: {motion.name}";
		}

		/// <summary>
		/// Recursively builds blend tree hierarchy showing all child motions.
		/// </summary>
		private static void BuildBlendTreeHierarchy(UnityEditor.Animations.BlendTree blendTree, string prefix, StringBuilder sb)
		{
			sb.AppendLine($"{prefix}├ BlendParameter: {blendTree.blendParameter}" +
				(blendTree.blendType == UnityEditor.Animations.BlendTreeType.Simple1D ? "" : $", {blendTree.blendParameterY}"));

			var children = blendTree.children;
			for (int i = 0; i < children.Length; i++)
			{
				var child = children[i];
				bool isLast = (i == children.Length - 1);
				string branchChar = isLast ? last_branch_char : branch_char;

				string motionName = child.motion != null ? child.motion.name : "(null)";
				string thresholdInfo = blendTree.blendType == UnityEditor.Animations.BlendTreeType.Simple1D
					? $"threshold:{child.threshold:F2}"
					: $"pos:({child.position.x:F2},{child.position.y:F2})";

				sb.AppendLine($"{prefix}{branchChar}[{thresholdInfo}] {motionName} (speed:{child.timeScale:F2}x)");

				// If child is also a blend tree, recurse
				if (child.motion is UnityEditor.Animations.BlendTree childBlendTree)
				{
					string childPrefix = prefix + (isLast ? empty_indent_space : vertical_line);
					BuildBlendTreeHierarchy(childBlendTree, childPrefix + "  ", sb);
				}
			}
		}

		/// <summary>
		/// Converts blend tree type to readable string.
		/// </summary>
		private static string GetBlendTreeTypeString(UnityEditor.Animations.BlendTreeType blendType)
		{
			switch (blendType)
			{
				case UnityEditor.Animations.BlendTreeType.Simple1D: return "1D";
				case UnityEditor.Animations.BlendTreeType.SimpleDirectional2D: return "2D Directional";
				case UnityEditor.Animations.BlendTreeType.FreeformDirectional2D: return "2D Freeform Directional";
				case UnityEditor.Animations.BlendTreeType.FreeformCartesian2D: return "2D Freeform Cartesian";
				case UnityEditor.Animations.BlendTreeType.Direct: return "Direct";
				default: return blendType.ToString();
			}
		}

		/// <summary>
		/// Gets formatted transition information including conditions and timing.
		/// </summary>
		private static string GetTransitionInfo(UnityEditor.Animations.AnimatorStateTransition trans)
		{
			string conditionsStr = GetTransitionConditions(trans.conditions);
			string timing = $"exitTime:{trans.exitTime:F2} | transition:{trans.duration:F2}s";

			if (!trans.hasExitTime && trans.conditions.Length > 0)
			{
				return $"[{conditionsStr}] ({timing})";
			}
			else if (trans.hasExitTime && trans.conditions.Length > 0)
			{
				return $"[{conditionsStr}] ({timing})";
			}
			else if (trans.hasExitTime)
			{
				return $"[auto] ({timing})";
			}
			else
			{
				return $"[immediate] ({timing})";
			}
		}

		/// <summary>
		/// Gets transition conditions as formatted string.
		/// </summary>
		private static string GetTransitionConditions(UnityEditor.Animations.AnimatorCondition[] conditions)
		{
			if (conditions.Length == 0)
				return "no conditions";

			var conditionStrings = new System.Collections.Generic.List<string>();
			foreach (var condition in conditions)
			{
				string modeStr = GetConditionModeString(condition.mode);
				string valueStr = condition.mode == UnityEditor.Animations.AnimatorConditionMode.If ||
								 condition.mode == UnityEditor.Animations.AnimatorConditionMode.IfNot
					? ""
					: $" {condition.threshold}";

				conditionStrings.Add($"{condition.parameter} {modeStr}{valueStr}");
			}

			return string.Join(" && ", conditionStrings);
		}

		/// <summary>
		/// Converts parameter type enum to readable string.
		/// </summary>
		private static string GetParameterTypeString(UnityEngine.AnimatorControllerParameterType type)
		{
			switch (type)
			{
				case UnityEngine.AnimatorControllerParameterType.Float: return "float";
				case UnityEngine.AnimatorControllerParameterType.Int: return "int";
				case UnityEngine.AnimatorControllerParameterType.Bool: return "bool";
				case UnityEngine.AnimatorControllerParameterType.Trigger: return "trigger";
				default: return type.ToString();
			}
		}

		/// <summary>
		/// Gets parameter default value as string.
		/// </summary>
		private static string GetParameterDefaultValue(UnityEngine.AnimatorControllerParameter param)
		{
			switch (param.type)
			{
				case UnityEngine.AnimatorControllerParameterType.Float: return param.defaultFloat.ToString("F2");
				case UnityEngine.AnimatorControllerParameterType.Int: return param.defaultInt.ToString();
				case UnityEngine.AnimatorControllerParameterType.Bool: return param.defaultBool.ToString().ToLower();
				case UnityEngine.AnimatorControllerParameterType.Trigger: return "false";
				default: return "unknown";
			}
		}

		/// <summary>
		/// Converts condition mode enum to operator string.
		/// </summary>
		private static string GetConditionModeString(UnityEditor.Animations.AnimatorConditionMode mode)
		{
			switch (mode)
			{
				case UnityEditor.Animations.AnimatorConditionMode.If: return "= true";
				case UnityEditor.Animations.AnimatorConditionMode.IfNot: return "= false";
				case UnityEditor.Animations.AnimatorConditionMode.Greater: return ">";
				case UnityEditor.Animations.AnimatorConditionMode.Less: return "<";
				case UnityEditor.Animations.AnimatorConditionMode.Equals: return "==";
				case UnityEditor.Animations.AnimatorConditionMode.NotEqual: return "!=";
				default: return mode.ToString();
			}
		}

		#endregion

		#region Bonus: Copy Full Path

		[MenuItem("GameObject/Copy Full Path", false, 1)]
		private static void CopyFullPath()
		{
			GameObject selected = Selection.activeGameObject;

			if (selected == null)
			{
				Debug.Log("[CopyHierarchy] No GameObject selected!".colorTag("yellow"));
				return;
			}

			string path = GetFullPath(selected.transform);
			EditorGUIUtility.systemCopyBuffer = path;
			Debug.Log($"[CopyHierarchy] Copied path: {path}".colorTag("cyan"));
		}

		[MenuItem("GameObject/Copy Full Path", true)]
		private static bool ValidateCopyFullPath()
		{
			return Selection.activeGameObject != null;
		}

		private static string GetFullPath(Transform transform)
		{
			if (transform.parent == null)
				return transform.name;

			return GetFullPath(transform.parent) + "/" + transform.name;
		}

		#endregion
	}
}
