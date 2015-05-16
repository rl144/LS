using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Cassiopeia // RL244
    {
    
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 850f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 850f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 700f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 825f, TargetSelector.DamageType.Magical);

            
            Q.SetSkillshot(0.65f, 130f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.75f, 106f, 2500f, false, SkillshotType.SkillshotCircle);
            E.SetTargetted(0.25f, 1900f);
            R.SetSkillshot(0.3f, (float)(80 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();
            AIO_Menu.Champion.Combo.addItem("R Min Targets", new Slider(2, 1, 5));

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana(20);

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW(false);
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana(10);
            
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealW", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange(false);
            AIO_Menu.Champion.Drawings.addItem("W Real Range", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
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

            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealW"))
                KillstealW();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();

            Q.MinHitChance = AIO_Menu.Champion.Misc.SelectedHitchance;
            W.MinHitChance = AIO_Menu.Champion.Misc.SelectedHitchance;
            R.MinHitChance = AIO_Menu.Champion.Misc.SelectedHitchance;
}

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawWr = AIO_Menu.Champion.Drawings.getCircleValue("W Real Range");
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            var wtarget = TargetSelector.GetTarget(W.Range + Player.MoveSpeed * W.Delay, TargetSelector.DamageType.Magical);

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color, 3);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color, 3);

            if (W.IsReady() && drawWr.Active && wtarget != null)
                Render.Circle.DrawCircle(Player.Position, W.Range - wtarget.MoveSpeed*W.Delay, drawWr.Color, 3);
        
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color, 3);
        
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color, 3);
        }
        
        static bool IsPoisoned(Obj_AI_Base unit)
        {
            return
                unit.Buffs.Where(buff => buff.IsActive && buff.Type == BuffType.Poison)
                    .Any(buff => buff.EndTime >= (Game.Time + 0.35 + 700 / 1900));
        }
        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (R.CanCast(gapcloser.Sender) && gapcloser.Sender.IsFacing(Player))
                R.Cast(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (R.CanCast(sender) && sender.IsFacing(Player))
                R.Cast(sender);
        }

        static void Combo()
        {

            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                AIO_Func.CCast(Q,Qtarget);
            }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                var Etarget = HeroManager.Enemies.Where(x => x.IsValidTarget(E.Range) && IsPoisoned(x) && !TargetSelector.IsInvulnerable(x, E.DamageType)).OrderByDescending(x=> E.GetDamage(x)).FirstOrDefault();

                if(Etarget != null)
                    E.CastOnUnit(Etarget);
            }

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
                AIO_Func.CCast(W,Wtarget);
            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                //스턴걸리는애들 마릿수 체크 => R시전 (테스트x)
                //var rTarget = R.GetTarget();
                //if (R.GetCollision(Player.ServerPosition.To2D(), new List<Vector2> { rTarget.ServerPosition.To2D() }).Where(x => x.IsFacing(Player)).Count() >= AIO_Menu.Champion.Combo.getSliderValue("R Min Targets").Value)
                //    R.Cast(rTarget);

                //그냥 맞는놈들 마릿수 체크 => R시전
                R.CastIfWillHit(R.GetTarget(), AIO_Menu.Champion.Combo.getSliderValue("R Min Targets").Value);
            }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;
        
            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                AIO_Func.CCast(Q,Qtarget);
            }

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                var Etarget = HeroManager.Enemies.Where(x => x.IsValidTarget(E.Range) && IsPoisoned(x) && !TargetSelector.IsInvulnerable(x, E.DamageType)).OrderByDescending(x => E.GetDamage(x)).FirstOrDefault();

                if (Etarget != null)
                    E.CastOnUnit(Etarget);
            }

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
                AIO_Func.CCast(W,Wtarget);
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
                var qloc = Q.GetCircularFarmLocation(Minions.Where(x=>x.IsValidTarget(Q.Range)).ToList());

                if (qloc.MinionsHit >= 1)
                    Q.Cast(qloc.Position);
            }

            if (AIO_Menu.Champion.Laneclear.UseW && W.IsReady())
            {
                var wloc = W.GetCircularFarmLocation(Minions.Where(x => x.IsValidTarget(Q.Range)).ToList());

                if (wloc.MinionsHit >= 1)
                    W.Cast(wloc.Position);
            }
            
            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
            {
                var eTarget = Minions.FirstOrDefault(m => AIO_Func.isKillable(m, E, 0) && HealthPrediction.GetHealthPrediction(m, (int)(Player.Distance(m, false) / E.Speed), (int)(E.Delay * 1000 + Game.Ping / 2)) > 0 && IsPoisoned(m));
                
                if (eTarget != null)
                    E.CastOnUnit(eTarget);
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
                if (Mobs[0].IsValidTarget(Q.Range))
                    AIO_Func.CCast(Q,Mobs[0]);
            }

            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady())
            {
                if (Mobs[0].IsValidTarget(W.Range))
                    AIO_Func.CCast(W,Mobs[0]);
            }
            
            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
            {
                if (Mobs[0].IsValidTarget(E.Range) && IsPoisoned(Mobs[0]))
                    E.Cast(Mobs[0]);
            }

        }

        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    AIO_Func.CCast(Q,target);
            }
        }
        
        static void KillstealW()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (W.CanCast(target) && AIO_Func.isKillable(target, W))
                    AIO_Func.CCast(W,target);
            }
        }
        
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                    E.Cast(target);
            }
        }

        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (R.CanCast(target) && AIO_Func.isKillable(target, R))
                    R.Cast(target);
            }
        }
        
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);

            if (W.IsReady())
                damage += W.GetDamage2(enemy);
                
            if (E.IsReady())
                damage += E.GetDamage2(enemy)*4;

            if (R.IsReady())
                damage += R.GetDamage2(enemy);
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
            return damage;
        }
    }
}
