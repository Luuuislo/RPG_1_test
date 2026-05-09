# RPG Project — Claude Context

## Stack
- Unity 6
- C# / MonoBehaviour
- 2D Top-down RPG

## Project Structure
- Assets/Scripts/Player/     → Player.cs, DamageReceiverPlayer.cs
- Assets/Scripts/Enemy/      → Enemy.cs, RangedEnemy.cs, NPC.cs
- Assets/Scripts/Projectile/ → Projectile.cs
- Assets/Scripts/UI/         → UiManager.cs
- Assets/Scripts/Items/      →Inventario, items

## Architecture Rules
- DamageReceiverPlayer → solo recibe daño y delega a Player.TakeDamage()
- Player.cs → gestiona vida, muerte y respawn
- Enemy hereda de NPC
- RangedEnemy hereda de Enemy
- DamageReceiver → para enemigos y NPCs
- UiManager es Singleton → UiManager.Instance

## Conventions
- Español para nombres de variables y comentarios no, inglés para código
- Usar protected en Enemy para que hijos puedan sobreescribir
- Animator triggers en PascalCase: DoAttack, CastSpell, IsGuarding