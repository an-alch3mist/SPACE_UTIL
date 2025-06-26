using System.Security.Cryptography;

namespace SPACE_NAME_GEN
{
	public class JapaneseNameGenerator
	{
		// Large pool of Japanese family names (surnames)
		private static readonly string[] familyNames = 
		{
			"Tanaka", "Suzuki", "Takahashi", "Watanabe", "Ito", "Yamamoto", "Nakamura", "Kobayashi", "Kato", "Yoshida",
			"Yamada", "Sasaki", "Yamaguchi", "Matsumoto", "Inoue", "Kimura", "Hayashi", "Shimizu", "Yamazaki", "Mori",
			"Abe", "Ikeda", "Hashimoto", "Yamashita", "Ishikawa", "Nakajima", "Maeda", "Fujita", "Ogawa", "Goto",
			"Okada", "Hasegawa", "Murakami", "Kondo", "Ishii", "Saito", "Sakamoto", "Endo", "Aoki", "Fujii",
			"Nishimura", "Fukuda", "Ota", "Miura", "Fujiwara", "Okamoto", "Matsuda", "Nakagawa", "Nakano", "Harada",
			"Ono", "Tamura", "Takeuchi", "Kaneko", "Wada", "Nakayama", "Ishida", "Ueda", "Morita", "Hara",
			"Shibata", "Sakai", "Kudo", "Yokoyama", "Miyazaki", "Miyamoto", "Uchida", "Takagi", "Ando", "Taniguchi",
			"Ohno", "Maruyama", "Imai", "Takeda", "Fujimoto", "Takahata", "Arai", "Kawai", "Tsuchiya", "Yamane",
			"Yoshikawa", "Matsui", "Iwasaki", "Takizawa", "Kataoka", "Nakamura", "Shibuya", "Kanai", "Kuroda", "Miyauchi",
			"Sakoda", "Masuwaka", "Chihaya", "Matsuda", "Shibasaki", "Mashima", "Fujioka", "Masutomi", "Hayakawa", "Nomura",
			"Mizuno", "Hiraoka", "Tsuboi", "Kawasaki", "Ueno", "Sugiyama", "Yoshimura", "Nagata", "Iwata", "Hirano",
			"Ogata", "Kojima", "Nagano", "Miyata", "Komiya", "Iwamoto", "Fukui", "Nagai", "Tani", "Sawada",
			"Matsuo", "Sugawara", "Watanabe", "Kawada", "Ishigaki", "Okawa", "Yamakawa", "Hirata", "Kubota", "Ishibashi",
			"Sakurai", "Sugita", "Noda", "Yoshioka", "Nakada", "Kamiya", "Yano", "Kawamoto", "Tomioka", "Shirai",
			"Yagi", "Ikegami", "Nakao", "Nishida", "Katayama", "Yamaki", "Sonoda", "Horie", "Matsunaga", "Terada"
		};

		// Large pool of Japanese given names (first names)
		private static readonly string[] givenNames = 
		{
			"Akira", "Hiroshi", "Takeshi", "Satoshi", "Kenji", "Masahiro", "Kazuhiro", "Naoki", "Taro", "Jiro",
			"Yuki", "Ai", "Yui", "Rina", "Sakura", "Hana", "Miku", "Yuka", "Rei", "Ami",
			"Mimori", "Ichiko", "Hinako", "Kotori", "Kazue", "Yumeko", "Mari", "Futaba", "Sayako", "Haruka",
			"Shiori", "Akane", "Misaki", "Nanami", "Asuka", "Kana", "Nana", "Saki", "Risa", "Emi",
			"Daiki", "Ryota", "Shota", "Kota", "Yuta", "Sota", "Ren", "Riku", "Haruto", "Yamato",
			"Ayaka", "Mizuki", "Yuzuki", "Kokoro", "Sara", "Akari", "Miyu", "Honoka", "Karin", "Nodoka",
			"Takuya", "Yuya", "Shinya", "Tatsuya", "Kazuya", "Tomoya", "Hiroya", "Naoya", "Yoshiaki", "Masaki",
			"Chika", "Nozomi", "Kanade", "Minami", "Riko", "Yui", "Moe", "Kohaku", "Tsukasa", "Miyuki",
			"Koichi", "Shoichi", "Junichi", "Kenichi", "Shinichi", "Daisuke", "Keisuke", "Ryosuke", "Yusuke", "Kosuke",
			"Yuzuru", "Mamoru", "Isamu", "Osamu", "Minoru", "Kaoru", "Satoru", "Noboru", "Akira", "Kiyoshi",
			"Mayumi", "Kazumi", "Megumi", "Hiromi", "Satomi", "Masami", "Hitomi", "Tomomi", "Naomi", "Kiyomi",
			"Machiko", "Sachiko", "Mariko", "Noriko", "Yuriko", "Akiko", "Reiko", "Keiko", "Seiko", "Meiko",
			"Yoshiko", "Kumiko", "Fumiko", "Rumiko", "Sumiko", "Tamiko", "Emiko", "Kimiko", "Tomoko", "Hanako",
			"Toshio", "Yukio", "Norio", "Hideo", "Tadao", "Kazuo", "Haruo", "Masao", "Takao", "Yoshio",
			"Chiaki", "Maki", "Asami", "Izumi", "Kaori", "Saori", "Shiori", "Midori", "Hikari", "Akari",
			"Sayuri", "Yayoi", "Miyuki", "Azuki", "Kohana", "Wakana", "Kana", "Rana", "Mana", "Hana",
			"Hayato", "Masato", "Minato", "Yamato", "Makoto", "Hiroto", "Naoto", "Akito", "Yukito", "Kaito",
			"Aoi", "Koi", "Yoi", "Noi", "Soi", "Toi", "Roi", "Moi", "Hoi", "Joi",
			"Subaru", "Tsukiko", "Yumiko", "Fumiko", "Sumire", "Kaede", "Momiji", "Tsubaki", "Azami", "Yukiko"
		};

		/// <summary>
		/// Converts a Unity SystemInfo.deviceUniqueIdentifier to a consistent Japanese name
		/// </summary>
		/// <param name="UniqueId">The unique ID from SystemInfo.deviceUniqueIdentifier</param>
		/// <returns>A Japanese name in "FamilyName GivenName" format</returns>
		public static string ConvertToJapaneseName(string UniqueId = "0")
		{
			// Use SHA-256 to create a consistent hash from the unique ID
			// This ensures the same ID always produces the same name
			using (SHA256 sha256 = SHA256.Create()) // dispose once complete
			{
				// get a 256bit Hash From UniqueId
				byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(UniqueId));

				// Convert first 8 bytes to long for family name selection
				long familyNameSeed = System.BitConverter.ToInt64(hashBytes, 0);
				if (familyNameSeed < 0) familyNameSeed = -familyNameSeed;

				// Convert next 8 bytes to long for given name selection
				long givenNameSeed = System.BitConverter.ToInt64(hashBytes, 8);
				if (givenNameSeed < 0) givenNameSeed = -givenNameSeed;

				// Select names based on hash values
				string familyName = familyNames[familyNameSeed % familyNames.Length];
				string givenName = givenNames[givenNameSeed % givenNames.Length];

				return $"{familyName} {givenName}";
			}
		}
	}

	// Extension class for easier usage
	public static class SystemIdToJapaneseName_Extension
	{
		public static string ToJapaneseName(this string Id)
		{
			return JapaneseNameGenerator.ConvertToJapaneseName(Id);
		}
	}
}