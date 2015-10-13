using System;
using LeagueSharp;
using LeagueSharp.Common;
using SPrediction;
using ES = KurisuNidalee.Essentials;
using KN = KurisuNidalee.KurisuNidalee;

namespace KurisuNidalee
{
    internal class Combat
    {
        // Human Q Logic
        internal static void CastJavelin(Obj_AI_Base target, string mode)
        {
            // if not harass mode ignore mana check
            if (!ES.CatForm() && (mode != "ha" || ES.Player.ManaPercent > 65))
            {
                if (ES.SpellTimer["Javelin"].IsReady() && KN.Root.Item("ndhq" + mode).GetValue<bool>())
                {
                    if (target.IsValidTarget(ES.Spells["Javelin"].Range))
                    {
                        // try prediction on champion
                        if (target.IsChampion() && KN.Root.Item("ndhqcheck").GetValue<bool>())
                            ES.Spells["Javelin"].SPredictionCast((Obj_AI_Hero) target, ES.MyHitChance("hq"));

                        if (!target.IsChampion())
                            ES.Spells["Javelin"].Cast(target);
                    }
                }
            }
        }

        // Human W Logic
        internal static void CastBushwack(Obj_AI_Base target, string mode)
        {           
            // if not harass mode ignore mana check
            if (!ES.CatForm() && (mode != "ha" || ES.Player.ManaPercent > 65))
            {
                if (ES.SpellTimer["Bushwhack"].IsReady() && KN.Root.Item("ndhw" + mode).GetValue<bool>())
                {
                    if (target.IsValidTarget(ES.Spells["Bushwhack"].Range))
                    {
                        // try bushwack prediction
                        if (KN.Root.Item("ndhwforce").GetValue<StringList>().SelectedIndex == 0)
                        {
                            if (target.IsChampion())
                                ES.Spells["Bushwhack"].SPredictionCast((Obj_AI_Hero) target, HitChance.VeryHigh);
                            else
                                ES.Spells["Bushwhack"].Cast(target.ServerPosition);
                        }

                        // try bushwack behind target
                        if (KN.Root.Item("ndhwforce").GetValue<StringList>().SelectedIndex == 1)
                        {
                            // todo: add adjust-able range & maybe add prediction
                            ES.Spells["Bushwhack"].Cast(target.ServerPosition.Extend(ES.Player.ServerPosition, -75f));
                        }
                    }
                }
            }
        }


        // Cougar Q Logic
        internal static void CastTakedown(Obj_AI_Base target, string mode)
        {
            if (ES.CatForm())
            {
                if (ES.SpellTimer["Takedown"].IsReady() && KN.Root.Item("ndcq" + mode).GetValue<bool>())
                {
                    // temp logic to prevent takdown cast before swipe
                    if (!ES.SpellTimer["Swipe"].IsReady() || ES.NotLearned(ES.Spells["Swipe"]) || !KN.Root.Item("ndce" + mode).GetValue<bool>())
                    {
                        if (target.IsValidTarget(ES.Player.AttackRange + ES.Spells["Takedown"].Range))
                        {
                            ES.Spells["Takedown"].CastOnUnit(ES.Player);

                            // force attack order on target (smoother)
                            if (ES.Player.HasBuff("Takedown", true))
                                Orbwalking.LastAATick = 0;
                        }
                    }
                }
            }
        }


        // Cougar W Logic
        internal static void CastPounce(Obj_AI_Base target, string mode)
        {
            if (!ES.CatForm())
                return;

            if (ES.SpellTimer["Pounce"].IsReady() && KN.Root.Item("ndcw" + mode).GetValue<bool>())
            {
                if (!target.IsValidTarget(ES.Spells["ExPounce"].Range))
                    return;

                // if target is hunted in 750 range
                if (target.IsHunted())
                {
                    var radius = ES.Player.AttackRange + ES.Player.Distance(ES.Player.BBox.Minimum) + 1;

                    // force pounce if menu item enabled
                    if (target.IsHunted() && KN.Root.Item("ndcwhunt").GetValue<bool>() ||

                        // or of target is greater than my attack range
                        target.Distance(ES.Player.ServerPosition) > radius ||

                        // or is jungling or waveclearing (with farm distance check)
                        mode == "jg" || mode == "wc" && !KN.Root.Item("ndcwdistwc").GetValue<bool>() ||

                        // or combo mode and ignoring distance check
                        mode == "co" && !KN.Root.Item("ndcwdistco").GetValue<bool>())
                    {

                        // allow kiting between pounce if desired
                        if (mode == "jg" && target.Distance(ES.Player.ServerPosition) < 250)
                        {
                            ES.Spells["Pounce"].Cast(!KN.Root.Item("jgsticky").GetValue<bool>()
                                ? Game.CursorPos : target.ServerPosition);
                        }

                        else
                            ES.Spells["Pounce"].Cast(target.ServerPosition);
                    }
                }

                // if target is not hunted
                else
                {
                    if (target.Distance(ES.Player.ServerPosition) > ES.Spells["Pounce"].Range)
                        return;

                    var radius = ES.Player.AttackRange + ES.Player.Distance(ES.Player.BBox.Minimum) + 1;

                    // check minimum distance before pouncing
                    if (target.Distance(ES.Player.ServerPosition) > radius || 

                        // or is jungling or waveclearing (with no distance checking)
                        mode == "jg" ||  mode == "wc" && !KN.Root.Item("ndcwdistwc").GetValue<bool>() ||

                        // or combo mode with no distance checking
                        mode == "co" && !KN.Root.Item("ndcwdistco").GetValue<bool>())
                    {
                        if (target.IsChampion())
                        {
                            if (KN.Root.Item("ndcwcheck").GetValue<bool>())
                            {
                                //ES.Spells["Javelin"].SPredictionCast((Obj_AI_Hero)target, ES.MyHitChance("cw"));
                                ES.Spells["Pounce"].Cast(target.ServerPosition);
                            }

                            else
                                ES.Spells["Pounce"].Cast(target.ServerPosition);
                        }

                        else 
                        {
                            // check pouncing near enemies
                            if (mode == "wc" && KN.Root.Item("ndcwhunt").GetValue<bool>() &&
                                target.ServerPosition.CountEnemiesInRange(850) > 0)
                                return;

                            // check pouncing under turret
                            if (mode == "wc" && KN.Root.Item("ndcwtow").GetValue<bool>() &&
                                target.ServerPosition.UnderTurret(true))
                                return;

                            // allow kiting between pounce if desired
                            if (mode == "jg" && target.Distance(ES.Player.ServerPosition) < 250)
                                ES.Spells["Pounce"].Cast(!KN.Root.Item("jgsticky").GetValue<bool>()
                                    ? Game.CursorPos : target.ServerPosition);
                            else
                                ES.Spells["Pounce"].Cast(target.ServerPosition);
                        }
                    }
                }
            }
        }


        // Cougar E Logic
        internal static void CastSwipe(Obj_AI_Base target, string mode)
        {
            if (!ES.CatForm()) 
                return;

            if (ES.SpellTimer["Swipe"].IsReady() && KN.Root.Item("ndce" + mode).GetValue<bool>())
            {
                // check valid target in range
                if (target.IsValidTarget(ES.Spells["Swipe"].Range))
                {
                    if (target.IsChampion())
                    {
                        if (KN.Root.Item("ndcecheck").GetValue<bool>())
                        {
                            //ES.Spells["Javelin"].SPredictionCast((Obj_AI_Hero) target, ES.MyHitChance("ce"));
                            ES.Spells["Swipe"].Cast(target.ServerPosition);
                        }

                        else
                            ES.Spells["Swipe"].Cast(target.ServerPosition);
                    }

                    else
                    {
                        // try aoe swipe if menu item > 1
                        var minhit = KN.Root.Item("ndcenum").GetValue<Slider>().Value;
                        if (minhit > 1 && mode == "wc")
                            ES.CastSmartSwipe();

                        // or cast normal
                        else
                            ES.Spells["Swipe"].Cast(target.ServerPosition);
                    }
                }
            }
        }


        internal static void SwitchForm(Obj_AI_Base target, string mode)
        {
            if (!target.IsValidTarget(ES.Spells["Javelin"].Range))
                return;

            // catform -> human
            if (ES.CatForm() && ES.Spells["Aspect"].IsReady() && KN.Root.Item("ndcr" + mode).GetValue<bool>())
            {
                var radius = ES.Player.AttackRange + ES.Player.Distance(ES.Player.BBox.Minimum) + 1;

                // check valid target in Q range
                if (target.IsValidTarget(ES.Spells["Javelin"].Range))
                {
                    // change form if Q is ready and meets hitchance
                    if (ES.SpellTimer["Javelin"].IsReady())
                    {
                        if (target.IsChampion())
                        {
                            HitChance hitchance;
                            var objHero = target as Obj_AI_Hero;

                            var pos = SPrediction.Prediction.GetPredictionMethod2(target, ES.Spells["Javelin"],
                                objHero.GetWaypoints(), objHero.AvgMovChangeTime(), objHero.LastMovChangeTime(),
                                objHero.AvgPathLenght(), out hitchance, ES.Player.ServerPosition);

                            if (!ES.Spells["Javelin"].DoesCollide(pos) &&
                                hitchance >= (HitChance) (KN.Root.Item("ndhqch").GetValue<StringList>().SelectedIndex + 3))
                                ES.Spells["Aspect"].Cast();
                        }
                    }

                    // is jungling
                    if (mode == "jg")
                    {
                        if (Game.CursorPos.Distance(ES.Player.ServerPosition) >= 375 &&
                            !KN.Root.Item("jgsticky").GetValue<bool>())
                        {
                            ES.Spells["Aspect"].Cast();
                        }

                        if (ES.SpellTimer["Bushwhack"].IsReady() || ES.SpellTimer["Javelin"].IsReady() && 
                           !ES.Spells["Javelin"].DoesCollide(target.ServerPosition.To2D()))
                        {
                            if (!ES.SpellTimer["Swipe"].IsReady() && !ES.SpellTimer["Takedown"].IsReady() &&
                                !ES.SpellTimer["Pounce"].IsReady(2))
                            {
                                ES.Spells["Aspect"].Cast();
                            }
                        }
                    }

                    else
                    {
                        // change to human if out of pounce range and can die
                        if (!ES.SpellTimer["Pounce"].IsReady())
                        {
                            if (target.Distance(ES.Player.ServerPosition) > radius)
                            {
                                if (ES.Player.GetAutoAttackDamage(target, true) * 3 >= target.Health)
                                    ES.Spells["Aspect"].Cast();
                            }
                        }
                    }
                }
            }

            // human -> catform
            if (!ES.CatForm() && ES.Spells["Aspect"].IsReady() && KN.Root.Item("ndhr" + mode).GetValue<bool>())
            {
                if (mode == "jg" && ES.Counter < 2 && KN.Root.Item("jgaacount").GetValue<bool>())
                    return;

                if (mode == "wc")
                {
                    if (target.IsValidTarget(375) && target.IsMinion)
                    {
                        ES.Spells["Aspect"].Cast();
                        return;
                    }
                }

                if (mode == "gap")
                {
                    if (target.IsValidTarget(375))
                    {
                        ES.Spells["Aspect"].Cast();
                        return;
                    }
                }

                // pounce only hunted
                if (KN.Root.Item("ndhrwh").GetValue<StringList>().SelectedIndex == 1)
                {
                    if (target.IsValidTarget(ES.Spells["ExPounce"].Range) && target.IsHunted())
                        ES.Spells["Aspect"].Cast();
                }

                // pounce always any condition
                if (KN.Root.Item("ndhrwh").GetValue<StringList>().SelectedIndex == 2)
                {
                    if (target.IsValidTarget(ES.Spells["ExPounce"].Range) && target.IsHunted())
                        ES.Spells["Aspect"].Cast();

                    if (target.IsValidTarget(ES.Spells["Pounce"].Range) && !target.IsHunted())
                        ES.Spells["Aspect"].Cast();
                }

                // pounce with my recommended condition
                if (KN.Root.Item("ndhrwh").GetValue<StringList>().SelectedIndex == 0)
                {
                    if (target.IsHunted())
                    {
                        // force switch no swipe/takedown req
                        if (!KN.Root.Item("ndhrcreq").GetValue<bool>() && mode == "co" ||
                            !KN.Root.Item("ndhrjreq").GetValue<bool>() && mode == "jg")
                        {
                            ES.Spells["Aspect"].Cast();
                            return;
                        }

                        // or check if pounce timer is ready before switch
                        if (ES.SpellTimer["Pounce"].IsReady() && target.IsValidTarget(ES.Spells["ExPounce"].Range))
                        {
                            if (ES.SpellTimer["Takedown"].IsReady() || ES.SpellTimer["Swipe"].IsReady())
                                ES.Spells["Aspect"].Cast();
                        }
                    }

                    else
                    {
                        // define our q target
                        var qtarget = TargetSelector.GetTarget(ES.Spells["Javelin"].Range, TargetSelector.DamageType.Magical);

                        if (target.IsValidTarget(ES.Spells["Pounce"].Range + 100))
                        {
                            if (mode == "jg")
                            {
                                if (!ES.SpellTimer["Javelin"].IsReady(2) ||
                                     ES.Spells["Javelin"].DoesCollide(target.ServerPosition.To2D()))
                                     ES.Spells["Aspect"].Cast();
                            }

                            if (ES.CatDamage(target) >= target.Health && mode == "co")
                                ES.Spells["Aspect"].Cast();

                            // switch if Q is not ready in 2 sec, not learnedm or not enabled, also wait for .4 sec to cast R
                            if (!KN.Root.Item("ndhq" + mode).GetValue<bool>() || ES.NotLearned(ES.Spells["Javelin"]) ||
                                !ES.SpellTimer["Javelin"].IsReady(2) && Utils.GameTimeTickCount - (int) (ES.TimeStamp["Javelin"] * 1000) +
                                ((6 + (6 * ES.Player.PercentCooldownMod)) * 1000) >= 400)
                            {
                                ES.Spells["Aspect"].Cast();
                            }

                            if (qtarget.IsValidTarget(ES.Spells["Javelin"].Range) && target.IsChampion())
                            {
                                if (ES.SpellTimer["Javelin"].IsReady())
                                {
                                    HitChance hitchance;

                                    var pos = SPrediction.Prediction.GetPredictionMethod2(target, ES.Spells["Javelin"],
                                        qtarget.GetWaypoints(), qtarget.AvgMovChangeTime(), qtarget.LastMovChangeTime(),
                                        qtarget.AvgPathLenght(), out hitchance, ES.Player.ServerPosition);

                                    // if we dont meet hitchance on Q target pounce nearest target
                                    if (ES.Spells["Javelin"].DoesCollide(pos) || 
                                        hitchance < (HitChance) (KN.Root.Item("ndhqch").GetValue<StringList>().SelectedIndex + 3))
                                        ES.Spells["Aspect"].Cast();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
