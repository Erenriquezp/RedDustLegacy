using UnityEngine;
public class MainMenuController : MonoBehaviour
{
   public void OnPlayPressed()
   {
      SceneLoader.Instance.LoadLevel01();
   }

   public void OnQuitPressed()
   {
      #if UNITY_EDITOR
         UnityEditor.EditorApplication.isPlaying = false;
      #else
         Application.Quit();
      #endif
   }
}