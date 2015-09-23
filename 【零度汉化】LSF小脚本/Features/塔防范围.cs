using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Color = System.Drawing.Color;
using Keys = System.Windows.Forms.Keys;
using LeagueSharp;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;
using LeagueSharp.SDK.Core.UI.INotifications;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.UI.IMenu.Abstracts;
using LeagueSharp.SDK.Core.UI.IMenu.Customizer;
using LeagueSharp.SDK.Core.UI.IMenu.Skins;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Wrappers;
using SharpDX.Direct3D9;
using SharpDX;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Utils;
using LeagueSharp.SDK.Core.Extensions.SharpDX;

namespace LSF小脚本.Features {
	class 塔防范围 {

		private const int TurretRange = 875;
		private Menu TurretMenu;
		private ColorBGRA AttackColor = new ColorBGRA(225, 0, 0, 225);
		private ColorBGRA AllyColor = new ColorBGRA(0, 255, 0, 225);
		private ColorBGRA EnemyColor = new ColorBGRA(0, 0, 255, 225);

		public 塔防范围() {
			TurretMenu = new Menu("塔防范围", "塔防范围");
            TurretMenu.Add(new MenuColor("敌军颜色", "敌军颜色", EnemyColor));
			TurretMenu.Add(new MenuColor("友军颜色", "友军颜色", AllyColor));
			Program.Config.Add(TurretMenu);

			Drawing.OnDraw += Drawing_OnDraw;
        }

		private void Drawing_OnDraw(EventArgs args) {
			var TurretList = GameObjects.Turrets.Where(t => t.IsValid && t.IsVisible && !t.IsDead && t.Position.IsOnScreen() && t.Position.Distance(GameObjects.Player.Position)> TurretRange);
			foreach (var turret in TurretList)
			{
				if (turret.Position.Distance(GameObjects.Player.Position) <= 2000 && turret.Position.Distance(GameObjects.Player.Position) <= 2000)
				{
					int Alpha = (int)(turret.Position.Distance(GameObjects.Player.Position) - TurretRange) / (2000 - TurretRange);
					if (turret.IsAlly)
					{
						//AllyColor = new ColorBGRA(0, 255, 0, Alpha);
						//Drawing.DrawCircle(turret.Position,
						//	TurretRange,
						//	Color.FromArgb(Alpha,Color.Green));

						new LeagueSharp.SDK.Core.Math.Polygons.Circle(turret.Position, TurretRange, 5).Draw(Color.FromArgb(Alpha, Color.Green));
                    }
					else
					{
						//Drawing.DrawCircle(turret.Position,
						//	TurretRange,
						//	Color.FromArgb(Alpha, Color.GreenYellow));
						new LeagueSharp.SDK.Core.Math.Polygons.Circle(turret.Position, TurretRange, 5).Draw(Color.FromArgb(Alpha, Color.YellowGreen));
					}
				}
				if (turret.Position.Distance(GameObjects.Player.Position) <= TurretRange)
				{
					Color color = Color.Red;
					if (turret.IsAlly)
					{
						color = Color.Green;
                    }
					if (turret.IsEnemy)
					{
						if (turret.Target.IsMe)
						{
							color = Color.Red;
						}
						else
						{
							color = Color.GreenYellow;
						}
					}
					//Drawing.DrawCircle(turret.Position,
					//		TurretRange,
					//		color);
					new LeagueSharp.SDK.Core.Math.Polygons.Circle(turret.Position, TurretRange, 5).Draw(color);
				}

			}
		}
	}
}
