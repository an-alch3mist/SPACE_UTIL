#if UNITY_EDITOR
namespace SPACE_UTIL.EditorUtil
{
	using UnityEngine;
	using UnityEditor;
	using System.IO;
	using System.Collections.Generic;
	using System.Linq;
	using SPACE_UTIL;

	/// <summary>
	/// Editor window for decrypting and managing game data files.
	/// Menu: Tools > Game Data Decryptor
	/// 
	/// Features:
	/// - View all encrypted game data files
	/// - Decrypt individual or all files
	/// - Test encryption/decryption system
	/// - Show file status (encrypted/plain/corrupted)
	/// - Display file sizes
	/// - Scale-responsive layout
	/// 
	/// Dependencies: Only SPACE_UTIL namespace (LOG class)
	/// </summary>
	public class ViewGameDataDecrEditorUtil : EditorWindow
	{
		#region Data Structures
		private enum FileStatus
		{
			Encrypted,      // Successfully decryptable
			Plain,          // Not encrypted
			Corrupted       // Cannot decrypt or read
		}

		private class FileInfo
		{
			public string fileName;
			public string fullPath;
			public long sizeBytes;
			public FileStatus status;
			public string statusMessage;

			public string GetSizeString()
			{
				if (sizeBytes < 1024)
					return $"{sizeBytes}B";
				else if (sizeBytes < 1024 * 1024)
					return $"{(sizeBytes / 1024f):F1}KB";
				else
					return $"{(sizeBytes / (1024f * 1024f)):F2}MB";
			}
		}
		#endregion

		#region Fields
		private GUISkin customSkin;
		private List<FileInfo> fileList = new List<FileInfo>();
		private Vector2 scrollPosition;
		private bool isScanning = false;
		private string statusMessage = "Ready";
		private int encryptedCount = 0;
		private int plainCount = 0;
		private int corruptedCount = 0;

		// Layout constants (scale with window)
		private const float minWinWidth = 400f;
		private const float minWinHeight = 300f;
		private const float headerHeight = 80f;
		private const float statusHeight = 80f;
		private const float btnHeight = 35f;
		private const float iconSize = 20f;
		private const float rowHeight = 35f;
		private const float padding = 10f;

		// Colors
		private Color encryptedColor = new Color(1f, 0.7f, 0.3f); // Orange
		private Color plainColor = new Color(0.5f, 0.8f, 1f);     // Light blue
		private Color corruptedColor = new Color(1f, 0.3f, 0.3f); // Red
		private Color successColor = new Color(0.3f, 1f, 0.3f);   // Green
		#endregion

		#region Unity Menu
		[MenuItem("EditorUtil/View Game Data Decr")]
		public static void ShowWindow()
		{
			ViewGameDataDecrEditorUtil window = GetWindow<ViewGameDataDecrEditorUtil>("View Game Data Decr");
			window.minSize = new Vector2(minWinWidth, minWinHeight);
			window.Show();
		}
		#endregion

		#region Unity Lifecycle
		private void OnEnable()
		{
			LoadGUISkin();
			// RefreshFileList();
		}

		private void OnGUI()
		{
			if (customSkin != null)
				GUI.skin = customSkin;

			DrawHeader();
			DrawStatusBar();
			DrawFileList();

			// Refresh on layout changes
			if (Event.current.type == EventType.Repaint && !isScanning)
			{
				Repaint();
			}
		}
		#endregion

		#region GUI Drawing
		private void DrawHeader()
		{
			GUILayout.BeginArea(new Rect(0, 0, position.width, headerHeight));
			GUILayout.BeginVertical(GUI.skin.box);

			// Title
			GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
			{
				fontStyle = FontStyle.Bold,
				alignment = TextAnchor.MiddleCenter
			};
			GUILayout.Label("🔓 Game Data Decryptor", titleStyle, GUILayout.Height(30));

			GUILayout.Space(5);

			// Action buttons
			GUILayout.BeginHorizontal();

			float buttonWidth = (position.width - padding * 4) / 3f;

			// Refresh button
			GUI.backgroundColor = new Color(0.7f, 0.7f, 1f);
			if (GUILayout.Button("⟳ Corrupted?", GUILayout.Height(btnHeight), GUILayout.Width(buttonWidth)))
			{
				RefreshFileList();
			}

			// Export All button
			GUI.backgroundColor = new Color(0.95f, 0.95f, 0.95f);
			if (GUILayout.Button("Export All", GUILayout.Height(btnHeight), GUILayout.Width(buttonWidth)))
			{
				ExportAll();
			}
			// Test button
			GUI.backgroundColor = new Color(0.8f, 0.65f, 0.5f);
			if (GUILayout.Button("EncrCheck", GUILayout.Height(btnHeight), GUILayout.Width(buttonWidth)))
			{
				CheckEncryption();
			}

			GUI.backgroundColor = Color.white;
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
		private void DrawStatusBar()
		{
			float yPos = headerHeight;
			GUILayout.BeginArea(new Rect(0, yPos, position.width, statusHeight));
			GUILayout.BeginVertical(GUI.skin.box);

			// Status message
			GUIStyle statusStyle = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleLeft,
				wordWrap = true
			};
			GUILayout.Label(statusMessage, statusStyle);

			// File counts
			GUILayout.BeginHorizontal();

			GUIStyle countStyle = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleLeft
			};

			GUI.color = encryptedColor;
			GUILayout.Label($"🔒 Encrypted: {encryptedCount}", countStyle);

			GUI.color = plainColor;
			GUILayout.Label($"📄 Plain: {plainCount}", countStyle);

			GUI.color = corruptedColor;
			GUILayout.Label($"⚠ Corrupted: {corruptedCount}", countStyle);

			GUI.color = Color.white;
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
		private void DrawFileList()
		{
			float yPos = headerHeight + statusHeight;
			float listHeight = position.height - yPos - padding;

			GUILayout.BeginArea(new Rect(0, yPos, position.width, listHeight));

			scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUI.skin.box);

			if (fileList.Count == 0)
			{
				GUIStyle emptyStyle = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.MiddleCenter,
					fontStyle = FontStyle.Italic
				};
				GUI.color = new Color(0.7f, 0.7f, 0.7f);
				GUILayout.Label($"Try Refresh/No files found in {LOG.locGameDataDirectory} directory", emptyStyle, GUILayout.Height(listHeight));
				GUI.color = Color.white;
			}
			else
			{
				foreach (var file in fileList)
				{
					DrawFileRow(file);
					GUILayout.Space(5);
				}
			}

			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}
		private void DrawFileRow(FileInfo file)
		{
			GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(rowHeight));

			// Status icon (non-interactive)
			GUIStyle iconStyle = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				fixedWidth = iconSize * 2,
				fontStyle = FontStyle.Bold
			};

			switch (file.status)
			{
				case FileStatus.Encrypted:
					GUI.color = encryptedColor;
					GUILayout.Label("🔒", iconStyle);
					break;
				case FileStatus.Plain:
					GUI.color = plainColor;
					GUILayout.Label("📄", iconStyle);
					break;
				case FileStatus.Corrupted:
					GUI.color = corruptedColor;
					GUILayout.Label("⚠", iconStyle);
					break;
			}
			GUI.color = Color.white;

			// File info (name + size)
			GUILayout.BeginVertical();

			GUIStyle nameStyle = new GUIStyle(GUI.skin.label)
			{
				fontStyle = FontStyle.Bold,
				alignment = TextAnchor.MiddleLeft
			};
			GUILayout.Label(file.fileName, nameStyle);

			GUIStyle sizeStyle = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleLeft,
				normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
			};
			GUILayout.Label($"{file.GetSizeString()} • {file.statusMessage}", sizeStyle);

			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();

			// Save button
			float saveButtonWidth = Mathf.Max(60f, position.width / 8f);
			GUI.backgroundColor = file.status == FileStatus.Corrupted ? corruptedColor : successColor;
			GUI.enabled = file.status != FileStatus.Corrupted;

			if (GUILayout.Button("💾 Save", GUILayout.Width(saveButtonWidth), GUILayout.Height(btnHeight)))
			{
				ExportFile(file);
			}

			GUI.enabled = true;
			GUI.backgroundColor = Color.white;

			GUILayout.EndHorizontal();
		}
		#endregion

		#region Core Functionality
		private void LoadGUISkin()
		{
			customSkin = Resources.Load<GUISkin>("custom GUISkin");
			if (customSkin == null)
			{
				Debug.LogWarning("[GameDataDecryptor] Custom GUISkin not found at 'Resources/custom GUISkin'. Using default skin.");
			}
		}

		private void RefreshFileList()
		{
			isScanning = true;
			fileList.Clear();
			encryptedCount = 0;
			plainCount = 0;
			corruptedCount = 0;

			string dataDir = LOG.locGameDataDirectory;

			if (!Directory.Exists(dataDir))
			{
				statusMessage = $"⚠ Directory not found: {dataDir}";
				isScanning = false;
				return;
			}

			string[] files = Directory.GetFiles(dataDir, "*.json");

			if (files.Length == 0)
			{
				statusMessage = "📁 No .json files found in GameData directory";
				isScanning = false;
				return;
			}

			foreach (string filePath in files)
			{
				FileInfo info = new FileInfo
				{
					fileName = Path.GetFileName(filePath),
					fullPath = filePath,
					sizeBytes = new System.IO.FileInfo(filePath).Length
				};

				// Test file status with actual decryption attempt
				info.status = DetectFileStatus(filePath, out string message);
				info.statusMessage = message;

				// Update counts
				switch (info.status)
				{
					case FileStatus.Encrypted:
						encryptedCount++;
						break;
					case FileStatus.Plain:
						plainCount++;
						break;
					case FileStatus.Corrupted:
						corruptedCount++;
						break;
				}

				fileList.Add(info);
			}

			fileList = fileList.OrderBy(f => f.fileName).ToList();
			statusMessage = $"✓ Scanned {fileList.Count} files";
			isScanning = false;
			Repaint();
		}

		private FileStatus DetectFileStatus(string filePath, out string message)
		{
			try
			{
				string rawContent = File.ReadAllText(filePath);
				string fileName = Path.GetFileNameWithoutExtension(filePath);

				// First check if it looks like plain JSON
				string trimmed = rawContent.Trim();
				bool looksLikeJSON = trimmed.StartsWith("{") || trimmed.StartsWith("[");

				// Try to decrypt
				try
				{
					string decrypted = LOG.LoadGameData(fileName, encryptRequired: true);

					if (!string.IsNullOrEmpty(decrypted))
					{
						// Successfully decrypted
						message = "Encrypted (verified)";
						return FileStatus.Encrypted;
					}
					else if (looksLikeJSON)
					{
						// Decryption returned empty but looks like JSON - probably plain
						message = "Plain JSON";
						return FileStatus.Plain;
					}
					else
					{
						// Decryption returned empty and doesn't look like JSON
						message = "Cannot decrypt or parse(does'nt look like JSON)";
						return FileStatus.Corrupted;
					}
				}
				catch (System.Security.Cryptography.CryptographicException)
				{
					// Decryption explicitly failed
					if (looksLikeJSON)
					{
						// It's valid JSON, just not encrypted
						message = "Plain JSON";
						return FileStatus.Plain;
					}
					else
					{
						// Not JSON and can't decrypt
						message = "Decryption failed";
						return FileStatus.Corrupted;
					}
				}
				catch (System.Exception e)
				{
					// Other error during decryption attempt
					if (looksLikeJSON)
					{
						message = "Plain JSON (decrypt error)";
						return FileStatus.Plain;
					}
					else
					{
						message = $"Error: {e.Message.Substring(0, System.Math.Min(30, e.Message.Length))}";
						return FileStatus.Corrupted;
					}
				}
			}
			catch (System.Exception e)
			{
				message = $"Read error: {e.Message.Substring(0, System.Math.Min(30, e.Message.Length))}";
				return FileStatus.Corrupted;
			}
		}

		private void ExportFile(FileInfo file)
		{
			string outputDir = LOG.locGameDataNoEncrDirectory;

			if (!Directory.Exists(outputDir))
			{
				Directory.CreateDirectory(outputDir);
			}

			string outputPath = Path.Combine(outputDir, file.fileName);

			try
			{
				string content = "";

				if (file.status == FileStatus.Encrypted)
				{
					// Decrypt and save
					content = LOG.LoadGameData(Path.GetFileNameWithoutExtension(file.fileName), encryptRequired: true);

					if (string.IsNullOrEmpty(content))
					{
						statusMessage = $"⚠ Decryption returned empty for: {file.fileName}";
						Debug.LogWarning($"[GameDataDecryptor] Decryption returned empty for {file.fileName}");

						// Mark as corrupted and update UI
						file.status = FileStatus.Corrupted;
						file.statusMessage = "Decryption failed (empty)";
						encryptedCount--;
						corruptedCount++;
						Repaint();
						return;
					}
				}
				else if (file.status == FileStatus.Plain)
				{
					// Copy as-is
					content = File.ReadAllText(file.fullPath);
				}
				else
				{
					statusMessage = $"⚠ Cannot export corrupted file: {file.fileName}";
					Debug.LogWarning($"[GameDataDecryptor] Attempted to export corrupted file: {file.fileName}");
					return;
				}

				File.WriteAllText(outputPath, content);
				statusMessage = $"✓ Exported: {file.fileName} → GameDataNoEncr/";

				Debug.Log($"[GameDataDecryptor] Exported {file.fileName} to {outputPath}");
			}
			catch (System.Security.Cryptography.CryptographicException e)
			{
				statusMessage = $"❌ Decryption failed: {file.fileName}";
				Debug.LogError($"[GameDataDecryptor] Decryption failed for {file.fileName}: {e.Message}");

				// Mark as corrupted and update UI
				file.status = FileStatus.Corrupted;
				file.statusMessage = "Decryption error";
				if (encryptedCount > 0) encryptedCount--;
				corruptedCount++;
				Repaint();
			}
			catch (System.Exception e)
			{
				statusMessage = $"❌ Export failed: {e.Message}";
				Debug.LogError($"[GameDataDecryptor] Failed to export {file.fileName}: {e.Message}");
			}
		}

		private void ExportAll()
		{
			int successCount = 0;
			int failCount = 0;

			string outputDir = LOG.locGameDataNoEncrDirectory;
			if (!Directory.Exists(outputDir))
			{
				Directory.CreateDirectory(outputDir);
			}

			foreach (var file in fileList)
			{
				if (file.status == FileStatus.Corrupted)
				{
					failCount++;
					Debug.LogWarning($"[GameDataDecryptor] Skipped corrupted file: {file.fileName}");
					continue;
				}

				try
				{
					string content = "";

					if (file.status == FileStatus.Encrypted)
					{
						content = LOG.LoadGameData(Path.GetFileNameWithoutExtension(file.fileName), encryptRequired: true);

						if (string.IsNullOrEmpty(content))
						{
							// Mark as corrupted and skip
							file.status = FileStatus.Corrupted;
							file.statusMessage = "Decryption failed (empty)";
							encryptedCount--;
							corruptedCount++;
							failCount++;
							Debug.LogWarning($"[GameDataDecryptor] Decryption returned empty for {file.fileName}");
							continue;
						}
					}
					else
					{
						content = File.ReadAllText(file.fullPath);
					}

					string outputPath = Path.Combine(outputDir, file.fileName);
					File.WriteAllText(outputPath, content);
					successCount++;
				}
				catch (System.Security.Cryptography.CryptographicException e)
				{
					// Mark as corrupted on decryption failure
					file.status = FileStatus.Corrupted;
					file.statusMessage = "Decryption error";
					if (encryptedCount > 0) encryptedCount--;
					corruptedCount++;
					failCount++;
					Debug.LogError($"[GameDataDecryptor] Decryption failed for {file.fileName}: {e.Message}");
				}
				catch (System.Exception e)
				{
					failCount++;
					Debug.LogError($"[GameDataDecryptor] Export failed for {file.fileName}: {e.Message}");
				}
			}

			if (failCount > 0)
			{
				statusMessage = $"⚠ Exported {successCount} files, {failCount} failed (check Console)";
			}
			else
			{
				statusMessage = $"✓ Successfully exported all {successCount} files to GameDataNoEncr/";
			}

			Debug.Log($"[GameDataDecryptor] Export complete: {successCount} success, {failCount} failed");
			Repaint();
		}

		private void CheckEncryption()
		{
			try
			{
				LOG.CheckEncryption();
				statusMessage = "✓ Encryption test PASSED - Check Console for details";
			}
			catch (System.Exception e)
			{
				statusMessage = $"❌ Encryption test FAILED: {e.Message}";
			}
		}
		#endregion
	} 
}
#endif