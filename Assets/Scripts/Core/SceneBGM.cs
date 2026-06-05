using UnityEngine;

public class SceneBGM : MonoBehaviour
{
    [Header("BGM Configuration")]
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool stopPreviousMusic = true;

    private void Start()
    {
        if (AudioManager.Instance != null && bgmClip != null)
        {
            AudioManager.Instance.PlayMusic(bgmClip, loop);
        }
        else if (AudioManager.Instance != null && stopPreviousMusic)
        {
            AudioManager.Instance.StopMusic();
        }
    }
}
