using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class MonkeyKing
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 365f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 650f, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R, 315f, TargetSelector.DamageType.Physical) { Delay = 0.25f };

            E.SetTargetted(0.25f, 2200f);

            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();
            AIO_Menu.Champion.Combo.addItem("Cast R if Enemy number >=", new Slider(2, 1, 5));

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();
            
            AIO_Menu.Champion.Laneclear.addUseQ(false); // AIO_Func.AALcJc(Q)를 위해서 추가함. 사용자에 따라 평캔 원할수도 있으니까.. 기본은 false로
            AIO_Menu.Champion.Laneclear.addIfMana(); // 이하동일
            
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();
            
            AIO_Menu.Champion.Flee.addUseW();
            AIO_Menu.Champion.Flee.addIfMana();

            AIO_Menu.Champion.Misc.addUseKillsteal();
            AIO_Menu.Champion.Misc.addUseInterrupter();

            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(100))
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Harass();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        Jungleclear();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        break;
                }
            }
            #region Killsteal
            if (AIO_Menu.Champion.Misc.UseKillsteal)
                Killsteal();
            #endregion
            #region AfterAttack
            AIO_Func.AASkill(Q); // 액티베이터에서 아이템 - 스킬 중 선으로 쓸 것을 선택할수 있도록 Q스킬이 AA용 스킬임을 인식시킴. 이걸 해야 평q히드라평 콤보가 가능. 기본은 평히드라평q
            if(AIO_Func.AfterAttack()) // 지금은 딱히 필요없을수도 있지만.. 암튼 무기연성
            AA();
            #endregion

            Orbwalker.SetAttack(!Player.HasBuff("MonkeyKingSpinToWin", true));
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color, 3);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color, 3);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (R.CanCast(sender) && args.DangerLevel == Interrupter2.DangerLevel.High)
                CastR1();
        }
        
        static void AA()
        {
            AIO_Func.AACb(Q); // AIO_Func 참고.. 딱히 길게 할 필요 없을것 같아서 짧게 줄였습니다.
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe || !target.IsValidTarget())
                return;

            AIO_Func.AALcJc(Q); //정글 클리어 or 라인 클리어 Q 사용 설정시 Q로 평캔.

            if(!utility.Activator.AfterAttack.AIO) //무기연성 방식이 아닐 경우.
            AA();
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && (args.SData.Name == Q.Instance.SData.Name || args.SData.Name == E.Instance.SData.Name))
                Orbwalking.ResetAutoAttackTimer2();
        }

        static void CastR1()
        {
            if (R.Instance.Name == "MonkeyKingSpinToWin" && R.IsReady())
                R.Cast();
        }

        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
                E.CastOnBestTarget();

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady() && !Q.IsReady() && !E.IsReady() && !Player.HasBuff("MonkeyKingDoubleAttack", true))
            {
                if (AIO_Func.SelfAOE_Prediction.HitCount(R.Delay, R.Range) >= AIO_Menu.Champion.Combo.getSliderValue("Cast R if Enemy number >=").Value)
                    CastR1();
            }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
                E.CastOnBestTarget();
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;
// AA에 AIO_Func.AALcJc(Q)를 추가했기 때문에 여기에 추가 안해도 정글에서 Q로 평캔합니다.
            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
            {
                if (E.CanCast(Mobs[0]))
                    E.Cast(Mobs[0]);
            }
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                    E.Cast(target);

                if (R.CanCast(target) && AIO_Func.isKillable(target, R.GetDamage2(target) * 4))
                    CastR1();
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);

            if (E.IsReady())
                damage += E.GetDamage2(enemy);

            if (R.IsReady())
                damage += R.GetDamage2(enemy) * 4;

            return damage;
        }
    }
}
