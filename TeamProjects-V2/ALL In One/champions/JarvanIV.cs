using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class JarvanIV// By RL244 jarvanivmartialcadence jarvanivdemacianstandardbuff jarvanivmartialcadencecheck (target)jarvanivdragonstrikedebuff jarvanivdragonstrikeph2 (target) JarvanIVGoldenAegis JarvanIVCataclysm
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float QD {get{return Menu.Item("Misc.Qtg").GetValue<Slider>().Value; }}
        static float getEBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "jarvanivdemacianstandardbuff"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getWBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "JarvanIVGoldenAegis"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getRBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "JarvanIVCataclysm"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 770f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 525f, TargetSelector.DamageType.Physical);
            E = new Spell(SpellSlot.E, 830f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 650f, TargetSelector.DamageType.Physical);

            Q.SetSkillshot(0.6f, 70f, float.MaxValue, false, SkillshotType.SkillshotLine); //EQ는 1450f속도임(자르반 날아가는속도)
            E.SetSkillshot(0.5f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetTargetted(0.25f, float.MaxValue);
            
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW(false);
            AIO_Menu.Champion.Harass.addUseE(false);
            AIO_Menu.Champion.Harass.addIfMana();
            
            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW(false);
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();
            
            AIO_Menu.Champion.Flee.addUseQ();
            AIO_Menu.Champion.Flee.addUseE();
            AIO_Menu.Champion.Flee.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Qtg", "Additional Qrange")).SetValue(new Slider(25, 0, 150));
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();
            AIO_Menu.Champion.Drawings.addItem("W Timer", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addItem("E Timer", new Circle(true, Color.LightGreen));
            AIO_Menu.Champion.Drawings.addItem("R Timer", new Circle(true, Color.LightGreen));  
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(10))
            {
                if(Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo || !E.IsReady())
                    AIO_Func.SC(Q,QD);
                AIO_Func.SC(W);
                AIO_Func.SC(E);
                AIO_Func.FleeToPosition(E);
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    Combo();
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
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            var drawWTimer = AIO_Menu.Champion.Drawings.getCircleValue("W Timer");
            var drawETimer = AIO_Menu.Champion.Drawings.getCircleValue("E Timer");
            var drawRTimer = AIO_Menu.Champion.Drawings.getCircleValue("R Timer");
            var pos_temp = Drawing.WorldToScreen(Player.Position);
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
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
        
        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || Player.IsDead)
                return;

            if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseQ || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && AIO_Menu.Champion.Harass.UseQ))
            {
                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.E).Name && HeroManager.Enemies.Any(x => x.IsValidTarget(Q.Range)))
                {
                    var Qtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                    AIO_Func.LCast(Q,Qtarget,QD,float.MaxValue);
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Flee)
            {
                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.E).Name)
                {
                    AIO_Func.FleeToPosition(Q);
                }
            }


        }
        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                var Rtarget = TargetSelector.GetTarget(R.Range, R.DamageType);
                if(!Player.HasBuff("JarvanIVCataclysm") && AIO_Func.isKillable(Rtarget, (((AIO_Func.getManaPercent(Player) > 15) ? Q.GetDamage2(Rtarget) : (Q.IsReady() ? Q.GetDamage2(Rtarget) : 0)) + R.GetDamage2(Rtarget)))) //R.Instance.Name == "JarvanIVCataclysm"
                R.Cast(Rtarget);
            }
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
                if (R.CanCast(target) && AIO_Func.isKillable(target, R) && !Player.HasBuff("JarvanIVCataclysm"))
                    R.Cast(target);
            }
        }
        
        static float JarvanPDamage(Obj_AI_Base enemy) //Code Made By RL244. 
        {
            return (float)Damage.CalcDamage(Player,enemy, Damage.DamageType.Physical, 
            (float)(!enemy.HasBuff("jarvanivmartialcadencecheck") ? (enemy.Health)*0.1d : 0));
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
                
            if (!enemy.HasBuff("jarvanivmartialcadencecheck"))
                damage += JarvanPDamage(enemy) + (float)Player.GetAutoAttackDamage2(enemy, false);
                
            return damage;
        }
    }
}
