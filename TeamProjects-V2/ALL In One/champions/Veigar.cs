using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Veigar // By RL244
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float QD {get{return Menu.Item("Misc.Qtg").GetValue<Slider>().Value; }}

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 950f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 900f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 1040f);
            R = new Spell(SpellSlot.R, 650f, TargetSelector.DamageType.Magical);

            
            Q.SetSkillshot(0.25f, 70f, 2000f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(1.25f, 112.5f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.75f, 340f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetTargetted(0.25f, 1400f);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW(false);
            AIO_Menu.Champion.Harass.addUseE(false);
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Lasthit.addUseQ();
            AIO_Menu.Champion.Lasthit.addIfMana(20);
            
            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW(false);
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW(false);
            AIO_Menu.Champion.Jungleclear.addIfMana();
            

            AIO_Menu.Champion.Misc.addHitchanceSelector();

            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Qtg", "Additional Range")).SetValue(new Slider(25, 0, 150));
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Misc.addItem("W on stuned target", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange(false);
            AIO_Menu.Champion.Drawings.addItem("E Real Range", new Circle(true, Color.Green));
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
                
            if (Orbwalking.CanMove(10))
            {
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

            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();
            if (AIO_Menu.Champion.Misc.getBoolValue("W on stuned target"))
                stw();
                
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawEr = AIO_Menu.Champion.Drawings.getCircleValue("E Real Range");
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            var etarget = TargetSelector.GetTarget(E.Range + Player.MoveSpeed * E.Delay, TargetSelector.DamageType.Magical);

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

            if (E.IsReady() && drawEr.Active && etarget != null)
                Render.Circle.DrawCircle(Player.Position, E.Range - etarget.MoveSpeed*E.Delay, drawEr.Color);
                
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (E.IsReady()
            && Player.Distance(gapcloser.Sender.Position) <= E.Range + Player.MoveSpeed*E.Delay)
                castE((Vector3)gapcloser.End);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (E.IsReady()
            && Player.Distance(sender.Position) <= E.Range)
                castE(sender);
        }

        
        static void Combo()
        {
            var TTTget = TargetSelector.GetTarget(E.Range + Player.MoveSpeed*E.Delay, TargetSelector.DamageType.Magical);
            if(TTTget == null)
            return;

            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                AIO_Func.LCast(Q,Qtarget,QD,1f);
            }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(E.Range + Player.MoveSpeed*E.Delay, TargetSelector.DamageType.Magical);
                castE(Etarget);
            }

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
                var pred = W.GetPrediction(Wtarget);
                if (pred.Hitchance == HitChance.Immobile || Wtarget.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay - 0.3f && W.IsReady())
                W.Cast(Wtarget, false, true);
            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                var Rtarget = TargetSelector.GetTarget(R.Range, R.DamageType);
            
                if(AIO_Func.isKillable(Rtarget, R) || Rtarget.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= R.Delay && R.IsReady())
                { 
                    if (HeroManager.Enemies.Any(x => x.IsValidTarget(R.Range)))
                    R.Cast(Rtarget);
                }
            }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;
            var TTTget = TargetSelector.GetTarget(E.Range + Player.MoveSpeed*E.Delay, TargetSelector.DamageType.Magical);
            if(TTTget == null)
            return;

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                AIO_Func.LCast(Q,Qtarget,QD,1f);
            }

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(E.Range + Player.MoveSpeed*E.Delay, TargetSelector.DamageType.Magical);
                castE(Etarget);
            }

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
                var pred = W.GetPrediction(Wtarget);
                if (pred.Hitchance == HitChance.Immobile || Wtarget.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay - 0.3f && W.IsReady())
                W.Cast(Wtarget, false, true);
            }
        }

        static void Lasthit()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Lasthit.IfMana))
                return;
            if (AIO_Menu.Champion.Lasthit.UseQ && Q.IsReady())
                AIO_Func.LH(Q,1f);
        }
        
        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
                AIO_Func.LH(Q,1f);

            if (AIO_Menu.Champion.Laneclear.UseW && W.IsReady() && Minions.Any(x => x.IsValidTarget(W.Range)))
            {
                var farmloc = W.GetCircularFarmLocation(Minions);
                if (farmloc.MinionsHit >= 3)
                    W.Cast(farmloc.Position);
            }
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady() && Q.CanCast(Mobs.FirstOrDefault()))
                AIO_Func.LCast(Q,Mobs.FirstOrDefault(),QD,1f);

            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady() && Mobs.Any(x=>x.IsValidTarget(W.Range)))
                AIO_Func.CCast(W,Mobs[0]);
        }

        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    AIO_Func.LCast(Q,target,0,1f);
            }
        }
        
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (R.CanCast(target) && AIO_Func.isKillable(target, R))
                    R.Cast(target);
            }
        }
        static void stw()
        {
            if (AIO_Menu.Champion.Combo.UseW && W.IsReady() && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
            {
            var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
            if(Wtarget == null)
            return;
            var pred = W.GetPrediction(Wtarget);
            if (pred.Hitchance == HitChance.Immobile || Wtarget.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay - 0.4f && W.IsReady())
            W.Cast(Wtarget, false, true);
            }
        }
        
        static void castE(Obj_AI_Base target) //E CAST(base)
        {
            PredictionOutput pred = Prediction.GetPrediction(target, E.Delay);
            Vector2 castVec = pred.UnitPosition.To2D() +
                              Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width;
            Vector2 castVec2 = pred.UnitPosition.To2D() -
                              Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width;
                              
            if (pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance && E.IsReady() && Vector3.Distance(Player.Position, pred.UnitPosition) <= 700 - E.Width)
            {
                E.Cast(castVec, false);
            }
            if (pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance && E.IsReady() && Vector3.Distance(Player.Position, pred.UnitPosition) > 700 - E.Width)
            {
                E.Cast(castVec2, false);
            }
        }

        
        static void castE(Obj_AI_Hero target) //E CAST(hero)
        {
            PredictionOutput pred = Prediction.GetPrediction(target, E.Delay);
            Vector2 castVec = pred.UnitPosition.To2D() +
                              Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width;
            Vector2 castVec2 = pred.UnitPosition.To2D() -
                              Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width;
                              
            if (pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance && E.IsReady() && Vector3.Distance(Player.Position, pred.UnitPosition) <= 700 - E.Width)
            {
                E.Cast(castVec, false);
            }
            if (pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance && E.IsReady() && Vector3.Distance(Player.Position, pred.UnitPosition) > 700 - E.Width)
            {
                E.Cast(castVec2, false);
            }
        }
        
        public static void castE(Vector3 pos)
        {
            Vector2 castVec = pos.To2D() +
                              Vector2.Normalize(pos.To2D() - Player.Position.To2D()) * E.Width;
                              
            Vector2 castVec2 = pos.To2D() -
                              Vector2.Normalize(pos.To2D() - Player.Position.To2D()) * E.Width;

            if (E.IsReady() && Vector3.Distance(Player.Position, pos) <= 700 - E.Width)
            {
                E.Cast(castVec, false);
            }
            if (E.IsReady() && Vector3.Distance(Player.Position, pos) > 700 - E.Width)
            {
                E.Cast(castVec2, false);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);

            if (W.IsReady())
                damage += W.GetDamage2(enemy);

            if (R.IsReady())
                damage += R.GetDamage2(enemy);
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
            return damage;
        }
        
        
    }
}
