using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Kalista
    {
        static Menu Menu { get { return AIO_Menu.MainMenu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1150f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 5000f);
            E = new Spell(SpellSlot.E, 1000f, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R, 1500f);

            Q.SetSkillshot(0.25f, 40f, 1200f, true, SkillshotType.SkillshotLine);

            Menu.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("comboUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("comboUseE", "Use E", true).SetValue(true));

            Menu.SubMenu("Champion").SubMenu("Harass").AddItem(new MenuItem("harassUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Harass").AddItem(new MenuItem("HrsMana", "if Mana % >", true).SetValue(new Slider(50, 0, 100)));

            Menu.SubMenu("Champion").SubMenu("Laneclear").AddItem(new MenuItem("laneclearUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Laneclear").AddItem(new MenuItem("laneclearQnum", "Cast Q If Can Kill Minion Number >=", true).SetValue(new Slider(3, 1, 5)));
            Menu.SubMenu("Champion").SubMenu("Laneclear").AddItem(new MenuItem("laneclearUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Laneclear").AddItem(new MenuItem("laneclearEnum", "Cast E If Can Kill Minion Number >=", true).SetValue(new Slider(2, 1, 5)));
            Menu.SubMenu("Champion").SubMenu("Laneclear").AddItem(new MenuItem("laneclearMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Champion").SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearMana", "if Mana % >", true).SetValue(new Slider(20, 0, 100)));

            Menu.SubMenu("Champion").SubMenu("Misc").AddItem(new MenuItem("killsteal", "Use Killsteal (With E)", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Misc").AddItem(new MenuItem("mobsteal", "Use Mobsteal (With E)", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Misc").AddItem(new MenuItem("lasthitassist", "Use Lasthit Assist (With E)", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Misc").AddItem(new MenuItem("soulboundsaver", "Use Soulbound Saver (With R)", true).SetValue(true));

            Menu.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Q Range", true).SetValue(new Circle(true, Color.FromArgb(0, 230, 255))));
            Menu.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("drawW", "W Range", true).SetValue(new Circle(false, Color.FromArgb(0, 230, 255))));
            Menu.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("drawE", "E Range", true).SetValue(new Circle(true, Color.FromArgb(0, 230, 255))));
            Menu.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("drawR", "R Range", true).SetValue(new Circle(false, Color.FromArgb(0, 230, 255))));

            AIO_Menu.Champion.Drawings.addDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Orbwalking.OnNonKillableMinion += Orbwalking_OnNonKillableMinion;
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

            #region Call Killsteal
            if (Menu.Item("killsteal", true).GetValue<bool>() && E.IsReady())
                Killsteal(); 
            #endregion

            #region Call Mobsteal
            if (Menu.Item("mobsteal", true).GetValue<bool>() && E.IsReady())
                Mobsteal(); 
            #endregion

            #region E harass with lasthit for anytime
            if (MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Enemy).Any(x => E.IsKillable(x)) && HeroManager.Enemies.Any(x => x.IsValidTarget(E.Range) && E.GetDamage(x) >= 1 && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield)))
                E.Cast();
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = Menu.Item("drawQ", true).GetValue<Circle>();
            var drawW = Menu.Item("drawW", true).GetValue<Circle>();
            var drawE = Menu.Item("drawE", true).GetValue<Circle>();
            var drawR = Menu.Item("drawR", true).GetValue<Circle>();

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color, 3);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color, 3);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color, 3);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color, 3);
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == E.Instance.Name)
                    Utility.DelayAction.Add(250, Orbwalking.ResetAutoAttackTimer2);

            if (Menu.Item("soulboundsaver", true).GetValue<bool>() && sender.Type == GameObjectType.obj_AI_Hero && sender.IsEnemy && R.IsReady())
            {
                var soulbound = HeroManager.Allies.FirstOrDefault(hero => hero.HasBuff("kalistacoopstrikeally", true) && args.Target.NetworkId == hero.NetworkId && AIO_Func.getHealthPercent(hero) <= 20);

                if (soulbound != null)
                    R.Cast();
            }
        }

        static void Orbwalking_OnNonKillableMinion(AttackableUnit minion)
        {
            if (!Menu.Item("lasthitassist", true).GetValue<bool>())
                return;

            if (E.CanCast((Obj_AI_Base)minion) && E.IsKillable((Obj_AI_Base)minion))
                E.Cast();
        }

        static void Combo()
        {
            if (Menu.Item("comboUseQ", true).GetValue<bool>())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical, true);

                if (Q.CanCast(Qtarget) && Q.GetPrediction(Qtarget).Hitchance >= HitChance.VeryHigh && !Player.IsWindingUp && !Player.IsDashing())
                    Q.Cast(Qtarget);
            }

            if (Menu.Item("comboUseE", true).GetValue<Boolean>() && E.IsReady())
            {
                if (HeroManager.Enemies.Any(x => x.IsValidTarget(E.Range) && E.IsKillable(x) && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield)))
                    E.Cast();
            }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > Menu.Item("HrsMana", true).GetValue<Slider>().Value))
                return;

            if (Menu.Item("harassUseQ", true).GetValue<bool>())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical, true);

                if (Q.CanCast(Qtarget) && Q.GetPrediction(Qtarget).Hitchance >= HitChance.VeryHigh && !Player.IsWindingUp && !Player.IsDashing())
                    Q.Cast(Qtarget);
            }
        }

        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > Menu.Item("LcMana", true).GetValue<Slider>().Value))
                return;

            var Minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (Menu.Item("laneclearUseQ", true).GetValue<bool>() && Q.IsReady())
            {
                foreach (var minion in Minions.Where(x => Q.IsKillable(x)))
                {
                    var killcount = 0;

                    foreach (var colminion in AIO_Func.getCollisionMinions(Player, Player.ServerPosition.Extend(minion.ServerPosition, Q.Range), Q.Delay, Q.Width, Q.Speed))
                    {
                        if (Q.IsKillable(colminion))
                            killcount++;
                        else
                            break;
                    }

                    if (killcount >= Menu.Item("laneclearQnum", true).GetValue<Slider>().Value && Q.GetPrediction(minion).Hitchance >= HitChance.Medium)
                    {
                        Q.Cast(minion.ServerPosition);
                        break;
                    }
                }
            }

            if (Menu.Item("laneclearUseE", true).GetValue<bool>() && E.IsReady())
            {
                var minionkillcount = 0;

                foreach (var Minion in Minions.Where(x => x.IsValidTarget(E.Range) && E.IsKillable(x))){minionkillcount++;}

                if (minionkillcount >= Menu.Item("laneclearEnum", true).GetValue<Slider>().Value)
                    E.Cast();
            }
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > Menu.Item("JcMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (Menu.Item("jungleclearUseQ", true).GetValue<bool>() && Q.IsReady())
            {
                var qTarget = Mobs.FirstOrDefault(x => x.IsValidTarget(Q.Range) && Q.GetPrediction(x).Hitchance >= HitChance.Medium);

                if(qTarget != null)
                    Q.Cast(qTarget);
            }

            if (Menu.Item("jungleclearUseE", true).GetValue<bool>() && E.IsReady())
            {
                if (Mobs.Any(x => x.IsValidTarget(E.Range) && E.IsKillable(x)))
                    E.Cast();
            }
        }

        static float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (E.IsReady())
                damage += E.GetDamage(enemy);

            return damage;
        }

        static void Killsteal()
        {
            if (!E.IsReady())
                return;

            var target = HeroManager.Enemies.FirstOrDefault(x => !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield) && x.IsValidTarget(E.Range) && AIO_Func.isKillable(x, E));

            if (target != null)
                E.Cast();
        }

        static void Mobsteal()
        {
            if (!E.IsReady())
                return;

            if (MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).Any(x => E.IsKillable(x)))
                E.Cast();

            if (MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).Any(x => E.IsKillable(x) && (x.SkinName.ToLower().Contains("siege") || x.SkinName.ToLower().Contains("super"))))
                E.Cast();
        }
    }
}
