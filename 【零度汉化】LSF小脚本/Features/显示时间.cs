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
	class 显示时间 {
		Menu ShowTimeMenu = null;
		Font 字体;

		public 显示时间() {
			ShowTimeMenu = new Menu("显示时间", "显示时间");
			ShowTimeMenu.Add(new MenuBool("显示真实时间", "显示真实时间", true));
			ShowTimeMenu.Add(new MenuSlider("TimeX", "横向显示位置(占屏幕百分比)", 71));
			ShowTimeMenu.Add(new MenuSlider("TimeY", "竖向显示位置(占屏幕百分比)", 0));
			ShowTimeMenu.Add(new MenuColor("颜色", "颜色", new SharpDX.ColorBGRA(0,255,147,255)));
			Program.Config.Add(ShowTimeMenu);

			
			字体 = new Font(Drawing.Direct3DDevice,
						new FontDescription
						{
							FaceName = "微软雅黑",
							Height = 28,
							OutputPrecision = FontPrecision.Default,
							Quality = FontQuality.Default
						});
			Drawing.OnDraw += Drawing_OnDraw;
		}

		private void Drawing_OnDraw(EventArgs args) {
			var Enable = ShowTimeMenu["显示真实时间"].GetValue<MenuBool>().Value;
			if (!Enable) return;
			var SelectColor = ShowTimeMenu["颜色"].GetValue<MenuColor>().Color;
			字体.DrawText(null, "现在时间：" + DateTime.Now.ToShortTimeString(),
						Drawing.Width * ShowTimeMenu["TimeX"].GetValue<MenuSlider>().Value / 100,
						Drawing.Height * ShowTimeMenu["TimeY"].GetValue<MenuSlider>().Value / 100,
						SelectColor);
		}
	}
}
