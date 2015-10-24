namespace LeagueSharp.Common
{
    using System;
    using Lost = LeagueSharp.Hacks;

    class Flowers
    {
        public static bool duramk = false;
        public static float gameTime1 = 0;
        public static readonly Menu fl = new Menu("开发功能", "Flowers Utility");

        internal static void Initilalize()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            fl.AddItem(new MenuItem("Flowers", "自动设置AA后摇(花边)").SetShared().SetValue(true));
            fl.AddItem(new MenuItem("Disable Drawing", "屏蔽线圈开关 默认按键:Home").SetValue(new KeyBind(36, KeyBindType.Toggle)));
            fl.AddItem(new MenuItem("zoom hack", "无限视距").SetValue(false));
            fl.AddItem(new MenuItem("disable say", "禁止脚本发聊天信息").SetValue(true));
            fl.AddItem(new MenuItem("Tower Ranges", "显示防御塔范围(靠近)").SetValue(false));
            fl.AddItem(new MenuItem("SaySomething", "开局自动屏蔽掉聊天框信息").SetValue(false));
            fl.AddItem(new MenuItem("SaySomething1", "By 花边"));
            CommonMenu.Config.AddSubMenu(fl);

            if (fl.Item("SaySomething").IsActive())
            {
                Game.Say("/");
                Game.Say("/");
                Game.Say("/");
                Game.Say("/");
            }

            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (fl.Item("zoom hack").IsActive())
            {
                Lost.ZoomHack = true;
            }
            else
            {
                Lost.ZoomHack = false;
            }

            if (fl.Item("Disable Drawing").GetValue<KeyBind>().Active)
            {
                Lost.DisableDrawings = true;
            }
            else
            {
                Lost.DisableDrawings = false;
            }

            if (fl.Item("disable say").GetValue<KeyBind>().Active)
            {
                Lost.DisableSay = true;
            }
            else
            {
                Lost.DisableSay = false;
            }

            if (fl.Item("Tower Ranges").GetValue<KeyBind>().Active)
            {
                Lost.TowerRanges = true;
            }
            else
            {
                Lost.TowerRanges = false;
            }
        }
    }
}
