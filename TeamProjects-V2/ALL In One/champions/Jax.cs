using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Jax// By RL244
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}} //
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;

        
        static float getQBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "TalonNoxianDiplomacyBuff"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 700f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 187.5f, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R);

            E.SetTargetted(0.25f, float.MaxValue);
            
            AIO_Menu.Champion.Combo.addUseQ();
            Menu.SubMenu("Combo").AddItem(new MenuItem("ComboQD", "Q Min Distance", true).SetValue(new Slider(160, 0, 700)));
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ(false);
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();


            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addUseKillsteal();
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addItem("Q Timer", new Circle(true, Color.LightGreen));

            
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(10))
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    Combo();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                    Harass();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    Laneclear();
                    Jungleclear();
                }
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.UseKillsteal)
            Killsteal();
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
        var drawE = AIO_Menu.Champion.Drawings.Erange;
        var drawQTimer = AIO_Menu.Champion.Drawings.getCircleValue("Q Timer");
    
        if (Q.IsReady() && drawQ.Active)
        Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
    
        if (E.IsReady() && drawE.Active)
        Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

        var pos_temp = Drawing.WorldToScreen(Player.Position);
        
        if (drawQTimer.Active && getQBuffDuration > 0)
        Drawing.DrawText(pos_temp[0], pos_temp[1], drawQTimer.Color, "Q: " + getQBuffDuration.ToString("0.00"));
        
        }
        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;


        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;


        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || Player.IsDead)
                return;
                

        }
        
        static void AA() // 챔피언 대상 평캔 ( 빼낸 이유는 AA방식 두개로 할시 두번 적어야 해서 단순화하기 위함.
        {
            AIO_Func.AACb(W);
        }
        
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || Target == null)
                return;

            AIO_Func.AALcJc(W);
            AIO_Func.AALcJc(E);

            if(!utility.Activator.AfterAttack.AIO)
            AA();
        }

        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                var QD = Menu.Item("ComboQD", true).GetValue<Slider>().Value;
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);
                if(qTarget.Distance(Player.Position) > QD)
                Q.Cast(qTarget);
            }
            
            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                var eTarget = TargetSelector.GetTarget(E.Range, E.DamageType, true);
                if (eTarget != null && !Player.IsDashing())
                    E.Cast(eTarget);       
            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady() && (AIO_Func.EnemyCount(1000, 10, 100) >= 2 || AIO_Func.EnemyCount(1000, 10, 100) == 1 && AIO_Func.getHealthPercent(Player) < 50))
            {
                R.Cast();
            }
                
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;
                
            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
                var QD = Menu.Item("ComboQD", true).GetValue<Slider>().Value;
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);
                if(qTarget.Distance(Player.Position) > QD)
                Q.Cast(qTarget);
            }
                
            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                var eTarget = TargetSelector.GetTarget(E.Range, E.DamageType, true);
                if (eTarget != null && !Player.IsDashing())
                    E.Cast(eTarget);       
            }

        }
        
        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;
                
            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
                Q.Cast(Minions[0]);
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;
            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
                Q.Cast(Mobs[0]);

        }

        static void Killsteal()
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
                damage += Q.GetDamage2(enemy)+(float)Player.GetAutoAttackDamage2(enemy, true);
            
            if (W.IsReady())
                damage += W.GetDamage2(enemy) + (float)Player.GetAutoAttackDamage2(enemy, true);

            if (R.IsReady() && AIO_Menu.Champion.Combo.UseR)
                damage += R.GetDamage2(enemy);

            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
                
            return damage;
        }
    }
}
