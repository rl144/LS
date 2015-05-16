using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Ashe// By RL244 Q Waiting For Remake. 리메이크 수치 확인용으로 일단 대기. asheqcastready AsheQ(<- Q스택 수) ashepassiveslow(target) asheqattack asheqbuff
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float WD {get{return Menu.Item("Misc.Wtg").GetValue<Slider>().Value; }}
        static float getQBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "asheqattack"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 650f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 1200f, TargetSelector.DamageType.Physical);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 20000f, TargetSelector.DamageType.Magical);

            W.SetSkillshot(0.25f, 70f, 1500f, true, SkillshotType.SkillshotLine); // 투과 안되는 Cone이므로 충돌 판단을 쉽게 하기 위해 스킬타입 Line인 것처럼 설정. 실제 게임상 문제없음.
            R.SetSkillshot(0.25f, 130f, 1600f, true, SkillshotType.SkillshotLine);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ(false);
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addIfMana();
            
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addIfMana();
            
            AIO_Menu.Champion.Flee.addUseW();
            AIO_Menu.Champion.Flee.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Wtg", "Additional Range")).SetValue(new Slider(0, 0, 250));
            AIO_Menu.Champion.Misc.addItem("KillstealW", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter(false);
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addItem("Q Timer", new Circle(true, Color.LightPink));

        
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(35))
            {
                if(Player.HasBuff("asheqcastready"))
                AIO_Func.SC(Q);
                AIO_Func.SC(W,WD,0f);
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    Combo();
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealW"))
                KillstealW();
            #endregion
            #region AfterAttack
            AIO_Func.AASkill(W);
            if(AIO_Func.AfterAttack())
            AA();
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawQTimer = AIO_Menu.Champion.Drawings.getCircleValue("Q Timer");
            var pos_temp = Drawing.WorldToScreen(Player.Position);
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (drawQTimer.Active && getQBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawQTimer.Color, "Q Buff : " + getQBuffDuration.ToString("0.00"));
        }
        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (R.CanCast(gapcloser.Sender))
                AIO_Func.LCast(R,gapcloser.Sender,0f,0f);
        }
        
        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (R.CanCast(sender))
                AIO_Func.LCast(R,sender,0f,0f);
        }
        
        static void AA()
        {
            AIO_Func.AACb(W);
        }
        
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || Target == null)
                return;
            AIO_Func.AALcJc(W);
            if(!utility.Activator.AfterAttack.AIO)
            AA();
        }
        
        static void Combo()
        {            
            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
                {
                    if (R.CanCast(target) && AIO_Func.isKillable(target, getComboDamage(target) * 2) && target.Distance(Player.ServerPosition) < 1000)
                        AIO_Func.LCast(R,target,0f,0f);
                    else if (R.CanCast(target) && AIO_Func.isKillable(target, R) && target.Distance(Player.ServerPosition) < 3000)
                        AIO_Func.LCast(R,target,0f,0f);
                }
            }
        }
        
        static void KillstealW()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (W.CanCast(target) && AIO_Func.isKillable(target, W))
                    AIO_Func.LCast(W,target,0);
            }
        }
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += (float)Player.GetAutoAttackDamage2(enemy, false)*2;
            
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
