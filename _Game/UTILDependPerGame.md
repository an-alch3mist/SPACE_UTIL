# UTIL Game Dependency Unique Per Game

## Assets/\_Game
`template from _/_Game`

### Assets/.gitignore
`template from _/.gitignore`
```bash
# Ignore /_Secure situated directory situated anywhere, when push is done via git
**/_Secure/
```
### Assets/../\_Secure/\_Secure.cs
`template from _/_Secure which won't be included due to .gitignore`
```cs
namespace SPACE_WebReqSystem
{
	// located @ ./Assets/../_Secure/_Secure.cs 
	public static class _Secure
	{
		public static string webhook_url = "";
	}
}
```