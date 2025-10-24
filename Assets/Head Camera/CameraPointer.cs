using System.Collections;
using UnityEngine;

public class CameraPointer : MonoBehaviour
{
    private GameObject gazedAtObject = null;

    public void Update()
    {
        //Casts ray towards camera's forward direction, to detect if a GameObject is being gazed at
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1000))
        {
            //GameObject detected in front of the camera
            if (gazedAtObject != hit.collider.gameObject)
            {
                //Different GameObject
                if (gazedAtObject != null)
                    gazedAtObject.SendMessage("OnPointerExit",null, SendMessageOptions.DontRequireReceiver);

                gazedAtObject = hit.transform.gameObject;
                gazedAtObject.SendMessage("OnPointerEnter", null, SendMessageOptions.DontRequireReceiver);
            }
        }
        else
        {
            //No GameObject detected in front of the camera
            if (gazedAtObject != null)
                gazedAtObject.SendMessage("OnPointerExit", null, SendMessageOptions.DontRequireReceiver);

            gazedAtObject = null;
        }

    }
}
