using BrannPack.AbilityHandling;
using BrannPack.Character;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.AIBehavior
{
    public partial class EnemyAI : Node
    {
        private enum AIState
        {
            Idle,
            Chase,
            Attack,
            Flee,
            Patrol
        }

        public CharacterMaster Master;
        //public ITargetSelector TargetSelector = new NearestTargetSelector(); // Default

        private BaseCharacterBody currentTarget;
        private AIState currentState;
        private Ability selectedAbility;

        public override void _Process(double delta)
        {
            //List<Character> visibleEnemies = FindVisibleTargets(); // You'll define this
           // currentTarget = TargetSelector.GetTarget(this, visibleEnemies);

            if (currentTarget == null)
            {
                currentState = AIState.Idle;
                return;
            }

            //float distanceToTarget = (currentTarget.GlobalPosition - OwnerMaster.GlobalPosition).Length();
            //selectedAbility = GetUsableAbility(distanceToTarget);
            //UpdateState(selectedAbility, distanceToTarget);
            ActBasedOnState();
        }

        private List<BaseCharacterBody> FindVisibleTargets()
        {
            // This should filter the characters that are on opposing teams, not dead, etc.
            return GetTree().GetNodesInGroup("Players") // Or "Characters"
                .OfType<BaseCharacterBody>()
                .Where(c => c.CharacterMaster.IsAlive && c.CharacterMaster.Team != Master.Team)
                .ToList();
        }

        private void ActBasedOnState()
        {
            switch (currentState)
            {
                case AIState.Chase:
                    MoveTowardTarget();
                    break;
                case AIState.Flee:
                    MoveAwayFromTarget();
                    break;
                case AIState.Attack:
                    //if (selectedAbility != null)
                        //OwnerMaster.TryUseAbility(selectedAbility);
                    break;
                default:
                    break;
            }
        }

        private void MoveTowardTarget()
        {
            // Move toward player
        }

        private void MoveAwayFromTarget()
        {
            // Move away from player
        }
    }

    public interface ITargetSelector
    {
        BaseCharacterBody GetTarget(EnemyAI self, List<BaseCharacterBody> potentialTargets);
    }

    //public class NearestTargetSelector : ITargetSelector
    //{
    //    public BaseCharacterBody GetTarget(EnemyAI self, List<BaseCharacterBody> potentialTargets)
    //    {
    //        return potentialTargets
    //            .OrderBy(t => (t.GlobalPosition - self.OwnerMaster.Body.GlobalPosition).LengthSquared())
    //            .FirstOrDefault();
    //    }
    //}


}
