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
	class 屏蔽显示 {

		
		static Menu StreamMenu;
		static MenuKeyBind DisalbeDrawMenu;
		static bool DisableDrawings;
		Obj_AI_Hero Player = ObjectManager.Player;
		static int Kills = 0;
		/// <summary>
		/// If the user is attacking
		/// Currently used for the second style of fake clicks
		/// </summary>
		private static bool attacking = false;

		/// <summary>
		/// The delta t for click frequency
		/// </summary>
		private static float deltaT = .2f;

		/// <summary>
		/// The last direction of the player
		/// </summary>
		private static Vector3 direction;

		/// <summary>
		/// The last endpoint the player was moving to.
		/// </summary>
		private static Vector3 lastEndpoint = new Vector3();

		/// <summary>
		/// The last order the player had.
		/// </summary>
		private static GameObjectOrder lastOrder;

		/// <summary>
		/// The time of the last order the player had.
		/// </summary>
		private static float lastOrderTime = 0f;

		/// <summary>
		/// The last time a click was done.
		/// </summary>
		private static float lastTime = 0f;

		/// <summary>
		/// The Player.
		/// </summary>
		private static Obj_AI_Hero player = ObjectManager.Player;

		/// <summary>
		/// The Random number generator
		/// </summary>
		private static Random r = new Random();

		

		/// <summary>
		/// The move fake click after attacking
		/// </summary>
		/// <param name="unit">
		/// The unit.
		/// </param>
		/// <param name="target">
		/// The target.
		/// </param>
		//private static void AfterAttack(AttackableUnit unit, AttackableUnit target) {
		//	attacking = false;
		//	var t = target as Obj_AI_Hero;
		//	if (t != null && unit.IsMe)
		//	{
		//		Hud.ShowClick(ClickType.Move, RandomizePosition(t.Position));
		//	}
		//}

		/// <summary>
		/// The angle between two vectors.
		/// </summary>
		/// <param name="a">
		/// The first vector.
		/// </param>
		/// <param name="b">
		/// The second vector.
		/// </param>
		/// <returns>
		/// The Angle between two vectors
		/// </returns>
		private static float AngleBetween(Vector3 a, Vector3 b) {
			var dotProd = Vector3.Dot(a, b);
			var lenProd = a.Length() * b.Length();
			var divOperation = dotProd / lenProd;
			return (float)(Math.Acos(divOperation) * (180.0 / Math.PI));
		}

		/// <summary>
		/// The before attack fake click.
		/// Currently used for the second style of fake clicks
		/// </summary>
		/// <param name="args">
		/// The args.
		/// </param>
		//private static void BeforeAttackFake(Orbwalking.BeforeAttackEventArgs args) {
		//	if (root.SubMenu("Fake Clicks").Item("Click Mode").GetValue<StringList>().SelectedIndex == 1)
		//	{
		//		Hud.ShowClick(ClickType.Attack, RandomizePosition(args.Target.Position));
		//		attacking = true;
		//	}
		//}

		/// <summary>
		/// The fake click before you cast a spell
		/// </summary>
		/// <param name="s">
		/// The Spell Book.
		/// </param>
		/// <param name="args">
		/// The args.
		/// </param>
		private static void BeforeSpellCast(Spellbook s, SpellbookCastSpellEventArgs args) {
			if (args.Target.Position.Distance(player.Position) >= 5f)
			{
				Hud.ShowClick(ClickType.Attack, args.Target.Position);
			}
		}

		/// <summary>
		/// The on new path fake.
		/// Currently used for the second style of fake clicks
		/// </summary>
		/// <param name="sender">
		/// The sender.
		/// </param>
		/// <param name="args">
		/// The args.
		/// </param>
		private static void DrawFake(Obj_AI_Base sender, GameObjectNewPathEventArgs args) {
			if (sender.IsMe && lastTime + deltaT < Game.Time && args.Path.LastOrDefault() != lastEndpoint
				&& args.Path.LastOrDefault().Distance(player.ServerPosition) >= 5f
				&& StreamMenu["模拟点击"].GetValue<MenuBool>()
				&& StreamMenu["模拟方式"].GetValue<MenuList>().Index == 1)
			{
				lastEndpoint = args.Path.LastOrDefault();
				if (!attacking)
				{
					Hud.ShowClick(ClickType.Move, Game.CursorPos);
				}
				else
				{
					Hud.ShowClick(ClickType.Attack, Game.CursorPos);
				}

				lastTime = Game.Time;
			}
		}
		/// <summary>
		/// The OnIssueOrder event delegate.
		/// Currently used for the first style of fake clicks
		/// </summary>
		/// <param name="sender">
		/// The sender.
		/// </param>
		/// <param name="args">
		/// The args.
		/// </param>
		private static void OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args) {
			if (sender.IsMe
				&& (args.Order == GameObjectOrder.MoveTo || args.Order == GameObjectOrder.AttackUnit
					|| args.Order == GameObjectOrder.AttackTo)
				&& lastOrderTime + r.NextFloat(deltaT, deltaT + .2f) < Game.Time
				&& StreamMenu["模拟点击"].GetValue<MenuBool>()
				&& StreamMenu["模拟方式"].GetValue<MenuList>().Index == 0)
			{
				var vect = args.TargetPosition;
				vect.Z = player.Position.Z;
				if (args.Order == GameObjectOrder.AttackUnit || args.Order == GameObjectOrder.AttackTo)
				{
					Hud.ShowClick(ClickType.Attack, RandomizePosition(vect));
				}
				else
				{
					Hud.ShowClick(ClickType.Move, vect);
				}

				lastOrderTime = Game.Time;
			}
		}

		public 屏蔽显示() {
			StreamMenu = new Menu("屏蔽显示", "屏蔽显示");
			StreamMenu.Add(new MenuBool("屏蔽发话", "禁止脚本发话"));
			StreamMenu.Add(new MenuKeyBind("屏蔽显示", "屏蔽L#的显示", Keys.Home, KeyBindType.Toggle));
			StreamMenu.Add(new MenuKeyBind("按下显示", "屏蔽时按下可显示L#内容", Keys.Shift, KeyBindType.Press));
			StreamMenu.Add(new MenuBool("死亡屏蔽显示", "死亡屏蔽显示"));
			StreamMenu.Add(new MenuSlider("已连杀人数", "已连杀人数", 0, 0, 8));
			StreamMenu.Add(new MenuBool("超神屏蔽显示", "超神屏蔽显示", true));
			StreamMenu.Add(new MenuBool("多杀屏蔽显示", "多杀屏蔽显示", true));
			StreamMenu.Add(new MenuSlider("多杀屏蔽时间", "多杀屏蔽时间", 4, 0, 10));
			Program.Config.Add(StreamMenu);

			Game.OnUpdate += OnUpdate;
			Obj_AI_Base.OnNewPath += DrawFake;
			Orbwalker.OnAction += Orbwalker_OnAction;
			Spellbook.OnCastSpell += BeforeSpellCast;
			Obj_AI_Base.OnIssueOrder += OnIssueOrder;
			Game.OnNotify += Game_OnNotify;

			StreamMenu["屏蔽显示"].GetValue<MenuKeyBind>().ValueChanged += 屏蔽显示_ValueChanged;
			StreamMenu["按下显示"].GetValue<MenuKeyBind>().ValueChanged += 按下显示_ValueChanged;
			StreamMenu["屏蔽发话"].GetValue<MenuBool>().ValueChanged += 屏蔽发话_ValueChanged;
        }

		private void 屏蔽发话_ValueChanged(object sender, EventArgs e) {

			if (StreamMenu["屏蔽发话"].GetValue<MenuBool>().Value)
			{
				Hacks.DisableSay = true;
			}
			else
			{
				Hacks.DisableSay = false;
			}
		}

		private static void 按下显示_ValueChanged(object sender, EventArgs e) {
			if (StreamMenu["按下显示"].GetValue<MenuKeyBind>().Active && Hacks.DisableDrawings == true)
			{
				Hacks.DisableDrawings = false;
			}
		}

		private static void 屏蔽显示_ValueChanged(object sender, EventArgs e) {
			if (StreamMenu["屏蔽显示"].GetValue<MenuKeyBind>().Active)
			{
				Hacks.DisableDrawings = true;
			}
			else
			{
				Hacks.DisableDrawings = false;
			}
		}

		private void Game_OnNotify(GameNotifyEventArgs args) {


			if (args.EventId == GameEventId.OnGameStart || args.EventId == GameEventId.OnEndGame)
			{
				StreamMenu["已连杀人数"].GetValue<MenuSlider>().Value = 0;
			}

			if (args.EventId == GameEventId.OnKill && args.NetworkId == GameObjects.Player.NetworkId)
			{
				StreamMenu["已连杀人数"].GetValue<MenuSlider>().Value = 0;
			}

			if (args.NetworkId == GameObjects.Player.NetworkId
				&& (args.EventId == GameEventId.OnChampionTripleKill
					|| args.EventId == GameEventId.OnChampionQuadraKill
					|| args.EventId == GameEventId.OnChampionPentaKill
					|| args.EventId == GameEventId.OnAce)
				&& Hacks.DisableDrawings == false)
			{

				if (StreamMenu["多杀屏蔽显示"].GetValue<MenuBool>().Value)
				{
					int time = StreamMenu["多杀屏蔽时间"].GetValue<MenuSlider>().Value;
					Hacks.DisableDrawings = true;
					DelayAction.Add(time * 1000, () =>
					{
						Hacks.DisableDrawings = false;
					});
				}

			}

			if (args.EventId == GameEventId.OnChampionDie
				&& args.NetworkId == GameObjects.Player.NetworkId)
			{
				StreamMenu["已连杀人数"].GetValue<MenuSlider>().Value += 1;


				if (StreamMenu["已连杀人数"].GetValue<MenuSlider>().Value >= 8
					&& StreamMenu["超神屏蔽显示"].GetValue<MenuBool>().Value
					&& Hacks.DisableDrawings == false)
				{
					int time = StreamMenu["多杀屏蔽时间"].GetValue<MenuSlider>().Value;

					Hacks.DisableDrawings = true;
					DelayAction.Add(time * 1000, () =>
					{
						Hacks.DisableDrawings = false;
					});
				}

			}


		}

		private void OnUpdate(EventArgs args) {

		}

		private void Orbwalker_OnAction(object sender, Orbwalker.OrbwalkerActionArgs e) {
			var target = e.Target;
			if (e.Type == OrbwalkerType.BeforeAttack)
			{
				BeforeAttackFake(e);
            }
			if (e.Type == OrbwalkerType.AfterAttack)
			{
				AfterAttack(sender,e.Target);
            }
		}
		private static void AfterAttack(object unit, AttackableUnit target) {
			attacking = false;
			var sender = unit as Obj_AI_Hero;
            var t = target as Obj_AI_Hero;
			if (t != null && sender.IsMe)
			{
				Hud.ShowClick(ClickType.Move, RandomizePosition(t.Position));
			}
		}

		private static void BeforeAttackFake(Orbwalker.OrbwalkerActionArgs e) {
			
            if (StreamMenu["模拟方式"].GetValue<MenuList>().Index == 1)
			{
				Hud.ShowClick(ClickType.Attack, RandomizePosition(e.Target.Position));
				attacking = true;
			}
		}

		/// <summary>
		/// The RandomizePosition function to randomize click location.
		/// </summary>
		/// <param name="input">
		/// The input Vector3.
		/// </param>
		/// <returns>
		/// A Vector within 100 units of the unit
		/// </returns>
		private static Vector3 RandomizePosition(Vector3 input) {
			if (r.Next(2) == 0)
			{
				input.X += r.Next(100);
			}
			else
			{
				input.Y += r.Next(100);
			}

			return input;
		}

		
	}
}
