using UnityEngine;
using System.Collections.Generic;
using PvZ.Core;

namespace PvZ.Zombies
{
    public class ZombieStateMachine
    {
        private Dictionary<ZombieState, IZombieState> states;
        private IZombieState currentState;
        private ZombieController owner;
        
        public ZombieState CurrentStateType { get; private set; }
        public IZombieState CurrentState => currentState;
        
        public ZombieStateMachine(ZombieController owner)
        {
            this.owner = owner;
            InitializeStates();
        }
        
        private void InitializeStates()
        {
            states = new Dictionary<ZombieState, IZombieState>
            {
                { ZombieState.Walking, new WalkingState() },
                { ZombieState.Eating, new EatingState() },
                { ZombieState.Attacking, new AttackingState() },
                { ZombieState.Dying, new DyingState() },
                { ZombieState.Special, new SpecialState() }
            };
        }
        
        public void Start(ZombieState initialState)
        {
            ChangeState(initialState);
        }
        
        public void Update()
        {
            currentState?.Update(owner);
        }
        
        public void ChangeState(ZombieState newState)
        {
            if (currentState != null && CurrentStateType == newState)
                return;
            
            currentState?.Exit(owner);
            
            CurrentStateType = newState;
            currentState = states[newState];
            
            currentState?.Enter(owner);
        }
        
        public bool CanTransitionTo(ZombieState newState)
        {
            // Add logic for valid state transitions
            switch (CurrentStateType)
            {
                case ZombieState.Dying:
                    return false; // Can't transition from dying
                case ZombieState.Special:
                    return newState == ZombieState.Walking || newState == ZombieState.Dying;
                default:
                    return true;
            }
        }
    }
    
    public interface IZombieState
    {
        void Enter(ZombieController zombie);
        void Update(ZombieController zombie);
        void Exit(ZombieController zombie);
    }
    
    // Walking State
    public class WalkingState : IZombieState
    {
        public void Enter(ZombieController zombie)
        {
            zombie.SetAnimationTrigger("Walk");
        }
        
        public void Update(ZombieController zombie)
        {
            // Check for plants to eat
            var nearbyPlant = zombie.FindNearbyPlant();
            if (nearbyPlant != null)
            {
                zombie.StateMachine.ChangeState(ZombieState.Eating);
                return;
            }
            
            // Continue walking
            zombie.MoveForward();
            
            // Check if reached end of lane
            if (zombie.HasReachedEnd())
            {
                zombie.ReachHouse();
            }
        }
        
        public void Exit(ZombieController zombie)
        {
            // Stop movement
        }
    }
    
    // Eating State
    public class EatingState : IZombieState
    {
        private float eatTimer;
        
        public void Enter(ZombieController zombie)
        {
            zombie.SetAnimationTrigger("Eat");
            eatTimer = 0f;
        }
        
        public void Update(ZombieController zombie)
        {
            var nearbyPlant = zombie.FindNearbyPlant();
            
            if (nearbyPlant == null)
            {
                // No plant to eat, go back to walking
                zombie.StateMachine.ChangeState(ZombieState.Walking);
                return;
            }
            
            eatTimer += Time.deltaTime;
            
            if (eatTimer >= 1f) // Default eat speed: 1 attack per second
            {
                // Deal damage to plant
                nearbyPlant.TakeDamage(zombie.ZombieData.damage, zombie);
                zombie.PlayEatSound();
                eatTimer = 0f;
            }
        }
        
        public void Exit(ZombieController zombie)
        {
            eatTimer = 0f;
        }
    }
    
    // Attacking State (for ranged zombies)
    public class AttackingState : IZombieState
    {
        private float attackTimer;
        
        public void Enter(ZombieController zombie)
        {
            zombie.SetAnimationTrigger("Attack");
            attackTimer = 0f;
        }
        
        public void Update(ZombieController zombie)
        {
            // Projectile system simplified - switch to melee combat
            zombie.StateMachine.ChangeState(ZombieState.Eating);
            return;
            
            var target = zombie.FindAttackTarget();
            if (target == null)
            {
                zombie.StateMachine.ChangeState(ZombieState.Walking);
                return;
            }
            
            attackTimer += Time.deltaTime;
            
            if (attackTimer >= 1f) // Default attack speed: 1 attack per second
            {
                zombie.LaunchProjectile(target);
                attackTimer = 0f;
            }
        }
        
        public void Exit(ZombieController zombie)
        {
            attackTimer = 0f;
        }
    }
    
    // Dying State
    public class DyingState : IZombieState
    {
        public void Enter(ZombieController zombie)
        {
            zombie.SetAnimationTrigger("Die");
            zombie.PlayDeathSound();
            zombie.DropRewards();
        }
        
        public void Update(ZombieController zombie)
        {
            // Wait for death animation to complete
            if (zombie.IsDeathAnimationComplete())
            {
                zombie.DestroyZombie();
            }
        }
        
        public void Exit(ZombieController zombie)
        {
            // Clean up
        }
    }
    
    // Special State (for special abilities)
    public class SpecialState : IZombieState
    {
        public void Enter(ZombieController zombie)
        {
            zombie.SetAnimationTrigger("Special");
        }
        
        public void Update(ZombieController zombie)
        {
            // Handle special ability logic
            // This would be customized based on specific zombie abilities
        }
        
        public void Exit(ZombieController zombie)
        {
            // Return to walking after special ability
        }
    }
}
