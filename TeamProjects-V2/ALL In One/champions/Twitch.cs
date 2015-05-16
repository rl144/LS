using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Twitch
    {
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }

        static Spell Q, W, E, Recall;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 950f, TargetSelector.DamageType.True);
            E = new Spell(SpellSlot.E, 1200f);
            Recall = new Spell(SpellSlot.Recall);

            W.SetSkillshot(0.25f, 250f, 1400f, false, SkillshotType.SkillshotCircle);

            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addItem("Cast E if Stacks >=", new Slider(6, 1, 6));

            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addItem("Cast E if Stacks >=", new Slider(4, 1, 6));
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addUseKillsteal();
            AIO_Menu.Champion.Misc.addItem("Stealth Recall", new KeyBind('T', KeyBindType.Press));

            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addItem("Stealth Timer", true);
            AIO_Menu.Champion.Drawings.addItem("R Timer", true);
            AIO_Menu.Champion.Drawings.addItem("R Pierce Line", true);

            AIO_Menu.Champion.Drawings.addDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(100))
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Harass();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        Laneclear();
                        Jungleclear();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        break;
                }
            }

            if (AIO_Menu.Champion.Misc.getKeyBIndValue("Stealth Recall").Active)
            {
                if (Q.IsReady() && Recall.IsReady())
                {
                    Q.Cast();
                    Recall.Cast();
                }
            }

            Killsteal();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (E.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawW.Color);

            if (AIO_Menu.Champion.Drawings.getBoolValue("Stealth Timer"))
            {
                var buff = AIO_Func.getBuffInstance(Player, "TwitchHideInShadows");
                var mypos = Drawing.WorldToScreen(Player.Position);

                if (buff != null)
                    Drawing.DrawText(mypos[0] - 10, mypos[1], Color.SpringGreen, (buff.EndTime - Game.ClockTime).ToString("0.00"));
            }

            if (AIO_Menu.Champion.Misc.getKeyBIndValue("Stealth Recall").Active)
            {
                var mypos = Drawing.WorldToScreen(Player.Position);

                if (Q.IsReady() && Recall.IsReady())
                    Drawing.DrawText(mypos[0] - 60, mypos[1] - 50, Color.SpringGreen, "Try Stealth recall");
                else if (Player.HasBuff("TwitchHideInShadows") && Player.HasBuff("Recall"))
                    Drawing.DrawText(mypos[0] - 60, mypos[1] - 50, Color.SpringGreen, "Stealth Recall Activated");
                else if (!Player.HasBuff("recall"))
                    Drawing.DrawText(mypos[0] - 60, mypos[1] - 50, Color.SpringGreen, "Q is not ready");
            }

            if (AIO_Menu.Champion.Drawings.getBoolValue("R Pierce Line"))
            {
                if (Player.HasBuff("TwitchFullAutomatic", true))
                {
                    var aatarget = TargetSelector.GetTarget(Orbwalking.GetRealAutoAttackRange(Player), TargetSelector.DamageType.Physical);

                    if (aatarget != null)
                    {
                        var from = Drawing.WorldToScreen(Player.Position);
                        var dis = (Orbwalking.GetRealAutoAttackRange(Player) + 300) - Player.Distance(aatarget, false);
                        var to = Drawing.WorldToScreen(dis > 0 ? aatarget.ServerPosition.Extend(Player.Position, -dis) : aatarget.ServerPosition);

                        Drawing.DrawLine(from[0], from[1], to[0], to[1], 10, Color.FromArgb(100, 71, 200, 62));
                    }
                }
            }

            if (AIO_Menu.Champion.Drawings.getBoolValue("R Timer"))
            {
                var buff = AIO_Func.getBuffInstance(Player, "TwitchFullAutomatic");
                var mypos = Drawing.WorldToScreen(Player.Position);

                if (buff != null)
                    Drawing.DrawText(mypos[0] - 10, mypos[1], Color.SpringGreen, (buff.EndTime - Game.ClockTime).ToString("0.00"));
            }

        }

        static void Killsteal()
        {
            if (!AIO_Menu.Champion.Misc.UseKillsteal)
                return;

            foreach (Obj_AI_Hero target in HeroManager.Enemies.Where(x => x.IsValidTarget(E.Range) && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield)))
            {
                if (target != null)
                {
                    if (E.CanCast(target) && (target.Health + (target.HPRegenRate / 2)) <= E.GetDamage2(target))
                    {
                        E.Cast();
                        break;
                    }
                }
            }
        }

        static float GetComboDamage(Obj_AI_Base enemy)
        {
            return E.IsReady() ? E.GetDamage2(enemy) : 0;
        }

        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.True, false);

                if (W.CanCast(Wtarget) && W.GetPrediction(Wtarget).Hitchance >= HitChance.VeryHigh)
                    W.Cast(Wtarget);
            }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical, true);

                if (E.CanCast(Etarget))
                {
                    foreach (var buff in Etarget.Buffs)
                    {
                        if (buff.Name == "twitchdeadlyvenom")
                        {
                            if (buff.Count >= AIO_Menu.Champion.Combo.getSliderValue("Cast E if Stacks >=").Value)
                            {
                                E.Cast();
                                break;
                            }
                        }
                    }
                }
            }

        }

        static void Harass()
        {
            if (!(Player.ManaPercent > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.True, false);

                if (W.CanCast(Wtarget) && W.GetPrediction(Wtarget).Hitchance >= HitChance.VeryHigh)
                    W.Cast(Wtarget);
            }

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical, true);

                if (E.CanCast(Etarget))
                {
                    foreach (var buff in Etarget.Buffs)
                    {
                        if (buff.Name == "twitchdeadlyvenom")
                        {
                            if (buff.Count >= AIO_Menu.Champion.Harass.getSliderValue("Cast E if Stacks >=").Value)
                            {
                                E.Cast();
                                break;
                            }
                        }
                    }
                }
            }
        }

        static void Laneclear()
        {
            if (!(Player.ManaPercent > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (W.IsReady() && AIO_Menu.Champion.Laneclear.UseW)
            {
                var Farmloc = W.GetCircularFarmLocation(Minions);

                if (Farmloc.MinionsHit >= 3)
                    W.Cast(Farmloc.Position);
            }

            if (E.IsReady() && AIO_Menu.Champion.Laneclear.UseE)
            {
                var killcount = 0;

                foreach (var Minion in Minions)
                {
                    foreach (var buff in Minion.Buffs)
                    {
                        if (buff.Name == "twitchdeadlyvenom")
                        {
                            if (buff.Count >= 6)
                            {
                                E.Cast();
                                break;
                            }
                        }
                    }

                    if (Minion.Health <= E.GetDamage2(Minion))
                        killcount++;
                }

                if (killcount >= 2)
                    E.Cast();
            }
        }

        static void Jungleclear()
        {
            if (!(Player.ManaPercent > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count < 1)
                return;

            if (W.CanCast(Mobs[0]) && AIO_Menu.Champion.Jungleclear.UseW)
                W.Cast(Mobs[0]);

            if (E.CanCast(Mobs[0]) && AIO_Menu.Champion.Jungleclear.UseE)
            {
                foreach (var buff in Mobs[0].Buffs)
                {
                    if (buff.Name == "twitchdeadlyvenom")
                    {
                        if (buff.Count >= 6)
                        {
                            E.Cast();
                            break;
                        }
                    }
                }

                if ((Mobs[0].Health + Mobs[0].HPRegenRate) <= E.GetDamage2(Mobs[0]))
                    E.Cast();
            }
        }
    }
}
