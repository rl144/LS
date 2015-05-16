using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class DrMundo// By RL244 mundodododododododododo
    {
        static Menu Menu { get { return AIO_Menu.MainMenu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float QD = 25f;
        
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1000f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 190f, TargetSelector.DamageType.Magical); // 실제 범위는 162.5이지만 상대 챔피언의ㅏ width 덕분에 조금 더 멀리있어도 데미지를 입음. 1티모미터는 75f정도.
            E = new Spell(SpellSlot.E, Orbwalking.GetRealAutoAttackRange(Player) + 32.5f, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R){Delay = 0.25f};

            Q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine); 
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            
            AIO_Menu.Champion.Lasthit.addUseQ();
            
            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            
            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("AutoR", true);
            AIO_Menu.Champion.Drawings.addQrange();

        
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(10))
            {
                AIO_Func.SC(Q,QD,0f,0);
                if(!Player.HasBuff("BurningAgony"))
                AIO_Func.SC(W,0,0,0);
                AIO_Func.SC(E,0,0,0);
                WOff();
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            #endregion
            if (AIO_Menu.Champion.Misc.getBoolValue("AutoR"))
                AutoR();

        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
        }
        
        static void AutoR()
        {
            if(AIO_Func.getHealthPercent(Player) < 20 && R.IsReady())
            R.Cast();
        }
        
        static void WOff()
        {
            if(Player.HasBuff("BurningAgony"))
            {
            var Target = TargetSelector.GetTarget(W.Range, W.DamageType);
            if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None || (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed) && Target == null)
            W.Cast();
            }
        }
        
        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    AIO_Func.LCast(Q,target,QD,0f);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);
            
            if (W.IsReady())
                damage += W.GetDamage2(enemy);
                
            return damage;
        }
    }
}
