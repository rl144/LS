using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Evelynn// By RL244 evelynnpassive evelynnrshield evelynnstealthmarker EvelynnW
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}} //
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        //static int RM = 1;  안씀
        static float getPBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "evelynnstealthmarker"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getWBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "EvelynnW"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getRBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "evelynnrshield"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 500f); // Q.Speed = 2000f(에서 거리에 따라 점점 감소)
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 225f, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R, 650f, TargetSelector.DamageType.Magical);

            E.SetTargetted(0.25f, float.MaxValue);
            R.SetSkillshot(0.25f, 250f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();
            //Menu.SubMenu("Combo").AddItem(new MenuItem("ComboRM", "R Min Targets", true).SetValue(new Slider(1, 0, 5)));

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana(30);

            AIO_Menu.Champion.Lasthit.addUseQ();
            AIO_Menu.Champion.Lasthit.addIfMana(10);
            
            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana(20);


            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();
            
            AIO_Menu.Champion.Flee.addUseW();
            AIO_Menu.Champion.Flee.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();
            AIO_Menu.Champion.Drawings.addItem("P Timer", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addItem("W Timer", new Circle(true, Color.LightGreen));
            AIO_Menu.Champion.Drawings.addItem("R Timer", new Circle(true, Color.LightGreen));  
            
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(10))
            {
                AIO_Func.SC(R);
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

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
            #endregion
            
            #region AfterAttack
            AIO_Func.AASkill(E);
            if(AIO_Func.AfterAttack())
            AA();
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
        var drawPTimer = AIO_Menu.Champion.Drawings.getCircleValue("P Timer");
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
        if (drawPTimer.Active && getPBuffDuration > 0)
            Drawing.DrawText(pos_temp[0], pos_temp[1], drawPTimer.Color, "P: " + getPBuffDuration.ToString("0.00"));
        if (drawRTimer.Active && getRBuffDuration > 0)
            Drawing.DrawText(pos_temp[0], pos_temp[1], drawRTimer.Color, "R: " + getRBuffDuration.ToString("0.00"));

        }
        
        static void AA() // 챔피언 대상 평캔 ( 빼낸 이유는 AA방식 두개로 할시 두번 적어야 해서 단순화하기 위함.
        {
            AIO_Func.AACb(E);
        }
        
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || Target == null)
                return;

            AIO_Func.AALcJc(E);
            
            if(!utility.Activator.AfterAttack.AIO)
            AA();
        }
        
        static void Combo()
        {
           // RM = Menu.Item("ComboRM").GetValue<Slider>().Value;

            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.Range - 10, R.DamageType, true);
                if(qTarget.Distance(Player.ServerPosition) <= Q.Range - 10)
                Q.Cast();
            }
            
            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                var ETarget = TargetSelector.GetTarget(E.Range, E.DamageType, true);
                if (ETarget.Distance(Player.ServerPosition) >= Orbwalking.GetRealAutoAttackRange(Player) + 40)
                E.Cast(ETarget);
            }
            
            //if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            //   R.CastIfWillHit(R.GetTarget(), RM);
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;
                
            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.Range - 10, R.DamageType, true);
                if(qTarget.Distance(Player.ServerPosition) <= Q.Range - 10)
                Q.Cast();
            }
                
            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                var ETarget = TargetSelector.GetTarget(E.Range, E.DamageType, true);
                if (ETarget.Distance(Player.ServerPosition) >= Orbwalking.GetRealAutoAttackRange(Player) + 40)
                E.Cast(ETarget);
            }

        }
        
        static void Lasthit()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Lasthit.IfMana))
                return;

            if(AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            AIO_Func.LH(Q);
        }
        
        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(500f, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count > 0)
            {
            if(AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            Q.Cast();
            }
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(500f, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count > 0)
            {
            if(AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            Q.Cast();
            }
        }

        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.Cast();
            }
        }
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if(E.CanCast(target) && AIO_Func.isKillable(target, E))
                E.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy) * 2;

            if (E.IsReady())
                damage += E.GetDamage2(enemy);
                
            if (R.IsReady())
                damage += R.GetDamage2(enemy);
                
            return damage;
        }
    }
}
