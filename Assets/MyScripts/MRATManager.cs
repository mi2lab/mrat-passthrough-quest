using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRATManager : MonoBehaviour
{
    [Header("Tracking targets")]
    public GameObject ovrCameraRig;
    public GameObject centerEyeAnchor;

    [Space(10)]
    [Header("Passthrough")]
    public GameObject virtualEnv;
    public GameObject phone;
    public GameObject fov;
    public string[] fovMaskTags;
    //public GameObject fovCam;

    [Space(10)]
    [Header("Recording and Replaying")]
    public float recordingDeltaTime = 0.1f;
    public bool trackHands = true;

    [Space(10)]
    [Header("Live Sync")]
    public bool liveSyncTrackHands = true;

    [Space(10)]
    [Header("Hand Tracking")]
    public string targetLeftHandName = "L_Hand_MRTK_Rig";
    public string targetRightHandName = "R_Hand_MRTK_Rig";

    [Space(10)]
    [Header("Function testing")]
    public bool headRec;
    public bool increaseRecordingId;
    public bool decreaseRecordingId;
    public bool headReplay;
    public bool pauseReplay;
    public bool syncDatabase;
    public bool upSync;
    public bool downSync;
    public bool startLogin;
    public string userNameInput;
    public bool finishLogin;

    [Space(10)]
    [Header("Indicators")]
    public float recordingDeltaTimeInd;
    public bool recordingTrackHands;
    public bool isRecording;
    public bool isReplaying;
    public bool replayPaused;
    public int currentRecordingId;
    public bool upSyncing;
    public bool downSyncing;
    public bool liveSyncTrackHandsInd;
    public string userName;

    [Space(10)]
    [Header("Prefabs")]
    public GameObject headPrefab;
    public GameObject jointPrefab;
    public GameObject jointTipPrefab;

    [Space(10)]
    [Header("Managers")]
    public GameObject passthroughManager;
    public GameObject arModeManager;
    public GameObject recordReplayManager;
    public GameObject databaseManager;
    public GameObject controlManager;
    public GameObject applicationManager;

    private PassthroughManager pM;
    private ARModeManager armM;
    private Recorder recE;
    private RecordingDatabse recDB;
    private Replayer repE;
    private ReplayerControl repC;
    private HandTracker hT;
    private RecordReplayManager rrM;
    private DatabaseSync sDB;
    private LiveDemonstrator liveD;
    private AccountLogger accL;

    private bool set = false;

    void Awake()
    {
        // Initialization
        pM = passthroughManager.GetComponent<PassthroughManager>();
        pM.passthrough = ovrCameraRig.GetComponent<OVRPassthroughLayer>();

        recE = recordReplayManager.GetComponent<Recorder>();
        recDB = databaseManager.GetComponent<RecordingDatabse>();
        repE = recordReplayManager.GetComponent<Replayer>();
        repC = recordReplayManager.GetComponent<ReplayerControl>();
        rrM = recordReplayManager.GetComponent<RecordReplayManager>();
        hT = databaseManager.GetComponent<HandTracker>();

        sDB = databaseManager.GetComponent<DatabaseSync>();
        accL = databaseManager.GetComponent<AccountLogger>();
        liveD = databaseManager.GetComponent<LiveDemonstrator>();

        armM = arModeManager.GetComponent<ARModeManager>();

        pM.VirtualEnv = virtualEnv;

        armM.phone = phone;
        armM.fov = fov;
        armM.fovCam = centerEyeAnchor.GetComponent<Camera>();
        armM.masks = fovMaskTags;

        recE.target = centerEyeAnchor;
        recE.replayer = repE;
        recE.database = recDB;
        recE.handTracker = hT;
        recE.trackHands = trackHands;
        recE.recordDeltaTime = recordingDeltaTime;

        recDB.database = sDB;

        repE.control = repC;
        repE.database = recDB;
        repE.HeadPrefab = headPrefab;
        repE.handJointPrefab = jointPrefab;
        repE.handJointTipPrefab = jointTipPrefab;

        repC.replayer = repE;

        rrM.recorder = recE;
        rrM.replayer = repE;

        
        sDB.target = centerEyeAnchor;
        sDB.demo = liveD;
        sDB.logger = accL;
        sDB.handTracker = hT;
        sDB.trackHands = liveSyncTrackHands;

        liveD.prefab = headPrefab;
        liveD.handJointPrefab = jointPrefab;
        liveD.handJointTipPrefab = jointTipPrefab;

        hT.targetLHandName = targetLeftHandName;
        hT.targetRHandName = targetRightHandName;
    }

    void Update()
    {
        /*
        if (!set)
        {
            InitAll();
            set = true;
        }
        */

        // Testings
        if (headRec)
        {
            rrM.ToggleRecording();
            headRec = false;
        }
        if (increaseRecordingId)
        {
            repC.IncreaseId();
            increaseRecordingId = false;
        }
        if (decreaseRecordingId)
        {
            repC.DecreaseId();
            increaseRecordingId = false;
        }
        if (headReplay)
        {
            rrM.ToggleReplay();
        }
        if (pauseReplay)
        {
            rrM.TogglePause();
        }
        if (syncDatabase)
        {
            rrM.UpdateReplay();
        }
        if (upSync)
        {
            sDB.ToggleSync();
            upSync = false;
        }
        if (downSync)
        {
            sDB.ToggleDownSync();
            downSync = false;
        }
        if (startLogin)
        {
            sDB.ToggleLogIn();
            startLogin = false;
        }
        if (finishLogin)
        {
            accL.inputText = userNameInput;
            accL.ToggleFinished();
            startLogin = false;
        }

        // Indicators
        recordingDeltaTimeInd = recE.recordDeltaTime;
        recordingTrackHands = recE.trackHands;
        isRecording = recE.isRecording();
        currentRecordingId = repC.currentId;
        isReplaying = repE.isReplaying();
        replayPaused = repE.isPaused();
        upSyncing = sDB.isUpSynchronizing();
        downSyncing = sDB.isDownSynchronizing();
        liveSyncTrackHandsInd = sDB.trackHands;
        userName = sDB.personalId;
    }
}
