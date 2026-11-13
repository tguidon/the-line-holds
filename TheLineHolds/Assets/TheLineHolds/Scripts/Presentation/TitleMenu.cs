using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheLineHolds
{
    public sealed class TitleMenu : MonoBehaviour
    {
        public const string SceneName = "MainMenu";
        public Button countryside, winterForest, quit;
        public RectTransform content;
        bool deploying;

        void Start()
        {
            Application.targetFrameRate=60;
            countryside.onClick.AddListener(()=>Deploy("Countryside"));
            winterForest.onClick.AddListener(()=>Deploy("WinterForest"));
            quit.onClick.AddListener(Quit);
            EventSystem.current.SetSelectedGameObject(countryside.gameObject);
            Fit();
        }

        void OnRectTransformDimensionsChange() { if(content)Fit(); }

        void Fit()
        {
            var area=((RectTransform)transform).rect;
            float scale=Mathf.Min(area.width/1400f,area.height/850f);
            content.localScale=new Vector3(scale,scale,1);
        }

        void Deploy(string scene)
        {
            if(deploying)return;
            deploying=true;
            SceneManager.LoadSceneAsync(scene,LoadSceneMode.Single);
        }

        void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
    }
}
