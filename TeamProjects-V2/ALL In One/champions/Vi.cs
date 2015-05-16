using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Vi
    {
        static Menu Menu { get { return AIO_Menu.MainMenu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        const byte defaltRange = 190;

        static bool QisAllGood(Obj_AI_Base target) { return Q.IsReady() && Q.IsCharging && target.IsValidTarget(Q.Range) && Q.GetPrediction(target).Hitchance >= HitChance.High; }
        static int getWBuffStacks(Obj_AI_Base target) { var stacks = AIO_Func.getBuffInstance(Player, "viwproc", Player); return stacks != null ? stacks.Count : 0; }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 840f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 190f, TargetSelector.DamageType.Physical);
            E = new Spell(SpellSlot.E, 240f, TargetSelector.DamageType.Physical);//splash 600
            R = new Spell(SpellSlot.R, 800f, TargetSelector.DamageType.Physical);

            Q.SetSkillshot(0.25f, 50f, 1500f, false, SkillshotType.SkillshotLine);
            Q.SetCharged("ViQ", "ViQ", 100, 840, 1f);

            R.SetTargetted(0.25f, 1500f);

            Menu.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("CbUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("CbUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("CbUseR", "Use R", true).SetValue(true));

            Menu.SubMenu("Champion").SubMenu("Harass").AddItem(new MenuItem("HrsUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Harass").AddItem(new MenuItem("HrsUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Harass").AddItem(new MenuItem("HrsMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            //Menu.SubMenu("Champion").SubMenu("Laneclear").AddItem(new MenuItem("LcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Laneclear").AddItem(new MenuItem("LcUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Laneclear").AddItem(new MenuItem("LcMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            //Menu.SubMenu("Champion").SubMenu("Jungleclear").AddItem(new MenuItem("JcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Jungleclear").AddItem(new MenuItem("JcUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Jungleclear").AddItem(new MenuItem("JcMana", "if Mana % >", true).SetValue(new Slider(20, 0, 100)));

            Menu.SubMenu("Champion").SubMenu("Misc").AddItem(new MenuItem("miscKs", "Use KillSteal", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Misc").AddItem(new MenuItem("miscAntigap", "Use Anti-Gapcloser", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Misc").AddItem(new MenuItem("miscAutointer", "Use Interrupter", true).SetValue(true));

            Menu.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Q Range", true).SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("drawE", "E Range", true).SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("drawR", "R Range", true).SetValue(new Circle(true, Color.Red)));

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
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

            Orbwalker.SetAttack(!Q.IsCharging);

            #region Killsteal
            if (Menu.Item("miscKs", true).GetValue<bool>())
                Killsteal();
            #endregion
            
            #region AfterAttack
            AIO_Func.AASkill(E);
            if(AIO_Func.AfterAttack())
            AA();
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = Menu.Item("drawQ", true).GetValue<Circle>();
            var drawE = Menu.Item("drawE", true).GetValue<Circle>();
            var drawR = Menu.Item("drawR", true).GetValue<Circle>();

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);


        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Menu.Item("miscAntigap", true).GetValue<bool>() || Player.IsDead)
                return;

            if (QisAllGood(gapcloser.Sender))
                Q.Cast(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Menu.Item("miscAutointer", true).GetValue<bool>() || Player.IsDead)
                return;

            if (QisAllGood(sender))
                Q.Cast(sender);

            if (R.CanCast(sender) && args.DangerLevel >= Interrupter2.DangerLevel.High)
                R.Cast(sender);
        }
        
        static void AA()
        {
            AIO_Func.AACb(E);
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;

            if (!unit.IsMe || !Orbwalking.CanMove(10) || !E.IsReady() || Target == null)
                return;

            if(!utility.Activator.AfterAttack.AIO)
            AA();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                if (Menu.Item("LcUseE", true).GetValue<bool>() && AIO_Func.getManaPercent(Player) > Menu.Item("LcMana", true).GetValue<Slider>().Value)
                    E.Cast();

                if (Menu.Item("JcUseE", true).GetValue<bool>() && AIO_Func.getManaPercent(Player) > Menu.Item("JcMana", true).GetValue<Slider>().Value) // <- 이렇게 하면 JcUseE, LcUseE 둘중에 하나만 켜도 정글 라인 안가리고 항상 E를 쓰는 문제가 발생함. 분리하려면 제가한것처럼 AALaneclear AAJunglecelar 등으로 분리해야..
                    E.Cast();
            }
        }

        static void Combo()
        {
            if (Menu.Item("CbUseQ", true).GetValue<bool>() && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.ChargedMaxRange, Q.DamageType);

                if (QisAllGood(qTarget))
                    Q.Cast(qTarget);

                if (!Q.IsCharging && qTarget != null)
                    Q.StartCharging();
            }

            if (Menu.Item("CbUseE", true).GetValue<bool>() && E.IsReady())
            {
                var eTarget = TargetSelector.GetTarget(E.Range, E.DamageType);

                if (!eTarget.IsValidTarget(defaltRange) && eTarget.IsValidTarget(E.Range))
                    E.Cast();
            }

            if (Menu.Item("CbUseR", true).GetValue<bool>() && R.IsReady())
            {
                var rTarget = TargetSelector.GetTarget(R.Range, R.DamageType);

                if (R.CanCast(rTarget) && rTarget.Health <= getComboDamage(rTarget) + (Q.GetDamage2(rTarget) * 2))
                    R.Cast(rTarget);
            }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > Menu.Item("HrsMana", true).GetValue<Slider>().Value))
                return;

            if (Menu.Item("HrsUseQ", true).GetValue<bool>() && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.ChargedMaxRange, Q.DamageType);

                if (QisAllGood(qTarget))
                    Q.Cast(qTarget);

                if (!Q.IsCharging && qTarget != null)
                    Q.StartCharging();
            }

            if (Menu.Item("HrsUseE", true).GetValue<bool>() && E.IsReady())
            {
                var eTarget = TargetSelector.GetTarget(E.Range, E.DamageType);

                if (!eTarget.IsValidTarget(defaltRange) && eTarget.IsValidTarget(E.Range))
                    E.Cast();
            }
        }

        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > Menu.Item("LcMana", true).GetValue<Slider>().Value))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (Menu.Item("LcUseQ", true).GetValue<bool>() && Q.IsReady())
            { }
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > Menu.Item("JcMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (Menu.Item("JcUseQ", true).GetValue<bool>() && Q.IsReady())
            { }
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.IsReady() && target.IsValidTarget(Q.ChargedMaxRange) && Q.IsCharging && Q.IsKillable(target) && Q.GetPrediction(target).Hitchance >= HitChance.High)
                    Q.Cast(target);

                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                    E.Cast(target);

                if (R.CanCast(target) && AIO_Func.isKillable(target, R))
                    R.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);

            if(getWBuffStacks(enemy) == 2)
                damage += W.GetDamage2(enemy);

            if (E.IsReady())
                damage += (E.GetDamage2(enemy) * E.Instance.Ammo) + ((float)Player.GetAutoAttackDamage2(enemy, false) * E.Instance.Ammo);

            if (R.IsReady())
                damage += R.GetDamage2(enemy);

            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);

            return damage;
        }
    }
}
