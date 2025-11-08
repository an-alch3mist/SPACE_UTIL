// Claude 4.5 Sonnet //
using UnityEngine;
using UnityEditor;
using System.Text;

using SPACE_UTIL;

namespace SPACE_UnityEditor
{
	/// <summary>
	/// Unity Editor script that adds "Copy Hierarchy as Text" to the GameObject context menu.
	/// Right-click any GameObject → "Copy Hierarchy as Text" to copy its structure with proper indentation.
	/// Also logs the hierarchy to LOG.AddLog() for future reference.
	/// </summary>
	public static class CopyHierarchyToClipboard
	{
		// Adjustable indentation settings
		private const string branch_char = "├─ ";
		private const string last_branch_char = "└─ ";
		private const string vertical_line = "│  ";
		private const string empty_indent_space = "   ";

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
			string hierarchyStr = BuildHierarchyString(selected);

			// Copy to clipboard
			EditorGUIUtility.systemCopyBuffer = hierarchyStr;

			// Log to file using UTIL.cs LOG system
			try
			{
				LOG.AddLog(hierarchyStr, syntaxType: "txt");
				Debug.Log($"[CopyHierarchy] Copied hierarchy of '{selected.name}' to clipboard and saved to LOG.md".colorTag("lime"));
			}
			catch (System.Exception e)
			{
				Debug.Log($"[CopyHierarchy] Failed to log hierarchy: {e.Message}".colorTag("red"));
			}
		}

		// Validate menu item (only show when GameObject is selected)
		[MenuItem("GameObject/Copy Hierarchy as Text", true)]
		private static bool ValidateCopyHierarchyAsText()
		{
			return Selection.activeGameObject != null;
		}

		/// <summary>
		/// Builds a formatted hierarchy string starting from the given GameObject.
		/// Format matches box-drawing style with configurable indentation.
		/// </summary>
		private static string BuildHierarchyString(GameObject root)
		{
			StringBuilder sb = new StringBuilder();

			// Root level - show with ./ prefix
			sb.AppendLine($"./{root.name}/");

			// Process children
			Transform rootTransform = root.transform;
			int childCount = rootTransform.childCount;

			for (int i = 0; i < childCount; i++)
			{
				bool isLastChild = (i == childCount - 1);
				Transform child = rootTransform.GetChild(i);
				BuildHierarchyRecursive(child, "", isLastChild, sb);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Recursively builds hierarchy string with proper box-drawing characters.
		/// </summary>
		/// <param name="current">Current transform to process</param>
		/// <param name="prefix">Current line prefix (accumulated indentation)</param>
		/// <param name="isLast">Whether this is the last child in its parent's list</param>
		/// <param name="sb">StringBuilder to accumulate result</param>
		private static void BuildHierarchyRecursive(Transform current, string prefix, bool isLast, StringBuilder sb)
		{
			// Build current line with proper branch character
			string branchChar = isLast ? last_branch_char : branch_char;

			sb.AppendLine($"{prefix}{branchChar}{current.name}");

			// Calculate prefix for children
			string childPrefix = prefix + (isLast ? empty_indent_space : vertical_line);

			// Process children
			int childCount = current.childCount;
			for (int i = 0; i < childCount; i++)
			{
				bool isLastChild = (i == childCount - 1);
				Transform child = current.GetChild(i);
				BuildHierarchyRecursive(child, childPrefix, isLastChild, sb);
			}
		}

		/// <summary>
		/// Generates indentation spaces based on configurable size.
		/// </summary>
		private static string GetIndentSpaces(int count)
		{
			return new string(' ', count);
		}

		#region Alternative Format Methods (commented out - can be enabled if preferred)

		// Alternative 1: Simple indentation (no box-drawing)
		/*
		private static string BuildSimpleHierarchy(GameObject root, int depth = 0)
		{
			StringBuilder sb = new StringBuilder();
			string indent = new string(' ', depth * INDENT_SPACE_SIZE);
			
			sb.AppendLine($"{indent}{root.name}");
			
			foreach (Transform child in root.transform)
			{
				sb.Append(BuildSimpleHierarchy(child.gameObject, depth + 1));
			}
			
			return sb.ToString();
		}
		*/

		// Alternative 2: Markdown-style hierarchy
		/*
		private static string BuildMarkdownHierarchy(GameObject root, int depth = 0)
		{
			StringBuilder sb = new StringBuilder();
			string indent = new string(' ', depth * INDENT_SPACE_SIZE);
			string marker = depth == 0 ? "##" : "-";
			
			sb.AppendLine($"{indent}{marker} {root.name}");
			
			foreach (Transform child in root.transform)
			{
				sb.Append(BuildMarkdownHierarchy(child.gameObject, depth + 1));
			}
			
			return sb.ToString();
		}
		*/

		#endregion

		#region Bonus: Copy Full Path
		// Additional utility: Copy full hierarchy path to selected object
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

		/// <summary>
		/// Gets the full hierarchy path to a transform (e.g., "Root/Parent/Child")
		/// </summary>
		private static string GetFullPath(Transform transform)
		{
			if (transform.parent == null)
				return transform.name;

			return GetFullPath(transform.parent) + "/" + transform.name;
		}
		#endregion
	}
}