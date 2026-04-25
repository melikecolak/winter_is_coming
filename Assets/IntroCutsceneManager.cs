using UnityEngine;
using UnityEngine.Playables;

public class IntroCutsceneManager : MonoBehaviour
{
    public PlayableDirector timeline;
    public GameObject[] introCameras;

    void Start()
    {
        timeline.stopped += OnTimelineStopped;
    }

    void OnTimelineStopped(PlayableDirector director)
    {
        foreach (GameObject cam in introCameras)
        {
            cam.SetActive(false);
        }
    }
}