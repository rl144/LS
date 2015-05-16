using System;
using System.Linq;
using System.Collections.Generic;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One
{
    static class AIO_Pred
    {
        internal static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } } 
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } } 
        
        internal static float getHealthPercent(Obj_AI_Base unit)
        {
            return AIO_Func.getHealthPercent(unit);
        }

        internal static float getManaPercent(Obj_AI_Base unit)
        {
            return AIO_Func.getManaPercent(unit);
        }
        
        internal static float PredHealth(Obj_AI_Base Target, Spell spell)
        {
            return HealthPrediction.GetHealthPrediction(Target, (int)(Player.Distance(Target, false) / spell.Speed), (int)(spell.Delay * 1000 + Game.Ping / 2));
        }
        
        internal static void CCast(Spell spell, Obj_AI_Base target) //for Circular spells
        {
            if(spell.Type == SkillshotType.SkillshotCircle || spell.Type == SkillshotType.SkillshotCone) // Cone 스킬은 임시로
            {
                if(spell != null && target !=null)
                {
                    var pred = Prediction.GetPrediction(target, spell.Delay, spell.Width/2, spell.Speed);
                    SharpDX.Vector2 castVec = (pred.UnitPosition.To2D() + target.ServerPosition.To2D()) / 2 ;
                    SharpDX.Vector2 castVec2 = Player.ServerPosition.To2D() +
                                               SharpDX.Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * (spell.Range);
                    
                    if (target.IsValidTarget(spell.Range))
                    {
                        if(target.MoveSpeed*(Game.Ping/2000 + spell.Delay+Player.ServerPosition.Distance(target.ServerPosition)/spell.Speed) <= spell.Width*1/2)
                            spell.Cast(target.ServerPosition); //Game.Ping/2000  추가함.
                        else if(pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance && pred.UnitPosition.Distance(target.ServerPosition) < Math.Max(spell.Width,300f))
                        {
                            if(target.MoveSpeed*(Game.Ping/2000 + spell.Delay+Player.ServerPosition.Distance(target.ServerPosition)/spell.Speed) <= spell.Width*2/3 && castVec.Distance(pred.UnitPosition) <= spell.Width*1/2 && castVec.Distance(Player.ServerPosition) <= spell.Range)
                            {
                                spell.Cast(castVec);
                            }
                            else if(castVec.Distance(pred.UnitPosition) > spell.Width*1/2 && Player.ServerPosition.Distance(pred.UnitPosition) <= spell.Range)
                            {
                                spell.Cast(pred.UnitPosition);
                            }
                            else
                                spell.Cast(pred.CastPosition); // <- 별로 좋은 선택은 아니지만.. 
                        }
                    }
                    else if (target.IsValidTarget(spell.Range + spell.Width/2)) //사거리 밖 대상에 대해서
                    {
                        if(pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance && Player.ServerPosition.Distance(pred.UnitPosition) <= spell.Range+spell.Width*1/2 && pred.UnitPosition.Distance(target.ServerPosition) < Math.Max(spell.Width,300f))
                        {
                            if(Player.ServerPosition.Distance(pred.UnitPosition) <= spell.Range)
                            {
                                if(Player.ServerPosition.Distance(pred.CastPosition) <= spell.Range)
                                spell.Cast(pred.CastPosition);
                            }
                            else if(Player.ServerPosition.Distance(pred.UnitPosition) <= spell.Range+spell.Width*1/2 && target.MoveSpeed*(Game.Ping/2000 + spell.Delay+Player.ServerPosition.Distance(target.ServerPosition)/spell.Speed) <= spell.Width/2)
                            {
                                if(Player.Distance(castVec2) <= spell.Range)
                                spell.Cast(castVec2);
                            }
                        }
                    }
                }
            }
        }
        
        internal static void LCast(Spell spell, Obj_AI_Base target, float alpha = 0f, float colmini = float.MaxValue, bool HeroOnly = false) //for Linar spells  사용예시 AIO_Func.LCast(Q,Qtarget,50,0)  
        {                            //        AIO_Func.LCast(E,Etarget,Menu.Item("Misc.Etg").GetValue<Slider>().Value,float.MaxValue); <- 이런식으로 사용.
            if(spell.Type == SkillshotType.SkillshotLine)
            {
                if(spell != null && target !=null)
                {
                    var pred = Prediction.GetPrediction(target, spell.Delay, spell.Width/2, spell.Speed); //spell.Width/2
                    var collision = spell.GetCollision(Player.ServerPosition.To2D(), new List<SharpDX.Vector2> { pred.CastPosition.To2D() });
                    //var minioncol = collision.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);
                    var minioncol = collision.Count(x => (HeroOnly == false ? x.IsMinion : (x is Obj_AI_Hero)));

                    if (target.IsValidTarget(spell.Range - target.MoveSpeed * (spell.Delay + Player.Distance(target.ServerPosition) / spell.Speed) + alpha) && minioncol <= colmini && pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
                    {
                        spell.Cast(pred.CastPosition);
                    }
                }
            }
        }
        
        internal static void AtoB(Spell spell, Obj_AI_Base T, float Drag = 700f) //Coded By RL244 AtoB Drag 기본값 700f는 빅토르를 위한 것임.
        {
            if(T != null)
            {
                var T2 = HeroManager.Enemies.Where(x => x != T && AIO_Func.CanHit(spell,x,Drag)).FirstOrDefault();
                var pred = Prediction.GetPrediction(T, spell.Delay, spell.Width/2, spell.Speed);
                var T2pred = Prediction.GetPrediction(T2, spell.Delay, spell.Width/2, spell.Speed);
                SharpDX.Vector2 castVec = (pred.UnitPosition.To2D() + T.ServerPosition.To2D()) / 2 ;
                SharpDX.Vector2 castVec2 = Player.ServerPosition.To2D() +
                                           SharpDX.Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * (spell.Range);
                SharpDX.Vector2 castVec3 = T.ServerPosition.To2D() -
                                           SharpDX.Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * (40f);
                if(pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
                {
                    if(T.Distance(Player.ServerPosition) >= spell.Range)
                    {
                        if(AIO_Func.CanHit(spell,T,Drag) && T2 == null && pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
                        spell.Cast(castVec2,pred.UnitPosition.To2D());
                        else //if(AIO_Func.CanHit(spell,T,Drag) && T2 != null && T2pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)//별로 좋은 생각이 더 안나고 피곤해서 걍관둠.
                        {
                        spell.Cast(castVec2,T.ServerPosition.To2D());//별로 좋은 생각이 더 안나고 피곤해서 걍관둠.
                        }
                    }
                    else
                    {
                        if(T2 == null || !AIO_Func.CanHit(spell,T2,Drag))
                        spell.Cast(castVec3,T.ServerPosition.To2D());
                        else if(T2 != null && AIO_Func.CanHit(spell,T2,Drag) && T2pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
                        {
                            SharpDX.Vector2 castVec4 = T.ServerPosition.To2D() -
                                                       SharpDX.Vector2.Normalize(T2pred.UnitPosition.To2D() - T.ServerPosition.To2D()) * (40f);
                            spell.Cast(castVec4,T2pred.UnitPosition.To2D());
                        }
                    }
                }
            }
        }
		
        internal static void RMouse(Spell spell)
        {
            SharpDX.Vector2 ReverseVec = Player.ServerPosition.To2D() -
                                       SharpDX.Vector2.Normalize(Game.CursorPos.To2D() - Player.Position.To2D()) * (spell.Range);
            if(spell.IsReady())
            spell.Cast(ReverseVec);
        }
        
        internal static void FleeToPosition(Spell spell, string W = "N") // N 정방향, R 역방향.
        {
            bool FM = true;
            if (Menu.Item("Flee.If Mana >" + spell.Slot.ToString(), true) != null)
            {
                FM = getManaPercent(Player) > AIO_Menu.Champion.Flee.IfMana;
            }
            else
            {
                FM = true;
            }
            SharpDX.Vector2 NormalVec = Player.ServerPosition.To2D() +
                                       SharpDX.Vector2.Normalize(Game.CursorPos.To2D() - Player.Position.To2D()) * (spell.Range);
            if(Menu.Item("Flee.Use " + spell.Slot.ToString(), true) != null && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Flee)
            {
                if(Menu.Item("Flee.Use " + spell.Slot.ToString(), true).GetValue<bool>() && spell.IsReady())
                {
                    if(W == "N")
                    spell.Cast(NormalVec);
                    else
                    RMouse(spell);
                }
            }
        }
        
    }
}