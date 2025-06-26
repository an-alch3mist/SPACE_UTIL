using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SPACE_NAME_GEN;

namespace SPACE_WebReqSystem
{
	/*
		Call From External As:
			WebReqSystemManager.Discord.SendPayLoadJson_SysSpec();					   [done]
			WebReqSystemManager.Discord.SendPayLoadJson_Feedback(string Feedback_str); [done]
	*/
	public class WebReqManager : MonoBehaviour
	{
		[TextArea(minLines: 3, maxLines: 10)]
		[SerializeField] string README = $@"0. Attach {typeof(WebReqManager).Name} to Empty Obj in Scene
1. Set The webhook_url[SerilizeField], webhook_profile_url[SerilizeField]
Call From External As:
	2. {typeof(WebReqManager).Name}.Discord.SendPayLoadJson_SysSpec();
	2. {typeof(WebReqManager).Name}.Discord.SendPayLoadJson_Feedback(string Feedback_str);";


		[Header("Discord")]
		[TextArea(minLines: 3, maxLines: 5)]
		[SerializeField] public string webhook_url = "https://discord.com/api/webhooks/<channel-id>/<webhook-id>";
		[TextArea(minLines: 3, maxLines: 10)]
		[SerializeField] public string webhook_profile_url = "https://media.discordapp.net/attachments/1380909082298421410/1380909111918723144/cunning-anime-hacker-modern-female-ninja-with-long-dark-brown-hair-brown-eyes-epic-b_983420-159985.png?ex=68459754&is=684445d4&hm=c0966cf03a4e4e052fbcafd9c285ca682b4d3598f3e499b805c798cf44ea1b84&=&format=webp&quality=lossless&width=814&height=814";
		[Space(5)]


		public static WebReqManager instance;
		private void Awake()
		{
			Debug.Log("Awake(): " + this);
			instance = this;
		}

		// depend on WebReqSystemManager.instance serilize field config
		#region SendPayLoadJson
		public static void SendPayLoadJson(string url, byte[] data)
		{
			if (WebReqManager.instance == null)
				Debug.LogError("WebReqSystemManager isn't attached to GameObject in Scene");

			IEnumerator routine()
			{
				using (UnityWebRequest req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)) // IDisposable
				{
					req.SetRequestHeader("Content-Type", "application/json");
					req.uploadHandler = new UploadHandlerRaw(data);
					req.downloadHandler = new DownloadHandlerBuffer();
					yield return req.SendWebRequest();
					Debug.Log(req.responseCode + " // " + url);
				}
			}
			WebReqManager.instance.StartCoroutine(routine());
		}

		#endregion

		public class Discord
		{
			#region discohook.org preview
			/*
				https://discohook.org/?data=eyJtZXNzYWdlcyI6W3siZGF0YSI6eyJjb250ZW50IjoiSGV5LCB3ZWxjb21lIHRvIDw6ZGlzY29ob29rOjczNjY0ODM5ODA4MTYyMjAxNj4gKipEaXNjb2hvb2sqKiEgVGhlIGVhc2llc3Qgd2F5IHRvIHBlcnNvbmFsaXNlIHlvdXIgRGlzY29yZCBzZXJ2ZXIuXG5cblRoZXJlJ3MgbW9yZSBpbmZvIGJlbG93LCBidXQgeW91IGRvbid0IGhhdmUgdG8gcmVhZCBpdC4gSWYgeW91J3JlIHJlYWR5IHByZXNzICoqQ2xlYXIgQWxsKiogaW4gdGhlIHRvcCBvZiB0aGUgZWRpdG9yIHRvIGdldCBzdGFydGVkLlxuXG5EaXNjb2hvb2sgaGFzIGEgW3N1cHBvcnQgc2VydmVyXShodHRwczovL2Rpc2NvaG9vay5hcHAvZGlzY29yZCksIGlmIHlvdSBuZWVkIGhlbHAgZmVlbCBmcmVlIHRvIGpvaW4gaW4gYW5kIGFzayBxdWVzdGlvbnMsIHN1Z2dlc3QgZmVhdHVyZXMsIG9yIGp1c3QgY2hhdCB3aXRoIHRoZSBjb21tdW5pdHkuXG5cbldlIGFsc28gaGF2ZSBbY29tcGxlbWVudGFyeSBib3RdKGh0dHBzOi8vZGlzY29ob29rLmFwcC9ib3QpIHRoYXQgbWF5IGhlbHAgb3V0LCBmZWF0dXJpbmcgcmVhY3Rpb24gcm9sZXMgYW5kIG90aGVyIHV0aWxpdGllcy5cbl8gXyIsImVtYmVkcyI6W3sidGl0bGUiOiJXaGF0J3MgdGhpcyBhYm91dD8iLCJkZXNjcmlwdGlvbiI6IkRpc2NvaG9vayBpcyBhIGZyZWUgdG9vbCB0aGF0IGFsbG93cyB5b3UgdG8gcGVyc29uYWxpc2UgeW91ciBzZXJ2ZXIgdG8gbWFrZSB5b3VyIHNlcnZlciBzdGFuZCBvdXQgZnJvbSB0aGUgY3Jvd2QuIFRoZSBtYWluIHdheSBpdCBkb2VzIHRoaXMgaXMgdXNpbmcgW3dlYmhvb2tzXShodHRwczovL3N1cHBvcnQuZGlzY29yZC5jb20vaGMvZW4tdXMvYXJ0aWNsZXMvMjI4MzgzNjY4KSwgd2hpY2ggYWxsb3dzIHNlcnZpY2VzIGxpa2UgRGlzY29ob29rIHRvIHNlbmQgYW55IG1lc3NhZ2VzIHdpdGggZW1iZWRzIHRvIHlvdXIgc2VydmVyLlxuXG5UbyBnZXQgc3RhcnRlZCB3aXRoIHNlbmRpbmcgbWVzc2FnZXMsIHlvdSBuZWVkIGEgd2ViaG9vayBVUkwsIHlvdSBjYW4gZ2V0IG9uZSB2aWEgdGhlIFwiSW50ZWdyYXRpb25zXCIgdGFiIGluIHlvdXIgc2VydmVyJ3Mgc2V0dGluZ3MuIElmIHlvdSdyZSBoYXZpbmcgaXNzdWVzIGNyZWF0aW5nIGEgd2ViaG9vaywgW3RoZSBib3RdKGh0dHBzOi8vZGlzY29ob29rLmFwcC9ib3QpIGNhbiBoZWxwIHlvdSBjcmVhdGUgb25lIGZvciB5b3UuXG5cbktlZXAgaW4gbWluZCB0aGF0IERpc2NvaG9vayBjYW4ndCBkbyBhdXRvbWF0aW9uIHlldCwgaXQgb25seSBzZW5kcyBtZXNzYWdlcyB3aGVuIHlvdSB0ZWxsIGl0IHRvLiBJZiB5b3UgYXJlIGxvb2tpbmcgZm9yIGFuIGF1dG9tYXRpYyBmZWVkIG9yIGN1c3RvbSBjb21tYW5kcyB0aGlzIGlzbid0IHRoZSByaWdodCB0b29sIGZvciB5b3UuIiwiY29sb3IiOjE2NzQyNzQ0LCJmaWVsZHMiOlt7Im5hbWUiOiJmaWVsZCBuYW1lIDEiLCJ2YWx1ZSI6ImZpZWxkIHZhbHVlIHdpdGggaW5saW5lIiwiaW5saW5lIjp0cnVlfSx7Im5hbWUiOiJmaWVsZCAyIiwidmFsdWUiOiJmaWVsZCB2YWx1ZSB3aXRoIGlubGluZSIsImlubGluZSI6dHJ1ZX0seyJuYW1lIjoiZmllbGQgMyIsInZhbHVlIjoiZmllbGQgdmFsdWUgd2l0aCBpbmxpbmUiLCJpbmxpbmUiOnRydWV9XSwiZm9vdGVyIjp7InRleHQiOiJmb290ZXIiLCJpY29uX3VybCI6Imh0dHBzOi8vYXZhdGFycy5naXRodWJ1c2VyY29udGVudC5jb20vdS83OTQ1OTA1P3M9ODAmdj00In0sInRpbWVzdGFtcCI6IjIwMjUtMDUtMzFUMTg6MDA6MDAuMDAwWiIsImltYWdlIjp7InVybCI6Imh0dHBzOi8vY2Ftby5naXRodWJ1c2VyY29udGVudC5jb20vNDc5NzViMjAyZmU0ZjA4NDExYWMwZDI1NWQ3MDM0NWE3OWQ1NWY4MmI4ZjFhYWIxZTJjZDM5NDJiYTgzMjk3Ny82ODc0NzQ3MDczM2EyZjJmNjkyZTY5NmQ2Nzc1NzIyZTYzNmY2ZDJmNmI3NjQ1NWE1NTM5MzcyZTcwNmU2NyJ9LCJ0aHVtYm5haWwiOnsidXJsIjoiaHR0cHM6Ly9hdmF0YXJzLmdpdGh1YnVzZXJjb250ZW50LmNvbS91Lzc5NDU5MDU_cz04MCZ2PTQifX0seyJ0aXRsZSI6IkRpc2NvcmQgYm90IiwiZGVzY3JpcHRpb24iOiJEaXNjb2hvb2sgaGFzIGEgYm90IGFzIHdlbGwsIGl0J3Mgbm90IHN0cmljdGx5IHJlcXVpcmVkIHRvIHNlbmQgbWVzc2FnZXMgaXQgbWF5IGJlIGhlbHBmdWwgdG8gaGF2ZSBpdCByZWFkeS5cblxuQmVsb3cgaXMgYSBzbWFsbCBidXQgaW5jb21wbGV0ZSBvdmVydmlldyBvZiB3aGF0IHRoZSBib3QgY2FuIGRvIGZvciB5b3UuIiwiY29sb3IiOjU4MTQ3ODMsImZpZWxkcyI6W3sibmFtZSI6IkdldHRpbmcgc3BlY2lhbCBmb3JtYXR0aW5nIGZvciBtZW50aW9ucywgY2hhbm5lbHMsIGFuZCBlbW9qaSIsInZhbHVlIjoiVGhlICoqL2Zvcm1hdCoqIGNvbW1hbmQgb2YgdGhlIGJvdCBjYW4gZ2l2ZSB5b3Ugc3BlY2lhbCBmb3JtYXR0aW5nIGZvciB1c2UgaW4gRGlzY29yZCBtZXNzYWdlcyB0aGF0IGxldHMgeW91IGNyZWF0ZSBtZW50aW9ucywgdGFnIGNoYW5uZWxzLCBvciB1c2UgZW1vamkgcmVhZHkgdG8gcGFzdGUgaW50byB0aGUgZWRpdG9yIVxuXG5UaGVyZSBhcmUgW21hbnVhbCB3YXlzXShodHRwczovL2Rpc2NvcmQuZGV2L3JlZmVyZW5jZSNtZXNzYWdlLWZvcm1hdHRpbmcpIG9mIGRvaW5nIHRoaXMsIGJ1dCBpdCdzIHZlcnkgZXJyb3IgcHJvbmUuIFRoZSBib3Qgd2lsbCBtYWtlIHN1cmUgeW91J2xsIGFsd2F5cyBnZXQgdGhlIHJpZ2h0IGZvcm1hdHRpbmcgZm9yIHlvdXIgbmVlZHMuIn0seyJuYW1lIjoiQ3JlYXRpbmcgcmVhY3Rpb24gcm9sZXMiLCJ2YWx1ZSI6IllvdSBjYW4gbWFuYWdlIHJlYWN0aW9uIHJvbGVzIHdpdGggdGhlIGJvdCB1c2luZyB0aGUgKiovcmVhY3Rpb24tcm9sZSoqIGNvbW1hbmQuXG5cblRoZSBzZXQtdXAgcHJvY2VzcyBpcyB2ZXJ5IGludHVpdGl2ZTogdHlwZSBvdXQgKiovcmVhY3Rpb24tcm9sZSBjcmVhdGUqKiwgcGFzdGUgYSBtZXNzYWdlIGxpbmssIHNlbGVjdCBhbiBlbW9qaSwgYW5kIHBpY2sgYSByb2xlLiBIaXQgZW50ZXIgYW5kIHlvdSdyZSBkb25lLCB5b3VyIG1lbWJlcnMgY2FuIG5vdyByZWFjdCB0byBhbnkgb2YgeW91ciBtZXNzYWdlcyB0byBwaWNrIHRoZWlyIHJvbGVzLiJ9LHsibmFtZSI6IlJlY292ZXIgRGlzY29ob29rIG1lc3NhZ2VzIGZyb20geW91ciBzZXJ2ZXIiLCJ2YWx1ZSI6Ikl0IGNhbiBhbHNvIHJlc3RvcmUgYW55IG1lc3NhZ2Ugc2VudCBpbiB5b3VyIERpc2NvcmQgc2VydmVyIGZvciB5b3UgdmlhIHRoZSBhcHBzIG1lbnUuXG5cblRvIGdldCBzdGFydGVkLCByaWdodC1jbGljayBvciBsb25nLXByZXNzIG9uIGFueSBtZXNzYWdlIGluIHlvdXIgc2VydmVyLCBwcmVzcyBvbiBhcHBzLCBhbmQgdGhlbiBwcmVzcyAqKlJlc3RvcmUgdG8gRGlzY29ob29rKiouIEl0J2xsIHNlbmQgeW91IGEgbGluayB0aGF0IGxlYWRzIHRvIHRoZSBlZGl0b3IgcGFnZSBjb250YWluaW5nIHRoZSBtZXNzYWdlIHlvdSBzZWxlY3RlZCEifSx7Im5hbWUiOiJPdGhlciBmZWF0dXJlcyIsInZhbHVlIjoiRGlzY29ob29rIGNhbiBhbHNvIGdyYWIgaW1hZ2VzIGZyb20gcHJvZmlsZSBwaWN0dXJlcyBvciBlbW9qaSwgbWFuYWdlIHlvdXIgd2ViaG9va3MsIGFuZCBtb3JlLiBJbnZpdGUgdGhlIGJvdCBhbmQgdXNlICoqL2hlbHAqKiB0byBsZWFybiBhYm91dCBhbGwgdGhlIGJvdCBvZmZlcnMhIn1dfV0sImF0dGFjaG1lbnRzIjpbXX19XX0

				click json data editor to view in JsonFormat
			*/
			#endregion

			public static void SendPayLoadJson_SysSpec()
			{
				// depends on WebReqSystemManager instance
				string webhook_url = WebReqManager.instance.webhook_url;
				string webhook_profile_url = WebReqManager.instance.webhook_profile_url;

				#region data collected
				string id = SystemInfo.deviceUniqueIdentifier;
				string get_sys_spec_md()
				{
					// Collect each spec into a bullet‐point line:
					string os = SystemInfo.operatingSystem;
					string cpu = SystemInfo.processorType;
					int cores = SystemInfo.processorCount;
					int ramMB = SystemInfo.systemMemorySize;
					string gpu = SystemInfo.graphicsDeviceName;
					string gpu_version = SystemInfo.graphicsDeviceVersion;
					string unityVer = Application.unityVersion;
					string device = SystemInfo.deviceModel;
					string screen = $"{Screen.currentResolution.width}×{Screen.currentResolution.height}";
					string dpi = $"{Screen.dpi}";

					// Build a Markdown list. Each line starts with "• " (Discord will render as a bullet).
					var sb = new System.Text.StringBuilder();
					sb.AppendLine("**System Specs:**");
					sb.AppendLine();
					sb.AppendLine($"• **Uid:** `{id}`");
					sb.AppendLine($"• **OS:** `{os}`");
					sb.AppendLine($"• **Device:** `{device}`");
					sb.AppendLine($"• **Resolution:** `{screen} {dpi}(dpi)`");
					sb.AppendLine($"• **CPU:** `{cpu} ({cores} cores)`");
					sb.AppendLine($"• **GPU:** `{gpu} {gpu_version}`");
					sb.AppendLine($"• **RAM:** `{ramMB} MB`");
					sb.AppendLine(); // extra blank line at the end
					sb.AppendLine($"• **Gmt:** `{TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).ToString()}`");
					return sb.ToString();
				}
				string footer_str = $"{Application.productName} by {Application.companyName}";
				#endregion
				/*
					study source: https://gist.github.com/Birdie0/78ee79402a4301b1faf412ab5f1cdcf9
				*/

				// Construct one Embed object
				#region embed
				Embed embed = new Embed
				{
					author = new Author
					{
						name = "System Reporter",
						url = null, // optional link
						icon_url = webhook_profile_url,
					},
					title = "",
					url = null, // optional
					description = get_sys_spec_md(),
					color = 0x72D7F1, // decimal 7506394
					fields = null,     // no fields—everything is in description
					thumbnail = null,     // optional
					image = null,     // optional
					footer = new Footer
					{
						text = footer_str,
						icon_url = webhook_profile_url,
					},
					timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
				};

				#endregion

				// Wrap it in the root payload
				DiscordPayLoad discordPayLoad = new DiscordPayLoad
				{
					username = JapaneseNameGenerator.ConvertToJapaneseName(id),
					avatar_url = webhook_profile_url,
					content = "",
					embeds = new List<Embed> { embed },
				};

				WebReqManager.SendPayLoadJson(
					webhook_url,
					discordPayLoad.ToJson().ToBytes()
				);
			}

			#region /// <summary>
			/// <summary>
			/// Sends a payload JSON to the Discord webhook with feedback and system specs.
			/// <param name="Feedback_str">The feedback string to send. Supports Discord rich text formatting.</param>
			/// <param name="AlongWithSysSpec">Whether to include system specs in the payload. Default is true.</param>
			/// <para>Discord Rich Text Formatting Guide:</para>
			/// <para>• ### heading: #3# text</para>
			/// <para>• **Bold**: **text**</para>
			/// <para>• __Underline__: __text__</para>
			/// <para>• ||Spoiler||: ||text||</para>
			/// <para>• `code`: `text`</para>
			/// <para>• *Italic*: *text*</para>
			/// <para>• ~~Strikethrough~~: ~~text~~</para>
			/// </summary>
			#endregion
			public static void SendPayLoadJson_Feedback(string Feedback_str = "", bool AlongWithSysSpec = true)
			{
				// depends on WebReqSystemManager instance
				string webhook_url = WebReqManager.instance.webhook_url;
				string webhook_profile_url = WebReqManager.instance.webhook_profile_url;

				#region data collected
				string id = SystemInfo.deviceUniqueIdentifier;
				string get_sys_spec_md()
				{
					// Collect each spec into a bullet‐point line:
					string os = SystemInfo.operatingSystem;
					string cpu = SystemInfo.processorType;
					int cores = SystemInfo.processorCount;
					int ramMB = SystemInfo.systemMemorySize;
					string gpu = SystemInfo.graphicsDeviceName;
					string gpu_version = SystemInfo.graphicsDeviceVersion;
					string unityVer = Application.unityVersion;
					string device = SystemInfo.deviceModel;
					string screen = $"{Screen.currentResolution.width}×{Screen.currentResolution.height}";
					string dpi = $"{Screen.dpi}";

					// Build a Markdown list. Each line starts with "• " (Discord will render as a bullet).
					var sb = new System.Text.StringBuilder();
					sb.AppendLine("**System Specs:**");
					sb.AppendLine();
					sb.AppendLine($"• **Uid:** `{id}`");
					sb.AppendLine($"• **OS:** `{os}`");
					sb.AppendLine($"• **Device:** `{device}`");
					sb.AppendLine($"• **Resolution:** `{screen} {dpi}(dpi)`");
					sb.AppendLine($"• **CPU:** `{cpu} ({cores} cores)`");
					sb.AppendLine($"• **GPU:** `{gpu} {gpu_version}`");
					sb.AppendLine($"• **RAM:** `{ramMB} MB`");
					sb.AppendLine(); // extra blank line at the end
					sb.AppendLine($"• **Gmt:** `{TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).ToString()}`");
					return sb.ToString();
				}
				string footer_str = $"{Application.productName} by {Application.companyName}";
				#endregion
				/*
					study source: https://gist.github.com/Birdie0/78ee79402a4301b1faf412ab5f1cdcf9
				*/

				// Construct one Embed object
				#region embed
				Embed embed = new Embed
				{
					author = new Author
					{
						name = "System Reporter",
						url = null, // optional link
						icon_url = webhook_profile_url,
					},
					title = "",
					url = null, // optional
					description = get_sys_spec_md(),
					color = 0x72D7F1, // decimal 7506394
					fields = null,     // no fields—everything is in description
					thumbnail = null,     // optional
					image = null,     // optional
					footer = new Footer
					{
						text = footer_str,
						icon_url = webhook_profile_url,
					},
					timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
				};

				#endregion

				// Wrap it in the root payload
				DiscordPayLoad discordPayLoad = new DiscordPayLoad
				{
					username = JapaneseNameGenerator.ConvertToJapaneseName(id),
					avatar_url = webhook_profile_url,
					content = Feedback_str,
					embeds = (AlongWithSysSpec)? new List<Embed> { embed } : new List<Embed>(),
				};

				WebReqManager.SendPayLoadJson(
					webhook_url,
					discordPayLoad.ToJson().ToBytes()
				);
			}

			#region JSON PayLoad Hierarchy
			/* JSON Hierarchy
			{
			  "username": "<string>",
			  "avatar_url": "<string>",
			  "content": "<string>",
			  "embeds": [
				{
				  "author": {
					"name": "<string>",
					"url": "<string>",
					"icon_url": "<string>"
				  },
				  "title": "<string>",
				  "url": "<string>",
				  "description": "<string>", // supports md
				  "color": <integer>,
				  "fields": [
					{
					  "name": "<string>",
					  "value": "<string>",
					  "inline": <boolean>
					}
					// …additional field objects
				  ],
				  "thumbnail": {
					"url": "<string>"
				  },
				  "image": {
					"url": "<string>"
				  },
				  "footer": {
					"text": "<string>",
					"icon_url": "<string>"
				  }
				}
				// …additional embed objects
			  ]
			}
			*/
			#endregion

			#region Json Payload Root -> C# [System.Serializable] Class Heierarchy
			// ─── 1) Root payload ───────────────────────────────────────────────
			[Serializable]
			public class DiscordPayLoad
			{
				public string username;    // Bot’s display name for this message
				public string avatar_url;  // Bot’s profile picture URL
				public string content;     // Top‐level text (we’ll include game name + company)
				public List<Embed> embeds;      // List of Embed objects (we’ll put specs in description)
			}

			// ─── 2) Embed object ─────────────────────────────────────────────
			[Serializable]
			public class Embed
			{
				public Author author;       // embed.author (optional)
				public string title;        // e.g. "🖥 System & Game Report"
				public string url;          // (Optional) hyperlink on title
				public string description;  // Markdown‐supported multiline text
				public int color;        // Decimal RGB, e.g. 0x72D7F1 = 7506394
				public List<EmbedField> fields;       // We’ll leave this null now (not used)
				public Thumbnail thumbnail;    // (Optional)
				public EmbedImage image;        // (Optional)
				public Footer footer;       // (Optional)
				public string timestamp;    // ISO8601 UTC (Optional)
			}

			// ─── 3) Author sub‐object ────────────────────────────────────────
			[Serializable]
			public class Author
			{
				public string name;     // e.g. "ReporterBot"
				public string url;      // (Optional) link on author name
				public string icon_url; // (Optional) small icon next to author
			}

			// ─── 4) Single field inside embed.fields ────────────────────────
			[Serializable]
			public class EmbedField
			{
				public string name;   // Field label
				public string value;  // Field value
				public bool inline; // true → attempt to display side‐by‐side
			}

			// ─── 5) Thumbnail sub‐object ────────────────────────────────────
			[Serializable]
			public class Thumbnail
			{
				public string url;    // Thumbnail URL (Optional)
			}

			// ─── 6) Image sub‐object ────────────────────────────────────────
			[Serializable]
			public class EmbedImage
			{
				public string url;    // Large image URL (Optional)
			}

			// ─── 7) Footer sub‐object ────────────────────────────────────────
			[Serializable]
			public class Footer
			{
				public string text;     // e.g. "Generated at 2025-06-01 13:26:41 UTC"
				public string icon_url; // (Optional) small icon next to footer text
			}
			#endregion
		}
	}

	// json extensions
	static class C_JsonExtension
	{
		/*
			[System.Serializable]object.ToJson() -> string
			str.FromJson<T>() -> T
		*/
		#region json operations
		/// <summary>
		/// Convert A Serielizable (object) To JSON (string).
		/// called as string Json = object.ToJson(true);
		/// </summary>
		public static string ToJson(this object obj, bool pretify = true)
		{
			if (obj == null)
				return "obj is null";

			return JsonUtility.ToJson(obj, pretify);
		}

		/// <summary>
		/// Deserializes a JSON string into a new instance of T.
		/// called as T _T = str.FromJson<T>();
		/// </summary>
		public static T FromJson<T>(this string json)
		{
			try
			{
				return JsonUtility.FromJson<T>(json);
			}
			catch (Exception e)
			{
				Debug.LogError("Cannot Parse Json to Type T");
				//return default;
				throw;
			}
		}

		public static byte[] ToBytes(this string json)
		{
			return System.Text.Encoding.UTF8.GetBytes(json);
		}
		#endregion
	}
}
