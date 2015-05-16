using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class MasterYi
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        
        static Spell Q, W, E, R;

        static void Wcancel() { Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos); }

        static float getPBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "doublestrike"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getRBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "Highlander"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 600f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            Q.SetTargetted(0.25f, float.MaxValue);

            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addItem("R Timer", new Circle(true, Color.SpringGreen));
            AIO_Menu.Champion.Drawings.addItem("P Timer", new Circle(true, Color.SpringGreen));

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

            Orbwalker.SetAttack(Player.IsTargetable);

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
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
            var drawRTimer = AIO_Menu.Champion.Drawings.getCircleValue("R Timer");
            var drawPTimer = AIO_Menu.Champion.Drawings.getCircleValue("P Timer");
            var pos_temp = Drawing.WorldToScreen(Player.Position);
            
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (drawRTimer.Active && getRBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawRTimer.Color, "R: " + getRBuffDuration.ToString("0.00"));
            if (drawPTimer.Active && getPBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawPTimer.Color, "P: " + getPBuffDuration.ToString("0.00"));
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || Player.IsDead)
                return;

            if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed) && args.Target.Type != GameObjectType.obj_AI_Minion)
            {
                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.W).Name && HeroManager.Enemies.Any(x => x.IsValidTarget(1000)))
                {
                    if (AIO_Menu.Champion.Combo.UseW || AIO_Menu.Champion.Harass.UseW)
                    {
                        Utility.DelayAction.Add(50, Orbwalking.ResetAutoAttackTimer2);
                        Utility.DelayAction.Add(50, Wcancel);
                    }
                }

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo &&
                args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.Q).Name)
                {
                    Utility.DelayAction.Add(250, Orbwalking.ResetAutoAttackTimer2);

                    if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
                        E.Cast();
                }
            }

        }

        static void AA()
        {
            if (HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                AIO_Func.AACb(W);

            AIO_Func.AACb(R);
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
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady() && !Player.IsWindingUp)
                Q.CastOnBestTarget();

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady() && HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                E.Cast();
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady() && !Player.IsWindingUp)
                Q.CastOnBestTarget();
        }

        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady() && !Player.IsWindingUp)
                Q.Cast(Minions.FirstOrDefault());
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady() && !Player.IsWindingUp)
                Q.Cast(Mobs.FirstOrDefault());

            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady() && Mobs.Any(x => Orbwalking.InAutoAttackRange(x)))
                E.Cast();
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
                
            if (E.IsReady())
                damage += E.GetDamage2(enemy);
                
            if (getPBuffDuration > 0)
                damage += (float)Player.GetAutoAttackDamage2(enemy, false) / 2;
                
            if (W.IsReady())
                damage += (float)Player.GetAutoAttackDamage2(enemy, false);
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
                
            return damage;
        }
    }
}

