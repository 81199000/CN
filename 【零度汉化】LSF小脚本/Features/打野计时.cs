using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;
using LeagueSharp.SDK.Core.UI.INotifications;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Wrappers;
using SharpDX.Direct3D9;
using SharpDX;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Utils;
using LeagueSharp.SDK.Core.Extensions.SharpDX;

namespace LSF小脚本.Features {


	class 打野计时 {
		private Menu 菜单;
		private Font 大地图字体;
		private Font 小地图字体;
		private List<Camp> 野怪集合 = new List<Camp>();
		private int 小龙层数;
		private float 上次检测 = Environment.TickCount;
		private const float 检测间隔 = 800f;

		public 打野计时() {
			菜单 = new Menu("打野计时", "打野计时");
			菜单.Add(new MenuList<string>("时间格式","时间格式",new string[] {"分:秒","秒" }));
			菜单.Add(new MenuSlider("大地图字体大小", "大地图字体大小", 35, 30, 55));
			菜单.Add(new MenuColor("大地图字体颜色", "大地图字体颜色", new ColorBGRA(225, 225, 225, 225)));
			菜单.Add(new MenuSlider("小地图字体大小", "小地图字体大小", 23,15,35));
			菜单.Add(new MenuColor("小地图字体颜色", "小地图字体颜色", new ColorBGRA(225, 225, 225, 225)));
			菜单.Add(new MenuBool("通知", "大刷野怪刷新时通知", true));

			Program.Config.Add(菜单);

			初始化();

			

			int 小地图字体大小 = 菜单["小地图字体大小"].GetValue<MenuSlider>().Value;
			小地图字体 = new Font(Drawing.Direct3DDevice,
						new FontDescription
						{
							FaceName = "微软雅黑",
							Height = 小地图字体大小,
							OutputPrecision = FontPrecision.Default,
							Quality = FontQuality.Default
						});

			int 大地图字体大小 = 菜单["大地图字体大小"].GetValue<MenuSlider>().Value;
			大地图字体 = new Font(Drawing.Direct3DDevice,
						new FontDescription
						{
							FaceName = "微软雅黑",
							Height = 大地图字体大小,
							OutputPrecision = FontPrecision.Default,
							Quality = FontQuality.Default
						});

			Logging.Write()(LogLevel.Info, "时间格式：" + 菜单["时间格式"].GetValue<MenuList>().Index);

			Game.OnUpdate += Game_OnUpdate;
			GameObject.OnCreate += GameObject_OnCreate;
			GameObject.OnDelete += GameObject_OnDelete;
			Drawing.OnEndScene += Drawing_OnEndScene;
			Drawing.OnDraw += Drawing_OnDraw;
        }

		private void Drawing_OnDraw(EventArgs args) {

			var 格式 = 菜单["时间格式"].GetValue<MenuList>().Index;
			var 颜色 = 菜单["大地图字体颜色"].GetValue<MenuColor>();
			bool UseNotificed = 菜单["通知"].GetValue<MenuBool>().Value;

			foreach (var camp in 野怪集合.Where(c => c.Dead))
			{
				
                if (UseNotificed && camp.IsBig && (int)camp.NextRespawnTime - (int)Game.Time < 20)
				{
					string team = "";
					if (camp.Team == ObjectManager.Player.Team)
					{
						team = "我方";
					}
					else if (camp.Team != GameObjectTeam.Neutral)
					{
						team = "敌方";
					}

					string hander = team + camp.Name + "马上复活";
					Notification RespawnNotification  = new Notification(hander,"");
					if (camp.Noticed == false)
					{
						Notifications.Add(RespawnNotification);
						camp.Noticed = true;
                        DelayAction.Add(20, () => {
							Notifications.Remove(RespawnNotification);
							camp.Noticed = false;
                        });
					}
				}

				if (camp.NextRespawnTime - Game.Time <= 0)
				{
					camp.Dead = false;
					continue;
				}

				if (camp.Position.IsOnScreen())
				{
					int time = (int)camp.NextRespawnTime - (int)Game.Time;
					string timeFormat = "";
                    if (格式 == 0 && time > 69)
					{
						TimeSpan ts = new TimeSpan(0, 0, time);
						timeFormat = ts.Minutes + ":" + ts.Seconds;
					}
					else
					{
						timeFormat = time.ToString();
                    }
					var rec = Drawing.GetTextExtent(timeFormat);
					大地图字体.DrawText(null, 
						timeFormat,
						(int)Drawing.WorldToScreen(camp.Position).X - rec.Width / 2, 
						(int)Drawing.WorldToScreen(camp.Position).Y - rec.Height / 2,
						颜色);
				}
			}
		}

		private void Drawing_OnEndScene(EventArgs args) {
			var 格式 = 菜单["时间格式"].GetValue<MenuList>().Index;
			var 颜色 = 菜单["小地图字体颜色"].GetValue<MenuColor>();
            
			foreach (var camp in 野怪集合.Where(c => c.Dead))
			{
				
				int time = (int)camp.NextRespawnTime - (int)Game.Time;
				string timeFormat = "";
				if (格式 == 0 && time>69)
				{
					TimeSpan ts = new TimeSpan(0, 0, time);
					timeFormat = ts.Minutes + ":" + ts.Seconds;
				}
				else
				{
					timeFormat = time.ToString();
				}

				var rec = Drawing.GetTextExtent(timeFormat);
				小地图字体.DrawText(null,timeFormat,
						(int)(camp.MinimapPosition.X - rec.Width / 2),
						(int)(camp.MinimapPosition.Y - rec.Height/ 2),
						颜色);
			}
		}

		private void GameObject_OnDelete(GameObject sender, EventArgs args) {
			if (!sender.IsValid 
				|| sender.Type != GameObjectType.obj_AI_Minion 
				|| sender.Team != GameObjectTeam.Neutral)
			{
				return;
			}

			foreach (var camp in 野怪集合)
			{
				var mob =
					camp.Mobs.FirstOrDefault(m => m.Name.ToLower().Contains(sender.Name.ToLower()));//, StringComparison.OrdinalIgnoreCase
				if (mob != null)
				{
					mob.Dead = true;
					camp.Dead = camp.Mobs.All(m => m.Dead);
					if (camp.Dead)
					{
						camp.Dead = true;
						camp.NextRespawnTime = (int)Game.Time + camp.RespawnTime - 3;
					}
				}
			}
		}

		private void GameObject_OnCreate(GameObject sender, EventArgs args) {
			if (!sender.IsValid || sender.Type != GameObjectType.obj_AI_Minion ||
				   sender.Team != GameObjectTeam.Neutral)
			{
				return;
			}

			foreach (var camp in 野怪集合)
			{
				var mob = camp.Mobs.FirstOrDefault(m => m.Name.Contains(sender.Name));//, StringComparison.OrdinalIgnoreCase
				if (mob != null)
				{
					mob.Dead = false;
					camp.Dead = false;
				}
			}
		}

		private void Game_OnUpdate(EventArgs args) {
			if (上次检测 + 检测间隔 > Environment.TickCount)
			{
				return;
			}

			上次检测 = Environment.TickCount;

			var dragonStacks = 0;
			foreach (var enemy in GameObjects.EnemyHeroes)
			{
				var buff =
					enemy.Buffs.FirstOrDefault(
						b => b.Name.Equals("s5test_dragonslayerbuff", StringComparison.OrdinalIgnoreCase));
				if (buff != null)
				{
					dragonStacks = buff.Count;
				}
			}

			if (dragonStacks > 小龙层数 || dragonStacks == 5)
			{
				var dCamp = 野怪集合.FirstOrDefault(c => c.Mobs.Any(m => m.Name.Contains("Dragon")));
				if (dCamp != null && !dCamp.Dead)
				{
					dCamp.Dead = true;
					dCamp.NextRespawnTime = (int)Game.Time + dCamp.RespawnTime;
				}
			}

			小龙层数 = dragonStacks;

			var bCamp = 野怪集合.FirstOrDefault(c => c.Mobs.Any(m => m.Name.Contains("Baron")));
			if (bCamp != null && !bCamp.Dead)
			{
				var heroes = GameObjects.EnemyHeroes.Where(e => e.IsVisible);
				foreach (var hero in heroes)
				{
					var buff =
						hero.Buffs.FirstOrDefault(
							b => b.Name.Equals("exaltedwithbaronnashor", StringComparison.OrdinalIgnoreCase));
					if (buff != null)
					{
						bCamp.Dead = true;
						bCamp.NextRespawnTime = (int)buff.StartTime + bCamp.RespawnTime;
					}
				}
			}
		}

		private void 初始化() {
			野怪集合.AddRange(
					Data.Jungle.Camps.Where(c => c.MapType == Map.GetMap().Type)
						.Select(c => new Camp(c.SpawnTime, c.RespawnTime, c.Position, c.Mobs, c.IsBig, c.MapType, c.Team,c.Name)));
			
		}

		private class Camp : Data.Jungle.Camp {
			public Camp(float spawnTime,
				float respawnTime,
				Vector3 position,
				List<Data.Jungle.Mob> mobs,
				bool isBig,
				MapType mapType,
				GameObjectTeam team,
				string name,
				bool dead = false,
				bool noticed = false) : base(spawnTime, respawnTime, position, mobs, isBig, mapType, team, name) {
				Dead = dead;
				Noticed = noticed;
				Mobs = mobs.Select(mob => new Mob(mob.Name)).ToList();
			}

			public new List<Mob> Mobs { get; private set; }
			public float NextRespawnTime { get; set; }
			public bool Dead { get; set; }
			public bool Noticed { get; set; }
        }

		private class Mob : Data.Jungle.Mob {
			public Mob(string name, bool dead = false) : base(name) {
				Dead = dead;
			}

			public bool Dead { get; set; }
		}
	}

}
