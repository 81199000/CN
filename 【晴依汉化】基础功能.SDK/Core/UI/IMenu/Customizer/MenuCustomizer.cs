namespace LeagueSharp.SDK.Core.UI.IMenu.Customizer
{
    using LeagueSharp.SDK.Core.UI.IMenu.Skins;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;

    using SharpDX;
    using SharpDX.Direct3D9;

    /// <summary>
    /// This menu allows the user to modify several properties in <see cref="MenuSettings"/>.
    /// </summary>
    public sealed class MenuCustomizer : Menu
    {

        /// <summary>
        /// An instance of this MenuCustomizer
        /// </summary>
        public static MenuCustomizer Instance;

        /// <summary>
        /// Holds the X position of the menu.
        /// </summary>
        public MenuSlider PositionX { get; private set; }

        /// <summary>
        /// Holds the Y position of the menu.
        /// </summary>
        public MenuSlider PositionY { get; private set; }

        /// <summary>
        /// True if dragging of the menu is enabled.
        /// </summary>
        public MenuBool LockPosition { get; private set; }

        /// <summary>
        /// Holds the container height
        /// </summary>
        public MenuSlider ContainerHeight { get; private set; }

        /// <summary>
        /// Holds the font height
        /// </summary>
        public MenuSlider FontHeight { get; private set; }

        /// <summary>
        /// Holds the background color
        /// </summary>
        public MenuColor BackgroundColor { get; private set; }

        /// <summary>
        /// Creates a new instance of MenuCustomizer
        /// </summary>
        /// <param name="menu">The menu that will hold this MenuCustomizer</param>
        public static void Initialize(Menu menu)
        {
            if (Instance == null)
            {
                Instance = new MenuCustomizer(menu);
            }
        }

        private MenuCustomizer(Menu parentMenu)
            : base("menucustomizer", "菜单设置", false, "")
        {
            parentMenu.Add(this);
            BuildCustomizer();
            BuildOptions();

            ApplyChanges();
        }

        private void BuildCustomizer()
        {
            var customizeMenu = Add(new Menu("customize", "自定义"));
            AddPosition(customizeMenu);
            AddContainer(customizeMenu);
            customizeMenu.Add(new MenuButton("apply", "应用设置", "Apply") { Action = ApplyChanges });
            customizeMenu.Add(
                new MenuButton("reset", "重置设置", "Reset")
                    {
                        Action = delegate
                            {
                                customizeMenu.RestoreDefault();
                                ApplyChanges();
                            }
                    });
        }

        private void AddContainer(Menu menu)
        {
            ContainerHeight =
                menu.Add(new MenuSlider("containerheight", "菜单选项高度", MenuSettings.ContainerHeight, 15, 50));
            FontHeight =
                menu.Add(new MenuSlider("fontheight", "字体大小", MenuSettings.Font.Description.Height, 10, 30));
            BackgroundColor =
                menu.Add(new MenuColor("backgroundcolor", "背景颜色", MenuSettings.RootContainerColor));
        }

        private void AddPosition(Menu menu)
        {
            PositionX = menu.Add(new MenuSlider("x", "菜单X坐标(横)", (int)MenuSettings.Position.X, 0, Drawing.Width));
            PositionY = menu.Add(new MenuSlider("y", "菜单Y坐标(竖)", (int)MenuSettings.Position.Y, 0, Drawing.Height));
        }

        private void BuildOptions()
        {
            LockPosition = Add(new MenuBool("lock","锁定菜单位置"));
        }

        private void ApplyChanges()
        {
            MenuSettings.Position = new Vector2(PositionX.Value, PositionY.Value);
            MenuSettings.ContainerHeight = ContainerHeight.Value;
            var oldFont = MenuSettings.Font;
            MenuSettings.Font = new Font(
                Drawing.Direct3DDevice,
                FontHeight.Value,
                0,
                FontWeight.DoNotCare,
                0,
                false,
                FontCharacterSet.Default,
                FontPrecision.Raster,
                FontQuality.Antialiased,
                FontPitchAndFamily.DontCare | FontPitchAndFamily.Decorative,
                "Tahoma");
            oldFont.Dispose();
            MenuSettings.RootContainerColor = BackgroundColor.Color;
            MenuManager.Instance.ResetWidth();
        }


    }
}
