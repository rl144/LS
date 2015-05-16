using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Sona// By RL244
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static int RM {get{return Menu.Item("Combo.RM").GetValue<Slider>().Value; }}
        
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 840f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 1000f);
            E = new Spell(SpellSlot.E, 350f);
            R = new Spell(SpellSlot.R, 1000f, TargetSelector.DamageType.Magical);

            R.SetSkillshot(0.25f, 140f, 2400f, false, SkillshotType.SkillshotLine);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo.RM", "R Min Targets")).SetValue(new Slider(1, 1, 5));

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addIfMana();
            
            AIO_Menu.Champion.Lasthit.addUseQ(false);
            AIO_Menu.Champion.Lasthit.addIfMana(20);
            
            AIO_Menu.Champion.Laneclear.addUseQ(false);
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Flee.addUseE();
            AIO_Menu.Champion.Flee.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
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

            if (Orbwalking.CanMove(35))
            {
                AIO_Func.SC(Q);
                AIO_Func.Heal(W);
                if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Flee && AIO_Menu.Champion.Flee.UseE && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Flee.IfMana && E.IsReady())
                    E.Cast();
                
                if(AIO_Func.EnemyCount(R.Range - 10) >= RM && R.IsReady() && AIO_Menu.Champion.Combo.UseR)
                {
                    foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
                    {
                        if (R.CanCast(target))
                            R.CastIfWillHit(target,RM);
                    }
                }
                if(AIO_Func.EnemyCount(600) == 0 && AIO_Func.EnemyCount(1500) >= 1 && E.IsReady() && AIO_Menu.Champion.Combo.UseE && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                {
                    E.Cast();
                }
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
            var Sender = (Obj_AI_Base) sender;
            var STarget = (Obj_AI_Hero) args.Target;
            if (!sender.IsMe || Player.IsDead) // 
                return;
            if (args.Target.IsMe && !sender.IsAlly && W.IsReady() && AIO_Func.getHealthPercent(Player) < 80 //args.Target.IsMe && AIO_Menu.Champion.Misc.getBoolValue("R Myself Only")
                && Player.Distance(args.End) < 150 && AIO_Menu.Champion.Combo.UseW)
                W.Cast();
            if (!sender.IsAlly && W.IsReady() && AIO_Func.getHealthPercent(Player) < 80 && Player.Distance(args.End) < 150 &&
                Sender.Distance(Player.ServerPosition) <= 1000f && AIO_Menu.Champion.Combo.UseW)
                W.Cast();
            if (!sender.IsAlly && W.IsReady() && AIO_Func.getHealthPercent(Player) < 80 &&
                Sender.Distance(Player.ServerPosition) <= 700f && AIO_Menu.Champion.Combo.UseW)
                W.Cast();
        }
        
        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.Cast();
            }
        }
        
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (R.CanCast(target) && AIO_Func.isKillable(target, R))
                    AIO_Func.LCast(R,target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);
            
            if (R.IsReady())
                damage += R.GetDamage2(enemy);
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
                
            return damage;
        }
    }
}
