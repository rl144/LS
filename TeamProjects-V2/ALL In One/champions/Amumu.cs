using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Amumu
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1050f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 300f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 320f, TargetSelector.DamageType.Magical) { Delay = 0.35f};
            R = new Spell(SpellSlot.R, 550f, TargetSelector.DamageType.Magical) { Delay = 0.25f};

            Q.SetSkillshot(0.25f, 90f, 2000f, true, SkillshotType.SkillshotLine);

            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();
            AIO_Menu.Champion.Combo.addItem("Cast R if Enemy number >=", new Slider(2, 1, 5));

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana(60);

            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addUseInterrupter();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
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
                        if (W.IsReady() && W.Instance.ToggleState == 2 && !HeroManager.Enemies.Any(x => x.IsValidTarget(W.Range)) && !MinionManager.GetMinions(W.Range, MinionTypes.All, MinionTeam.NotAlly).Any())
                            W.Cast();
                        break;
                }
            }

            if (AIO_Menu.Champion.Misc.UseKillsteal)
                Killsteal();

            Q.MinHitChance = AIO_Menu.Champion.Misc.SelectedHitchance;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color, 3);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color, 3);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color, 3);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color, 3);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (R.CanCast(sender) && args.DangerLevel == Interrupter2.DangerLevel.High)
                R.Cast();
        }

        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
                Q.CastOnBestTarget();

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            {
                if (HeroManager.Enemies.Any(x => x.IsValidTarget(W.Range)) && W.Instance.ToggleState == 1)
                    W.Cast();
                else if (!HeroManager.Enemies.Any(x => x.IsValidTarget(W.Range)) && W.Instance.ToggleState == 2)
                    W.Cast();
            }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                if (AIO_Func.SelfAOE_Prediction.HitCount(E.Delay, E.Range) >= 1)
                    E.Cast();
            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                if (AIO_Func.SelfAOE_Prediction.HitCount(R.Delay, R.Range) >= AIO_Menu.Champion.Combo.getSliderValue("Cast R if Enemy number >=").Value)
                    R.Cast();
            }
        }

        static void Harass()
        {
            if (W.IsReady() && !HeroManager.Enemies.Any(x => x.IsValidTarget(W.Range)) && W.Instance.ToggleState == 2)
                W.Cast();

            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
            {
                if (W.IsReady() && W.Instance.ToggleState == 2)
                    W.Cast();

                return;
            }

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
                Q.CastOnBestTarget();

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            {
                if (HeroManager.Enemies.Any(x => x.IsValidTarget(W.Range)) && W.Instance.ToggleState == 1)
                    W.Cast();
            }

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                if (AIO_Func.SelfAOE_Prediction.HitCount(E.Delay, E.Range) >= 1)
                    E.Cast();
            }
        }

        static void Laneclear()
        {
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (W.IsReady() && !Minions.Any(x => x.IsValidTarget(W.Range)) && W.Instance.ToggleState == 2)
                W.Cast();

            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
            {
                if (W.IsReady() && W.Instance.ToggleState == 2)
                    W.Cast();

                return;
            }

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseW && W.IsReady())
            {
                if (Minions.Any(x => x.IsValidTarget(W.Range)) && W.Instance.ToggleState == 1)
                    W.Cast();
            }

            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
            {
                if (Minions.Any(x => x.IsValidTarget(E.Range)))
                    E.Cast();
            }
        }

        static void Jungleclear()
        {
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (W.IsReady() && !Mobs.Any(x => x.IsValidTarget(W.Range)) && W.Instance.ToggleState == 2)
                W.Cast();

            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
            {
                if (W.IsReady() && W.Instance.ToggleState == 2)
                    W.Cast();

                return;
            }

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                if (Q.CanCast(Mobs[0]))
                    Q.Cast(Mobs[0]);
            }

            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady())
            {
                if (Mobs.Any(x => x.IsValidTarget(W.Range)) && W.Instance.ToggleState == 1)
                    W.Cast();
            }

            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
            {
                if (Mobs.Any(x => x.IsValidTarget(E.Range)))
                    E.Cast();
            }
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.Cast(target);

                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                    E.Cast(target);

                if (R.CanCast(target) && AIO_Func.isKillable(target, R))
                    R.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);

            if (W.IsReady())
                damage += W.GetDamage2(enemy);

            if (E.IsReady())
                damage += E.GetDamage2(enemy);

            if (R.IsReady())
                damage += R.GetDamage2(enemy);

            return damage;
        }
    }
}
