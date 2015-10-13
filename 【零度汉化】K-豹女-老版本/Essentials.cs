using System;
using LeagueSharp;
using LeagueSharp.Common;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using SPrediction;
using CM = KurisuNidalee.Combat;
using KN = KurisuNidalee.KurisuNidalee;

namespace KurisuNidalee
{
    //  _____         _         __    _ _   
    // |  |  |_ _ ___|_|___ _ _|  |  |_| |_ 
    // |    -| | |  _| |_ -| | |  |__| | . |
    // |__|__|___|_| |_|___|___|_____|_|___|
    // Copyright © Kurisu Solutions 2015

    internal static class Essentials
    {
        internal static int Counter;
        internal static int MissileCount;
        internal static int LastAttack;
        internal static AttackableUnit LastUnit;
        internal static SPrediction.Collision Collision;
        internal static SpellSlot Smite;
        internal static bool SmiteInGame;
        internal static Obj_AI_Hero Player = ObjectManager.Player;
        internal static Dictionary<int, Obj_AI_Base> MinionCache = new Dictionary<int, Obj_AI_Base>(); 

        static Essentials()
        {
            SetSpells();
            GetSmiteSlot();
            Collision = new SPrediction.Collision();

            // Core
            Game.OnUpdate += SpellsOnUpdate;
            Game.OnUpdate += SmiteOnUpdate;

            // Orbwalk shit
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;

            // Minion Handelr
            GameObject.OnCreate += MinionOnCreate;
            GameObject.OnDelete += MinionOnDelete;

            // Missile Handler
            GameObject.OnCreate += MissileClient_OnCreate;

            // Cast Handler
            Obj_AI_Base.OnProcessSpellCast += HeroOnCast;

            // Anti-Gapclosing
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        /// <summary>
        /// Swipe/Javelin on gapclosers
        /// </summary>
        /// <param name="gapcloser"></param>
        internal static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var attacker = gapcloser.Sender;
            if (attacker.IsValidTarget(275f))
            {
                if (CatForm())
                {
                    CM.CastJavelin(attacker, "gap");
                    CM.SwitchForm(attacker, "gap");
                }

                else
                {
                    CM.CastSwipe(attacker, "gap");
                }
            }
        }

        /// <summary>
        /// Checks if a auto attack missile has been created.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        internal static void MissileClient_OnCreate(GameObject sender, EventArgs args)
        {
            var missile = sender as MissileClient;
            if (missile != null && missile.SpellCaster.IsMe)
            {
                if (missile.SData.IsAutoAttack())
                    MissileCount += 1;
            }
        }

        /// <summary>
        /// After Attack (Fires after an Orbwalker attack)
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="target"></param>
        internal static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe && MissileCount > 0)
            {
                Counter += 1;
                MissileCount = 0;
                LastUnit = unit;
                LastAttack = Utils.GameTimeTickCount;
            }
        }

        /// <summary>
        /// Before Attack (Fires before an Orbwalker attack)
        /// Check if before target is not mini
        /// </summary>
        /// <param name="args"></param>
        internal static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (KN.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Obj_AI_Minion mob = null;

                if (MinionList.Any(x => args.Target.Name.StartsWith(x) && !args.Target.Name.Contains("Mini")))
                    mob = (Obj_AI_Minion) args.Target;

                if (args.Target.Name.Contains("Mini") && mob.IsValidTarget(450))
                    args.Process = false;
            }
        }

        /// <summary>
        /// Returns our selected hitchance.
        /// </summary>
        /// <returns></returns>
        internal static HitChance MyHitChance(string ide)
        {
            return (HitChance) (KN.Root.Item("nd" + ide + "ch").GetValue<StringList>().SelectedIndex + 3);
        }

        /// <summary>
        /// Returns true if the spell is ready via game time.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        internal static bool IsReady(this float time, float extra = 0f)
        {
            return time < 1 + extra;
        }

        /// <summary>
        /// Returns true if the unit is Q marked.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        internal static bool IsHunted(this Obj_AI_Base unit)
        {
            return unit.HasBuff("nidaleepassivehunted");
        }

        /// <summary>
        /// Returns true if a spell is not learned.
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        internal static bool NotLearned(Spell spell)
        {
            return Player.Spellbook.GetSpell(spell.Slot).State == (SpellState) 12;
        }

        /// <summary>
        /// Checks if Nidalee is in Cougar form or not.
        /// </summary>
        /// <returns></returns>
        internal static bool CatForm()
        {
            return Player.Spellbook.GetSpell(SpellSlot.Q).Name != "JavelinToss";
        }

        /// <summary>
        /// Returns true if a target is immobile.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        internal static bool Immobile(Obj_AI_Hero unit)
        {
            return unit.HasBuffOfType(BuffType.Charm) || unit.HasBuffOfType(BuffType.Fear) ||
                   unit.HasBuffOfType(BuffType.Flee) || unit.HasBuffOfType(BuffType.Stun) ||
                   unit.HasBuffOfType(BuffType.Knockup) || unit.HasBuffOfType(BuffType.Snare) ||
                   unit.HasBuffOfType(BuffType.Taunt) || unit.HasBuffOfType(BuffType.Suppression);
        }

        /// <summary>
        /// Returns if the spell collides with anything.
        /// </summary>
        /// <param name="spell"></param>
        /// <param name="unit"></param>
        /// <param name="checkWall"></param>
        /// <returns></returns>
        internal static bool DoesCollide(this Spell spell, Vector2 endpos, bool checkWall = false)
        {
            return Collision.CheckCollision(Player.ServerPosition.To2D(), endpos, spell, true, true,
                true, false, checkWall);
        }

        /// <summary>
        /// Returns the available cougar damage dealt to the target
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        internal static float CatDamage(Obj_AI_Base target)
        {
            var damage = 0d;

            if (SpellTimer["Takedown"].IsReady())
                damage += Player.GetSpellDamage(target, SpellSlot.Q, 1);
            if (SpellTimer["Pounce"].IsReady())
                damage += Player.GetSpellDamage(target, SpellSlot.W, 1);
            if (SpellTimer["Swipe"].IsReady())
                damage += Player.GetSpellDamage(target, SpellSlot.E, 1);

            return (float) (damage + Player.GetAutoAttackDamage(target) * 2);
        }

        /// <summary>
        /// Minion Handler OnCreate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        internal static void MinionOnCreate(GameObject sender, EventArgs args)
        {
            var unit = sender as Obj_AI_Minion;
            if (unit.IsValid<Obj_AI_Minion>() && !unit.IsAlly)
            {
                if (!MinionCache.ContainsKey(unit.NetworkId))
                    MinionCache.Add(unit.NetworkId, unit);
            }
        }

        /// <summary>
        /// Minion Handler OnDelete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        internal static  void MinionOnDelete(GameObject sender, EventArgs args)
        {
            var unit = sender as Obj_AI_Minion;
            if (unit != null)
            {
                if (MinionCache.ContainsKey(unit.NetworkId))
                    MinionCache.Remove(unit.NetworkId);
            }
        }

        /// <summary>
        /// Casts swipe to the best location (aoe)
        /// </summary>
        internal static void CastSmartSwipe()
        {
            var minionpositions = MinionCache.Values.Select(x => x.Position.To2D()).ToList();
            var swipelocation = MinionManager.GetBestCircularFarmLocation(minionpositions, 275f, 375f);

            if (swipelocation.MinionsHit >= KurisuNidalee.Root.Item("ndcenum").GetValue<Slider>().Value)
                if (SpellTimer["Swipe"].IsReady())
                    Spells["Swipe"].Cast(swipelocation.Position);
        }

        /// <summary>
        /// Sets the nidalee spell data on load.
        /// </summary>
        internal static void SetSpells()
        {
            try
            {
                Spells["Javelin"].SetSkillshot(0.25f, 40f, 1300f, true, SkillshotType.SkillshotLine);
                Spells["Bushwhack"].SetSkillshot(0.25f, 100f, float.MaxValue, false, SkillshotType.SkillshotCircle);
                Spells["Swipe"].SetSkillshot(0.25f, (float)(15 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);
                Spells["Pounce"].SetSkillshot(0.50f, 400f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            }

            catch (Exception e)
            {
                Console.WriteLine("KL: SetSpellsException (" + e.Message + ")");
            }
        }

        /// <summary>
        /// Updates the spell timers on update.
        /// </summary>
        /// <param name="args"></param>
        internal static void SpellsOnUpdate(EventArgs args)
        {
            SpellTimer["Takedown"] = ((TimeStamp["Takedown"] - Game.Time) > 0) ? (TimeStamp["Takedown"] - Game.Time) : 0;
            SpellTimer["Pounce"] = ((TimeStamp["Pounce"] - Game.Time) > 0) ? (TimeStamp["Pounce"] - Game.Time) : 0;
            SpellTimer["Swipe"] = ((TimeStamp["Swipe"] - Game.Time) > 0) ? (TimeStamp["Swipe"] - Game.Time) : 0;
            SpellTimer["Javelin"] = ((TimeStamp["Javelin"] - Game.Time) > 0) ? (TimeStamp["Javelin"] - Game.Time) : 0;
            SpellTimer["Bushwhack"] = ((TimeStamp["Bushwhack"] - Game.Time) > 0) ? (TimeStamp["Bushwhack"] - Game.Time) : 0;
            SpellTimer["Primalsurge"] = ((TimeStamp["Primalsurge"] - Game.Time) > 0) ? (TimeStamp["Primalsurge"] - Game.Time) : 0;
        }


        /// <summary>
        /// Tracks the spells nidalee casts.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void HeroOnCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            try
            {
                if (sender.IsMe && args.SData.Name.ToLower() == "pounce")
                {
                    var unit = args.Target as Obj_AI_Base;
                    if (unit.IsValid<Obj_AI_Base>() && unit.IsHunted())
                        TimeStamp["Pounce"] = Game.Time + 1.5f;
                    else
                        TimeStamp["Pounce"] = Game.Time + (5 + (5 * Player.PercentCooldownMod));

                }

                if (sender.IsMe && args.SData.Name.ToLower() == "swipe")
                    TimeStamp["Swipe"] = Game.Time + (5 + (5 * Player.PercentCooldownMod));

                if (sender.IsMe && args.SData.Name.ToLower() == "takedown")
                    Utility.DelayAction.Add(Game.Ping + 250, Orbwalking.ResetAutoAttackTimer);

                if (sender.IsMe && args.SData.Name.ToLower() == "primalsurge")
                    TimeStamp["Primalsurge"] = Game.Time + (12 + (12 * Player.PercentCooldownMod));

                if (sender.IsMe && args.SData.Name.ToLower() == "bushwhack")
                {
                    var wperlevel = new[] { 13, 12, 11, 10, 9 }[Spells["Bushwhack"].Level];
                    TimeStamp["Bushwhack"] = Game.Time + (wperlevel + (wperlevel * Player.PercentCooldownMod));
                }

                if (sender.IsMe && args.SData.Name.ToLower() == "javelintoss")
                {
                    Counter = 0;
                    TimeStamp["Javelin"] = Game.Time + (6 + (6 * Player.PercentCooldownMod));

                    if (KN.Root.Item("exaa").GetValue<bool>())
                        Utility.DelayAction.Add(Game.Ping + 200, Orbwalking.ResetAutoAttackTimer);
                }

                if (sender.IsMe && args.SData.Name.ToLower() == "aspectofthecougar" && CatForm())
                {                    
                    Counter = 0;
                    if (KN.Root.Item("exaa").GetValue<bool>())
                        Utility.DelayAction.Add(Game.Ping + 200, Orbwalking.ResetAutoAttackTimer);
                }

                if (sender.IsMe && args.SData.IsAutoAttack() && Player.HasBuff("Takedown", true))
                    TimeStamp["Takedown"] = Game.Time + (5 + (5 * Player.PercentCooldownMod));
            }

            catch (Exception e)
            {
                Console.WriteLine("KL: OnCastException (" + e.Message + ")");
            }
        }

        /// <summary>
        /// Stores the nidalee spells.
        /// </summary>
        internal static Dictionary<string, Spell> Spells = new Dictionary<string, Spell>
        {
            { "Takedown", new Spell(SpellSlot.Q, 400f) },
            { "Pounce", new Spell(SpellSlot.W, 365f) },
            { "ExPounce", new Spell(SpellSlot.W, 740f) },
            { "Swipe", new Spell(SpellSlot.E, 300f) },
            { "Javelin", new Spell(SpellSlot.Q, 1500f) },
            { "Bushwhack", new Spell(SpellSlot.W, 875f) },
            { "Primalsurge", new Spell(SpellSlot.E, 600f)},
            { "Aspect", new Spell(SpellSlot.R) }
        };


        /// <summary>
        /// Stores when the last spell was used.
        /// </summary>
        internal static Dictionary<string, float> TimeStamp = new Dictionary<string, float>
        {
            { "Takedown", 0f },
            { "Pounce", 0f },
            { "Swipe", 0f },
            { "Javelin", 0f },
            { "Bushwhack", 0f },
            { "Primalsurge", 0f },
        };

        /// <summary>
        /// Stores the current tickcount of the spell.
        /// </summary>
        internal static Dictionary<string, float> SpellTimer = new Dictionary<string, float>
        {
            { "Takedown", 0f },
            { "Pounce", 0f },
            { "ExPounce", 0f },
            { "Swipe", 0f },
            { "Javelin", 0f },
            { "Bushwhack", 0f },
            { "Primalsurge", 0f },
            { "Aspect", 0f  }
        };

        /// <summary>
        /// Entire neutral list.
        /// </summary>
        internal static readonly string[] MinionList =
        {
            // summoners rift
            "SRU_Razorbeak", "SRU_Krug", "Sru_Crab", "SRU_Baron", "SRU_Dragon",
            "SRU_Blue", "SRU_Red", "SRU_Murkwolf", "SRU_Gromp", 
            
            // twisted treeline
            "TT_NGolem5", "TT_NGolem2", "TT_NWolf6", "TT_NWolf3",
            "TT_NWraith1", "TT_Spider"
        };

        /// <summary>
        /// Epic minion list.
        /// </summary>
        internal static readonly string[] EpicList =
        {
            "TT_Spiderboss", "SRU_Baron", "SRU_Dragon"
        };

        /// <summary>
        /// Small minion list.
        /// </summary>
        internal static readonly string[] SmallList =
        {
            "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Krug", "SRU_Gromp"
        };

        internal static readonly string[] LargeList =
        {
            "SRU_Blue", "SRU_Red", "TT_NWraith", "TT_NGolem", "TT_NWolf"
        };

        /// <summary>
        /// Gets the correct smite slot.
        /// </summary>
        internal static void GetSmiteSlot()
        {
            if (Player.GetSpell(SpellSlot.Summoner1).Name.ToLower().Contains("smite"))
            {
                SmiteInGame = true;
                Smite = SpellSlot.Summoner1;
            }

            if (Player.GetSpell(SpellSlot.Summoner2).Name.ToLower().Contains("smite"))
            {
                SmiteInGame = true;
                Smite = SpellSlot.Summoner2;
            }   
        }

        internal static Vector2? GetFirstWallPoint(Vector3 from, Vector3 to, float step = 25)
        {
            return GetFirstWallPoint(from.To2D(), to.To2D(), step);
        }

        internal static Vector2? GetFirstWallPoint(Vector2 from, Vector2 to, float step = 25)
        {
            var direction = (to - from).Normalized();

            for (float d = 0; d < from.Distance(to); d = d + step)
            {
                var testPoint = from + d * direction;
                var flags = NavMesh.GetCollisionFlags(testPoint.X, testPoint.Y);
                if (flags.HasFlag(CollisionFlags.Wall) || flags.HasFlag(CollisionFlags.Building))
                {
                    return from + (d - step) * direction;
                }
            }

            return null;
        }

        internal static void SmiteOnUpdate(EventArgs args)
        {
            if (!KN.Root.Item("jgsmite").GetValue<bool>())
                return;

            foreach (var minion in MinionManager.GetMinions(900f, MinionTypes.All, MinionTeam.Neutral))
            {
                var damage = Player.Spellbook.GetSpell(Smite).State == SpellState.Ready
                    ? (float) Player.GetSummonerSpellDamage(minion, Damage.SummonerSpell.Smite)
                    : 0;

                if (minion.Distance(Player.ServerPosition) > 500 + minion.BoundingRadius + Player.BoundingRadius)
                    return;

                if (LargeList.Any(name => minion.Name.StartsWith(name) && !minion.Name.Contains("Mini")))
                {
                    if (KN.Root.Item("jgsmitebg").GetValue<bool>())
                    {
                        if (KN.Root.Item("jgsmitetd").GetValue<bool>())
                        {
                            if (Player.GetSpellDamage(minion, SpellSlot.Q, 1) + damage >= minion.Health)
                                CM.CastTakedown(minion, "jg");
                        }
                            
                        if (damage >= minion.Health)
                        {
                            Player.Spellbook.CastSpell(Smite, minion);
                        }
                    }
                }

                if (SmallList.Any(name => minion.Name.StartsWith(name) && !minion.Name.Contains("Mini")))
                {
                    if (KN.Root.Item("jgsmitesm").GetValue<bool>())
                    {                       
                        if (KN.Root.Item("jgsmitetd").GetValue<bool>())
                        {
                            if (Player.GetSpellDamage(minion, SpellSlot.Q, 1) + damage >= minion.Health)
                                CM.CastTakedown(minion, "jg");
                        }

                        if (damage >= minion.Health)
                        {
                            Player.Spellbook.CastSpell(Smite, minion);
                        }
                    }
                }

                if (EpicList.Any(name => minion.Name.StartsWith(name)))
                {
                    if (KN.Root.Item("jgsmitetd").GetValue<bool>())
                    {
                        if (Player.GetSpellDamage(minion, SpellSlot.Q, 1) + damage >= minion.Health)
                            CM.CastTakedown(minion, "jg");
                    }

                    if (KN.Root.Item("jgsmiteep").GetValue<bool>())
                    {
                        if (damage >= minion.Health)
                        {
                            Player.Spellbook.CastSpell(Smite, minion);
                        }
                    }
                }
            }

            if (!KN.Root.Item("jgsmitehe").GetValue<bool>())
                return;

            // smite hero blu/red
            if (Player.GetSpell(Smite).Name.ToLower() == "s5_summonersmiteduel" ||
                Player.GetSpell(Smite).Name.ToLower() == "s5_summonersmiteplayerganker")
            {
                // KS Smite
                if (Player.GetSpell(Smite).Name.ToLower() == "s5_summonersmiteplayerganker")
                {
                    foreach (
                        var hero in
                            HeroManager.Enemies.Where(
                                h =>
                                    h.IsValidTarget(500) && !h.IsZombie &&
                                    h.Health <= 20 + 8 * Player.Level))
                    {
                        Player.Spellbook.CastSpell(Smite, hero);
                    }
                }

                // Combo Smite
                if (Player.GetSpell(Smite).Name.ToLower() == "s5_summonersmiteduel" ||
                    Player.GetSpell(Smite).Name.ToLower() == "s5_summonersmiteplayerganker")
                {
                    foreach (
                        var hero in
                            HeroManager.Enemies
                                .Where(h => h.IsValidTarget(500) && !h.IsZombie)
                                .OrderBy(h => h.Distance(Game.CursorPos)))
                    {
                        Player.Spellbook.CastSpell(Smite, hero);
                    }
                }
            }
        }
    }
}
