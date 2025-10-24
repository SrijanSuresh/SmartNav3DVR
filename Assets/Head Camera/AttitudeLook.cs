using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttitudeLook : MonoBehaviour
{
    private InputAction m_Look;

    public void Awake()
    {
        if (AttitudeSensor.current == null)
            this.gameObject.SetActive(false);
        m_Look = new InputAction("look", binding: "<AttitudeSensor>/attitude");
    }

    private void OnEnable()
    {
        Debug.Log("AttitudeTest enable");
        Debug.Log("" + AttitudeSensor.current);
        if (AttitudeSensor.current != null)
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
            m_Look.Enable();
        }
    }

    private void OnDisable()
    {
        Debug.Log("AttitudeTest disable");
        if (AttitudeSensor.current != null)
        {
            InputSystem.DisableDevice(AttitudeSensor.current);
            m_Look.Disable();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //The phone orientation uses gravity and a compass to determine orientation.
        //Therefore, we have to define real-world up/down and north/south/west/east
        //  in our Unity world.
        //Unity uses a left-handed coordinate system, so let's say
        //  positive y-axis is up, negative y is down
        //  positive z-axis is north, negative z is south
        //  positive x-axis is east, negative z is west.

        Quaternion phoneOrientation = m_Look.ReadValue<Quaternion>();

        float angle = 0.0f;
        Vector3 axis = Vector3.zero;
        phoneOrientation.ToAngleAxis(out angle, out axis);
        //Debug.Log("angle: "+angle+"  axis: "+axis);

        Quaternion headOrientation = Quaternion.Euler(-90, 180, 0) * (new Quaternion(phoneOrientation.z, phoneOrientation.w, phoneOrientation.x, phoneOrientation.y));
        transform.rotation = headOrientation;
    }
}
