using LeagueSharp;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSF小脚本.library {
	class Utills {
		private static readonly Dictionary<int, Font> Fonts = new Dictionary<int, Font>();
		public static Font GetFont(int fontSize) {
			Font font = null;
			try
			{
				if (!Fonts.TryGetValue(fontSize, out font))
				{
					font = new Font(
						Drawing.Direct3DDevice,
						new FontDescription
						{
							//FaceName = Global.DefaultFont,
							Height = fontSize,
							OutputPrecision = FontPrecision.Default,
							Quality = FontQuality.Default
						});
					Fonts[fontSize] = font;
				}
				else
				{
					//if (!_unloaded && (font == null || font.IsDisposed))
					//{
					//	Fonts.Remove(fontSize);
					//	GetFont(fontSize);
					//}
				}
			}
			catch (Exception ex)
			{
			
			}
			return font;
		}
	}
}
