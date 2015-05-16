using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Garen//by xcsoft
    {
        static Menu Menu { get { return AIO_Menu.MainMenu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;
        
        static bool E1isReady { get { return Player.Spellbook.GetSpell(SpellSlot.E).Name == "GarenE" && E.IsReady(); } }
        static bool QisOn { get { return Player.HasBuff("GarenQ", true); } }
        static bool imSpinning { get { return Player.HasBuff("GarenE", true); } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 800f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 500f);
            E = new Spell(SpellSlot.E, 300f, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R, 400f, TargetSelector.DamageType.Magical);

            R.SetTargetted(0.25f, float.MaxValue);

            Menu.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("CbUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("CbUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("CbUseR", "Use R", true).SetValue(true));

            Menu.SubMenu("Champion").SubMenu("Harass").AddItem(new MenuItem("HrsUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Harass").AddItem(new MenuItem("HrsUseE", "Use E", true).SetValue(true));

            Menu.SubMenu("Champion").SubMenu("Laneclear").AddItem(new MenuItem("LcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Laneclear").AddItem(new MenuItem("LcUseE", "Use E", true).SetValue(true));

            Menu.SubMenu("Champion").SubMenu("Jungleclear").AddItem(new MenuItem("JcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Jungleclear").AddItem(new MenuItem("JcUseE", "Use E", true).SetValue(true));

            Menu.SubMenu("Champion").SubMenu("Misc").AddItem(new MenuItem("miscAutoW", "Use Auto-W", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Misc").AddItem(new MenuItem("miscKs", "Use KillSteal", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Misc").AddItem(new MenuItem("miscAntigap", "Use Anti-Gapcloser", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Misc").AddItem(new MenuItem("miscinter", "Use Interrupter", true).SetValue(true));

            Menu.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Q Range", true).SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("drawE", "E Range", true).SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("drawR", "R Range", true).SetValue(new Circle(true, Color.Red)));

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
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

            Orbwalker.SetAttack(!imSpinning);

            #region Call Killsteal
            if (Menu.Item("miscKs", true).GetValue<bool>())
                Killsteal();
            #endregion
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

            if (Q.CanCast(gapcloser.Sender))
                Q.Cast(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Menu.Item("miscinter", true).GetValue<bool>() || Player.IsDead)
                return;

            if (Q.CanCast(sender))
                Q.Cast(sender);
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Menu.Item("miscAutoW", true).GetValue<bool>() && sender.IsEnemy && sender.Type == GameObjectType.obj_AI_Hero && args.Target.IsMe && W.IsReady())
                W.Cast();
        }

        static void AA()
        {
            AIO_Func.AACb(Q,0,0,0f);
        }
        
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || Target == null)
                return;

            AIO_Func.AALcJc(Q,0,0,0f);

            if(!utility.Activator.AfterAttack.AIO)
            AA();
        }
        
        static void Combo()
        {
            if (Menu.Item("CbUseE", true).GetValue<bool>() && E1isReady && !QisOn)
            {
                if (HeroManager.Enemies.Any(x => x.IsValidTarget(E.Range)))
                    E.Cast();
            }

            if (Menu.Item("CbUseR", true).GetValue<bool>() && R.IsReady())
            {
                var rTarget = HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range) && R.IsKillable(x)).OrderByDescending(x => x.Health).FirstOrDefault();

                if (R.CanCast(rTarget))
                    R.Cast(rTarget);
            }
        }

        static void Harass()
        {
            if (Menu.Item("HrsUseE", true).GetValue<bool>() && E1isReady && !QisOn)
            {
                if (HeroManager.Enemies.Any(x => x.IsValidTarget(E.Range)))
                    E.Cast();
            }
        }

        static void Laneclear()
        {
            var Minions = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (Menu.Item("LcUseE", true).GetValue<bool>() && E1isReady)
            {
                if (Minions.Count(x => x.IsValidTarget(E.Range)) >= 3)
                    E.Cast();
            }
        }

        static void Jungleclear()
        {
            var Mobs = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (Menu.Item("JcUseE", true).GetValue<bool>() && E1isReady)
            {
                if (Mobs.Any(x => x.IsValidTarget(E.Range)))
                    E.Cast();
            }
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
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

            if (E1isReady)
                damage += E.GetDamage2(enemy) * 5;

            if (R.IsReady())
                damage += R.GetDamage2(enemy);

            return damage;
        }
    }
}
