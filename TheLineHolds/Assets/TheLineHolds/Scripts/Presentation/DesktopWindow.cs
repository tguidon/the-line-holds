using UnityEngine;
namespace TheLineHolds
{
    // Migrate the prototype's old saved window size once; later user resizing remains their choice.
    public static class DesktopWindow
    {
        const string SizeVersion="DioramaWindowSizeV1";
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ApplyDefault()
        {
#if !UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN)
            if(PlayerPrefs.GetInt(SizeVersion,0)!=0)return;
            var display=Screen.currentResolution;
            int width=Mathf.Min(1920,Mathf.RoundToInt(display.width*.9f));
            int height=Mathf.Min(1080,Mathf.RoundToInt(display.height*.85f));
            Screen.SetResolution(width,height,FullScreenMode.Windowed);
            PlayerPrefs.SetInt(SizeVersion,1);PlayerPrefs.Save();
#endif
        }
    }
}
