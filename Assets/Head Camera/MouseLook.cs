using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    private InputAction m_Look;
    private Transform eyeTransform;

    public void Awake()
    {
        if (Mouse.current == null)
            this.gameObject.SetActive(false);
        m_Look = new InputAction("look", binding: "<Mouse>/delta");
        Camera[] c = GetComponentsInChildren<Camera>();
        if (c.Length > 0)
            eyeTransform = c[0].gameObject.transform;
    }

    public void OnEnable()
    {
        m_Look.Enable();
    }

    public void OnDisable()
    {
        m_Look.Disable();
    }

    void Update()
    {
        Vector2 delta = m_Look.ReadValue<Vector2>();
        transform.Rotate(0, delta.x * 0.1f, 0);
        eyeTransform.Rotate(-delta.y * 0.1f, 0, 0);
    }
}
