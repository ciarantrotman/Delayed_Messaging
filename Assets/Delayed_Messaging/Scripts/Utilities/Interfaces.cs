namespace Delayed_Messaging.Scripts.Utilities
{
    public interface ISelectable
    {
        void Select();
    }
    
    public interface IDamageable<in T>
    {
        void Damage(T damageTaken);
    }

    public interface ISpawnable
    {
        void Spawn();
    }

    public interface IPlaceable
    {
        void Place();
    }
}
