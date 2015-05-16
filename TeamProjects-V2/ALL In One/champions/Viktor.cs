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
    class Viktor// By RL244 드디어 빅토르 완성!! VictorPowerTransfer viktorpowertransferreturn (<- 이게 Q강화) viktoreaug viktorqeaug viktorqweaug viktorqbuff
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static bool Q2 { get { return (Player.HasBuff("viktorqaug", true) || Player.HasBuff("viktorqwaug", true) || Player.HasBuff("viktorqeaug", true) || Player.HasBuff("viktorqweaug", true)); } }
        static bool W2 { get { return (Player.HasBuff("viktorwaug", true) || Player.HasBuff("viktorweaug", true) || Player.HasBuff("viktorqwaug", true) || Player.HasBuff("viktorqweaug", true)); } }
        static bool E2 { get { return (Player.HasBuff("viktoreaug", true) || Player.HasBuff("viktorweaug", true) || Player.HasBuff("viktorqeaug", true) || Player.HasBuff("viktorqweaug", true)); } }
        static bool R2 { get { return Player.HasBuff("viktorqweaug", true); } }
        static Spell Q, W, E, R;
        static float RDelay = 0f;
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 700f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 700f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 550f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 700f, TargetSelector.DamageType.Magical);
            
            
            Q.SetTargetted(0.25f, 2000f);
            W.SetSkillshot(1.5f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 250f, 1200f, false, SkillshotType.SkillshotLine); // 550f + 700f
            R.SetSkillshot(0.25f, 325f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW(false);
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();
            
            AIO_Menu.Champion.Lasthit.addUseQ();
            AIO_Menu.Champion.Lasthit.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseE(false);
            AIO_Menu.Champion.Laneclear.addIfMana();
            
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW(false);
            AIO_Menu.Champion.Jungleclear.addUseE(false);
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            //Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Etg", "Additional Erange")).SetValue(new Slider(50, 0, 250));
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addItem("E Safe Range", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addErange(false);
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
                
            if (Orbwalking.CanMove(35))
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
                        Orbwalker.SetAttack(!AIO_Menu.Champion.Lasthit.UseQ || !Q.IsReady() || Player.HasBuff("viktorpowertransferreturn"));
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
                Storm();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawEr = AIO_Menu.Champion.Drawings.getCircleValue("E Safe Range");
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            var etarget = TargetSelector.GetTarget(E.Range + 700f + Player.MoveSpeed * E.Delay, TargetSelector.DamageType.Magical);

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (E.IsReady() && drawEr.Active && etarget != null)
                Render.Circle.DrawCircle(Player.Position, E.Range + 700f - ((etarget.Distance(Player.ServerPosition)-E.Range)/E.Speed+E.Delay)*etarget.MoveSpeed, drawEr.Color);
        
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
        
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }
        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (E.IsReady()
                && Player.Distance(gapcloser.Sender.Position) <= E.Range)
                AIO_Func.AtoB(E,gapcloser.Sender);
        }
        
        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (R.CanCast(sender) && R.Instance.Name == "ViktorChaosStorm")
                AIO_Func.CCast(R,sender);
        }
        
        static void Storm()
        {
            if(R.Instance.Name != "ViktorChaosStorm" && Environment.TickCount - RDelay > 0)
            {
                var T = TargetSelector.GetTarget(R.Range*3/2, R.DamageType);
                if(T != null)
                {
                    R.Cast(T);
                    RDelay = Environment.TickCount + 2000f;
                }
            }
        }
        
        static void Combo()
        {

            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                if(Qtarget != null)
                Q.Cast(Qtarget);
            }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(E.Range + 700f, TargetSelector.DamageType.Magical);
                if(Etarget != null)
                AIO_Func.AtoB(E,Etarget);
            }

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                if(Wtarget != null)
                AIO_Func.CCast(W,Wtarget);
            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                var Rtarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                if(R.Instance.Name == "ViktorChaosStorm" && AIO_Func.isKillable(Rtarget, R.GetDamage2(Rtarget)+R.Width/Rtarget.MoveSpeed*R.GetDamage2(Rtarget,1)*3+(E.IsReady() ? (E2 ? E.GetDamage2(Rtarget,1) : E.GetDamage2(Rtarget)) : 0) + (Q.IsReady() ? Q.GetDamage2(Rtarget) : 0)) && Rtarget != null)
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
                if(Qtarget != null)
                Q.Cast(Qtarget);
            }
            
            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                if(Wtarget != null)
                AIO_Func.CCast(W,Wtarget);
            }

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(E.Range + 700f, TargetSelector.DamageType.Magical);
                if(Etarget != null)
                AIO_Func.AtoB(E,Etarget);
            }
        }

        static void Lasthit()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Lasthit.IfMana))
                return;
                
            if (AIO_Menu.Champion.Lasthit.UseQ && Q.IsReady())
            AIO_Func.LH(Q,0);
        }
        
        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;
        
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
            {
                var _m = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(m => (E2 ? AIO_Func.isKillable(m,Q,1) : AIO_Func.isKillable(m,Q,0)) && HealthPrediction.GetHealthPrediction(m, (int)(Player.Distance(m, false) / E.Speed), (int)(E.Delay * 1000 + Game.Ping / 2)) > 0);            
                if (_m != null)
                AIO_Func.AtoB(E,_m);
            }
            
            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
            AIO_Func.LH(Q,0);
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
                if (Q.CanCast(Mobs.FirstOrDefault()))
                Q.Cast(Mobs.FirstOrDefault());
            }
            
            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
            {
                if (Mobs.Any(x=>x.IsValidTarget(E.Range)))
                AIO_Func.AtoB(E,Mobs[0]);
            }

        }

        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                Q.Cast(target);
            }
        }

        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (target.IsValidTarget(R.Range) && AIO_Func.isKillable(target, R.GetDamage2(target)+R.Width/target.MoveSpeed*R.GetDamage2(target,1)))
                R.Cast(target.ServerPosition);
            }
        }
        
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                AIO_Func.AtoB(E,target);
            }
        }
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);
                                
            if (E.IsReady())
                damage += (E2 ? E.GetDamage2(enemy,1) : E.GetDamage2(enemy));

            if (R.IsReady())
                damage += R.GetDamage2(enemy)+R.Width/enemy.MoveSpeed*2*R.GetDamage2(enemy,1);

            if(Player.HasBuff("viktorpowertransferreturn"))
                damage += (float)Player.GetAutoAttackDamage2(enemy, true); //어짜피 여기에 빅토르 Q 증강뎀 들어감.
            return damage;
        }
    }
}
