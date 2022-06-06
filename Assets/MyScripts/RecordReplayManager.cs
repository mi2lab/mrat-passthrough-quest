using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordReplayManager : MonoBehaviour
{

    public Recorder recorder;
    public Replayer replayer;

    public bool test_replay = false;
    public bool test_recording = false;
    public bool test_replay_pause = false;
    public bool test_sync_database = false;

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

    public void Update()
    {
        if (test_replay)
        {
            ToggleReplay();
            test_replay = false;
        }
        if (test_replay_pause)
        {
            TogglePause();
            test_replay_pause = false;
        }
        if (test_recording)
        {
            ToggleRecording();
            test_recording = false;
        }
        if (test_sync_database)
        {
            UpdateReplay();
            test_sync_database = false;
        }
    }

}
