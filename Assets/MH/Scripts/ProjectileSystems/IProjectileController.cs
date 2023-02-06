using MH.ActorControllers;

namespace MH.ProjectileSystems
{
    /// <summary>
    /// <see cref="Projectile"/>を制御するインターフェイス
    /// </summary>
    public interface IProjectileController
    {
        void Setup(Projectile projectile, ProjectileData data, Actor actor);
    }
}
