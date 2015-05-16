using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Riven// By RL244 rivenpassiveaaboost rivenpassive rivenwindslashready RivenFengShuiEngine RivenFeint riventricleavesoundone riventricleavesoundtwo
       //아 Q 짜증..
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}} 
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static int Qtimer = 0;
        static float Qps {get {var buff = AIO_Func.getBuffInstance(Player, "rivenpassiveaaboost"); return buff != null ? buff.Count : 0; } }
        static float getRBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "RivenFengShuiEngine"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static bool NextQCastAllowed {get {return Qps <= 2;}} //NextQCastAllowed 쓰는거 잠시 보류
        static bool Qmove;
        static float RD {get{return Menu.Item("Combo.RD").GetValue<Slider>().Value; }}

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 260f + 37.5f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 250f + 37.5f, TargetSelector.DamageType.Physical){Delay = 0.25f};
            E = new Spell(SpellSlot.E, 325f + Orbwalking.GetRealAutoAttackRange(Player), TargetSelector.DamageType.Physical);//그냥 접근기로 쓰게 넣었음
            R = new Spell(SpellSlot.R, 1100f, TargetSelector.DamageType.Physical);
            

            Q.SetSkillshot(0.25f, 250f, 2000f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 150f, 2000f, false, SkillshotType.SkillshotCircle); //그냥 접근기로 쓰게 넣었음
            R.SetSkillshot(0.25f, 60f * (float)Math.PI / 180, 1600f, false, SkillshotType.SkillshotCone); //리븐너프
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();
            AIO_Menu.Champion.Combo.addItem("Use Auto R2", false);
            Menu.SubMenu("Combo").AddItem(new MenuItem("Combo.RD", "R Min Distance")).SetValue(new Slider(350, 0, 700));

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            
            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            
            AIO_Menu.Champion.Flee.addUseQ();
            AIO_Menu.Champion.Flee.addUseE();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Misc.addItem("Inteligent Q", true);
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();
            AIO_Menu.Champion.Drawings.addItem("R Timer", new Circle(true, Color.Red));

        
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            AttackableUnit.OnDamage += OnDamage;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Obj_AI_Base.OnPlayAnimation += OnPlayAnimation;
            Spellbook.OnCastSpell += OnCastSpell;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(10))
            {
                AIO_Func.SC(W,0,0,0f);
                AIO_Func.MouseSC(E,0f);
                AIO_Func.FleeToPosition(E,"N");
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
                }
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();
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
            var drawRTimer = AIO_Menu.Champion.Drawings.getCircleValue("R Timer");
            var pos_temp = Drawing.WorldToScreen(Player.Position);
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range - Orbwalking.GetRealAutoAttackRange(Player), drawE.Color);
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
            if (drawRTimer.Active && getRBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawRTimer.Color, "R: " + getRBuffDuration.ToString("0.00"));
        }
        
        static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            return;
            if (args.SData.Name == "RivenTriCleave")
            Qtimer = Utils.GameTimeTickCount;
        }
        
        static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!sender.Owner.IsMe)
            return;
            if (args.Slot == SpellSlot.W)
            {
            if(Items.HasItem((int)ItemId.Ravenous_Hydra_Melee_Only) && Items.CanUseItem((int)ItemId.Ravenous_Hydra_Melee_Only))
            Items.UseItem((int)ItemId.Ravenous_Hydra_Melee_Only);
            if(Items.HasItem((int)ItemId.Tiamat_Melee_Only) && Items.CanUseItem((int)ItemId.Tiamat_Melee_Only))
            Items.UseItem((int)ItemId.Tiamat_Melee_Only);
            }
        }
        
        static void OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            if (sender == null || args.TargetNetworkId != sender.NetworkId)
            return;
            
            var Sender = (Obj_AI_Base)sender;
            
            if ((int) args.Type != 70)
            return;
            
            if(Qtimer > Utils.GameTimeTickCount - 120)
            {
                Qmove = false;
                MotionCancle();
            }
        }
        
        static void MotionCancle()
        {
            if(Qmove)
            return;
            var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);
            var M = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            var Target = (qTarget != null ? qTarget : M[0]);
            
            if (Player.Distance(Target.Position) < 250)
            {
                var SP = Player.ServerPosition.Extend(Target.ServerPosition, 150);
                Player.IssueOrder(GameObjectOrder.MoveTo, SP);
                Orbwalking.ResetAutoAttackTimer2();
            }
            else
            {
                var CP = Player.ServerPosition.Extend(Game.CursorPos, 200);
                Player.IssueOrder(GameObjectOrder.MoveTo, CP);
                Orbwalking.ResetAutoAttackTimer2();
            }
            Qmove = true;
        }
        static void OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args) 
        {
            if (!sender.IsMe || (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)) return;
            var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);
            var M = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            var Target = (qTarget != null ? qTarget : M[0]);
            if (args.Animation.Contains("Spell1"))
            {
                Utility.DelayAction.Add(125 + Game.Ping/2, MotionCancle);
            }
            if (Qmove && args.Animation.Contains("Run") && Target != null)
            {
                Qmove = false;
                Orbwalking.LastAATick = Utils.GameTimeTickCount + Game.Ping/2;
                Player.IssueOrder(GameObjectOrder.AttackUnit, qTarget);
            }
            if (Qmove && args.Animation.Contains("Idle") && Target != null)
            {
                Qmove = false;
                Orbwalking.LastAATick = Utils.GameTimeTickCount + Game.Ping/2;
                Player.IssueOrder(GameObjectOrder.AttackUnit, qTarget);
            }
        }
        static void AA()
        {
            if(Qtimer < Utils.GameTimeTickCount - 150)
            AIO_Func.AACb(Q,0,0,0);
        }
        
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || Target == null)
                return;
            if(Qtimer < Utils.GameTimeTickCount - 150) // && NextQCastAllowed
            AIO_Func.AALcJc(Q,0,0,0);
            if(!utility.Activator.AfterAttack.AIO)
            AA();
        }
        
        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                var rTarget = TargetSelector.GetTarget(W.Range, R.DamageType, true);
                if(rTarget != null && !Player.HasBuff("rivenwindslashready"))
                R.Cast();
                if(Player.HasBuff("rivenwindslashready") && AIO_Menu.Champion.Combo.getBoolValue("Use Auto R2"))
                {
                    foreach (var target in HeroManager.Enemies.Where(x => x.Distance(Player.ServerPosition) >= RD).OrderByDescending(x => x.Health))
                    {
                        if (R.CanCast(target) && AIO_Func.isKillable(target, R))
                            R.Cast(target);
                    }
                }
            }
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady() && AIO_Menu.Champion.Misc.getBoolValue("Inteligent Q"))
            {
                var qTarget = TargetSelector.GetTarget(Q.Range+40, Q.DamageType, true);
                if(qTarget != null && (qTarget.Distance(Player.ServerPosition) > Orbwalking.GetRealAutoAttackRange(Player) + 90 || Qtimer < Utils.GameTimeTickCount - 1200))
                Q.Cast(qTarget.ServerPosition);
            }
        }
        
        static void Harass()
        {
            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady() && AIO_Menu.Champion.Misc.getBoolValue("Inteligent Q"))
            {
                var qTarget = TargetSelector.GetTarget(Q.Range+40, Q.DamageType, true);
                if(qTarget != null && (qTarget.Distance(Player.ServerPosition) > Orbwalking.GetRealAutoAttackRange(Player) + 90 || Qtimer < Utils.GameTimeTickCount - 1200))
                Q.Cast(qTarget.ServerPosition);
            }
        }

        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.Where(x => x.Distance(Player.ServerPosition) >= RD).OrderByDescending(x => x.Health))
            {
                if (R.CanCast(target) && AIO_Func.isKillable(target, R) && E.IsReady())
                {
                    E.Cast(target.ServerPosition);
                    if(!Player.HasBuff("rivenwindslashready"))
                    R.Cast();
                    else
                    R.Cast(target);
                }
                else if (R.CanCast(target) && AIO_Func.isKillable(target, R))
                {
                    if(!Player.HasBuff("rivenwindslashready"))
                    R.Cast();
                    else
                    R.Cast(target);
                }
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage2(enemy)*3 + (float)Player.GetAutoAttackDamage2(enemy, false)*4;
            
            if (W.IsReady())
                damage += W.GetDamage2(enemy);
            
            if (E.IsReady())
                damage += E.GetDamage2(enemy);
                
            if (R.IsReady())
                damage += R.GetDamage2(enemy);
            return damage;
        }
    }
}
