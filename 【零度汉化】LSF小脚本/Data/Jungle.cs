using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp;

namespace LSF小脚本.Data {
	class Jungle {
		public static List<Camp> Camps;

		static Jungle() {
			try
			{
				Camps = new List<Camp>
				{
                    // Order: Blue
                    new Camp(
						115, 300, new Vector3(3800.99f, 7883.53f, 52.18f),
						new List<Mob>(
							new[]
							{
								new Mob("SRU_Blue1.1.1", true), new Mob("SRU_BlueMini1.1.2"),
								new Mob("SRU_BlueMini21.1.3")
							}), true, MapType.SummonersRift,
						GameObjectTeam.Order,
						"蓝Buff"
						),
                    //Order: Wolves
                    new Camp(
						115, 100, new Vector3(3849.95f, 6504.36f, 52.46f),
						new List<Mob>(
							new[]
							{
								new Mob("SRU_Murkwolf2.1.1", true), new Mob("SRU_MurkwolfMini2.1.2"),
								new Mob("SRU_MurkwolfMini2.1.3")
							}), false, MapType.SummonersRift,
						GameObjectTeam.Order,
						"Murkwolf"),
                    //Order: Chicken
                    new Camp(
						115, 100, new Vector3(6943.41f, 5422.61f, 52.62f),
						new List<Mob>(
							new[]
							{
								new Mob("SRU_Razorbeak3.1.1", true), new Mob("SRU_RazorbeakMini3.1.2"),
								new Mob("SRU_RazorbeakMini3.1.3"), new Mob("SRU_RazorbeakMini3.1.4")
							}), false,
						MapType.SummonersRift, GameObjectTeam.Order,
						"Razorbeak"),
                    //Order: Red
                    new Camp(
						115, 300, new Vector3(7813.07f, 4051.33f, 53.81f),
						new List<Mob>(
							new[]
							{ new Mob("SRU_Red4.1.1", true), new Mob("SRU_RedMini4.1.2"), new Mob("SRU_RedMini4.1.3") }),
						true, MapType.SummonersRift, GameObjectTeam.Order,
						"红Buff"),
                    //Order: Krug
                    new Camp(
						115, 100, new Vector3(8370.58f, 2718.15f, 51.09f),
						new List<Mob>(new[] { new Mob("SRU_Krug5.1.2", true), new Mob("SRU_KrugMini5.1.1") }), false,
						MapType.SummonersRift, GameObjectTeam.Order,
						"SRU_Krug"),
                    //Order: Gromp
                    new Camp(
						115, 100, new Vector3(2164.34f, 8383.02f, 51.78f),
						new List<Mob>(new[] { new Mob("SRU_Gromp13.1.1", true) }), false,
						MapType.SummonersRift, GameObjectTeam.Order,
						"Gromp"),
                    //Chaos: Blue
                    new Camp(
						115, 300, new Vector3(10984.11f, 6960.31f, 51.72f),
						new List<Mob>(
							new[]
							{
								new Mob("SRU_Blue7.1.1", true), new Mob("SRU_BlueMini7.1.2"),
								new Mob("SRU_BlueMini27.1.3")
							}), true, MapType.SummonersRift,
						GameObjectTeam.Chaos,
						"蓝Buff"),
                    //Chaos: Wolves
                    new Camp(
						115, 100, new Vector3(10983.83f, 8328.73f, 62.22f),
						new List<Mob>(
							new[]
							{
								new Mob("SRU_Murkwolf8.1.1", true), new Mob("SRU_MurkwolfMini8.1.2"),
								new Mob("SRU_MurkwolfMini8.1.3")
							}), false, MapType.SummonersRift,
						GameObjectTeam.Chaos,
						"Murkwolf"),
                    //Chaos: Chicken
                    new Camp(
						115, 100, new Vector3(7852.38f, 9562.62f, 52.30f),
						new List<Mob>(
							new[]
							{
								new Mob("SRU_Razorbeak9.1.1", true), new Mob("SRU_RazorbeakMini9.1.2"),
								new Mob("SRU_RazorbeakMini9.1.3"), new Mob("SRU_RazorbeakMini9.1.4")
							}), false,
						MapType.SummonersRift, GameObjectTeam.Chaos,
						"Razorbeak"),
                    //Chaos: Red
                    new Camp(
						115, 300, new Vector3(7139.29f, 10779.34f, 56.38f),
						new List<Mob>(
							new[]
							{
								new Mob("SRU_Red10.1.1", true), new Mob("SRU_RedMini10.1.2"), new Mob("SRU_RedMini10.1.3")
							}), true, MapType.SummonersRift, GameObjectTeam.Chaos,
						"红Buff"),
                    //Chaos: Krug
                    new Camp(
						115, 100, new Vector3(6476.17f, 12142.51f, 56.48f),
						new List<Mob>(new[] { new Mob("SRU_Krug11.1.2", true), new Mob("SRU_KrugMini11.1.1") }), false,
						MapType.SummonersRift, GameObjectTeam.Chaos,
						"Krug"),
                    //Chaos: Gromp
                    new Camp(
						115, 100, new Vector3(12671.83f, 6306.60f, 51.71f),
						new List<Mob>(new[] { new Mob("SRU_Gromp14.1.1", true) }), false,
						MapType.SummonersRift, GameObjectTeam.Chaos,
						"Gromp"),
                    //Neutral: Dragon
                    new Camp(
						150, 360, new Vector3(9813.83f, 4360.19f, -71.24f),
						new List<Mob>(new[] { new Mob("SRU_Dragon6.1.1", true) }), true,
						MapType.SummonersRift, GameObjectTeam.Neutral,
						"小龙"),
                    //Neutral: Baron
                    new Camp(
						120, 420, new Vector3(4993.14f, 10491.92f, -71.24f),
						new List<Mob>(new[] { new Mob("SRU_Baron12.1.1", true) }), true,
						MapType.SummonersRift, GameObjectTeam.Neutral,
						"大龙"),
                    //Dragon: Crab
                    new Camp(
						150, 180, new Vector3(10647.70f, 5144.68f, -62.81f),
						new List<Mob>(new[] { new Mob("SRU_Crab15.1.1", true) }), false,
						MapType.SummonersRift, GameObjectTeam.Neutral,
						"河蟹"),
                    //Baron: Crab
                    new Camp(
						150, 180, new Vector3(4285.04f, 9597.52f, -67.60f),
						new List<Mob>(new[] { new Mob("SRU_Crab16.1.1", true) }), false,
						MapType.SummonersRift, GameObjectTeam.Neutral,
						"河蟹"),
                    //Order: Wraiths
                    new Camp(
						95, 75, new Vector3(4373.14f, 5842.84f, -107.14f),
						new List<Mob>(
							new[]
							{
								new Mob("TT_NWraith1.1.1", true), new Mob("TT_NWraith21.1.2"), new Mob("TT_NWraith21.1.3")
							}), false, MapType.TwistedTreeline, GameObjectTeam.Order,
						"NWraith"),
                    //Order: Golems
                    new Camp(
						95, 75, new Vector3(5106.94f, 7985.90f, -108.38f),
						new List<Mob>(new[] { new Mob("TT_NGolem2.1.1", true), new Mob("TT_NGolem22.1.2") }), false,
						MapType.TwistedTreeline, GameObjectTeam.Order,
						"NGolem"),
                    //Order: Wolves
                    new Camp(
						95, 75, new Vector3(6078.15f, 6094.45f, -98.63f),
						new List<Mob>(
							new[]
							{ new Mob("TT_NWolf3.1.1", true), new Mob("TT_NWolf23.1.2"), new Mob("TT_NWolf23.1.3") }),
						false, MapType.TwistedTreeline, GameObjectTeam.Order,
						"NWolf"),
                    //Chaos: Wraiths
                    new Camp(
						95, 75, new Vector3(11025.95f, 5805.61f, -107.19f),
						new List<Mob>(
							new List<Mob>(
								new[]
								{
									new Mob("TT_NWraith4.1.1", true), new Mob("TT_NWraith24.1.2"),
									new Mob("TT_NWraith24.1.3")
								})), false, MapType.TwistedTreeline,
						GameObjectTeam.Chaos,
						"Wraith"),
                    //Chaos: Golems
                    new Camp(
						95, 75, new Vector3(10276.81f, 8037.54f, -108.92f),
						new List<Mob>(new[] { new Mob("TT_NGolem5.1.1", true), new Mob("TT_NGolem25.1.2") }), false,
						MapType.TwistedTreeline, GameObjectTeam.Chaos,
						"Golem"),
                    //Chaos: Wolves
                    new Camp(
						95, 75, new Vector3(9294.02f, 6085.41f, -96.70f),
						new List<Mob>(
							new List<Mob>(
								new[]
								{ new Mob("TT_NWolf6.1.1", true), new Mob("TT_NWolf26.1.2"), new Mob("TT_NWolf26.1.3") })),
						false, MapType.TwistedTreeline, GameObjectTeam.Chaos
						,"Wolf"),
                    //Neutral: Vilemaw
                    new Camp(
						600, 360, new Vector3(7738.30f, 10079.78f, -61.60f),
						new List<Mob>(new List<Mob>(new[] { new Mob("TT_Spiderboss8.1.1", true) })), true,
						MapType.SummonersRift, GameObjectTeam.Neutral,
						"Spiderboss")
				};
			}
			catch (Exception ex)
			{
				Camps = new List<Camp>();
				//Global.Logger.AddItem(new LogItem(ex));
			}
		}

		public class Camp {
			public Camp(float spawnTime,
				float respawnTime,
				Vector3 position,
				List<Mob> mobs,
				bool isBig,
				MapType mapType,
				GameObjectTeam team,
				string name) {
				SpawnTime = spawnTime;
				RespawnTime = respawnTime;
				Position = position;
				MinimapPosition = Drawing.WorldToMinimap(Position);
				Mobs = mobs;
				IsBig = isBig;
				MapType = mapType;
				Team = team;
				Name = name;
            }

			public float SpawnTime { get; set; }
			public float RespawnTime { get; private set; }
			public Vector3 Position { get; private set; }
			public Vector2 MinimapPosition { get; private set; }
			public List<Mob> Mobs { get; private set; }
			public bool IsBig { get; set; }
			public MapType MapType { get; set; }
			public GameObjectTeam Team { get; set; }
			public string Name { get; set; }
		}

		public class Mob {
			public Mob(string name, bool isBig = false) {
				Name = name;
				IsBig = isBig;
			}

			public string Name { get; private set; }
			public bool IsBig { get; set; }
		}
	}
}
