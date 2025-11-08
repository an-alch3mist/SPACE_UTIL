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
	public static class CopyHierarchyToClipboard
	{
		// Adjustable indentation settings
		private const string branch_char = "├ ";
		private const string last_branch_char = "└ ";
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

			{ "txt", new[] { "TextAsset" } },
			
			// Rendering
			{ "cam", new[] { "Camera" } },
			{ "lit", new[] { "Light" } },
			{ "canvas", new[] { "Canvas" } },
			{ "cr", new[] { "CanvasRenderer", } },
			{ "sr", new[] { "ScrollRect", } },
			{ "tmp", new[] { "TextMeshProUGUI", "TextMeshPro" } },
			
			// Common UI combinations
			{ "btnO", new[] { "Button", "Image", "Outline" } },
			{ "btn", new[] { "Button", "Image" } },
			{ "autoFit", new[] { "HorizontalLayoutGroup", "ContentSizeFitter" } },
			{ "img", new[] { "Image" } },
			
			// Particles
			{ "ps", new[] { "ParticleSystem" } },
			{ "psr", new[] { "ParticleSystemRenderer" } },
			
			// Other
			{ "trail", new[] { "TrailRenderer" } },
			{ "line", new[] { "LineRenderer" } },
		};

		// Toggle for using abbreviations (can be disabled for full component names)
		private const bool USE_COMPONENT_ABBREVIATIONS = true;
		private const bool USE_SCALE_SHORTHAND = true;  // Use "1.0" instead of "(1.00,1.00,1.00)" for uniform scales
		private const bool USE_ASSET_TYPE_ABBREVIATIONS = true;  // Abbreviate asset type names
		private const bool USE_UNIFORM_BOUNDS_SHORTHAND = true;  // Use "0.04u" instead of "0.04×0.04×0.04"

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
			{ "Scene", "scene" }
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
			{ "Standard", "Std" },
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
				LOG.AddLog(hierarchyStr, syntaxType: "scene-hierarchy");
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
			if (!USE_SCALE_SHORTHAND)
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
				LOG.AddLog(hierarchyStr, syntaxType: "project-hierarchy");
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
			if (!USE_UNIFORM_BOUNDS_SHORTHAND)
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