using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordReplayManager : MonoBehaviour
{

    public Recorder recorder;
    public Replayer replayer;

    public void ToggleRecording()
    {
        if (!recorder.isRecording())
        {
            recorder.StartRecording();
        }
        else
        {
            recorder.StopRecording();
        }
    }
    public void ToggleReplay()
    {
        if (!replayer.isReplaying())
        {
            replayer.StartReplay();
        }
        else
        {
            replayer.StopReplay();
        }
    }

    public void TogglePause()
    {
        if (replayer.isReplaying())
        {
            replayer.PauseReplay();
        }
    }

    public void UpdateReplay()
    {
        replayer.UpdateReplay();
    }

    public void DeleteReplay()
    {
        replayer.DeleteReplay();
    }

    public void Start()
    {
        UpdateReplay();
    }

}
