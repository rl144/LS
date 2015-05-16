using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Ryze
    {
        static Menu Menu { get { return AIO_Menu.MainMenu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 880f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 600f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 600f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.25f, 50f, 1700f, true, SkillshotType.SkillshotLine);
            W.SetTargetted(0.25f, float.MaxValue);
            E.SetTargetted(0.25f, 2000f);

            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addUseKillsteal();
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(10))
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                {
                    Combo();
                    Orbwalker.SetAttack(!W.IsReady());
                }
                else
                    Orbwalker.SetAttack(true);

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                {
                    Harass();
                }

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    Laneclear();
                    Jungleclear();
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

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (W.CanCast(gapcloser.Sender))
                W.Cast(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (W.CanCast(sender))
                W.Cast(sender);
        }

        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTargetNoCollision(Q);

                if (qTarget.IsValidTarget(Q.Range))
                    Q.Cast(qTarget);
            }

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            {
                W.CastOnBestTarget();
            }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                E.CastOnBestTarget();
            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                if(!Q.IsReady() && !E.IsReady() && AIO_Func.getHealthPercent(Player) <= 98 && Player.CountEnemiesInRange(Q.Range) >= 1)
                    R.Cast();
            }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTargetNoCollision(Q);

                if (qTarget.IsValidTarget(Q.Range))
                    Q.Cast(qTarget);
            }

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            {
                W.CastOnBestTarget();
            }

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                E.CastOnBestTarget();
            }
        }

        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
                var qTarget = Minions.Where(x => x.IsValidTarget(Q.Range) && Q.GetPrediction(x).Hitchance >= HitChance.Medium && Q.IsKillable(x)).OrderByDescending(x => x.Health).FirstOrDefault();

                if (qTarget != null)
                    Q.Cast(qTarget);
            }

            if (AIO_Menu.Champion.Laneclear.UseW && W.IsReady())
            {
                var wTarget = Minions.Where(x => x.IsValidTarget(W.Range) && W.IsKillable(x)).OrderByDescending(x => x.Health).FirstOrDefault();
                
                if (wTarget != null)
                    W.Cast(wTarget);
            }

            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
            {
                if (Minions[0].IsValidTarget(E.Range))
                    E.Cast(Minions[0]);
            }
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                var qTarget = Mobs.FirstOrDefault(x => x.IsValidTarget(Q.Range) && Q.GetPrediction(x).Hitchance >= HitChance.Medium);

                if (qTarget != null)
                    Q.Cast(qTarget);
            }

            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady())
            {
                if (Mobs[0].IsValidTarget(W.Range))
                    W.Cast(Mobs[0]);
            }

            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
            {
                if (Mobs[0].IsValidTarget(E.Range))
                    E.Cast(Mobs[0]);
            }
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x=>x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.Cast(target);

                if (W.CanCast(target) && AIO_Func.isKillable(target, W))
                    W.Cast(target);

                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                    E.Cast(target);
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

            if (!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true) + (float)Player.GetAutoAttackDamage2(enemy, false);

            return damage;
        }
    }
}
