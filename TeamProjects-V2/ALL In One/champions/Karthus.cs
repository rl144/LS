using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Karthus
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {

            Q = new Spell(SpellSlot.Q, 875f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 1000f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 550f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(1.0f, 160f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.6f, 1f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW(false);
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana(60);

            AIO_Menu.Champion.Lasthit.addUseQ();
            AIO_Menu.Champion.Lasthit.addIfMana(0);

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();
            
            AIO_Menu.Champion.Flee.addUseE();
            AIO_Menu.Champion.Flee.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            Q.MinHitChance = AIO_Menu.Champion.Misc.SelectedHitchance;
            
            if (Orbwalking.CanMove(10))
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Orbwalker.SetAttack(false);
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Orbwalker.SetAttack(true);
                        Harass();
                        break;
                    case Orbwalking.OrbwalkingMode.LastHit:
                        Orbwalker.SetAttack(AIO_Func.getManaPercent(Player) <= AIO_Menu.Champion.Lasthit.IfMana || !AIO_Menu.Champion.Lasthit.UseQ || !Q.IsReady());
                        Lasthit();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        Orbwalker.SetAttack(true);
                        Laneclear();
                        Jungleclear();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        Orbwalker.SetAttack(true);
                        if (E.IsReady() && E.Instance.ToggleState == 2 && !HeroManager.Enemies.Any(x => x.IsValidTarget(E.Range)) && !MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.NotAlly).Any())
                            E.Cast();
                        break;
                }
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color, 3);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color, 3);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color, 3);
        }

        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
                Q.CastOnBestTarget(0f, false, true);

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
                W.CastOnBestTarget();

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                if (AIO_Func.anyoneValidInRange(E.Range) && E.Instance.ToggleState == 1)
                    E.Cast();
                else if (!AIO_Func.anyoneValidInRange(E.Range) && E.Instance.ToggleState == 2)
                    E.Cast();
            }
        }

        static void Harass()
        {
            if (E.IsReady() && !AIO_Func.anyoneValidInRange(E.Range) && E.Instance.ToggleState == 2)
                E.Cast();

            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
            {
                if (E.IsReady() && E.Instance.ToggleState == 2)
                    E.Cast();

                return;
            }

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
                Q.CastOnBestTarget(0f, false, true);

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
                W.CastOnBestTarget();

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                if (AIO_Func.anyoneValidInRange(E.Range) && E.Instance.ToggleState == 1)
                    E.Cast();
            }
        }

        static void Lasthit()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Lasthit.IfMana))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Lasthit.UseQ && Q.IsReady())
            {
                var qTarget = Minions.FirstOrDefault(x => x.IsValidTarget(Q.Range) && HealthPrediction.GetHealthPrediction((Obj_AI_Base)x, (int)(Q.Delay*1000)) <= Q.GetDamage2(x, 1));

                if (qTarget != null)
                    Q.Cast(qTarget);
            }
        }

        static void Laneclear()
        {
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (E.IsReady() && !Minions.Any(x => x.IsValidTarget(E.Range)) && E.Instance.ToggleState == 2)
                E.Cast();

            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
            {
                if (E.IsReady() && E.Instance.ToggleState == 2)
                    E.Cast();

                return;
            }

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
                var qloc = Q.GetCircularFarmLocation(Minions.Where(x=>x.IsValidTarget(Q.Range)).ToList());

                if (qloc.MinionsHit >= 1)
                    Q.Cast(qloc.Position);
            }

            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
            {
                if (Minions.Any(x => x.IsValidTarget(E.Range)) && E.Instance.ToggleState == 1)
                    E.Cast();
            }
        }

        static void Jungleclear()
        {
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (E.IsReady() && !Mobs.Any(x => x.IsValidTarget(E.Range)) && E.Instance.ToggleState == 2)
                E.Cast();

            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
            {
                if (E.IsReady() && E.Instance.ToggleState == 2)
                    E.Cast();

                return;
            }

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                var qloc = Q.GetCircularFarmLocation(Mobs.Where(x => x.IsValidTarget(Q.Range)).ToList());

                if (qloc.MinionsHit >= 1)
                    Q.Cast(qloc.Position);
            }

            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
            {
                if (Mobs.Any(x => x.IsValidTarget(E.Range)) && E.Instance.ToggleState == 1)
                    E.Cast();
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
                damage += R.GetDamage2(enemy);

            return damage;
        }
    }
}
