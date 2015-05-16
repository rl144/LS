using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Orianna
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        static SharpDX.Vector3 BallPosition;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 825f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 225f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 1120f);
            R = new Spell(SpellSlot.R, 380f) { Delay = 0.25f};

            Q.SetSkillshot(0f, 100f, 1200f, false, SkillshotType.SkillshotCircle);

            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();
            AIO_Menu.Champion.Combo.addItem("R Min Targets", new Slider(1, 2, 5));

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addIfMana();
            
            AIO_Menu.Champion.Flee.addUseW();
            AIO_Menu.Champion.Flee.addUseE();
            AIO_Menu.Champion.Flee.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addUseKillsteal();
            AIO_Menu.Champion.Misc.addUseInterrupter();
            AIO_Menu.Champion.Misc.addItem("Auto-E", true);

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
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
                        Laneclear();
                        Jungleclear();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        break;
                }
            }

            if (AIO_Menu.Champion.Misc.UseKillsteal)
                Killsteal();

            Q.MinHitChance = AIO_Menu.Champion.Misc.SelectedHitchance;

            Q.UpdateSourcePosition(BallPosition);
            W.UpdateSourcePosition(BallPosition, BallPosition);
            R.UpdateSourcePosition(BallPosition, BallPosition);

            if (Player.HasBuff("orianaghostself", true))
            {
                BallPosition = ObjectManager.Player.Position;
                return;
            }

            var ballowner = HeroManager.Allies.FirstOrDefault(x => x.IsAlly && !x.IsMe && x.HasBuff("orianaghost", true));

            if (ballowner != null)
                BallPosition = ballowner.Position;
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
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color, 3);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(BallPosition, W.Range, drawW.Color, 3);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color, 3);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(BallPosition, R.Range, drawR.Color, 3);

            Render.Circle.DrawCircle(BallPosition, 50, Color.LightSkyBlue, 3);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (Q.IsReady())
                Q.Cast(sender);

            if (R.CanCast(sender) && args.DangerLevel == Interrupter2.DangerLevel.High)
                R.Cast();
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.IsDead)
                return;

            if (AIO_Menu.Champion.Misc.getBoolValue("Auto-E") && sender.IsEnemy && args.Target.IsAlly && args.Target.Type == GameObjectType.obj_AI_Hero && sender.Type == GameObjectType.obj_AI_Hero && E.IsReady())
                E.CastOnUnit((Obj_AI_Hero)args.Target);

            if (sender.IsMe && args.SData.Name == Q.Instance.Name)
            {
                Utility.DelayAction.Add((int)(BallPosition.Distance(args.End) / 1.2 - 70 - Game.Ping), () => BallPosition = args.End);
                BallPosition = SharpDX.Vector3.Zero;
            }

            if (sender.IsMe && args.SData.Name == E.Instance.Name)
                BallPosition = SharpDX.Vector3.Zero;
        }

        static void Combo()
        {
            if (E.IsReady() && Player.Distance(BallPosition) >= 500)
                E.CastOnUnit(Player);

            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
                Q.CastOnBestTarget(0f, false, true);

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            {
                if (AIO_Func.SelfAOE_Prediction.HitCount(0f, W.Range, BallPosition) >= 1)
                    W.Cast();
            }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                if (AIO_Func.CollisionCheck(BallPosition, Player, 70f))
                    E.CastOnUnit(Player);
            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                if (AIO_Func.SelfAOE_Prediction.HitCount(R.Delay, R.Range, BallPosition) >= AIO_Menu.Champion.Combo.getSliderValue("R Min Targets").Value)
                    R.Cast();
            }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (E.IsReady() && Player.Distance(BallPosition) >= 500)
                E.CastOnUnit(Player);

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
                Q.CastOnBestTarget(0f, false, true);

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            {
                if (AIO_Func.SelfAOE_Prediction.HitCount(0f, W.Range, BallPosition) >= 1)
                    W.Cast();
            }

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                if (AIO_Func.CollisionCheck(BallPosition, Player, 70f))
                    E.CastOnUnit(Player);
            }
        }

        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
                var Qloc = Q.GetCircularFarmLocation(Minions.Where(x=>x.IsValidTarget(Q.Range)).ToList());

                if(Qloc.MinionsHit >= 2)
                    Q.Cast(Qloc.Position);
            }

            if (AIO_Menu.Champion.Laneclear.UseW && W.IsReady())
            {
                if (Minions.Count(x => x.IsValidTarget(W.Range, true, BallPosition)) >= 3)
                    W.Cast();
            }
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                var Qloc = Q.GetCircularFarmLocation(Mobs.Where(x => x.IsValidTarget(Q.Range)).ToList());

                if (Qloc.MinionsHit >= 1)
                    Q.Cast(Qloc.Position);
            }

            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady())
            {
                if (Mobs.Count(x => x.IsValidTarget(W.Range, true, BallPosition)) >= 1)
                    W.Cast();
            }
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.Cast(target);

                if (W.CanCast(target) && AIO_Func.isKillable(target, W))
                    W.Cast(target);

                if (R.CanCast(target) && AIO_Func.isKillable(target, R.GetDamage2(target) + (Q.IsReady() ? Q.GetDamage2(target) : 0)))
                    R.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy) * 2;

            if (W.IsReady())
                damage += W.GetDamage2(enemy);

            if (R.IsReady())
                damage += R.GetDamage2(enemy);

            if(!Player.IsWindingUp)
            damage += (float)Player.GetAutoAttackDamage2(Player) * 2;

            return damage;
        }
    }
}
