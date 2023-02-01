using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace MH.UISystems
{
    /// <summary>
    /// </summary>
    public sealed class UIManager : MonoBehaviour
    {
        [SerializeField]
        private RectTransform uiParent;

        [SerializeField]
        private DebugPanelUIView debugPanelUIView;

        private DebugPanelUIPresenter debugPanelUIPresenter;

        public static UIManager Instance { get; private set; }

        public static async UniTask SetupAsync()
        {
            Assert.IsNull(Instance);

            var prefab = await AssetLoader.LoadAsync<GameObject>("Assets/MH/Prefabs/UI/UIManager.prefab");
            Instance = Instantiate(prefab).GetComponent<UIManager>();
            DontDestroyOnLoad(Instance);

            Instance.debugPanelUIPresenter = new DebugPanelUIPresenter(Instance.debugPanelUIView, Instance.gameObject);
        }

        public static T Open<T>(T uiViewPrefab) where T : UIView
        {
            return Instantiate(uiViewPrefab, Instance.uiParent);
        }

        public static void Close(UIView uiView)
        {
            Destroy(uiView.gameObject);
        }

        public static void Show(UIView uiView)
        {
            uiView.gameObject.SetActive(true);
        }

        public static void Hidden(UIView uiView)
        {
            uiView.gameObject.SetActive(false);
        }

        public static void SetAsLastSibling(UIView uiView)
        {
            uiView.transform.SetAsLastSibling();
        }
    }
}
