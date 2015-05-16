using System;
using System.Drawing;
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
    class Brand // By RL244 brandpassive brandpassivesound 
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float QD = 50f;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1050f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 900f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 625f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 750f, TargetSelector.DamageType.Magical);

            
            Q.SetSkillshot(0.25f, 70f, 1400f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.625f, 250f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetTargetted(0.25f, float.MaxValue);
            R.SetTargetted(0.25f, 1000f);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();
            
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            PrioritySelector();

            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Qtg", "Additional Range")).SetValue(new Slider(50, 0, 250));
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealW", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addItem("W on stuned target", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();

            AIO_Menu.Champion.Drawings.addQrange(false);
            AIO_Menu.Champion.Drawings.addItem("Q Safe Range", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addWrange();
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
            QD = Menu.Item("Misc.Qtg").GetValue<Slider>().Value; 

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
            if (AIO_Menu.Champion.Misc.getBoolValue("W on stuned target"))
                stw();
                
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawQr = AIO_Menu.Champion.Drawings.getCircleValue("Q Safe Range");
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            var qtarget = TargetSelector.GetTarget(Q.Range + Player.MoveSpeed * Q.Delay, TargetSelector.DamageType.Magical);

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (Q.IsReady() && drawQr.Active && qtarget != null)
                Render.Circle.DrawCircle(Player.Position, Q.Range - qtarget.MoveSpeed*Q.Delay, drawQr.Color);
        
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
        
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (W.IsReady()
                && Player.Distance(gapcloser.Sender.Position) <= W.Range)
                AIO_Func.CCast(W,gapcloser.Sender); //W.Cast((Vector3)gapcloser.End);
            if (gapcloser.Sender.HasBuff("brandablaze") && Q.IsReady()
                && Player.Distance(gapcloser.Sender.Position) <= Q.Range)
                AIO_Func.LCast(Q,gapcloser.Sender,QD,0);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (W.IsReady()
            && Player.Distance(sender.Position) <= W.Range)
                AIO_Func.CCast(W,sender);
            if (sender.HasBuff("brandablaze") && Q.IsReady()
            && Player.Distance(sender.Position) <= Q.Range)
                AIO_Func.LCast(Q,sender,QD,0);
        }
        
        enum PriorSpell
        {
        Q = 0,
        W = 1,
        E = 2,
        N = 3
        }
        
        static void PrioritySelector(PriorSpell defaultSpell = PriorSpell.N)
        {
            int defaultindex;

            switch (defaultSpell)
            {
                case PriorSpell.Q:
                    defaultindex = 0;
                    break;
                case PriorSpell.W:
                    defaultindex = 1;
                    break;
                case PriorSpell.E:
                    defaultindex = 2;
                    break;
                case PriorSpell.N:
                    defaultindex = 3;
                    break;
                default:
                    defaultindex = 3;
                    break;
            }

            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.PriorSpell", "PriorSpell", true)).SetValue(new StringList(new string[] { "Q", "W", "E", "None" }, defaultindex));
        }
        
        static PriorSpell SelectedPriorSpell
        {
            get
            {
                switch (Menu.SubMenu("Misc").Item("Misc.PriorSpell", true).GetValue<StringList>().SelectedValue)
                {
                    case "Q":
                        return PriorSpell.Q;
                    case "W":
                        return PriorSpell.W;
                    case "E":
                        return PriorSpell.E;
                    case "None":
                        return PriorSpell.N;
                    default:
                        return PriorSpell.N;
                }
            }
        }

        
        static void Combo()
        {

            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                if((PriorSpell.Q != SelectedPriorSpell || Qtarget.HasBuff("brandablaze")) && Qtarget != null)
                    AIO_Func.LCast(Q,Qtarget,QD,0);
            }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                if((PriorSpell.E != SelectedPriorSpell || Etarget.HasBuff("brandablaze")) && Etarget != null)
                    E.Cast(Etarget);
            }

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
                if((PriorSpell.W != SelectedPriorSpell || Wtarget.HasBuff("brandablaze")) && Wtarget != null)
                    AIO_Func.CCast(W,Wtarget);

            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                var Rtarget = TargetSelector.GetTarget(R.Range, R.DamageType);
            
                if(Rtarget.Health + Rtarget.HPRegenRate <= R.GetDamage2(Rtarget)*2)
                { 
                    if (HeroManager.Enemies.Any(x => x.IsValidTarget(R.Range)))
                    R.Cast(Rtarget);
                }
            }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;
        
            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if(Qtarget != null)
                AIO_Func.LCast(Q,Qtarget,QD,0);
            }

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                var Etarget = TargetSelector.GetTarget(E.Range + Player.MoveSpeed*E.Delay, TargetSelector.DamageType.Magical);
                if(Etarget != null)
                E.Cast(Etarget);
            }

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
                if(Wtarget != null)
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
                AIO_Func.LH(Q);

            if (AIO_Menu.Champion.Laneclear.UseW && W.IsReady() && Minions.Any(x => x.IsValidTarget(W.Range)))
                AIO_Func.CCast(W,Minions[0]);
            
            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
                AIO_Func.LH(E);
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
                if (Q.CanCast(Mobs.FirstOrDefault()))
                    AIO_Func.LCast(Q,Mobs.FirstOrDefault(),QD,0);
            }

            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady())
            {
                if (Mobs.Any(x=>x.IsValidTarget(W.Range)))
                    AIO_Func.CCast(W,Mobs[0]);
            }
            
            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
            {
                if (Mobs.Any(x=>x.IsValidTarget(E.Range)))
                    E.Cast(Mobs[0]);
            }

        }

        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    AIO_Func.LCast(Q,target,QD,0);
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
        
        static void stw()
        {
            if (W.IsReady() && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
            {
            var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
            if(Wtarget == null)
            return;
            var pred = W.GetPrediction(Wtarget);
            if (pred.Hitchance == HitChance.Immobile || Wtarget.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay - 0.4f && W.IsReady())
            W.Cast(Wtarget, false, true);
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
                damage += E.GetDamage2(enemy);

            if (R.IsReady())
                damage += R.GetDamage2(enemy) * 2;
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
            return damage;
        }
    }
}
