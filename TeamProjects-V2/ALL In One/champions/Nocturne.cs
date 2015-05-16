using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Nocturne// By RL244 nocturneduskbringerhaste NocturnPeranoia nocturneparanoiatargetorder NearSight(target)
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static int RM {get{return Menu.Item("Combo.RM").GetValue<Slider>().Value; }}
        static float QD {get{return Menu.Item("Misc.Qtg").GetValue<Slider>().Value; }}
        
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1200f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 425f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 2000f, TargetSelector.DamageType.Physical);

            Q.SetSkillshot(0.25f, 60f, 1600f, false, SkillshotType.SkillshotLine);
            E.SetTargetted(2.0f, float.MaxValue);
            R.SetTargetted(2.0f, 2500f);
            
            AIO_Menu.Champion.Combo.addUseQ();
            //AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();
            
            AIO_Menu.Champion.Lasthit.addUseQ(false);
            AIO_Menu.Champion.Lasthit.addIfMana(20);
            
            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Qtg", "Additional Range")).SetValue(new Slider(50, 0, 150));
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", false);
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();

        
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
            R.Range = 1250f + R.Level*750f;

            if (Orbwalking.CanMove(35))
            {
                AIO_Func.SC(Q,QD);
                if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                {
                    foreach (var target in HeroManager.Enemies.Where(x => !x.IsDead && AIO_Func.ECTarget(x,900,60,100) <= 1 && AIO_Func.isKillable(x,getComboDamage(x)*2)))
                    {
                        if(target.Distance(Player.ServerPosition) <= R.Range && R.IsReady() && target != null && AIO_Menu.Champion.Combo.UseR)
                        {
                            if(Player.HasBuff("NocturnPeranoia"))
                                R.Cast(target);
                            else
                                R.Cast();
                        }
                    }
                }
                AIO_Func.SC(E);
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();
            #endregion
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
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }
        
        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            /*var Sender = (Obj_AI_Base) sender;
            var STarget = (Obj_AI_Hero) args.Target;
            if (!sender.IsMe || Player.IsDead) // W 로직 미완성
                return;
                
            if (args.Target.IsMe && !sender.IsAlly && W.IsReady() && AIO_Func.getHealthPercent(Player) < 80 //args.Target.IsMe && AIO_Menu.Champion.Misc.getBoolValue("R Myself Only")
                && Player.Distance(args.End) < 150 && AIO_Menu.Champion.Combo.UseW)
                W.Cast();
            if (!sender.IsAlly && W.IsReady() && AIO_Func.getHealthPercent(Player) < 80 && Player.Distance(args.End) < 150 &&
                Sender.Distance(Player.ServerPosition) <= 1000f && AIO_Menu.Champion.Combo.UseW)
                W.Cast();
            if (!sender.IsAlly && W.IsReady() && AIO_Func.getHealthPercent(Player) < 80 &&
                Sender.Distance(Player.ServerPosition) <= 700f && AIO_Menu.Champion.Combo.UseW)
                W.Cast();*/
        }
        
        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    AIO_Func.LCast(Q,target,QD);
            }
        }
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (R.CanCast(target) && AIO_Func.isKillable(target, R))
                {
                    if(Player.HasBuff("NocturnPeranoia"))
                        R.Cast(target);
                    else
                        R.Cast();
                }
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);
            
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
