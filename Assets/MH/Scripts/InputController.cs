namespace MH
{
    /// <summary>
    ///
    /// </summary>
    public sealed class InputController
    {
        public static MHInputActions InputActions { private set; get; }

        public static void Setup()
        {
            InputActions = new MHInputActions();
        }
    }
}
