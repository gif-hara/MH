using StandardAssets.Characters.Physics;

namespace MH.ActorControllers
{
    /// <summary>
    /// <see cref="Actor"/>の依存性を注入するインターフェイス
    /// </summary>
    public interface IActorDependencyInjector
    {
        AnimationController AnimationController { get; }

        ModelDataHolder ModelDataHolder { get; }

        OpenCharacterController OpenCharacterController { get; }
    }
}
