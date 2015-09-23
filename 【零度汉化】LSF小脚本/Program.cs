using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Keys = System.Windows.Forms.Keys;
using LeagueSharp;
using LeagueSharp.SDK.Core.UI.IMenu;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.UI.IMenu.Abstracts;
using LeagueSharp.SDK.Core.UI.IMenu.Customizer;
using LeagueSharp.SDK.Core.UI.IMenu.Skins;
using LeagueSharp.SDK.Core.UI.INotifications;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Utils;

namespace LSF小脚本 {
	class Program {

		public static Menu Config;
        static void Main(string[] args) {
			
			string str = "L#璧勬簮缃戝皬宸ュ叿杞藉叆";
			Game.PrintChat(str);

			Config = new Menu("LSF","【零度汉化】LSF小脚本",true);
			
			new Features.打野计时();
			new Features.显示时间();
			new Features.屏蔽显示();
			//new Features.塔防范围();
			new Features.检测外挂();
			Config.Attach();
			
			var startNotifi = new Notification("L#资源网小工具", "Http://lsharp.xyz\n我们将持续更新相关内容\n打野计时 载入");
			startNotifi.Icon = NotificationIconType.Check;
			startNotifi.IsOpen = true;
			startNotifi.IconFlash = true;
            Notifications.Add(startNotifi);
			DelayAction.Add(10000,()=> {
				Notifications.Remove(startNotifi);
			});
			
		}
	}
}
