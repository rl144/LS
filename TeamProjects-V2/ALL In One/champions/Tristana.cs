using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Tristana// By RL244 TristanaQ tristanaecharge(target) tristanawslow(target) tristanavomanykills tristanaechargesound(<- E 폭발시간)
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static float getQBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "TristanaQ"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static Spell Q, W, E, R;
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 900f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 700f, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R, 700f, TargetSelector.DamageType.Magical);

            W.SetSkillshot(0.5f, 250f, 1500f, false, SkillshotType.SkillshotCircle);
            E.SetTargetted(0.25f, 1400f);
            R.SetTargetted(0.25f, 1400f);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW(false);
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR(false);

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW(false);
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana(40);

            AIO_Menu.Champion.Laneclear.addUseQ(false);
            AIO_Menu.Champion.Laneclear.addUseW(false);
            AIO_Menu.Champion.Laneclear.addUseE(false);
            AIO_Menu.Champion.Laneclear.addIfMana(40);
            
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW(false);
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();
            
            AIO_Menu.Champion.Flee.addUseW();
            AIO_Menu.Champion.Flee.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();

            AIO_Menu.Champion.Misc.addItem("KillstealW", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter(false);

            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addItem("Q Timer", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addItem("E Timer", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_OnAfterAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            //Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
                
            E.Range = Orbwalking.GetRealAutoAttackRange(Player);
            R.Range = Orbwalking.GetRealAutoAttackRange(Player);

            if (Orbwalking.CanMove(10))
            {
                AIO_Func.FleeToPosition(W);
                foreach (var target in HeroManager.Enemies.Where(x => AIO_Func.CanHit(W,x,0)&& (float)AIO_Func.getBuffInstance(x, "tristanaecharge").Count > 2 && x.HasBuff("tristanaecharge") && (AIO_Func.getBuffInstance(x, "tristanaechargesound").EndTime - Game.ClockTime) > 0.6))
                {
                    if(target != null && W.IsReady())
                    AIO_Func.SC(W);
                }
            }

            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealW"))
                KillstealW();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();
                
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

            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawQTimer = AIO_Menu.Champion.Drawings.getCircleValue("Q Timer");
            var drawETimer = AIO_Menu.Champion.Drawings.getCircleValue("E Timer");
            var pos_temp = Drawing.WorldToScreen(Player.Position);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (drawQTimer.Active && getQBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawQTimer.Color, "Q: " + getQBuffDuration.ToString("0.00"));
            foreach (var target in HeroManager.Enemies.Where(x => x.HasBuff("tristanaechargesound")))
            {
                if(target != null)
                {
                    float getENuffDuration = (target.HasBuff("tristanaechargesound") ? AIO_Func.getBuffInstance(target, "tristanaechargesound").EndTime - Game.ClockTime : 0);
                    var pos_temp2 = Drawing.WorldToScreen(target.Position);
                    
                    if (drawETimer.Active && getENuffDuration > 0)
                        Drawing.DrawText(pos_temp[0], pos_temp2[1], drawETimer.Color, "E: " + getENuffDuration.ToString("0.00"));
                }
            }
        }

        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (R.CanCast(gapcloser.Sender))
                R.Cast(gapcloser.Sender);
        }
        
        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (R.CanCast(sender))
                R.Cast(sender);
        }

        static void Orbwalking_OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || (Target == null))
                return;

            AIO_Func.AALcJc(Q);
            AIO_Func.AALcJc(E);
            
            if(!utility.Activator.AfterAttack.AIO)
            AA();
        }
        
        static void AA()
        {
            var Target = TargetSelector.GetTarget(Orbwalking.GetRealAutoAttackRange(Player), E.DamageType);
            AIO_Func.AACb(Q);
            AIO_Func.AACb(E);
            if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseR ||
            Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && AIO_Menu.Champion.Harass.UseR && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana)
            && R.IsReady() && Target.Health + Target.HPRegenRate <= R.GetDamage2(Target)+ (float)Player.GetAutoAttackDamage2(Target, true))
            { // 평-R-평 => Kill
                R.Cast(Target);
            }
        }
        
        static void KillstealW()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if(W.IsReady() && target != null)
                {
                    var Buff = (target.HasBuff("tristanaechargesound") ? AIO_Func.getBuffInstance(target, "tristanaecharge") : null);
                    bool EK = (target.HasBuff("tristanaechargesound") && (float)Buff.Count > 0 && AIO_Func.isKillable(target, E.GetDamage2(target)*(((float)Buff.Count-1)*0.30f+1f)) || !target.HasBuff("tristanaechargesound"));
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Buff != null && !EK && W.CanCast(target) && R.IsReady() && AIO_Menu.Champion.Misc.getBoolValue("KillstealR") && target.HasBuff("tristanaechargesound") && (AIO_Func.getBuffInstance(target, "tristanaechargesound").EndTime - Game.ClockTime) > 0.6 && (float)Buff.Count > 0 && AIO_Func.isKillable(target, R.GetDamage2(target) + W.GetDamage2(target)*(((float)Buff.Count-1)*0.25f+1f) + E.GetDamage2(target)*(((float)Buff.Count-1)*0.25f+1f) + (float)Player.GetAutoAttackDamage2(target, true)))
                    AIO_Func.CCast(W,target);
                    if (W.CanCast(target) && target.HasBuff("tristanaechargesound") && (AIO_Func.getBuffInstance(target, "tristanaechargesound").EndTime - Game.ClockTime) > 0.6 && Buff != null && !EK && (float)Buff.Count > 0 && AIO_Func.isKillable(target, W.GetDamage2(target)*(((float)Buff.Count-1)*0.25f+1f) + E.GetDamage2(target)*(((float)Buff.Count-1)*0.30f+1f) + (float)Player.GetAutoAttackDamage2(target, true)))
                    AIO_Func.CCast(W,target);
                    else if (W.CanCast(target) && !EK && AIO_Func.isKillable(target, W.GetDamage2(target) + (float)Player.GetAutoAttackDamage2(target, true)))
                    AIO_Func.CCast(W,target);
                }
            }
        }

        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E) && E.IsReady() && target != null)
                E.Cast(target);
            }
        }
        
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if(R.IsReady() && target != null)
                {
                    var Buff = (target.HasBuff("tristanaechargesound") ? AIO_Func.getBuffInstance(target, "tristanaecharge") : null);
                    bool EK = (target.HasBuff("tristanaechargesound") && (float)Buff.Count > 0 && AIO_Func.isKillable(target, E.GetDamage2(target)*(((float)Buff.Count-1)*0.30f+1f)) || !target.HasBuff("tristanaechargesound"));
                    if (R.CanCast(target) && Buff != null && (float)Buff.Count > 0 && target.HasBuff("tristanaechargesound") && (AIO_Func.getBuffInstance(target, "tristanaechargesound").EndTime - Game.ClockTime) > 0.3 && AIO_Func.isKillable(target, R.GetDamage2(target) + E.GetDamage2(target)*(((float)Buff.Count-1)*0.30f+1f)) && !EK)
                        R.Cast(target);
                    else if (R.CanCast(target) && AIO_Func.isKillable(target, R) && !EK)
                        R.Cast(target);
                }
            }
        }
        
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;
            var Buff = AIO_Func.getBuffInstance(enemy, "tristanaecharge");
            if (Q.IsReady())
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);

            if (W.IsReady())
                damage += W.GetDamage2(enemy);//((float)Buff.Count > 0 && enemy.HasBuff("tristanaechargesound") ? W.GetDamage2(enemy)*(((float)Buff.Count-1)*0.25f+1f) : W.GetDamage2(enemy));
                
            if (E.IsReady())
                damage += E.GetDamage2(enemy);//((float)Buff.Count > 0 && enemy.HasBuff("tristanaechargesound") ? E.GetDamage2(enemy)*(((float)Buff.Count-1)*0.30f+1f) : E.GetDamage2(enemy));

            if (R.IsReady())
                damage += R.GetDamage2(enemy);
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
            return damage;
        }
    }
}
