using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadMainMenu()    => LoadScene("MainMenu");
    public void LoadLevel01()     => LoadScene("Level01");
    public void LoadLevel02()     => LoadScene("Level02");
    public void LoadNextLevel()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            LoadScene(next);
        else
            LoadMainMenu(); // si no hay más niveles, vuelve al menú
    }
    public void ReloadCurrentScene() =>
        LoadScene(SceneManager.GetActiveScene().buildIndex);

    private void LoadScene(string name)  => SceneManager.LoadScene(name);
    private void LoadScene(int index)    => SceneManager.LoadScene(index);
}