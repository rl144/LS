using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Yasuo// By RL244 WIP
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}} // yasuodashscalar yasuopassivemovementshield yasuopassivemscharge
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, QQ, W, E, EQ, R;
        static float QD {get{return Menu.Item("Misc.Qtg").GetValue<Slider>().Value; }}

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 475f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 400f);
            E = new Spell(SpellSlot.E, 475f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 1200f, TargetSelector.DamageType.Physical);
            QQ = new Spell(SpellSlot.Q, 900f, TargetSelector.DamageType.Physical);
            EQ = new Spell(SpellSlot.Q, 375f, TargetSelector.DamageType.Physical);

            Q.SetSkillshot(0.25f, 50f, float.MaxValue, false, SkillshotType.SkillshotLine);
            QQ.SetSkillshot(0.25f, 60f, 1200f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 300f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetTargetted(0.25f, float.MaxValue);
            R.SetTargetted(0.4f, float.MaxValue);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseE();

            AIO_Menu.Champion.Lasthit.addUseQ();
            AIO_Menu.Champion.Lasthit.addUseE();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseE();


            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseE();


            AIO_Menu.Champion.Misc.addHitchanceSelector();
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Qtg", "Additional Range")).SetValue(new Slider(50, 0, 150));
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addItem("AutoW", true);
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addItem("QQ Safe Range", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addItem("EQ Range", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();
            
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


            if (Orbwalking.CanMove(35))
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Orbwalker.SetAttack(true);
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Orbwalker.SetAttack(true);
                        Harass();
                        break;
                    case Orbwalking.OrbwalkingMode.LastHit:
                        Orbwalker.SetAttack(!AIO_Menu.Champion.Lasthit.UseQ || !Q.IsReady());
                        Lasthit();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        Orbwalker.SetAttack(true);
                        Laneclear();
                        Jungleclear();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        Orbwalker.SetAttack(true);
                        break;
                }
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
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

        var drawQ = AIO_Menu.Champion.Drawings.Qrange;
        var drawW = AIO_Menu.Champion.Drawings.Wrange;
        var drawE = AIO_Menu.Champion.Drawings.Erange;
        var drawR = AIO_Menu.Champion.Drawings.Rrange;
        var drawQQr = AIO_Menu.Champion.Drawings.getCircleValue("QQ Safe Range");
        var drawEQ = AIO_Menu.Champion.Drawings.getCircleValue("EQ Range");
        var QQTarget = TargetSelector.GetTarget(QQ.Range + Player.MoveSpeed * QQ.Delay, TargetSelector.DamageType.Magical);

    
        if (Q.IsReady() && drawQ.Active)
        Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
        
        if (EQ.IsReady() && drawEQ.Active)
        Render.Circle.DrawCircle(Player.Position, EQ.Range, drawEQ.Color);
        
        if (W.IsReady() && drawW.Active)
        Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
    
        if (E.IsReady() && drawE.Active)
        Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
        
        if (R.IsReady() && drawR.Active)
        Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        
        if (QQ.IsReady() && drawQQr.Active && Player.HasBuff("yasuoq3w") &&QQTarget != null)
        Render.Circle.DrawCircle(Player.Position, QQ.Range - QQTarget.MoveSpeed*QQ.Delay, drawQQr.Color);


        }
        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;


        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (QQ.IsReady() && Player.HasBuff("yasuoq3w") && !Dash.IsDashing(Player)
            && Player.Distance(sender.Position) <= QQ.Range)
                QQ.Cast(sender.Position);

        }
        
        static readonly string[] Attacks = { "jarvanivcataclysmattack", "monkeykingdoubleattack", "shyvanadoubleattack", "shyvanadoubleattackdragon", "caitlynheadshotmissile", "frostarrow", "garenslash2", "kennenmegaproc", "masteryidoublestrike", "quinnwenhanced", "renektonexecute", "renektonsuperexecute", "rengarnewpassivebuffdash", "trundleq", "xenzhaothrust", "viktorqbuff", "xenzhaothrust2", "xenzhaothrust3" };
        static readonly string[] NoAttacks = { "zyragraspingplantattack", "zyragraspingplantattack2", "zyragraspingplantattackfire", "zyragraspingplantattack2fire" };
        static readonly string[] OHSP = { "Parley", "EzrealMysticShot"};
        static readonly string[] AttackResets = { "dariusnoxiantacticsonh", "fioraflurry", "garenq", "hecarimrapidslash", "jaxempowertwo", "jaycehypercharge", "leonashieldofdaybreak", "monkeykingdoubleattack", "mordekaisermaceofspades", "nasusq", "nautiluspiercinggaze", "netherblade", "parley", "poppydevastatingblow", "powerfist", "renektonpreexecute", "rengarq", "shyvanadoubleattack", "sivirw", "takedown", "talonnoxiandiplomacy", "trundletrollsmash", "vaynetumble", "vie", "volibearq", "xenzhaocombotarget", "yorickspectral" };

        static bool IsSkill(string name)
        {
            return !(name.ToLower().Contains("attack")) && !Attacks.Contains(name.ToLower()) && !AttackResets.Contains(name.ToLower()) || NoAttacks.Contains(name.ToLower()) ||
            OHSP.Contains(name.ToLower());
        }
        
        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.IsDead) // 바람장막
                return;
                
            if(!sender.IsMe)
            {
                SharpDX.Vector2 castVec = Player.ServerPosition.To2D() +
                SharpDX.Vector2.Normalize(args.Start.To2D() - Player.Position.To2D()) * (100f);
                if (IsSkill(args.SData.Name) && (args.Target.IsMe || !sender.IsAlly) && W.IsReady()
                && Player.Distance(args.End) < 250 && AIO_Menu.Champion.Misc.getBoolValue("AutoW"))
                W.Cast(castVec);
            }
            else
            {
                if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseQ || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && AIO_Menu.Champion.Harass.UseQ))
                {
                    if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.E).Name && HeroManager.Enemies.Any(x => x.IsValidTarget(EQ.Range)))
                    {
                        EQ.Cast();
                    }
                }
            }
        }
        
        static void AA() // 챔피언 대상 평캔 ( 빼낸 이유는 AA방식 두개로 할시 두번 적어야 해서 단순화하기 위함.
        {
            AIO_Func.AACb(Q,QD);
        }
        
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || Target == null)
                return;

            AIO_Func.AALcJc(Q,QD);
            if(!utility.Activator.AfterAttack.AIO)
            AA();
        }
        
        static void Combo()
        {
            var buff = AIO_Func.getBuffInstance(Player, "yasuoq3w");
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                if(!Player.HasBuff("yasuoq3w"))
                {
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);
                if(qTarget.Distance(Player.Position) >= Orbwalking.GetRealAutoAttackRange(Player) + 50)
                AIO_Func.LCast(Q,qTarget,QD);
                }
                else
                {
                var qTarget = TargetSelector.GetTarget(QQ.Range, Q.DamageType, true);
                AIO_Func.LCast(QQ,qTarget,QD);
                }
            }
            
            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                var ETarget = TargetSelector.GetTarget(E.Range, E.DamageType, true);
                if(!ETarget.HasBuff("YasuoDashWrapper") && Player.HasBuff("yasuoq3w"))
                E.Cast(ETarget);
            }
            
            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                var RTarget = TargetSelector.GetTarget(R.Range, R.DamageType, true);
                if(R.CanCast(RTarget))
                R.Cast(RTarget);
            }
                
        }

        static void Harass()
        {

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
                if(!Player.HasBuff("yasuoq3w"))
                {
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);
                if(qTarget.Distance(Player.Position) >= Orbwalking.GetRealAutoAttackRange(Player) + 50)
                AIO_Func.LCast(Q,qTarget,QD);
                }
                else
                {
                var qTarget = TargetSelector.GetTarget(QQ.Range, Q.DamageType, true);
                AIO_Func.LCast(QQ,qTarget,QD);
                }
            }
                
            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                var Buff = AIO_Func.getBuffInstance(Player, "yasuodashscalar");
                var ETarget = TargetSelector.GetTarget(E.Range, E.DamageType, true);
                if(!ETarget.HasBuff("YasuoDashWrapper") && (float)Buff.Count > 1)
                E.Cast(ETarget);
            }

        }
        
        static void Lasthit()
        {

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;
                
                if(AIO_Menu.Champion.Lasthit.UseQ && Q.IsReady())
                AIO_Func.LH(Q,float.MaxValue);
                if(AIO_Menu.Champion.Lasthit.UseE && E.IsReady())
                AIO_Func.LH(E);
        }
        
        static void Laneclear()
        {

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);
            var EM = MinionManager.GetMinions(Game.CursorPos, 150f, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(x => !x.HasBuff("YasuoDashWrapper") && x.Distance(Player.ServerPosition) <= E.Range);
            if (Minions.Count <= 0)
                return;
                
                
            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
                if (Q.CanCast(Minions[0]))
                AIO_Func.LH(Q,float.MaxValue); //AIO_Func.LCast(Q,Minions[0],QD);
            }
            
            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
            {
                if (E.CanCast(EM))
                E.Cast(EM);
            }


        }

        static void Jungleclear()
        {

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var EM = MinionManager.GetMinions(Game.CursorPos, 150f, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault(x => !x.HasBuff("YasuoDashWrapper") && x.Distance(Player.ServerPosition) <= E.Range);

            if (Mobs.Count <= 0)
                return;
            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                if (Q.CanCast(Mobs[0]))
                AIO_Func.LCast(Q,Mobs[0],QD);
            }
            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
            {
                if (E.CanCast(EM))
                E.Cast(EM);
            }
        }

        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                Q.Cast(target);
            }
        }
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                var Buff = AIO_Func.getBuffInstance(Player, "yasuodashscalar");
                if(E.CanCast(target) && AIO_Func.isKillable(target, E.GetDamage2(target)*((float)Buff.Count*0.25f + 1f)) && !target.HasBuff("YasuoDashWrapper"))
                E.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;
            var Buff = AIO_Func.getBuffInstance(Player, "yasuodashscalar");
            if (Q.IsReady())
                damage += Q.GetDamage2(enemy);
            
            if (E.IsReady())
                damage += E.GetDamage2(enemy)*((float)Buff.Count*0.25f + 1f);
            
            if (R.IsReady())
                damage += R.GetDamage2(enemy);

            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage2(enemy, true);
                
            return damage;
        }
    }
}
