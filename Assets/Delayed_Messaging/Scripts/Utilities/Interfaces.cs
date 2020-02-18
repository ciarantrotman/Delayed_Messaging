namespace Delayed_Messaging.Scripts.Utilities
{
    public interface ISelectable
    {
        void HoverStart();
        void HoverStay();
        void HoverEnd();
        void SelectStart();
        void SelectStay();
        void SelectEnd();
    }

    public interface IMovable
    {
        void Move();
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
