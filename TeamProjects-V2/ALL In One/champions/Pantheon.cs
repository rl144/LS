using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Pantheon// By RL244 pantheonpassiveshield pantheonpassivecounter
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E;
        
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 600f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 600f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 500f, TargetSelector.DamageType.Physical); //실제 사거리는 600이지만 더 줄여서 씀.

            E.SetSkillshot(1.0f, 60f * (float)Math.PI / 180, float.MaxValue, false, SkillshotType.SkillshotCone);
            W.SetTargetted(0.25f, 1400f);
            Q.SetTargetted(0.25f, 1400f);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();
            
            AIO_Menu.Champion.Lasthit.addUseQ(false);
            AIO_Menu.Champion.Lasthit.addIfMana(20);
            
            AIO_Menu.Champion.Laneclear.addUseQ(false);
            AIO_Menu.Champion.Laneclear.addUseE(false);
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();

        
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(35))
            {
                AIO_Func.SC(Q);
                if(Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                AIO_Func.SC(W);
                else if(AIO_Menu.Champion.Combo.UseW && W.IsReady())
                {
                    foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
                    {
                        if (target != null && W.CanCast(target) && (target.Distance(Player.ServerPosition) > 400 || AIO_Func.getHealthPercent(Player) < 50 && Player.HasBuff("pantheonpassiveshield")))
                            W.Cast(target);
                    }
                }
                if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && (AIO_Menu.Champion.Combo.UseQ && !Q.IsReady() || !AIO_Menu.Champion.Combo.UseQ) || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && (AIO_Menu.Champion.Harass.UseQ && !Q.IsReady() || !AIO_Menu.Champion.Harass.UseQ) || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && (AIO_Menu.Champion.Jungleclear.UseQ && !Q.IsReady() || !AIO_Menu.Champion.Jungleclear.UseQ) && (AIO_Menu.Champion.Jungleclear.UseW && !W.IsReady() || !AIO_Menu.Champion.Jungleclear.UseW))
                AIO_Func.SC(E);
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
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
                
            if (W.IsReady())
                damage += W.GetDamage2(enemy);
            
            if (E.IsReady())
                damage += E.GetDamage2(enemy)*6;
            
            //if (R.IsReady()) 판테 궁으로 킬을 어떻게함 -_-;
            //    damage += R.GetDamage2(enemy);
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
                
            return damage;
        }
    }
}
