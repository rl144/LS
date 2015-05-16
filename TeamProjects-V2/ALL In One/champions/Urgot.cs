using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Urgot // 원작 Fakker지만.. 이후 사용자 제보로 RL244가 잘못된 코드나 부족한 로직 메꿈.
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, Q2, W, E, R;

        public static void Load()
        {
           
            Q = new Spell(SpellSlot.Q, 1000f, TargetSelector.DamageType.Physical);
            Q2 = new Spell(SpellSlot.Q, 1200f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1000f, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R);
         
            Q.SetSkillshot(0.25f, 60f, 1600f, true, SkillshotType.SkillshotLine);
            Q2.SetSkillshot(0.25f, 60f, 1600f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 250f, 1500f, false, SkillshotType.SkillshotCircle);

            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana(40);
            
            AIO_Menu.Champion.Lasthit.addUseQ();
            AIO_Menu.Champion.Lasthit.addIfMana(20);
            
            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseE(false);
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana(20);

            AIO_Menu.Champion.Misc.addHitchanceSelector();

            AIO_Menu.Champion.Misc.addUseKillsteal();
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();
           
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

            R.Range = 150 * R.Level + 400;

            if (Orbwalking.CanMove(10))
            {
                AIO_Func.SC(E);

                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Orbwalker.SetAttack(true);
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Orbwalker.SetAttack(true);
                        Harass();
                        break;
                    case Orbwalking.OrbwalkingMode.LastHit:
                        Orbwalker.SetAttack(AIO_Func.getManaPercent(Player) <= AIO_Menu.Champion.Lasthit.IfMana || !AIO_Menu.Champion.Lasthit.UseQ || !Q.IsReady());
                        Lasthit();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        Orbwalker.SetAttack(true);
                        Laneclear();
                        Jungleclear();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        Orbwalker.SetAttack(true);
                        break;
                }
            }

            if (AIO_Menu.Champion.Misc.UseKillsteal)
                Killsteal();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (Q.CanCast(gapcloser.Sender))
            {
                if(W.IsReady() && AIO_Menu.Champion.Combo.UseW)
                W.Cast();
                Q.Cast(gapcloser.Sender);
            }
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (R.CanCast(sender) && args.DangerLevel == Interrupter2.DangerLevel.High)
                R.Cast(sender);
        }

        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseQ && AIO_Menu.Champion.Combo.UseW && Q.IsReady())
            {
                var Q2target = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Physical);
                
                if(Q2target != null && Q2target.HasBuff("urgotcorrosivedebuff"))
                {
                    Q2.Cast(Q2target);
                    if(W.IsReady() && AIO_Menu.Champion.Combo.UseW)
                        W.Cast();
                }
                else
                {
                   var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

                    if (qTarget != null && Q.GetPrediction(qTarget).Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
                    {
                        Q.Cast(qTarget);
                        if(W.IsReady() && AIO_Menu.Champion.Combo.UseW)
                            W.Cast();
                    }    
                }
            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                var turret = ObjectManager.Get<Obj_AI_Turret>().OrderBy(t => t.Distance(rTarget.ServerPosition)).First(t => t.IsEnemy);
                if (rTarget != null && turret.Distance(rTarget.ServerPosition) >= 825 && rTarget.CountEnemiesInRange(700) <= Player.CountAlliesInRange(700) && Player.HealthPercent > 80)
                {
                    R.CastOnUnit(rTarget);
                }
            }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
                var Q2target = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Physical);
                
                if(Q2target != null && Q2target.HasBuff("urgotcorrosivedebuff"))
                {
                    Q2.Cast(Q2target);
                }
                else
                {
                    var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

                    if (qTarget != null && Q.GetPrediction(qTarget).Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
                        Q.Cast(qTarget);
                }
            }
        }
        
        static void Lasthit()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Lasthit.IfMana))
                return;

            var Minions = MinionManager.GetMinions(Q2.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Lasthit.UseQ && Q.IsReady())
            {
                var Q2t = MinionManager.GetMinions(Q2.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).Where(x => x.HasBuff("urgotcorrosivedebuff") && AIO_Func.isKillable(x,Q,0) && AIO_Func.PredHealth(x,Q) > 0).FirstOrDefault();
                var qTarget = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).FirstOrDefault(x => AIO_Func.isKillable(x,Q,0) && AIO_Func.PredHealth(x,Q) > 0);
                if(Q2t != null)
                {
                    Q2.Cast(Q2t);
                }
                else if(qTarget != null)
                {
                    if (Q.GetPrediction(qTarget).Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
                        Q.Cast(qTarget);
                }
            }
        }
        
        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(Q2.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
                var Q2t = MinionManager.GetMinions(Q2.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.HasBuff("urgotcorrosivedebuff"));
                if(Q2t != null)
                {
                    Q2.Cast(Q2t);
                }
                else
                {
                    var qTarget = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(x => AIO_Func.isKillable(x,Q,0) && AIO_Func.PredHealth(x,Q) > 0);

                    if (qTarget != null && Q.GetPrediction(qTarget).Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
                        Q.Cast(qTarget);
                }
            }
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(Q2.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                var Q2t = MinionManager.GetMinions(Q2.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.HasBuff("urgotcorrosivedebuff"));
                if(Q2t != null)
                {
                    Q2.Cast(Q2t);
                }
                else
                {
                    var qTarget = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

                    if (qTarget != null && Q.GetPrediction(qTarget[0]).Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
                        Q.Cast(qTarget[0]);
                }
            }

            if (AIO_Menu.Champion.Jungleclear.UseE && W.IsReady())
            {
                if (W.CanCast(Mobs.FirstOrDefault()))
                    W.Cast();
            }
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.Cast(target);
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
