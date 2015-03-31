﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using System.Collections.Generic;
using Collision = LeagueSharp.Common.Collision;

namespace RLProject.Champions
{
    public static class Sona
    {
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Orbwalking.Orbwalker Orbwalker { get { return RLProject.Orbwalker; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 850f);
            W = new Spell(SpellSlot.W, 1000f);
            E = new Spell(SpellSlot.E, 350f);
            R = new Spell(SpellSlot.R, 1000f);

            Q.SetSkillshot(0.25f, 15f * (float)Math.PI / 180, 2000f, false, SkillshotType.SkillshotCone);
            W.SetSkillshot(0.25f, 250f, 1650f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 100f, 2100f, false, SkillshotType.SkillshotLine);

            var drawDamageMenu = new MenuItem("Draw_RDamage", "Draw (R) Damage", true).SetValue(true);
            var drawFill = new MenuItem("Draw_Fill", "Draw (R) Damage Fill", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));

            RLProject.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseQ", "Use Q", true).SetValue(true));
            RLProject.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseW", "Use W", true).SetValue(true));
            RLProject.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseR", "Use R", true).SetValue(true));

            RLProject.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseQ", "Use Q", true).SetValue(true));
            RLProject.Menu.SubMenu("Harass").AddItem(new MenuItem("harassMana", "if Mana % >", true).SetValue(new Slider(50, 0, 100)));

            RLProject.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearUseQ", "Use Q", true).SetValue(true));
            RLProject.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            RLProject.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseQ", "Use Q", true).SetValue(true));
            RLProject.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearMana", "if Mana % >", true).SetValue(new Slider(20, 0, 100)));

            RLProject.Menu.SubMenu("Misc").AddItem(new MenuItem("empty1", "Empty", true));

            RLProject.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingAA", "Real AA Range", true).SetValue(new Circle(true, Color.FromArgb(183, 0, 0))));
            RLProject.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingQ", "Q Range", true).SetValue(new Circle(true, Color.FromArgb(183, 0, 0))));
            RLProject.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingW", "W Range", true).SetValue(new Circle(true, Color.FromArgb(183, 0, 0))));
            RLProject.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingE", "E Range", true).SetValue(new Circle(false, Color.FromArgb(183, 0, 0))));
            RLProject.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingR", "R Range", true).SetValue(new Circle(true, Color.FromArgb(183, 0, 0))));

            RLProject.Menu.SubMenu("Drawings").AddItem(drawDamageMenu);
            RLProject.Menu.SubMenu("Drawings").AddItem(drawFill);

            DamageIndicator.DamageToUnit = GetComboDamage;
            DamageIndicator.Enabled = drawDamageMenu.GetValue<Boolean>();
            DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
            DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;

            drawDamageMenu.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Enabled = eventArgs.GetNewValue<Boolean>();
            };

            drawFill.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

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

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawingAA = RLProject.Menu.Item("drawingAA", true).GetValue<Circle>();
            var drawingQ = RLProject.Menu.Item("drawingQ", true).GetValue<Circle>();
            var drawingW = RLProject.Menu.Item("drawingW", true).GetValue<Circle>();
            var drawingE = RLProject.Menu.Item("drawingE", true).GetValue<Circle>();
            var drawingR = RLProject.Menu.Item("drawingR", true).GetValue<Circle>();

            if (drawingAA.Active)
                Render.Circle.DrawCircle(Player.Position, Orbwalking.GetRealAutoAttackRange(Player), drawingAA.Color);

            if (drawingQ.Active && Q.IsReady())
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawingQ.Color);

            if (drawingW.Active && W.IsReady())
                Render.Circle.DrawCircle(Player.Position, W.Range, drawingW.Color);

            if (drawingE.Active && E.IsReady())
                Render.Circle.DrawCircle(Player.Position, E.Range, drawingE.Color);

            if (drawingR.Active && R.IsReady())
                Render.Circle.DrawCircle(Player.Position, R.Range, drawingR.Color);

        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (target.Type != GameObjectType.obj_AI_Hero)
                return;

            var Target = (Obj_AI_Base)target;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (RLProject.Menu.Item("comboUseQ", true).GetValue<Boolean>() && Q.CanCast(Target) && !Player.IsDashing())
                    Q.Cast(Target);
                
                if (RLProject.Menu.Item("comboUseW", true).GetValue<Boolean>() && W.CanCast(Target))
                    W.Cast(Target);
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && Player.ManaPercentage() > RLProject.Menu.Item("harassMana", true).GetValue<Slider>().Value)
            {
                if (RLProject.Menu.Item("harassUseQ", true).GetValue<Boolean>() && Q.CanCast(Target) && !Player.IsDashing())
                    Q.Cast(Target);
            }
        }

        static bool CollisionCheck(Obj_AI_Hero source, Obj_AI_Hero target, float width)
        {
            var input = new PredictionInput
            {
                Radius = width,
                Unit = source,
            };

            input.CollisionObjects[0] = CollisionableObjects.Heroes;
            input.CollisionObjects[1] = CollisionableObjects.YasuoWall;

            return !Collision.GetCollision(new List<Vector3> { target.ServerPosition }, input).Where(x => x.NetworkId != x.NetworkId).Any();
        }

        static float GetComboDamage(Obj_AI_Base enemy)
        {
            return R.IsReady() ? R.GetDamage(enemy) : 0;
        }

        static void Combo()
        {
            if (RLProject.Menu.Item("comboUseR", true).GetValue<Boolean>() && R.IsReady())
            {
                var Rtarget = HeroManager.Enemies.Where(x => R.CanCast(x) && x.Health + (x.HPRegenRate / 2) <= R.GetDamage(x) && R.GetPrediction(x).Hitchance >= HitChance.VeryHigh).OrderByDescending(x => x.Health).FirstOrDefault();

                if (R.CanCast(Rtarget))
                    R.Cast(Rtarget);
            }

            if (!Orbwalking.CanMove(1))
                return;

            if (RLProject.Menu.Item("comboUseQ", true).GetValue<Boolean>() && Q.IsReady())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical, true);

                if (Q.CanCast(Qtarget) && !Player.IsDashing())
                    Q.Cast(Qtarget);
            }

            if (RLProject.Menu.Item("comboUseW", true).GetValue<Boolean>() && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical, true);

                if (Q.CanCast(Wtarget))
                    Q.Cast(Wtarget);
            }
        }

        static void Harass()
        {
            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > RLProject.Menu.Item("harassMana", true).GetValue<Slider>().Value))
                return;

            if (RLProject.Menu.Item("harassUseQ", true).GetValue<Boolean>())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical, true);

                if (Q.CanCast(Qtarget))
                    Q.Cast(Qtarget);
            }
        }

        static void Laneclear()
        {
            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > RLProject.Menu.Item("laneclearMana", true).GetValue<Slider>().Value))
                return;

            var Minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (RLProject.Menu.Item("laneclearUseQ", true).GetValue<Boolean>() && Q.IsReady())
            {
                var Farmloc = Q.GetLineFarmLocation(Minions);

                if (Farmloc.MinionsHit >= 6)
                    Q.Cast(Farmloc.Position);
            }
        }

        static void Jungleclear()
        {
            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > RLProject.Menu.Item("jungleclearMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (RLProject.Menu.Item("jungleclearUseQ", true).GetValue<Boolean>() && Q.IsReady())
            {
                if(Q.CanCast(Mobs[0]))
                    Q.Cast(Mobs[0]);
            }
        }
    }
}
