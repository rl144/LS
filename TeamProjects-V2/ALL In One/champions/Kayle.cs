using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Kayle// By RL244 
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float getRBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "JudicatorRighteousFury"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getEBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "JudicatorRighteousFury"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getWBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "JudicatorIntervention"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float WM {get{return Menu.Item("Misc.WM").GetValue<Slider>().Value; }}
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 650f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 900f);
            E = new Spell(SpellSlot.E, 525f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 900f);

            Q.SetTargetted(0.25f, 1400f);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ(false);
            AIO_Menu.Champion.Laneclear.addUseE(false);
            AIO_Menu.Champion.Laneclear.addIfMana();
            
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();
            
            AIO_Menu.Champion.Flee.addUseW();
            AIO_Menu.Champion.Flee.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("Auto W", true);
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.WM", "W If Mana >")).SetValue(new Slider(40, 0, 100));
            //AIO_Menu.Champion.Misc.addItem("R Myself Only", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();
            AIO_Menu.Champion.Drawings.addItem("W Timer", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addItem("E Timer", new Circle(true, Color.Blue));
            AIO_Menu.Champion.Drawings.addItem("R Timer", new Circle(true, Color.LightGreen));
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(10))
            {
                AIO_Func.SC(Q);
                AIO_Func.SC(E);
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    Combo();
            }

            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("Auto W"))
                AIO_Func.Heal(W,WM);
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            var drawWTimer = AIO_Menu.Champion.Drawings.getCircleValue("W Timer");
            var drawETimer = AIO_Menu.Champion.Drawings.getCircleValue("E Timer");
            var drawRTimer = AIO_Menu.Champion.Drawings.getCircleValue("R Timer");
            var pos_temp = Drawing.WorldToScreen(Player.Position);
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
            if (drawWTimer.Active && getWBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawWTimer.Color, "W: " + getWBuffDuration.ToString("0.00"));
            if (drawETimer.Active && getEBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawETimer.Color, "E: " + getEBuffDuration.ToString("0.00"));
            if (drawRTimer.Active && getRBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawRTimer.Color, "R: " + getRBuffDuration.ToString("0.00"));
        }

        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (Q.IsReady()
                && Player.Distance(gapcloser.Sender.Position) <= Q.Range)
                Q.Cast(gapcloser.Sender);
        }
        
        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var Sender = (Obj_AI_Base) sender;
            var STarget = (Obj_AI_Hero) args.Target;
            if (!sender.IsMe || Player.IsDead) // 
                return;
            if (args.Target.IsMe && !sender.IsAlly && R.IsReady() && AIO_Func.getHealthPercent(Player) < 25 //args.Target.IsMe && AIO_Menu.Champion.Misc.getBoolValue("R Myself Only")
                && Player.Distance(args.End) < 150 && AIO_Menu.Champion.Combo.UseR)
                R.Cast(Player);
            if (!sender.IsAlly && R.IsReady() && AIO_Func.getHealthPercent(Player) < 25 && Player.Distance(args.End) < 150 &&
                Sender.Distance(Player.ServerPosition) <= 1000f && AIO_Menu.Champion.Combo.UseR)
                R.Cast(Player);
            if (!sender.IsAlly && R.IsReady() && AIO_Func.getHealthPercent(Player) < 15 &&
                Sender.Distance(Player.ServerPosition) <= 700f && AIO_Menu.Champion.Combo.UseR)
                R.Cast(Player);
        }
    
        static void Combo()
        {
            var Target = TargetSelector.GetTarget(Q.Range, Q.DamageType);
            if (AIO_Menu.Champion.Combo.UseW && W.IsReady() && Target != null)
            {
                W.Cast(Player);
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
        
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);
                
            if (E.IsReady())
                damage += (E.GetDamage2(enemy) + (float)Player.GetAutoAttackDamage2(enemy, true))*2;
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
            return damage;
        }
    }
}
