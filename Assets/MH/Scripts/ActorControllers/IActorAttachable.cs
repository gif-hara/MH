namespace MH
{
    /// <summary>
    /// <see cref="Actor"/>にアタッチ可能なオブジェクトのインターフェイス
    /// </summary>
    public interface IActorAttachable
    {
        /// <summary>
        /// アタッチする
        /// </summary>
        void Attach(Actor actor);
    }
}
