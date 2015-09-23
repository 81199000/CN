using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
using LSF小脚本.Interface;

namespace LSF小脚本.Features {
	class 检测外挂 {
		private readonly Dictionary<int, List<IDetector>> _detectors = new Dictionary<int, List<IDetector>>();
		private Menu Config;
		Notification notification = new Notification("下面的人员有开挂的嫌疑！", "");
		bool Notificed = false;

		public 检测外挂() {
			Config = new Menu("检测外挂", "检测外挂");
			Config.Add(new MenuBool("启用", "启用", true));
			var detectionType = new MenuList<string>("detection", "检测级别",new string[] { "优先","一般","不检测"});
			detectionType.ValueChanged += (sender, args) =>
			{
				foreach (var detector in _detectors)
				{
					detector.Value.ForEach(item => item.ApplySetting((DetectorSetting)detectionType.Index));
				}
			};
			Config.Add(detectionType);
			Program.Config.Add(Config);

			Obj_AI_Hero.OnNewPath += Obj_AI_Hero_OnNewPath;
			Game.OnUpdate += Game_OnUpdate;

			
			notification.IsOpen = true;
			
        }

		private void Game_OnUpdate(EventArgs args) {
			
			string content = "";

            foreach (var detector in _detectors)
			{
				var maxValue = detector.Value.Max(item => item.GetScriptDetections());
				var hero = GameObjects.AllyHeroes.First(h => h.NetworkId == detector.Key);

				if ( maxValue > 0)
				{
					if (!Notificed)
					{
						Notificed = true;
						Notifications.Add(notification);
					}
					string info = hero.Name + ": " + detector.Value.First(itemId => itemId.GetScriptDetections() == maxValue).GetName() + ";";
					content += info + Environment.NewLine;
				}
               
            }
			
            notification.Body = content;
			notification.OnUpdate();
		}

		private void Obj_AI_Hero_OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args) {
			if (!Config["启用"].GetValue<MenuBool>()) return;

			if (!_detectors.ContainsKey(sender.NetworkId))
			{
				var detectors = new List<IDetector> { new SacOrbwalkerDetector(), new LeaguesharpOrbwalkDetector() };
				detectors.ForEach(detector => detector.Initialize((Obj_AI_Hero)sender));
				_detectors.Add(sender.NetworkId, detectors);
			}
			else
			{
				_detectors[sender.NetworkId].ForEach(detector => detector.FeedData(args.Path.Last()));
			}
		}
	}
}
