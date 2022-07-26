using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarUpdateManager : MonoBehaviour
{
    public Transform trackingSpace;
    public Transform headTarget;
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    private Coroutine headCo;
    private bool headCoRunning = false;
    private Coroutine leftCo;
    private bool leftCoRunning = false;
    private Coroutine rightCo;
    private bool rightCoRunning = false;
    private Coroutine trackingCo;
    private bool trackingCoRunning = false;

    IEnumerator moveTowards(GameObject obj, HeadPos tar, float delta_time)
    {
        Vector3 localPos = tar.PosToVec();
        Quaternion localRot = tar.RotToQuat();
        Vector3 initPos = obj.transform.position;
        Quaternion initRot = obj.transform.rotation;
        float localTime = 0;
        while (localTime < delta_time)
        {
            //Debug.Log("#");
            //Debug.Log(localTime);
            //Debug.Log(delta_time);
            obj.transform.position = Vector3.Lerp(initPos, localPos, localTime / delta_time);
            obj.transform.rotation = Quaternion.Lerp(initRot, localRot, localTime / delta_time);
            localTime += Time.deltaTime;
            yield return null;// new WaitForSeconds(Time.deltaTime);
        }
        //break;
    }

    public void UpdateAvatar(Vector3 headPos, Quaternion headRot, Vector3 leftPos, Quaternion leftRot, Vector3 rightPos, Quaternion rightRot, Vector3 trackingPos, Quaternion trackingRot, float delta_time)
    {
        if (headCoRunning)
        {
            StopCoroutine(headCo);
        }
        headCo = StartCoroutine(moveTowards(headTarget.gameObject, new HeadPos(headPos, headRot), delta_time));

        if (leftPos != null && leftRot != null)
        {
            if (leftCoRunning)
            {
                StopCoroutine(leftCo);
            }
            //float y_angle_left = leftRot.eulerAngles.y + 90 > 360 ? leftRot.eulerAngles.y - 270 : leftRot.eulerAngles.y + 90;
            float y_angle_left = leftRot.eulerAngles.y + 270 > 360 ? leftRot.eulerAngles.y - 90 : leftRot.eulerAngles.y + 270;
            //float z_angle_left = leftRot.eulerAngles.x + 90 > 360 ? leftRot.eulerAngles.x - 270 : leftRot.eulerAngles.x + 90;
            float z_angle_left = leftRot.eulerAngles.x + 90 > 360 ? leftRot.eulerAngles.x - 270 : leftRot.eulerAngles.x + 90;
            Quaternion leftRotIn = Quaternion.Euler(leftRot.eulerAngles.z, y_angle_left, z_angle_left);
            leftCo = StartCoroutine(moveTowards(leftHandTarget.gameObject, new HeadPos(leftPos, leftRotIn), delta_time));
        }
        else
        {
            if (leftCoRunning)
            {
                StopCoroutine(leftCo);
            }
            Vector3 localLeftPos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Quaternion localLeftRot = new Quaternion(0, 0, 0, 0);
            leftCo = StartCoroutine(moveTowards(leftHandTarget.gameObject, new HeadPos(localLeftPos, localLeftRot), delta_time));
        }

        if (rightPos != null && rightRot != null)
        {
            if (rightCoRunning)
            {
                StopCoroutine(rightCo);
            }
            float y_angle_right = rightRot.eulerAngles.y + 90 > 360 ? rightRot.eulerAngles.y - 270 : rightRot.eulerAngles.y + 90;
            float z_angle_right = rightRot.eulerAngles.x + 90 > 360 ? rightRot.eulerAngles.x - 270 : rightRot.eulerAngles.x + 90;
            //Debug.Log("#");
            //Debug.Log(rightRot.eulerAngles.y);
            //Debug.Log(y_angle);
            //Debug.Log(rightRot.eulerAngles.z);
            //Debug.Log(z_angle);
            Quaternion rightRotIn = Quaternion.Euler(-rightRot.eulerAngles.z, y_angle_right, z_angle_right);
            rightCo = StartCoroutine(moveTowards(rightHandTarget.gameObject, new HeadPos(rightPos, rightRotIn), delta_time));
        }
        else
        {
            if (rightCoRunning)
            {
                StopCoroutine(rightCo);
            }
            Vector3 localRightPos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Quaternion localRightRot = new Quaternion(0, 0, 0, 0);
            rightCo = StartCoroutine(moveTowards(rightHandTarget.gameObject, new HeadPos(localRightPos, localRightRot), delta_time));
        }
        Debug.Log(rightPos);

        if (trackingCoRunning)
        {
            StopCoroutine(trackingCo);
        }
        trackingCo = StartCoroutine(moveTowards(trackingSpace.gameObject, new HeadPos(headPos, new Quaternion(0, 0, 0, 0)), delta_time));

        //UpdateTarget(trackingSpace, trackingPos, trackingRot);
        //UpdateTarget(headTarget, headPos, headRot);
        //UpdateTarget(leftHandTarget, leftPos, leftRot);
        //UpdateTarget(rightHandTarget, rightPos, rightRot);
    }

    public void UpdateTarget(Transform target, Vector3 pos, Quaternion rot)
    {
        target.position = pos;
        target.rotation = rot;
    }

}
