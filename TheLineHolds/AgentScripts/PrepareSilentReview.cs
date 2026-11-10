public static class PrepareSilentReview
{
 public static string All(){UnityEngine.AudioListener.volume=0;UnityEngine.Application.runInBackground=true;return "Agent editor review muted; frame="+UnityEngine.Time.frameCount;}
}
