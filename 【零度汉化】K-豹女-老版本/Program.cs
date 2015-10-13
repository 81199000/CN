using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.Collections.Generic;
using Color = System.Drawing.Color;
using SharpDX;

namespace KurisuNidalee
{
    //  _____ _   _     _         
    // |   | |_|_| |___| |___ ___ 
    // | | | | | . | .'| | -_| -_|
    // |_|___|_|___|__,|_|___|___|
    // Copyright © Kurisu Solutions 2015

    internal class Program
    {
        private static Menu _mainMenu;
        private static Obj_AI_Base _target;
        private static Orbwalking.Orbwalker _orbwalker;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;
        private static bool _cougarForm;

        static void Main(string[] args)
        {
            Console.WriteLine("KurisuNidalee injected..");
            CustomEvents.Game.OnGameLoad += Initialize;
        }

        private static readonly Spell Javelin = new Spell(SpellSlot.Q, 1500f);
        private static readonly Spell Bushwack = new Spell(SpellSlot.W, 900f);
        private static readonly Spell Primalsurge = new Spell(SpellSlot.E, 650f);
        private static readonly Spell Takedown = new Spell(SpellSlot.Q, 200f);
        private static readonly Spell Pounce = new Spell(SpellSlot.W, 375f);
        private static readonly Spell Swipe = new Spell(SpellSlot.E, 275f);
        private static readonly Spell Aspectofcougar = new Spell(SpellSlot.R);

        private static readonly List<Spell> HumanSpellList = new List<Spell>();
        private static readonly List<Spell> CougarSpellList = new List<Spell>();
        private static readonly IEnumerable<int> NidaItems = new[] { 3128, 3144, 3153, 3092 };
        private static bool TSCollision()
        {
            return _mainMenu.Item("coll").GetValue<bool>();
        }

        private static bool TargetHunted(Obj_AI_Base target)
        {
            return target.HasBuff("nidaleepassivehunted", true);
        }

        private static bool NotLearned(Spell spell)
        {
            return Me.Spellbook.CanUseSpell(spell.Slot) == SpellState.NotLearned;
        }

        private static readonly string[] Jungleminions =
        {
            "SRU_Razorbeak", "SRU_Krug", "Sru_Crab",
            "SRU_Baron", "SRU_Dragon", "SRU_Blue", "SRU_Red", "SRU_Murkwolf", "SRU_Gromp"     
        };

        #region Nidalee: Initialize
        private static void Initialize(EventArgs args)
        {
            // Check champion
            if (Me.ChampionName != "Nidalee")
            {
                return;
            }

            // Load main menu
            NidaMenu();

            // Add drawing skill list
            CougarSpellList.AddRange(new[] { Takedown, Pounce, Swipe });
            HumanSpellList.AddRange(new[] { Javelin, Bushwack, Primalsurge });

            // Set skillshot prediction (i has rito decode now)
            Javelin.SetSkillshot(Game.Version.Contains("5.15") 
                ? 0.25f : 0.125f, 
                40f, 1300f, true, SkillshotType.SkillshotLine);

            Bushwack.SetSkillshot(0.50f, 100f, 1500f, false, SkillshotType.SkillshotCircle);
            Swipe.SetSkillshot(0.50f, 375f, 1500f, false, SkillshotType.SkillshotCone);
            Pounce.SetSkillshot(0.50f, 400f, 1500f, false, SkillshotType.SkillshotCone);

            // GameOnGameUpdate Event
            Game.OnUpdate += NidaleeOnUpdate;

            // DrawingOnDraw Event
            Drawing.OnDraw += NidaleeOnDraw;

            // OnProcessSpellCast Event
            Obj_AI_Base.OnProcessSpellCast += NidaleeTracker;

            // AntiGapcloer Event
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!_mainMenu.Item("gapp").GetValue<bool>())
                return;

            var attacker = gapcloser.Sender;
            if (attacker.IsValidTarget(260f))
            {
                if (!_cougarForm)
                {
                    var prediction = Javelin.GetPrediction(attacker);
                    if (prediction.Hitchance != HitChance.Collision && Javelin.IsReady())
                        Javelin.Cast(prediction.CastPosition);

                    if (Aspectofcougar.IsReady())
                        Aspectofcougar.Cast();
                }

                if (_cougarForm)
                {
                    if (attacker.Distance(Me.ServerPosition) <= Takedown.Range && Takedown.IsReady())
                        Takedown.CastOnUnit(Me);
                    if (attacker.Distance(Me.ServerPosition) <= Swipe.Range && Swipe.IsReady())
                        Swipe.Cast(attacker.ServerPosition);
                }
            }
        }

        #endregion

        #region Nidalee: Menu
        private static void NidaMenu()
        {
            _mainMenu = new Menu("KurisuNidalee", "nidalee", true);

            var nidaOrb = new Menu("Nidalee: Orbwalker", "orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(nidaOrb);
            _mainMenu.AddSubMenu(nidaOrb);

            var nidaTS = new Menu("Nidalee: Selector", "target selecter");
            nidaTS.AddItem(new MenuItem("coll", "Check Collision")).SetValue(false);
            TargetSelector.AddToMenu(nidaTS);
            _mainMenu.AddSubMenu(nidaTS);

            var nidaKeys = new Menu("Nidalee: Keys", "keybindongs");
            nidaKeys.AddItem(new MenuItem("usecombo", "Combo")).SetValue(new KeyBind(32, KeyBindType.Press));
            nidaKeys.AddItem(new MenuItem("useharass", "Harass")).SetValue(new KeyBind(67, KeyBindType.Press));
            nidaKeys.AddItem(new MenuItem("usejungle", "Jungleclear")).SetValue(new KeyBind(86, KeyBindType.Press));
            nidaKeys.AddItem(new MenuItem("useclear", "Laneclear")).SetValue(new KeyBind(86, KeyBindType.Press));
            nidaKeys.AddItem(new MenuItem("uselasthit", "Last Hit")).SetValue(new KeyBind(35, KeyBindType.Press));
            nidaKeys.AddItem(new MenuItem("useflee", "Flee Mode/Walljump")).SetValue(new KeyBind(65, KeyBindType.Press));
            _mainMenu.AddSubMenu(nidaKeys);

            var nidaSpells = new Menu("Nidalee: Combo", "spells");
            nidaSpells.AddItem(new MenuItem("seth", "Javelin Hitchance")).SetValue(new Slider(4,1,4));
            nidaSpells.AddItem(new MenuItem("usehumanq", "Use Javelin Toss")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("usehumanw", "Use Bushwack")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("usecougarq", "Use Takedown")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("usecougarw", "Use Pounce")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("usecougare", "Use Swipe")).SetValue(true);
            nidaSpells.AddItem(new MenuItem("usecougarr", "Auto Switch Forms")).SetValue(true);
            _mainMenu.AddSubMenu(nidaSpells);

            var nidaHeals = new Menu("Nidalee: Heal", "hengine");
            nidaHeals.AddItem(new MenuItem("usedemheals", "Enable")).SetValue(true);
            nidaHeals.AddItem(new MenuItem("autorheal", "Auto Switch Forms")).SetValue(true);
            nidaHeals.AddItem(new MenuItem("sezz", "Heal Priority: ")).SetValue(new StringList(new[] { "Low HP", "Highest AD" }));
            nidaHeals.AddItem(new MenuItem("healmanapct", "Minimum Mana %")).SetValue(new Slider(55));

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly))
            {
                nidaHeals.AddItem(new MenuItem("heal" + hero.ChampionName, hero.ChampionName)).SetValue(true);
                nidaHeals.AddItem(new MenuItem("healpct" + hero.ChampionName, "Heal " + hero.ChampionName + " if under %")).SetValue(new Slider(75));
            }

            _mainMenu.AddSubMenu(nidaHeals);

            var nidaHarass = new Menu("Nidalee: Harass", "harass");
            nidaHarass.AddItem(new MenuItem("usehumanq2", "Use Javelin Toss")).SetValue(true);
            nidaHarass.AddItem(new MenuItem("harassform", "Auto Switch Form")).SetValue(true);
            nidaHarass.AddItem(new MenuItem("autoq", "Auto-Q Toggle")).SetValue(new KeyBind('Y', KeyBindType.Toggle));
            nidaHarass.AddItem(new MenuItem("humanqpct", "Minimum Mana %")).SetValue(new Slider(55));
            _mainMenu.AddSubMenu(nidaHarass);

            var nidaJungle = new Menu("Nidalee: Jungle", "jungleclear");
            nidaJungle.AddItem(new MenuItem("jghumanq", "Use Javelin Toss")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jghumanw", "Use Bushwack")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgcougarq", "Use Takedown")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgcougarw", "Use Pounce")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgcougare", "Use Swipe")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgcougarr", "Auto Switch Form")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgheal", "Switch Form to Heal")).SetValue(true);
            nidaJungle.AddItem(new MenuItem("jgpct", "Minimum Mana %")).SetValue(new Slider(35));
            _mainMenu.AddSubMenu(nidaJungle);

            var nidalhit = new Menu("Nidalee: Freeze", "lasthit");
            nidalhit.AddItem(new MenuItem("lhhumanq", "Use Javelin Toss")).SetValue(false);
            nidalhit.AddItem(new MenuItem("lhhumanw", "Use Bushwack")).SetValue(false);
            nidalhit.AddItem(new MenuItem("lhcougarq", "Use Takedown")).SetValue(true);
            nidalhit.AddItem(new MenuItem("lhcougarw", "Use Pounce")).SetValue(true);
            nidalhit.AddItem(new MenuItem("lhcougare", "Use Swipe")).SetValue(true);
            nidalhit.AddItem(new MenuItem("lhcougarr", "Auto Switch Form")).SetValue(false);
            nidalhit.AddItem(new MenuItem("lhpct", "Minimum Mana %")).SetValue(new Slider(45));
            _mainMenu.AddSubMenu(nidalhit);

            var nidalc = new Menu("Nidalee: Laneclear", "laneclear");
            nidalc.AddItem(new MenuItem("lchumanq", "Use Javelin Toss")).SetValue(false);
            nidalc.AddItem(new MenuItem("lchumanw", "Use Bushwack")).SetValue(false);
            nidalc.AddItem(new MenuItem("lccougarq", "Use Takedown")).SetValue(true);
            nidalc.AddItem(new MenuItem("lccougarw", "Use Pounce")).SetValue(true);
            nidalc.AddItem(new MenuItem("lccougare", "Use Swipe")).SetValue(true);
            nidalc.AddItem(new MenuItem("lccougarr", "Auto Switch Form")).SetValue(false);
            nidalc.AddItem(new MenuItem("lcpct", "Minimum Mana %")).SetValue(new Slider(35));
            _mainMenu.AddSubMenu(nidalc);

            var nidaD = new Menu("Nidalee: Drawings", "drawings");
            nidaD.AddItem(new MenuItem("drawline", "Draw TS Target")).SetValue(true);
            nidaD.AddItem(new MenuItem("drawQ", "Draw Q Range")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            nidaD.AddItem(new MenuItem("drawW", "Draw W Range")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            nidaD.AddItem(new MenuItem("drawE", "Draw E Range")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            nidaD.AddItem(new MenuItem("drawcds", "Draw Cooldowns")).SetValue(true);
            _mainMenu.AddSubMenu(nidaD);

            var nidaM = new Menu("Nidalee: Misc", "misc");
            nidaM.AddItem(new MenuItem("useitems", "Use Items")).SetValue(true);
            nidaM.AddItem(new MenuItem("dash", "Q on Dashing")).SetValue(false);
            nidaM.AddItem(new MenuItem("gapp", "Q Anti-Gapcloser")).SetValue(true);
            nidaM.AddItem(new MenuItem("qimmobile", "Q on Immobile")).SetValue(true);
            nidaM.AddItem(new MenuItem("formimm", "Switch Form Immobile")).SetValue(true);
            _mainMenu.AddSubMenu(nidaM);

            _mainMenu.AddToMainMenu();

            Game.PrintChat("<font color=\"#FF9900\"><b>KurisuNidalee:</b></font> Loaded");

        }

        #endregion

        #region Nidalee: OnUpdate
        private static void NidaleeOnUpdate(EventArgs args)
        {
            _cougarForm = Me.Spellbook.GetSpell(SpellSlot.Q).Name != "JavelinToss";

            _target = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Magical);

            AutoSpells();
            PrimalSurge();
            ProcessCooldowns();

            if (Me.HasBuff("Takedown"))
            {
                if (_target.IsValidTarget(Takedown.Range))
                {
                    if (_mainMenu.Item("usecombo").GetValue<KeyBind>().Active && _cougarForm)
                    {
                        Me.IssueOrder(GameObjectOrder.AttackUnit, _target);
                    }
                }
            }

            if (_mainMenu.Item("usecombo").GetValue<KeyBind>().Active)
                UseCombo(_target);

            if (_mainMenu.Item("useharass").GetValue<KeyBind>().Active ||
                _mainMenu.Item("autoq").GetValue<KeyBind>().Active)
            {
                UseHarass();
            }

            if (_mainMenu.Item("useclear").GetValue<KeyBind>().Active)
                UseLaneFarm();
            if (_mainMenu.Item("usejungle").GetValue<KeyBind>().Active)
                UseJungleFarm();
            if (_mainMenu.Item("uselasthit").GetValue<KeyBind>().Active)
                UseLastHit();
            if (_mainMenu.Item("useflee").GetValue<KeyBind>().Active)
                UseFlee();    
        }

        #endregion

        #region Nidalee: Auto Spells
        private static void AutoSpells()
        {      
            if (_mainMenu.Item("qimmobile").GetValue<bool>() && HQ == 0)
            {
                foreach (var unit in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(Javelin.Range)))
                {
                    if (Javelin.GetPrediction(unit).Hitchance == HitChance.Immobile)
                    {
                        if (!_cougarForm)
                        {
                            Javelin.Cast(unit);
                            if (unit.HasBuffOfType(BuffType.Knockup))
                            {
                                Javelin.Cast(unit);
                            }
                        }

                        else
                        {
                            if (_mainMenu.Item("formimm").GetValue<bool>())
                                if (Aspectofcougar.IsReady())
                                    Aspectofcougar.Cast();
                        }
                    }
                }
            }
        }

        #endregion

        #region Nidalee : Misc

        private static void UseInventoryItems(IEnumerable<int> items, Obj_AI_Base target)
        {
            if (!_mainMenu.Item("useitems").GetValue<bool>())
                return;

            foreach (var i in items.Where(x => Items.CanUseItem(x) && Items.HasItem(x)))
            {
                if (target.IsValidTarget(800))
                {
                    if (i == 3092)
                        Items.UseItem(i, target.ServerPosition);
                    else
                    {
                        Items.UseItem(i);
                        Items.UseItem(i, target);
                    }
                }
            }
        }

        private static bool CanKillAA(Obj_AI_Base target)
        {
            var damage = 0d;

            if (target.IsValidTarget(Me.AttackRange + 30))
                damage = Me.GetAutoAttackDamage(target);

            return target.Health <= (float) damage * 5;
        }

        private static float CougarDamage(Obj_AI_Base target)
        {
            var damage = 0d;

            if (CQ == 0)
                damage += Me.GetSpellDamage(target, SpellSlot.Q, 1);
            if ((CW == 0 || Pounce.IsReady()))
                damage += Me.GetSpellDamage(target, SpellSlot.W, 1);
            if (CE == 0)
                damage += Me.GetSpellDamage(target, SpellSlot.E, 1);

            return (float) damage;
        }

        #endregion

        #region Nidalee : Flee
        // Walljumper credits to Hellsing
        private static void UseFlee()
        {
            if (!_cougarForm && Aspectofcougar.IsReady() && Pounce.IsReady()) 
                Aspectofcougar.Cast();

            // We need to define a new move position since jumping over walls
            // requires you to be close to the specified wall. Therefore we set the move
            // point to be that specific piont. People will need to get used to it,
            // but this is how it works.
            var wallCheck = GetFirstWallPoint(Me.Position, Game.CursorPos);

            // Be more precise
            if (wallCheck != null)
                wallCheck = GetFirstWallPoint((Vector3)wallCheck, Game.CursorPos, 5);

            // Define more position point
            var movePosition = wallCheck != null ? (Vector3)wallCheck : Game.CursorPos;

            // Update fleeTargetPosition
            var tempGrid = NavMesh.WorldToGrid(movePosition.X, movePosition.Y);
            var fleeTargetPosition = NavMesh.GridToWorld((short)tempGrid.X, (short)tempGrid.Y);

            // Also check if we want to AA aswell
            Obj_AI_Base target = null;

            // Reset walljump indicators
            var wallJumpPossible = false;

            // Only calculate stuff when our Q is up and there is a wall inbetween
            if (_cougarForm && Pounce.IsReady() && wallCheck != null)
            {
                // Get our wall position to calculate from
                var wallPosition = movePosition;

                // Check 300 units to the cursor position in a 160 degree cone for a valid non-wall spot
                Vector2 direction = (Game.CursorPos.To2D() - wallPosition.To2D()).Normalized();
                float maxAngle = 80;
                float step = maxAngle/20;
                float currentAngle = 0;
                float currentStep = 0;
                bool jumpTriggered = false;
                while (true)
                {
                    // Validate the counter, break if no valid spot was found in previous loops
                    if (currentStep > maxAngle && currentAngle < 0)
                        break;

                    // Check next angle
                    if ((currentAngle == 0 || currentAngle < 0) && currentStep != 0)
                    {
                        currentAngle = (currentStep)*(float) Math.PI/180;
                        currentStep += step;
                    }

                    else if (currentAngle > 0)
                        currentAngle = -currentAngle;

                    Vector3 checkPoint;

                    // One time only check for direct line of sight without rotating
                    if (currentStep == 0)
                    {
                        currentStep = step;
                        checkPoint = wallPosition + Pounce.Range*direction.To3D();
                    }
                    // Rotated check
                    else
                        checkPoint = wallPosition + Pounce.Range*direction.Rotated(currentAngle).To3D();

                    // Check if the point is not a wall
                    if (!checkPoint.IsWall())
                    {
                        // Check if there is a wall between the checkPoint and wallPosition
                        wallCheck = GetFirstWallPoint(checkPoint, wallPosition);
                        if (wallCheck != null)
                        {
                            // There is a wall inbetween, get the closes point to the wall, as precise as possible
                            Vector3 wallPositionOpposite =
                                (Vector3) GetFirstWallPoint((Vector3) wallCheck, wallPosition, 5);

                            // Check if it's worth to jump considering the path length
                            if (Me.GetPath(wallPositionOpposite).ToList().To2D().PathLength() -
                                Me.Distance(wallPositionOpposite) > 200)
                            {
                                // Check the distance to the opposite side of the wall
                                if (Me.Distance(wallPositionOpposite, true) <
                                    Math.Pow(Pounce.Range - Me.BoundingRadius/2, 2))
                                {
                                    // Make the jump happen
                                    Pounce.Cast(wallPositionOpposite);

                                    // Update jumpTriggered value to not orbwalk now since we want to jump
                                    jumpTriggered = true;

                                    break;
                                }
                                // If we are not able to jump due to the distance, draw the spot to
                                // make the user notice the possibliy
                                else
                                {
                                    // Update indicator values
                                    wallJumpPossible = true;
                                }
                            }

                            else
                            {
                                // yolo
                                Render.Circle.DrawCircle(Game.CursorPos, 35, Color.Red, 2);
                            }
                        }
                    }
                }

                // Check if the loop triggered the jump, if not just orbwalk
                if (!jumpTriggered)
                    Orbwalking.Orbwalk(target, Game.CursorPos, 90f, 0f, false, false);
            }

            // Either no wall or W on cooldown, just move towards to wall then
            else
            {
                Orbwalking.Orbwalk(target, Game.CursorPos, 90f, 0f, false, false);
                if (_cougarForm && Pounce.IsReady())
                    Pounce.Cast(Game.CursorPos);
            }
        }

        #endregion

        #region Nidalee: SBTW
        private static void UseCombo(Obj_AI_Base target)
        {
            // Cougar combo
            if (_cougarForm && target.IsValidTarget(Javelin.Range))
            {
                UseInventoryItems(NidaItems, target);

                // Check if takedown is ready (on unit)
                if (Takedown.IsReady() && _mainMenu.Item("usecougarq").GetValue<bool>() && 
                    target.Distance(Me.ServerPosition, true) <= Takedown.RangeSqr + 150*150)
                {
                    Takedown.CastOnUnit(Me); 
                    if (Me.HasBuff("Takedown"))
                        Me.IssueOrder(GameObjectOrder.AttackUnit, target);
                }

                // Check is pounce is ready 
                if (Pounce.IsReady() && _mainMenu.Item("usecougarw").GetValue<bool>() && 
                   (target.Distance(Me.ServerPosition, true) > 275*275 || CougarDamage(target) >= target.Health))
                {
                    if (TargetHunted(target) & target.Distance(Me.ServerPosition, true) <= 735*735)
                    {
                        if (Takedown.IsReady())
                            Takedown.CastOnUnit(Me);

                        Pounce.Cast(target.ServerPosition);
                    }

                    else if (target.Distance(Me.ServerPosition, true) <= 400*400)
                    {
                        if (Takedown.IsReady())
                            Takedown.CastOnUnit(Me);

                        Pounce.Cast(target.ServerPosition);
                    }
                }

                // Check if swipe is ready (no prediction)
                if (Swipe.IsReady() && _mainMenu.Item("usecougare").GetValue<bool>())
                {
                    if (target.Distance(Me.ServerPosition, true) <= Swipe.RangeSqr)
                    {
                        if (!Pounce.IsReady() || NotLearned(Pounce))
                            Swipe.Cast(target.ServerPosition);
                    }
                }

                // force transform if q ready and no collision 
                if (HQ == 0 && _mainMenu.Item("usecougarr").GetValue<bool>())
                {
                    if (!Aspectofcougar.IsReady())
                    {
                        return;
                    }

                    // or return -- stay cougar if we can kill with available spells
                    if (target.Health <= CougarDamage(target) &&
                        target.Distance(Me.ServerPosition, true) <= Pounce.RangeSqr)
                    {
                        return;
                    }

                    var prediction = Javelin.GetPrediction(target);
                    if (prediction.Hitchance >= HitChance.Medium)
                        Aspectofcougar.Cast();
                }

                // Switch to human form if can kill in aa and cougar skill not available      
                if (!Pounce.IsReady() && !Swipe.IsReady() && !Takedown.IsReady())
                {
                    if (target.Distance(Me.ServerPosition, true) > Takedown.RangeSqr && CanKillAA(target))
                    {
                        if (_mainMenu.Item("usecougarr").GetValue<bool>())
                        {
                            if (target.Distance(Me.ServerPosition, true) <= Me.AttackRange*Me.AttackRange + 50*50)
                            {
                                if (Aspectofcougar.IsReady())
                                    Aspectofcougar.Cast();
                            }
                        }
                    }
                }
            }

            // human Q 
            if (!_cougarForm && target.IsValidTarget(Javelin.Range))
            {
                var qtarget = TSCollision()
                    ? TargetSelector.GetTargetNoCollision(Javelin)
                    : TargetSelector.GetTarget(Javelin.Range, TargetSelector.DamageType.Magical);

                if (qtarget.IsValidTarget() && Javelin.IsReady() && _mainMenu.Item("usehumanq").GetValue<bool>())
                {
                    Javelin.CastIfHitchanceEquals(qtarget,
                        (HitChance) _mainMenu.Item("seth").GetValue<Slider>().Value + 2);
                }
            }

            // Human combo
            if (!_cougarForm && target.IsValidTarget(Javelin.Range))
            {
                // Switch to cougar if target hunted or can kill target 
                if (Aspectofcougar.IsReady() && _mainMenu.Item("usecougarr").GetValue<bool>() && 
                   (TargetHunted(target) || target.Health <= CougarDamage(target) && (HQ != 0 || !Javelin.IsReady())))
                {
                    // check if pounce is ready and takedown or swipe before changing form
                    if (CW == 0 && (CQ == 0 || CE == 0))
                    {
                        if (TargetHunted(target) && target.Distance(Me.ServerPosition, true) <= 735*735)
                            Aspectofcougar.Cast();
                        if (target.Health <= CougarDamage(target) && target.Distance(Me.ServerPosition, true) <= 400*400)
                            Aspectofcougar.Cast();
                    }
                }

                // Check bushwack and cast underneath targets feet.
                if (Bushwack.IsReady() && _mainMenu.Item("usehumanw").GetValue<bool>())
                {
                    if (target.Distance(Me.ServerPosition, true) <= Bushwack.RangeSqr)
                    {
                        Bushwack.CastIfHitchanceEquals(target, HitChance.Medium);
                    }
                }
            }
        }

        #endregion

        #region Nidalee: Harass
        private static void UseHarass()
        {
            var qtarget = TSCollision()
                ? TargetSelector.GetTargetNoCollision(Javelin)
                : TargetSelector.GetTarget(Javelin.Range, TargetSelector.DamageType.Magical);

            if (qtarget.IsValidTarget())
            {
                var actualHeroManaPercent = (int) ((Me.Mana/Me.MaxMana)*100);
                var minPercent = _mainMenu.Item("humanqpct").GetValue<Slider>().Value;

                if (HQ == 0 && _mainMenu.Item("usehumanq2").GetValue<bool>())
                {
                    if (!_cougarForm)
                    {
                        if (qtarget.Distance(Me.ServerPosition, true) <= Javelin.RangeSqr &&
                            actualHeroManaPercent > minPercent)
                        {
                            Javelin.CastIfHitchanceEquals(qtarget,
                                (HitChance) _mainMenu.Item("seth").GetValue<Slider>().Value + 2);
                        }
                    }

                    else
                    {
                        if (_mainMenu.Item("harassform").GetValue<bool>() && Aspectofcougar.IsReady())
                        {
                            if (Javelin.GetPrediction(qtarget).Hitchance >=
                                (HitChance) _mainMenu.Item("seth").GetValue<Slider>().Value + 2)
                            {
                                Aspectofcougar.Cast();
                            }
                        }
                    }
                }
                
            }
        }

        #endregion

        #region Nidalee: Heal
        private static void PrimalSurge()
        {
            if (HE != 0 || !_cougarForm && !Primalsurge.IsReady())
                return;

            if (!_mainMenu.Item("usedemheals").GetValue<bool>())
                return;

            if (Me.IsRecalling() || Me.InFountain() || Me.Spellbook.IsChanneling)
                return;

            var actualHeroManaPercent = (int) ((Me.Mana/Me.MaxMana)*100);
            var selfManaPercent = _mainMenu.Item("healmanapct").GetValue<Slider>().Value;

            Obj_AI_Hero target;
            if (_mainMenu.Item("sezz").GetValue<StringList>().SelectedIndex == 0)
            {
                target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(hero => hero.IsValidTarget(Primalsurge.Range + 100, false) && hero.IsAlly)
                        .OrderBy(xe => xe.Health/xe.MaxHealth*100).First();
            }
            else
            {
                target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(hero => hero.IsValidTarget(Primalsurge.Range + 100, false) && hero.IsAlly)
                        .OrderByDescending(xe => xe.FlatPhysicalDamageMod).First();
            }

            if (_mainMenu.Item("heal" + target.ChampionName).GetValue<bool>())
            {
                var needed = _mainMenu.Item("healpct" + target.ChampionName).GetValue<Slider>().Value;
                var hp = (int)((target.Health / target.MaxHealth) * 100);

                if (actualHeroManaPercent > selfManaPercent && hp <= needed)
                {
                    if (!_cougarForm)
                    {
                        Primalsurge.CastOnUnit(target);
                    }

                    else
                    {
                        if (_mainMenu.Item("autorheal").GetValue<bool>() &&
                           !_mainMenu.Item("usecombo").GetValue<KeyBind>().Active &&
                           !_mainMenu.Item("useflee").GetValue<KeyBind>().Active)
                        {
                            if (Aspectofcougar.IsReady() && HE == 0)
                                Aspectofcougar.Cast();
                        }
                    }
                }
            }        
       }



        #endregion

        #region Nidalee: Farming
        private static void UseLaneFarm()
        {
            var actualHeroManaPercent = (int)((Me.Mana / Me.MaxMana) * 100);
            var minPercent = _mainMenu.Item("lcpct").GetValue<Slider>().Value;

            var minions = MinionManager.GetMinions(Me.Position, 600f,
                MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);

            var minionPositions = minions.Select(x => x.ServerPosition.To2D()).ToList();

            var spos = Swipe.GetCircularFarmLocation(minionPositions);

            foreach (var m in minions)
            {
                if (_cougarForm)
                {
                    if (m.Distance(Me.ServerPosition, true) <= Swipe.RangeSqr)
                    { 
                        if (Swipe.IsReady() && (!Pounce.IsReady() || NotLearned(Pounce)))
                        {
                            if (_mainMenu.Item("lccougare").GetValue<bool>())
                            {
                                if (spos.MinionsHit >= 3)
                                {
                                    Swipe.Cast(spos.Position);             
                                }
                            }
                        }
                    }

                    if (m.Distance(Me.ServerPosition, true) <= Pounce.RangeSqr)
                    {
                        if (Pounce.IsReady())
                        {
                            if (!Me.ServerPosition.Extend(m.ServerPosition, Pounce.Range).UnderTurret(true))
                            {
                                if (_mainMenu.Item("lccougarw").GetValue<bool>())
                                    Pounce.Cast(m.ServerPosition);
                            }
                        }
                    }

                    if (m.Distance(Me.ServerPosition) <= Takedown.RangeSqr)
                    {
                        if (Takedown.IsReady())
                        {
                            if (_mainMenu.Item("lccougarq").GetValue<bool>())
                                Takedown.CastOnUnit(Me);
                        }
                    }

                    if (HQ == 0 && _mainMenu.Item("lchumanq").GetValue<bool>())
                    {
                        if (Aspectofcougar.IsReady() &&  _mainMenu.Item("lccougarr").GetValue<bool>())
                        {
                            Aspectofcougar.Cast();
                        }
                    }

                    else if (!Pounce.IsReady() && !Takedown.IsReady() && !Swipe.IsReady())
                    {
                        if (Aspectofcougar.IsReady() &&  _mainMenu.Item("lccougarr").GetValue<bool>())
                        {
                            Aspectofcougar.Cast();
                        }
                    }
                }

                else
                {
                    if (actualHeroManaPercent > minPercent && Javelin.IsReady())
                    {
                        if (_mainMenu.Item("lchumanq").GetValue<bool>())
                            Javelin.Cast(m.ServerPosition);
                    }

                    if (m.Distance(Me.ServerPosition, true) <= Bushwack.RangeSqr && 
                        actualHeroManaPercent > minPercent && Bushwack.IsReady())
                    {
                        if (_mainMenu.Item("lchumanw").GetValue<bool>())
                            Bushwack.Cast(m.ServerPosition);
                    }

                    if (m.Distance(Me.ServerPosition, true) <= Pounce.RangeSqr && Aspectofcougar.IsReady())
                    {
                        if (_mainMenu.Item("lccougarr").GetValue<bool>())
                            Aspectofcougar.Cast();
                    }
                }

            }
        }


        private static void UseJungleFarm()
        {
            var actualHeroManaPercent = (int)((Me.Mana / Me.MaxMana) * 100);
            var minPercent = _mainMenu.Item("jgpct").GetValue<Slider>().Value;

            var minions = MinionManager.GetMinions(Me.Position, 700f,
                MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            foreach (var m in minions)
            {
                if (_cougarForm)
                {
                    if (m.Distance(Me.ServerPosition, true) <= Swipe.RangeSqr)
                    {
                        if (Swipe.IsReady() && _mainMenu.Item("jgcougare").GetValue<bool>())
                        {
                            if (!Pounce.IsReady() || NotLearned(Pounce))
                            {
                                Swipe.Cast(m.ServerPosition);
                            }
                        }
                    }

                    if (TargetHunted(m) & m.Distance(Me.ServerPosition, true) <= 735*735)
                    {
                        if (Pounce.IsReady())
                        {
                            if (_mainMenu.Item("jgcougarw").GetValue<bool>())
                                Pounce.Cast(m.ServerPosition);
                        }
                    }

                    else if (m.Distance(Me.ServerPosition, true) <= 400*400)
                    {
                        if (Pounce.IsReady())
                        {
                            if (_mainMenu.Item("jgcougarw").GetValue<bool>())
                                Pounce.Cast(m.ServerPosition);
                        }
                    }

                    if (m.Distance(Me.ServerPosition, true) <= Takedown.RangeSqr)
                    {
                        if (Takedown.IsReady())
                        {
                            if (_mainMenu.Item("jgcougarq").GetValue<bool>())
                                Takedown.CastOnUnit(Me);
                        }
                    }

                    if ((!Pounce.IsReady() || NotLearned(Pounce)) && 
                       (!Takedown.IsReady() || NotLearned(Takedown)) && 
                       (!Swipe.IsReady() || NotLearned(Swipe)))
                    {
                        if ((HQ == 0 ||
                             HE == 0 &&
                             Me.Health/Me.MaxHealth*100 <=
                             _mainMenu.Item("healpct" + Me.ChampionName).GetValue<Slider>().Value &&
                             _mainMenu.Item("jgheal").GetValue<bool>()))
                        {
                            if (Aspectofcougar.IsReady() && _mainMenu.Item("jgcougarr").GetValue<bool>())
                            {
                                if (actualHeroManaPercent > minPercent)
                                    Aspectofcougar.Cast();
                            }
                        }
                    }
                }

                else
                {
                    if (actualHeroManaPercent > minPercent && Javelin.IsReady())
                    {
                        if (_mainMenu.Item("jghumanq").GetValue<bool>() &&
                            Jungleminions.Any(x => m.Name.StartsWith(x) && !m.Name.Contains("Mini")))
                        {
                            var prediction = Javelin.GetPrediction(m);
                            if (prediction.Hitchance >= HitChance.Low)
                                Javelin.Cast(m.ServerPosition);
                        }
                    }

                    if (m.Distance(Me.ServerPosition, true) <= Bushwack.RangeSqr)
                    {
                        if (actualHeroManaPercent > minPercent && Bushwack.IsReady())
                        {
                            if (_mainMenu.Item("jghumanw").GetValue<bool>())
                                Bushwack.Cast(m.ServerPosition);
                        }
                    }

                    if (_mainMenu.Item("jgcougarr").GetValue<bool>() && Aspectofcougar.IsReady())
                    {
                        var poutput = Javelin.GetPrediction(m);
                        if (!Javelin.IsReady() || poutput.Hitchance == HitChance.Collision ||
                            actualHeroManaPercent >= minPercent)
                        {                    
                            // check if pounce is ready and takedown or swipe before changing form
                            if (CW == 0 && (CE == 0 || CQ == 0))
                            {
                                if (TargetHunted(m) & m.Distance(Me.ServerPosition, true) <= 735*735)
                                    Aspectofcougar.Cast();
                                else if (m.Distance(Me.ServerPosition, true) <= 450*450)
                                    Aspectofcougar.Cast();
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Nidalee: LastHit
        private static void UseLastHit()
        {
            var actualHeroManaPercent = (int)((Me.Mana / Me.MaxMana) * 100);
            var minPercent = _mainMenu.Item("lhpct").GetValue<Slider>().Value;

            foreach (
                var m in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(m => m.IsValidTarget(Javelin.Range) && Jungleminions.Any(name => !m.Name.StartsWith(name))))
            {
                var cqdmg = Me.GetSpellDamage(m, SpellSlot.Q, 1);
                var cwdmg = Me.GetSpellDamage(m, SpellSlot.W, 1);
                var cedmg = Me.GetSpellDamage(m, SpellSlot.E, 1);
                var hqdmg = Me.GetSpellDamage(m, SpellSlot.Q);

                if (_cougarForm)
                {
                    if (m.Distance(Me.ServerPosition, true) < Swipe.RangeSqr && Swipe.IsReady())
                    {
                        if (m.Health <= cedmg && _mainMenu.Item("lhcougare").GetValue<bool>())
                            Swipe.Cast(m.ServerPosition);
                    }


                    if (m.Distance(Me.ServerPosition, true) < Pounce.RangeSqr && Pounce.IsReady())
                    {
                        if (m.Health <= cwdmg && _mainMenu.Item("lhcougarw").GetValue<bool>())
                            Pounce.Cast(m.ServerPosition);
                    }

                    if (m.Distance(Me.ServerPosition, true) < Takedown.RangeSqr && Takedown.IsReady())
                    {
                        if (m.Health <= cqdmg && _mainMenu.Item("lhcougarq").GetValue<bool>())
                            Takedown.CastOnUnit(Me);
                    }
                }
                else
                {
                    if (actualHeroManaPercent > minPercent && HQ == 0)
                    {
                        if (m.Health <= hqdmg && _mainMenu.Item("lhhumanq").GetValue<bool>())
                            Javelin.Cast(m.ServerPosition);
                    }

                    if (m.Distance(Me.ServerPosition, true) <= Bushwack.RangeSqr && actualHeroManaPercent > minPercent && HW == 0)
                    {
                        if (_mainMenu.Item("lhhumanw").GetValue<bool>())
                            Bushwack.Cast(m.ServerPosition);
                    }

                    if (_mainMenu.Item("lhcougarr").GetValue<bool>() && m.Distance(Me.ServerPosition, true) <= Pounce.RangeSqr &&
                        actualHeroManaPercent > minPercent && Aspectofcougar.IsReady())
                    {
                        Aspectofcougar.Cast();
                    }
                }
            }
        }

        #endregion

        #region Nidalee: Tracker
        private static void NidaleeTracker(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                GetCooldowns(args);

                // check if it was a "unit" pounce (hunted pounce)
                if (args.SData.Name == "Pounce" && args.Target != null)
                {
                    // reset the cooldown timer for hunted targets
                    if (TargetHunted(args.Target as Obj_AI_Base))
                        CWRem = Game.Time + 1.5f;
                }
            }
        }

        private static readonly float[] HumanQcd = { 6, 6, 6, 6, 6 };
        private static readonly float[] HumanWcd = { 13, 12, 11, 10, 9 };
        private static readonly float[] HumanEcd = { 12, 12, 12, 12, 12 };

        private static float CQRem, CWRem, CERem;
        private static float HQRem, HWRem, HERem;
        private static float CQ, CW, CE;
        private static float HQ, HW, HE;

        private static void ProcessCooldowns()
        {
            if (Me.IsDead)
                return;

            CQ = ((CQRem - Game.Time) > 0) ? (CQRem - Game.Time) : 0;
            CW = ((CWRem - Game.Time) > 0) ? (CWRem - Game.Time) : 0;
            CE = ((CERem - Game.Time) > 0) ? (CERem - Game.Time) : 0;
            HQ = ((HQRem - Game.Time) > 0) ? (HQRem - Game.Time) : 0;
            HW = ((HWRem - Game.Time) > 0) ? (HWRem - Game.Time) : 0;
            HE = ((HERem - Game.Time) > 0) ? (HERem - Game.Time) : 0;
        }

        private static float CalculateCd(float time)
        {
            return time + (time * Me.PercentCooldownMod);
        }

        private static void GetCooldowns(GameObjectProcessSpellCastEventArgs spell)
        {
            if (_cougarForm)
            {
                if (spell.SData.Name == "Takedown")
                    CQRem = Game.Time + CalculateCd(5);
                if (spell.SData.Name == "Pounce")
                    CWRem = Game.Time + CalculateCd(5);
                if (spell.SData.Name == "Swipe")
                    CERem = Game.Time + CalculateCd(5);
            }
            else
            {
                if (spell.SData.Name == "JavelinToss")
                    HQRem = Game.Time + CalculateCd(HumanQcd[Javelin.Level - 1]);
                if (spell.SData.Name == "Bushwhack")
                    HWRem = Game.Time + CalculateCd(HumanWcd[Bushwack.Level - 1]);
                if (spell.SData.Name == "PrimalSurge")
                    HERem = Game.Time + CalculateCd(HumanEcd[Primalsurge.Level - 1]);
            }
        }

        #endregion

        #region Nidalee: On Draw
        private static void NidaleeOnDraw(EventArgs args)
        {
            if (_target != null && _mainMenu.Item("drawline").GetValue<bool>())
            {
                if (Me.IsDead)
                {
                    return;
                }

                Render.Circle.DrawCircle(_target.Position, _target.BoundingRadius - 50, Color.Yellow);
            }

            foreach (var spell in CougarSpellList)
            {
                var circle = _mainMenu.Item("draw" + spell.Slot).GetValue<Circle>();
                if (circle.Active && _cougarForm && !Me.IsDead)
                    Render.Circle.DrawCircle(Me.Position, spell.Range, circle.Color, 2);
            }

            foreach (var spell in HumanSpellList)
            {
                var circle = _mainMenu.Item("draw" + spell.Slot).GetValue<Circle>();
                if (circle.Active && !_cougarForm && !Me.IsDead)
                    Render.Circle.DrawCircle(Me.Position, spell.Range, circle.Color, 2);
            }

            if (!_mainMenu.Item("drawcds").GetValue<bool>()) 
                return;

            var wts = Drawing.WorldToScreen(Me.Position);

            if (!_cougarForm) // lets show cooldown timers for the opposite form :)
            {
                if (NotLearned(Javelin))
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.White, "Q: Null");
                else if (CQ == 0)
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.White, "Q: Ready");
                else
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.Orange, "Q: " + CQ.ToString("0.0"));
                if (NotLearned(Bushwack))
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.White, "W: Null");
                else if ((CW == 0 || !_cougarForm && Pounce.IsReady()))
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.White, "W: Ready");
                else
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.Orange, "W: " + CW.ToString("0.0"));
                if (NotLearned(Primalsurge))
                    Drawing.DrawText(wts[0], wts[1], Color.White, "E: Null");
                else if (CE == 0)
                    Drawing.DrawText(wts[0], wts[1], Color.White, "E: Ready");
                else
                    Drawing.DrawText(wts[0], wts[1], Color.Orange, "E: " + CE.ToString("0.0"));

            }
            else
            {
                if (NotLearned(Takedown))
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.White, "Q: Null");
                else if (HQ == 0)
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.White, "Q: Ready");
                else
                    Drawing.DrawText(wts[0] - 80, wts[1], Color.Orange, "Q: " + HQ.ToString("0.0"));
                if (NotLearned(Pounce))
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.White, "W: Null");
                else if (HW == 0)
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.White, "W: Ready");
                else
                    Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.Orange, "W: " + HW.ToString("0.0"));
                if (NotLearned(Swipe))
                    Drawing.DrawText(wts[0], wts[1], Color.White, "E: Null");
                else if (HE == 0)
                    Drawing.DrawText(wts[0], wts[1], Color.White, "E: Ready");
                else
                    Drawing.DrawText(wts[0], wts[1], Color.Orange, "E: " + HE.ToString("0.0"));

            }
        }

        #endregion

        #region Nidalee: Vector Helper
        // VectorHelper.cs by Hellsing
        public static bool IsLyingInCone(Vector2 position, Vector2 apexPoint, Vector2 circleCenter, double aperture)
        {
            // This is for our convenience
            double halfAperture = aperture / 2;

            // Vector pointing to X point from apex
            Vector2 apexToXVect = apexPoint - position;

            // Vector pointing from apex to circle-center point.
            Vector2 axisVect = apexPoint - circleCenter;

            // X is lying in cone only if it's lying in 
            // infinite version of its cone -- that is, 
            // not limited by "round basement".
            // We'll use dotProd() to 
            // determine angle between apexToXVect and axis.
            bool isInInfiniteCone = DotProd(apexToXVect, axisVect) / Magn(apexToXVect) / Magn(axisVect) >
                // We can safely compare cos() of angles 
                // between vectors instead of bare angles.
            Math.Cos(halfAperture);

            if (!isInInfiniteCone)
                return false;

            // X is contained in cone only if projection of apexToXVect to axis
            // is shorter than axis. 
            // We'll use dotProd() to figure projection length.
            bool isUnderRoundCap = DotProd(apexToXVect, axisVect) / Magn(axisVect) < Magn(axisVect);

            return isUnderRoundCap;
        }

        private static float DotProd(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        private static float Magn(Vector2 a)
        {
            return (float)(Math.Sqrt(a.X * a.X + a.Y * a.Y));
        }

        public static Vector2? GetFirstWallPoint(Vector3 from, Vector3 to, float step = 25)
        {
            return GetFirstWallPoint(from.To2D(), to.To2D(), step);
        }

        public static Vector2? GetFirstWallPoint(Vector2 from, Vector2 to, float step = 25)
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

        public static List<Obj_AI_Base> GetDashObjects(IEnumerable<Obj_AI_Base> predefinedObjectList = null)
        {
            var objects = predefinedObjectList != null
                ? predefinedObjectList.ToList()
                : ObjectManager.Get<Obj_AI_Base>().Where(o => o.IsValidTarget(Orbwalking.GetRealAutoAttackRange(o)));

            var apexPoint = Me.ServerPosition.To2D() +
                            (Me.ServerPosition.To2D() - Game.CursorPos.To2D()).Normalized()*
                            Orbwalking.GetRealAutoAttackRange(Me);

            return
                objects.Where(
                    o => IsLyingInCone(o.ServerPosition.To2D(), apexPoint, Me.ServerPosition.To2D(), Math.PI))
                    .OrderBy(o => o.Distance(apexPoint, true))
                    .ToList();
        }

        #endregion

    }
}
