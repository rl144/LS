using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Gangplank // By RL244
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}} //
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float getEBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "RaiseMoraleBuff"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 625f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1200f);
            R = new Spell(SpellSlot.R, 40000f, TargetSelector.DamageType.Magical);

            Q.SetTargetted(0.25f, float.MaxValue);
            R.SetSkillshot(0.25f, 575f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();
            
            AIO_Menu.Champion.Flee.addUseE();
            AIO_Menu.Champion.Flee.addIfMana();

            AIO_Menu.Champion.Misc.addUseKillsteal();
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Misc.addItem("Cleanse(W)", true);
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addItem("E Timer", new Circle(true, Color.LightGreen));
            
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
                if(AIO_Func.UnitIsImmobileUntil(Player) > 0.5 && AIO_Menu.Champion.Misc.getBoolValue("Cleanse(W)") && W.IsReady())
                W.Cast();
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.UseKillsteal)
            Killsteal();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
            KillstealR();
            #endregion
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
        var drawE = AIO_Menu.Champion.Drawings.Erange;
        var drawETimer = AIO_Menu.Champion.Drawings.getCircleValue("E Timer");
        
        if (Q.IsReady() && drawQ.Active)
        Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
        
        if (E.IsReady() && drawE.Active)
        Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

        var pos_temp = Drawing.WorldToScreen(Player.Position);
        
        if (drawETimer.Active && getEBuffDuration > 0)
        Drawing.DrawText(pos_temp[0], pos_temp[1], drawETimer.Color, "E: " + getEBuffDuration.ToString("0.00"));
        
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || Player.IsDead)
                return;
                

        }
        
        static void AA()
        {
            AIO_Func.AACb(Q);
        }
        
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || Target == null)
                return;
            if(!utility.Activator.AfterAttack.AIO)
                AA();
        }

        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);
                if (qTarget != null)
                E.Cast();
            }

            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
               
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);
      
                if (qTarget != null)
                    Q.Cast(qTarget);       
            }
            
            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                var rTarget = TargetSelector.GetTarget(Q.Range, R.DamageType, true);
                if (rTarget != null && AIO_Func.getHealthPercent(rTarget) <= 45)
                R.Cast(rTarget.Position);
            }
                
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;
                
            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
               
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);

        
                if (qTarget != null)
                    Q.Cast(qTarget);       
            }

        }
        
        static void AALaneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;
                
                //if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady() && utility.Activator.AfterAttack.ALLCancelItemsAreCasted)
                //    Q.Cast(Minions[0]);
        }

        static void AAJungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;
                
            //if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady() && utility.Activator.AfterAttack.ALLCancelItemsAreCasted)
            //    Q.Cast(Mobs[0]);
        }


        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

                
            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
                var _m = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.Health < ((Player.GetSpellDamage(m, SpellSlot.Q))));
      
                if (_m != null)
                    Q.Cast(_m);       
            }
                
            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
                E.Cast();
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                var _m = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.Health < ((Player.GetSpellDamage(m, SpellSlot.Q))));
      
                if (_m != null)
                    Q.Cast(_m);       
            }
            
            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
                E.Cast();
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.Cast(target);
            }
        }
        
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.Where(x => x.Distance(Player.ServerPosition) > 1000f).OrderByDescending(x => x.Health))
            {
                if (R.CanCast(target) && R.GetDamage2(target)*((R.Width/(target.MoveSpeed*0.8f))+3f) >= target.Health + target.HPRegenRate
                    && target.Distance(Player.ServerPosition) > 1200f && HeroManager.Allies.Where(x => x.Distance(target.ServerPosition) <= 500).Count() > 2)
                    R.Cast(target.ServerPosition);
                else if (R.CanCast(target) && R.GetDamage2(target)*((R.Width/(target.MoveSpeed*0.8f))+2f) >= target.Health + target.HPRegenRate
                    && target.Distance(Player.ServerPosition) > 1200f && HeroManager.Allies.Where(x => x.Distance(target.ServerPosition) <= 500).Count() > 1)
                    R.Cast(target.ServerPosition);
                else if (R.CanCast(target) && R.GetDamage2(target)*((R.Width/(target.MoveSpeed*0.8f))+1f) >= target.Health + target.HPRegenRate
                    && target.Distance(Player.ServerPosition) > 1200f && HeroManager.Allies.Where(x => x.Distance(target.ServerPosition) <= 500).Count() > 0)
                    R.Cast(target.ServerPosition);
                else if (R.CanCast(target) && R.GetDamage2(target)*((R.Width/(target.MoveSpeed*0.8f))) >= target.Health + target.HPRegenRate
                    && target.Distance(Player.ServerPosition) > 1200f && HeroManager.Allies.Where(x => x.Distance(target.ServerPosition) <= 500).Count() == 0)
                    R.Cast(target.ServerPosition);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);

            if (R.IsReady() && AIO_Menu.Champion.Combo.UseR)
                damage += R.GetDamage2(enemy)*((R.Width/(enemy.MoveSpeed*0.8f))+1f);

            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);

            return damage;
        }
    }
}
