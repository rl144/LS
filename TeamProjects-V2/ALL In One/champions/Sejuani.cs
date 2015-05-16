using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Sejuani// By RL244 sejuanipassivedisplay
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float QD = 0f;
        static float getPBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "sejuanipassivedisplay"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getWBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "SejuaniNorthernWinds"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static int RM {get{return Menu.Item("Misc.RM").GetValue<Slider>().Value; }}

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 650f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 350f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 1000f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 1175f, TargetSelector.DamageType.Magical);

            Q.SetSkillshot(0f, 70f, 1600f, false, SkillshotType.SkillshotLine); // No Delay
            R.SetSkillshot(0.26f, 110f, 1600f, false, SkillshotType.SkillshotLine);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();
            Menu.SubMenu("Combo").AddItem(new MenuItem("Misc.RM", "R Min Targets")).SetValue(new Slider(1, 0, 5));
            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();
            
            AIO_Menu.Champion.Lasthit.addUseE();
            AIO_Menu.Champion.Lasthit.addIfMana(20);
            
            AIO_Menu.Champion.Laneclear.addUseQ(false);
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();
            
            AIO_Menu.Champion.Flee.addUseE();
            AIO_Menu.Champion.Flee.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", false); //궁킬딸..ㅋㅋㅋㅋ 매우 비추천하긴함
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();
            AIO_Menu.Champion.Drawings.addItem("W Timer", new Circle(true, Color.LightGreen));
            AIO_Menu.Champion.Drawings.addItem("P Timer", new Circle(true, Color.LightGreen));

        
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(35))
            {
                AIO_Func.FleeToPosition(Q);
                AIO_Func.SC(Q,QD);
                if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                {
                    foreach (var target in HeroManager.Enemies.Where(x => x.HasBuff("sejuanifrost")))
                    {
                        if(target.Distance(Player.ServerPosition) <= E.Range && E.IsReady() && target != null)
                        AIO_Func.SC(E);
                    }
                }
                if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    var EMinion = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).Where(x => x.HasBuff("sejuanifrost"));
                    foreach (var target in EMinion)
                    {
                        if(target.Distance(Player.ServerPosition) <= E.Range && E.IsReady() && target != null && EMinion.Count() > 3)
                        AIO_Func.SC(E);
                    }
                }
                if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseR)
                {
                    foreach (var target in HeroManager.Enemies.Where(x => AIO_Func.ECTarget(x,600f) >= RM && x.Distance(Player.ServerPosition) > 300f).OrderByDescending(x => x.Health))
                    {
                        if (R.CanCast(target) && AIO_Func.isKillable(target, R) && target != null)
                            AIO_Func.LCast(R,target,QD);
                    }
                }
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();
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
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            var drawWTimer = AIO_Menu.Champion.Drawings.getCircleValue("W Timer");
            var drawPTimer = AIO_Menu.Champion.Drawings.getCircleValue("P Timer");
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
            if (drawPTimer.Active && getPBuffDuration > 0)
            Drawing.DrawText(pos_temp[0], pos_temp[1], drawPTimer.Color, "P: " + getPBuffDuration.ToString("0.00"));
        }
        
        static void AA()
        {
            if(!Player.HasBuff("sejuaninorthernwindsenrage"))
            AIO_Func.AACb(W);
        }
        
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || Target == null)
                return;
            if(!Player.HasBuff("sejuaninorthernwindsenrage"))
            AIO_Func.AALcJc(W);
            if(!utility.Activator.AfterAttack.AIO)
            AA();
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
                    AIO_Func.LCast(R,target,QD);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);
            
            if (W.IsReady())
                damage += W.GetDamage2(enemy);
            
            if (E.IsReady())
                damage += E.GetDamage2(enemy) + (float)Player.GetAutoAttackDamage2(enemy, false);
                
            if (R.IsReady())
                damage += R.GetDamage2(enemy);
                
            return damage;
        }
    }
}
