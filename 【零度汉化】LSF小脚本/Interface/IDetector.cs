using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using SharpDX;

namespace LSF小脚本.Interface {
	public enum DetectorSetting { Preferred, Safe, AntiHumanizer }
	interface IDetector {
		void Initialize(Obj_AI_Hero hero, DetectorSetting setting = DetectorSetting.Safe);
		void ApplySetting(DetectorSetting setting);
		void FeedData(Vector3 targetPos);
		int GetScriptDetections();
		string GetName();
	}
}
