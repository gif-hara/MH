using Cysharp.Threading.Tasks;

namespace MH.ProjectileSystems.Decorators
{
    /// <summary>
    /// <see cref="Projectile"/>に対して何かしらの装飾を行うインターフェイス
    /// </summary>
    public interface IProjectileDecorator
    {
        UniTaskVoid Decorate(Projectile projectile);
    }
}
