﻿using System;
using System.Linq;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Collision = LeagueSharp.Common.Collision;

namespace RLProject.Champions
{
    public static class Kalista
    {
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Orbwalking.Orbwalker Orbwalker { get { return RLProject.Orbwalker; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1180f);
            W = new Spell(SpellSlot.W, 5200f);
            E = new Spell(SpellSlot.E, 1000f);
            R = new Spell(SpellSlot.R, 1400f);

            Q.SetSkillshot(0.25f, 30f, 1700f, true, SkillshotType.SkillshotLine);

            var drawDamageMenu = new MenuItem("Draw_RDamage", "Draw (E) Damage", true).SetValue(true);
            var drawFill = new MenuItem("Draw_Fill", "Draw (E) Damage Fill", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));

            RLProject.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseQ", "Use Q", true).SetValue(true));
            RLProject.Menu.SubMenu("Combo").AddItem(new MenuItem("comboUseE", "Use E", true).SetValue(true));

            RLProject.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseQ", "Use Q", true).SetValue(true));
            RLProject.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseE", "Use E", true).SetValue(true));
            RLProject.Menu.SubMenu("Harass").AddItem(new MenuItem("harassMana", "if Mana % >", true).SetValue(new Slider(50, 0, 100)));

            RLProject.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearUseQ", "Use Q", true).SetValue(true));
            RLProject.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearQnum", "Cast Q If Can Kill Minion Number >=", true).SetValue(new Slider(3, 1, 5)));
            RLProject.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearUseE", "Use E", true).SetValue(true));
            RLProject.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearEnum", "Cast E If Can Kill Minion Number >=", true).SetValue(new Slider(2, 1, 5)));
            RLProject.Menu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            RLProject.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseQ", "Use Q", true).SetValue(true));
            RLProject.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseE", "Use E", true).SetValue(true));
            RLProject.Menu.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearMana", "if Mana % >", true).SetValue(new Slider(20, 0, 100)));

            RLProject.Menu.SubMenu("Misc").AddItem(new MenuItem("killsteal", "Use Killsteal (With E)", true).SetValue(true));
            RLProject.Menu.SubMenu("Misc").AddItem(new MenuItem("mobsteal", "Use Mobsteal (With E)", true).SetValue(true));
            RLProject.Menu.SubMenu("Misc").AddItem(new MenuItem("lasthitassist", "Use Lasthit Assist (With E)", true).SetValue(true));
            RLProject.Menu.SubMenu("Misc").AddItem(new MenuItem("soulboundsaver", "Use Soulbound Saver (With R)", true).SetValue(true));

            RLProject.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingAA", "Real AA Range", true).SetValue(new Circle(true, Color.FromArgb(0, 230, 255))));
            RLProject.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingQ", "Q Range", true).SetValue(new Circle(true, Color.FromArgb(0, 230, 255))));
            RLProject.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingW", "W Range", true).SetValue(new Circle(false, Color.FromArgb(0, 230, 255))));
            RLProject.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingE", "E Range", true).SetValue(new Circle(true, Color.FromArgb(0, 230, 255))));
            RLProject.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawingR", "R Range", true).SetValue(new Circle(false, Color.FromArgb(0, 230, 255))));

            RLProject.Menu.SubMenu("Drawings").AddItem(drawDamageMenu);
            RLProject.Menu.SubMenu("Drawings").AddItem(drawFill);

            DamageIndicator.DamageToUnit = GetComboDamage;
            DamageIndicator.Enabled = drawDamageMenu.GetValue<bool>();
            DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
            DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;

            drawDamageMenu.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            drawFill.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Orbwalking.OnNonKillableMinion += Orbwalking_OnNonKillableMinion;
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
            Killsteal();
            Mobsteal();
        }
		
		static float ehrs = Eharrass();
		
		static float Eharrass()
        {
            int lvl = Player.Level;
            int eharrass = (40 + 15 * lvl);
            return eharrass;
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

            if (Q.IsReady() && drawingQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawingQ.Color);

            if (W.IsReady() && drawingW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawingW.Color);

            if (E.IsReady() && drawingE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawingE.Color);

            if (R.IsReady() && drawingR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawingR.Color);
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "KalistaExpungeWrapper")
                    Utility.DelayAction.Add(250, Orbwalking.ResetAutoAttackTimer);

            if (RLProject.Menu.Item("soulboundsaver", true).GetValue<Boolean>() && R.IsReady())
            {
                if (sender.Type == GameObjectType.obj_AI_Hero && sender.IsEnemy)
                {
                    var soulboundhero = HeroManager.Allies.FirstOrDefault(hero => hero.HasBuff("kalistacoopstrikeally", true) && args.Target.NetworkId == hero.NetworkId && hero.HealthPercentage() <= 15);

                    if (R.CanCast(soulboundhero))
                        R.Cast();
                }
            }
        }

        static void Killsteal()
        {
            if (!RLProject.Menu.Item("killsteal", true).GetValue<Boolean>() || !E.IsReady())
                return;

            var target = HeroManager.Enemies.FirstOrDefault(x => !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield) && E.CanCast(x) && (x.Health + (x.HPRegenRate / 2)) <= E.GetDamage(x));

            if (E.CanCast(target))
                E.Cast();
        }

        static void Mobsteal()
        {
            if (!RLProject.Menu.Item("mobsteal", true).GetValue<Boolean>() || !E.IsReady())
                return;

            var Mob = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.Health + (x.HPRegenRate / 2) <= E.GetDamage(x));

            if (E.CanCast(Mob))
                E.Cast();

            var Minion = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.Health <= E.GetDamage(x) && (x.SkinName.ToLower().Contains("siege") || x.SkinName.ToLower().Contains("super")));

            if (E.CanCast(Minion))
                E.Cast();
        }

        static float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (E.IsReady())
                damage += E.GetDamage(enemy);

            return damage;
        }

        static List<Obj_AI_Base> Q_GetCollisionMinions(Obj_AI_Hero source, Vector3 targetposition)
        {
            var input = new PredictionInput
            {
                Unit = source,
                Radius = Q.Width,
                Delay = Q.Delay,
                Speed = Q.Speed,
            };

            input.CollisionObjects[0] = CollisionableObjects.Minions;

            return Collision.GetCollision(new List<Vector3> { targetposition }, input).OrderBy(obj => obj.Distance(source, false)).ToList();
        }

        static void Orbwalking_OnNonKillableMinion(AttackableUnit minion)
        {
            if (!RLProject.Menu.Item("lasthitassist", true).GetValue<Boolean>())
                return;

            if (E.CanCast((Obj_AI_Base)minion) && minion.Health <= E.GetDamage((Obj_AI_Base)minion))
                E.Cast();
        }

        static void Combo()
        {
            if (!Orbwalking.CanMove(1))
                return;

            if (RLProject.Menu.Item("comboUseQ", true).GetValue<Boolean>())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical, true);

                if (Q.CanCast(Qtarget) && Q.GetPrediction(Qtarget).Hitchance >= HitChance.VeryHigh && !Player.IsWindingUp && !Player.IsDashing())
                    Q.Cast(Qtarget);
            }

            if (RLProject.Menu.Item("comboUseE", true).GetValue<Boolean>() && E.IsReady())
            {
                var Minion = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy).Where(x => x.Health <= E.GetDamage(x)).OrderBy(x => x.Health).FirstOrDefault();
                var Target = HeroManager.Enemies.Where(x => E.CanCast(x) && E.GetDamage(x) >= 1 && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield)).OrderByDescending(x => E.GetDamage(x)).FirstOrDefault();

                if (Target.Health <= E.GetDamage(Target) || (E.CanCast(Minion) && E.CanCast(Target)))
                    E.Cast();
            }
        }

        static void Harass()
        {
            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > RLProject.Menu.Item("harassMana", true).GetValue<Slider>().Value))
                return;

            if (RLProject.Menu.Item("harassUseQ", true).GetValue<Boolean>())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical, true);

                if (Q.CanCast(Qtarget) && Q.GetPrediction(Qtarget).Hitchance >= HitChance.VeryHigh && !Player.IsWindingUp && !Player.IsDashing())
                    Q.Cast(Qtarget);
            }

            if (RLProject.Menu.Item("harassUseE", true).GetValue<Boolean>() && E.IsReady())
            {
                var EMinion = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy).Where(x => x.Health <= E.GetDamage(x)).OrderBy(x => x.Health).FirstOrDefault();
                var ETarget = HeroManager.Enemies.Where(x => E.CanCast(x) && E.GetDamage(x) >= ehrs && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield)).OrderByDescending(x => E.GetDamage(x)).FirstOrDefault();
		
                if (ETarget.Health <= E.GetDamage(ETarget) || (E.CanCast(EMinion) && E.CanCast(ETarget)))
                    E.Cast();
				/*	
				else if (ETarget.Health > E.GetDamage(ETarget) && E.CanCast(ETarget)) 
				{
					var predE = 10 + 10 * E.Level + 0.6 * (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod);
					foreach (var minion in
                        ObjectManager.Get<Obj_AI_Minion>()
							.Where(
								minion =>
									minion.IsValidTarget() && !E.CanCast(minion) && Player.Distance(minion.Position) >= 550 &&
									minion.Health <
									(predE + 2 *
									(ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod)))
						)
					{
						var t = (int) (Player.AttackCastDelay * 1000) - 100 + Game.Ping / 2 +
								1000 * (int) Player.Distance(minion.ServerPosition) / (int) Orbwalking.GetMyProjectileSpeed();
						var predHealth = HealthPrediction.GetHealthPrediction(minion, t, 0);
						if (minion.Team != GameObjectTeam.Neutral && MinionManager.IsMinion(minion, true))
						{
							if (predHealth > predE && predHealth <= Player.CalcDamage(minion, Damage.DamageType.Physical, predE) + Player.GetAutoAttackDamage(minion, true))
							{
							Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
							}
						}
					}
				}
				
				*/
            }
			
        }

        static void Laneclear()
        {
            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > RLProject.Menu.Item("laneclearMana", true).GetValue<Slider>().Value))
                return;

            var Minions = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (RLProject.Menu.Item("laneclearUseQ", true).GetValue<Boolean>() && Q.IsReady())
            {
                //-------------------------------------------------------------------------------------------------------------------------------
                foreach (var minion in Minions.Where(x => x.Health <= Q.GetDamage(x)))
                {
                    var killcount = 0;

                    foreach (var colminion in Q_GetCollisionMinions(Player, Player.ServerPosition.Extend(minion.ServerPosition, Q.Range)))
                    {
                        if (colminion.Health <= Q.GetDamage(colminion))
                            killcount++;
                        else
                            break;
                    }

                    if (killcount >= RLProject.Menu.Item("laneclearQnum", true).GetValue<Slider>().Value)
                    {
                        if (!Player.IsWindingUp && !Player.IsDashing())
                        { 
                            Q.Cast(minion);
                            break;
                        }
                    }
                }
                //-------------------------------------------------------------------------------------------------------------------------------
            }

            if (RLProject.Menu.Item("laneclearUseE", true).GetValue<Boolean>() && E.IsReady())
            {
                var minionkillcount = 0;

                foreach (var Minion in Minions.Where(x => E.CanCast(x) && x.Health <= E.GetDamage(x))){minionkillcount++;}

                if (minionkillcount >= RLProject.Menu.Item("laneclearEnum", true).GetValue<Slider>().Value)
                    E.Cast();
            }

            if (RLProject.Menu.Item("harassUseE", true).GetValue<Boolean>() && E.IsReady())
            {
                var EMinion = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy).Where(x => x.Health <= E.GetDamage(x)).OrderBy(x => x.Health).FirstOrDefault();
                var ETarget = HeroManager.Enemies.Where(x => E.CanCast(x) && E.GetDamage(x) >= ehrs && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield)).OrderByDescending(x => E.GetDamage(x)).FirstOrDefault();

                if (ETarget.Health <= E.GetDamage(ETarget) || (E.CanCast(EMinion) && E.CanCast(ETarget)))
                    E.Cast();
            }
        }

        static void Jungleclear()
        {
            if (!Orbwalking.CanMove(1) || !(Player.ManaPercentage() > RLProject.Menu.Item("jungleclearMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (RLProject.Menu.Item("jungleclearUseQ", true).GetValue<Boolean>() && Q.CanCast(Mobs[0]))
                Q.Cast(Mobs[0]);

            if (RLProject.Menu.Item("jungleclearUseE", true).GetValue<Boolean>() && E.CanCast(Mobs[0]))
            {
                if (Mobs[0].Health + (Mobs[0].HPRegenRate/2) <= E.GetDamage(Mobs[0]))
                    E.Cast();
            }
        }
    }
}
