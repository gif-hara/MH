using StandardAssets.Characters.Physics;

namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>の依存性を注入するインターフェイス
    /// </summary>
    public interface IActorDependencyInjector
    {
        AnimationController AnimationController { get; }

        BoneHolder BoneHolder { get; }

        OpenCharacterController OpenCharacterController { get; }
    }
}
