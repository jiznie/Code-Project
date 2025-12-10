using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SplashController : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene(1); // Load MainMenu (scene index 1)
    }
}
