using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

/*
MasteryBladeWeaving 무기연성
MasterySpellWeaving 주문연성 (평타시)
ElixirOfWrath 분노의영약
grievouswound 고통스런 상처
missfortunepassivestack 미포 w패시브
*/


namespace ALL_In_One.champions
{
    class MissFortune// By RL244 WIP
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;
        

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 650f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 800f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 1400f, TargetSelector.DamageType.Physical);

            
            Q.SetTargetted(0.25f, 1400f);
            E.SetSkillshot(0.25f, 150f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 60f * (float)Math.PI / 180, 2000f, false, SkillshotType.SkillshotCone);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW(false);
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ(false);
            AIO_Menu.Champion.Laneclear.addUseW(false);
            AIO_Menu.Champion.Laneclear.addUseE(false);
            AIO_Menu.Champion.Laneclear.addIfMana();
            
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();

            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", false); // 궁 킬딸 딱히 권장안함.
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.AfterAttack += Orbwalking_OnAfterAttack;
            //Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(35))
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Orbwalker.SetAttack(true);
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Orbwalker.SetAttack(true);
                        Harass();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        Orbwalker.SetAttack(true);
                        Laneclear();
                        Jungleclear();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        Orbwalker.SetAttack(true);
                        break;
                }
            }

            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();
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
            var drawR = AIO_Menu.Champion.Drawings.Rrange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
        
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);

        }

        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (E.IsReady()
                && Player.Distance(gapcloser.Sender.Position) <= E.Range)
                E.Cast((SharpDX.Vector3)gapcloser.End);
        }

        static void AA() // 챔피언 대상 평캔 ( 빼낸 이유는 AA방식 두개로 할시 두번 적어야 해서 단순화하기 위함.
        {
            AIO_Func.AACb(Q);
            AIO_Func.AACb(W);
        }
        
        static void Orbwalking_OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || (Target == null))
                return;
                
            AIO_Func.AALcJc(Q);
            AIO_Func.AALcJc(W);
            
            if(!utility.Activator.AfterAttack.AIO)
            AA();
        }

        
        static void Combo()
        {

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(E.Range, E.DamageType);
                AIO_Func.CCast(E,Etarget);
            }

        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                AIO_Func.CCast(E,Etarget);
            }

        }

        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;
        
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady() && Minions.Any(x => x.IsValidTarget(E.Range)))
                AIO_Func.CCast(E,Minions[0]);
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;
        
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;
            
            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady() && Mobs.Any(x=>x.IsValidTarget(E.Range)))
                AIO_Func.CCast(E,Mobs[0]);
        }

        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                Q.Cast(target);
            }
        }
        
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (target.IsValidTarget(R.Range) && AIO_Func.isKillable(target, R))
                AIO_Func.CCast(R,target);
            }
        }
        
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy) + (float)Player.GetAutoAttackDamage2(enemy, true);

            if (W.IsReady())
                damage += W.GetDamage2(enemy);
                
            if (E.IsReady())
                damage += E.GetDamage2(enemy);

            if (R.IsReady())
                damage += R.GetDamage2(enemy);
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
            return damage;
        }
    }
}
