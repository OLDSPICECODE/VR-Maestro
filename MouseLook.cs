// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Windows.Kinect;
// using System;

// public class MouseLook : MonoBehaviour
// {
//     public float mouseSensitivity = 800f;
//     public Transform playerBody;
//     float xRotation = 0f;

//     void Start()
//     {
//         Cursor.lockState = CursorLockMode.Locked;
//     }

//     void Update()
//     {
//         float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
//         float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

//         xRotation -= mouseY;
//         xRotation = Mathf.Clamp(xRotation, -90f, 90f);

//         // Aplica la rotaci�n en el eje Z junto con la rotaci�n en el eje X.
//         transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
//         playerBody.Rotate(Vector3.up * mouseX);
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class KinectCameraController : MonoBehaviour
{
    public float rotationSpeed = 2.0f;
    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _Data = null;

    public float mouseSensitivity = 800f;
    public Transform playerBody;
    float xRotation = 0f;

    void Start()
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.BodyFrameSource.OpenReader();

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }

    void Update()
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                if (_Data == null)
                {
                    _Data = new Body[_Sensor.BodyFrameSource.BodyCount];
                }

                frame.GetAndRefreshBodyData(_Data);

                foreach (var body in _Data)
                {
                    if (body != null && body.IsTracked)
                    {
                        // Detectar la posición del brazo izquierdo
                        Windows.Kinect.Joint shoulderJoint = body.Joints[JointType.ShoulderLeft];
                        Windows.Kinect.Joint elbowJoint = body.Joints[JointType.ElbowLeft];
                        Windows.Kinect.Joint wristJoint = body.Joints[JointType.WristLeft];

                        float mouseX = (wristJoint.Position.X - shoulderJoint.Position.X) * mouseSensitivity * Time.deltaTime;
                        float mouseY = (wristJoint.Position.Y - shoulderJoint.Position.Y) * mouseSensitivity * Time.deltaTime;

                        xRotation -= mouseY;
                        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

                        // Aplica la rotaci�n en el eje Z junto con la rotaci�n en el eje X.
                        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                        playerBody.Rotate(Vector3.up * mouseX);

                        // // Calcular la dirección horizontal y vertical del brazo
                        // Vector3 armDirection = new Vector3(wristJoint.Position.X - shoulderJoint.Position.X, wristJoint.Position.Y - shoulderJoint.Position.Y, 0.0f).normalized;

                        // // Convertir la dirección en ángulo y rotar la cámara
                        // float angleHorizontal = Mathf.Atan2(armDirection.x, armDirection.z) * Mathf.Rad2Deg;
                        // float angleVertical = Mathf.Atan2(armDirection.y, armDirection.z) * Mathf.Rad2Deg;

                        // Quaternion targetRotation = Quaternion.Euler(-angleVertical, angleHorizontal, 0);
                        // transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                        break; // Solo necesitas la posición del primer cuerpo rastreado.
                    }
                }

                frame.Dispose();
                frame = null;
            }
        }

        // Tu lógica adicional para controlar el volumen u otras acciones aquí.
    }

    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }
}
