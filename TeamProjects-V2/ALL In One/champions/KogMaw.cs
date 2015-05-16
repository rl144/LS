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
    class KogMaw // By RL244
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static int UltST {get{var buff = AIO_Func.getBuffInstance(Player, "kogmawlivingartillery"); return buff != null ? buff.Count : 0;} }
        static int UST {get{return Menu.Item("Misc.Rst").GetValue<Slider>().Value; }}
        static float ED {get{return Menu.Item("Misc.Etg").GetValue<Slider>().Value; }}
        static float QD {get{return Menu.Item("Misc.Qtg").GetValue<Slider>().Value; }}

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1000f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 630f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 1280f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 1200f, TargetSelector.DamageType.Magical);

            
            Q.SetSkillshot(0.25f, 70f, 1650f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1.2f, 150f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addUseR();
            AIO_Menu.Champion.Harass.addIfMana();
            
            AIO_Menu.Champion.Lasthit.addUseQ();
            AIO_Menu.Champion.Lasthit.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addUseR();
            AIO_Menu.Champion.Laneclear.addIfMana();
            
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addUseR();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();

            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Qtg", "Additional Qrange")).SetValue(new Slider(50, 0, 250));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Etg", "Additional Erange")).SetValue(new Slider(50, 0, 250));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Rst", "R Max Stacks")).SetValue(new Slider(1, 0, 10));
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addItem("Q Safe Range", new Circle(true, Color.Blue));
            AIO_Menu.Champion.Drawings.addItem("E Safe Range", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addErange(false);
            AIO_Menu.Champion.Drawings.addRrange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.AfterAttack += Orbwalking_OnAfterAttack;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(30))
            {
            R.Range = 900f +R.Level*300f;
            W.Range = 610f +W.Level*20f;
                AIO_Func.SC(W);
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    Combo();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                    Harass();
                    
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
                    Lasthit();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    Laneclear();
                    Jungleclear();
                }
            }

            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
                
            #region AfterAttack
            AIO_Func.AASkill(Q);
            if(AIO_Func.AfterAttack())
            AA();
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawQr = AIO_Menu.Champion.Drawings.getCircleValue("Q Safe Range");
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawEr = AIO_Menu.Champion.Drawings.getCircleValue("E Safe Range");
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            var etarget = TargetSelector.GetTarget(E.Range + Player.MoveSpeed * E.Delay, TargetSelector.DamageType.Magical);

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
                
            if (Q.IsReady() && drawQr.Active && etarget != null)
                Render.Circle.DrawCircle(Player.Position, Q.Range - etarget.MoveSpeed*Q.Delay, drawQr.Color);

            if (E.IsReady() && drawEr.Active && etarget != null)
                Render.Circle.DrawCircle(Player.Position, E.Range - etarget.MoveSpeed*E.Delay, drawEr.Color);
        
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
        
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }
        
        static void Orbwalking_OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || (Target == null))
                return;
                
            AIO_Func.AALcJc(Q);
            
            if(!utility.Activator.AfterAttack.AIO)
            AA();
        }
        
        static void AA()
        {
            AIO_Func.AACb(Q,QD,0f);
        }
        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (E.IsReady()
                && Player.Distance(gapcloser.Sender.Position) <= E.Range)
                E.Cast((Vector3)gapcloser.End);
        }

        
        static void Combo()
        {

            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                if(Qtarget.Distance(Player.ServerPosition) > Orbwalking.GetRealAutoAttackRange(Player))
                AIO_Func.LCast(Q,Qtarget,QD,0);
            }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                AIO_Func.LCast(E,Etarget,ED);
            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady() && UltST < UST)
            {
                var Rtarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                AIO_Func.CCast(R,Rtarget);
            }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;
        
            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if(Qtarget.Distance(Player.ServerPosition) > Orbwalking.GetRealAutoAttackRange(Player))
                AIO_Func.LCast(Q,Qtarget,QD,0);
            }

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                AIO_Func.LCast(E,Etarget,ED);
            }
            
            if (AIO_Menu.Champion.Harass.UseR && R.IsReady() && UltST < UST)
            {
                var Rtarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                AIO_Func.CCast(R,Rtarget);
            }
            
        }
        
        static void Lasthit()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Lasthit.IfMana))
                return;

            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
                AIO_Func.LH(Q,0f);
        }

        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;
        
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
                AIO_Func.LH(E,float.MaxValue);
            if (AIO_Menu.Champion.Laneclear.UseW && W.IsReady() && Minions.Any(x => Orbwalking.InAutoAttackRange(x)))
                W.Cast();
            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
                AIO_Func.LH(Q,0f);
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;
        
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady() && Mobs.Any(x => Orbwalking.InAutoAttackRange(x)))
                W.Cast();
            
            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady() && Mobs.Any(x=>x.IsValidTarget(E.Range)))
                AIO_Func.LCast(E,Mobs[0],ED);
        }

        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                AIO_Func.LCast(Q,target,QD,0);
            }
        }
        
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (target.IsValidTarget(R.Range) && AIO_Func.isKillable(target, R))
                    AIO_Func.CCast(R,target);
            }
        }
        
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                AIO_Func.LCast(E,target,ED);
            }
        }
        
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy) + (float)Player.GetAutoAttackDamage2(enemy, true);

            if (W.IsReady())
                damage += W.GetDamage2(enemy);
                
            if (E.IsReady())
                damage += E.GetDamage2(enemy);

            if (R.IsReady())
                damage += R.GetDamage2(enemy);
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
            return damage;
        }
    }
}
