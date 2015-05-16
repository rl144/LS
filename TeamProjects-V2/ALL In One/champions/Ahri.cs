using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Ahri// By RL244
    {
        static Menu Menu { get { return AIO_Menu.MainMenu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float QD = 25f;
        static bool RA {get{return Menu.Item("Combo.Use R").GetValue<KeyBind>().Active; }}
        
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 880f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 550f, TargetSelector.DamageType.Magical){Delay = 0.25f};
            E = new Spell(SpellSlot.E, 975f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 450f, TargetSelector.DamageType.Magical); //이동거리는 450이지만 데미지는 600까지 줌

            Q.SetSkillshot(0.25f, 100f, 1600f, false, SkillshotType.SkillshotLine); // 450~2500까지 증가하는 아리의 미사일.
            E.SetSkillshot(0.25f, 60f, 1550f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.25f, 600f, 1600f, false, SkillshotType.SkillshotCircle); // Circular Prediction을 이용하는게 좋음.
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            //AIO_Menu.Champion.Combo.addUseR(); 아래의 토글로 대체
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo.Use R", "Use R")).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle, true));

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE(false);
            AIO_Menu.Champion.Harass.addIfMana();
            
            AIO_Menu.Champion.Lasthit.addUseQ();
            AIO_Menu.Champion.Lasthit.addUseE(false);
            AIO_Menu.Champion.Lasthit.addIfMana(20);
            
            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseE(false);
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();
            
            AIO_Menu.Champion.Flee.addUseQ();
            AIO_Menu.Champion.Flee.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();

        
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(35))
            {
                AIO_Func.SC(Q,QD);
                AIO_Func.SC(W);
                AIO_Func.SC(E,QD,0);
                //AIO_Func.MouseSC(R); <- 이거 안써도 될듯.
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    Combo();
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
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (Q.IsReady()
                && Player.Distance(gapcloser.Sender.Position) <= Q.Range)
                AIO_Func.LCast(Q,gapcloser.Sender,QD); //W.Cast((Vector3)gapcloser.End);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (E.IsReady()
            && Player.Distance(sender.Position) <= E.Range)
                AIO_Func.LCast(E,sender);
        }
        
        static void Combo()
        {            
            if (RA && R.IsReady())
            {
                foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
                {
                    if (R.CanCast(target) && AIO_Func.isKillable(target, (Q.IsReady() ? Q.GetDamage2(target) : 0) + (W.IsReady() ? W.GetDamage2(target) : 0)
                    + (E.IsReady() ? E.GetDamage2(target) : 0) +(R.IsReady() ? R.GetDamage2(target)*3 : 0)) && target.Distance(Player.ServerPosition) < 1000 && target.Distance(Game.CursorPos) < 600f)
                        R.Cast(Game.CursorPos);
                    else if (R.CanCast(target) && AIO_Func.isKillable(target, R.GetDamage2(target)*2) && target.Distance(Player.ServerPosition) < 900 && target.Distance(Game.CursorPos) < 600f)
                        R.Cast(Game.CursorPos);
                }
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

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy) + Q.GetDamage2(enemy,1);
            
            if (W.IsReady())
                damage += W.GetDamage2(enemy);
            
            if (E.IsReady())
                damage += E.GetDamage2(enemy) + (float)Player.GetAutoAttackDamage2(enemy, false);
                
            if (R.IsReady())
                damage += R.GetDamage2(enemy)*2;
                
            return damage;
        }
    }
}
