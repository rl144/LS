using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Shen
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 475f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 620f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R);
         
            Q.SetTargetted(0.25f, 1500f);
            E.SetSkillshot(0, 50f, 1600f, false, SkillshotType.SkillshotLine);

            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseE();

            AIO_Menu.Champion.Harass.addUseQ();

            AIO_Menu.Champion.Lasthit.addUseQ();

            AIO_Menu.Champion.Laneclear.addUseQ();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            
            AIO_Menu.Champion.Flee.addUseE();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addUseInterrupter();
            AIO_Menu.Champion.Misc.addItem("Auto-W", true);

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addErange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(100))
            {
                AIO_Func.FleeToPosition(E);
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Harass();
                        break;
                    case Orbwalking.OrbwalkingMode.LastHit:
                        Lasthit();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        Laneclear();
                        Jungleclear();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        break;
                }
            }

            E.MinHitChance = AIO_Menu.Champion.Misc.SelectedHitchance;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color, 3);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color, 3);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (E.CanCast(sender) && args.DangerLevel == Interrupter2.DangerLevel.High)
                E.Cast(sender);
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.IsDead)
                return;

            if (sender.IsEnemy && args.Target.IsMe && W.IsReady() && AIO_Menu.Champion.Misc.getBoolValue("Auto-W"))
                W.Cast();
        }

        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
                Q.CastOnBestTarget();

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
                E.CastOnBestTarget();
        }

        static void Harass()
        {
            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
                Q.CastOnBestTarget();
        }

        static void Lasthit()
        {
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Lasthit.UseQ && Q.IsReady())
            {
                var qTarget = Minions.Where(x => x.IsValidTarget(Q.Range) && Q.IsKillable(x)).OrderBy(x => x.Health).FirstOrDefault();

                if (qTarget != null)
                    Q.Cast(qTarget);
            }
        }

        static void Laneclear()
        {
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
                var qTarget = Minions.Where(x => x.IsValidTarget(Q.Range) && Q.IsKillable(x)).OrderBy(x => x.Health).FirstOrDefault();

                if (qTarget != null)
                    Q.Cast(qTarget);
            }
        }

        static void Jungleclear()
        {
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                if (Mobs[0].IsValidTarget(Q.Range))
                    Q.Cast(Mobs[0]);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);

            if (E.IsReady())
                damage += E.GetDamage2(enemy);

            return damage;
        }
    }
}
