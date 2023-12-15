using UnityEngine;
using Windows.Kinect;
using System.Collections.Generic;

public class VolumeController : MonoBehaviour
{
    private UnityEngine.AudioSource activeAudioSource;
    public float volumeChangeSpeed = 0.5f;

    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _Data = null;

    private bool handAboveHead = false;
    private bool handAtStomach = false;

    // Diccionario para mantener el volumen anterior de cada pista de audio.
    private Dictionary<UnityEngine.AudioSource, float> previousVolumes = new Dictionary<UnityEngine.AudioSource, float>();

    public void SetActiveAudioSource(UnityEngine.AudioSource audioSource)
    {
        activeAudioSource = audioSource;
    }

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
        // Verificar si la mano está cerrada (en este caso, la mano izquierda)
        bool isHandClosed = IsLeftHandClosed();

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
                        // Detectar la posición de la mano en relación con la cabeza y el estómago.
                        Windows.Kinect.Joint headJoint = body.Joints[JointType.Head];
                        Windows.Kinect.Joint handJoint = body.Joints[JointType.HandRight];

                        if (handJoint.Position.Y > headJoint.Position.Y)
                        {
                            handAboveHead = true;
                            handAtStomach = false;
                        }
                        else if (handJoint.Position.Y < headJoint.Position.Y)
                        {
                            handAboveHead = false;
                            handAtStomach = true;
                        }
                        else
                        {
                            handAboveHead = false;
                            handAtStomach = false;
                        }

                        break; // Solo necesitas la posición de la primera mano rastreada.
                    }
                }

                frame.Dispose();
                frame = null;
            }
        }

        if (activeAudioSource != null)
        {
            // Si la mano está cerrada, poner en mute la pista de audio seleccionada.
            if (isHandClosed)
            {
                // Guardar el volumen anterior en el diccionario antes de silenciarlo.
                if (!previousVolumes.ContainsKey(activeAudioSource))
                {
                    previousVolumes[activeAudioSource] = activeAudioSource.volume;
                }

                activeAudioSource.volume = 0.0f;
            }
            else
            {
                // Si la mano no está cerrada, restaurar el volumen anterior si está en el diccionario.
                if (previousVolumes.ContainsKey(activeAudioSource))
                {
                    activeAudioSource.volume = previousVolumes[activeAudioSource];
                    previousVolumes.Remove(activeAudioSource); // Remover la pista del diccionario.
                }
            }

            // Ajustar el volumen basado en la posición de la mano.
            if (handAboveHead)
            {
                activeAudioSource.volume += volumeChangeSpeed * Time.deltaTime;
                activeAudioSource.volume = Mathf.Clamp01(activeAudioSource.volume);
            }
            else if (handAtStomach)
            {
                activeAudioSource.volume -= volumeChangeSpeed * Time.deltaTime;
                activeAudioSource.volume = Mathf.Clamp01(activeAudioSource.volume);
            }
        }
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

    // Función para verificar si la mano izquierda está cerrada
    bool IsLeftHandClosed()
    {
        if (_Sensor != null && _Data != null)
        {
            foreach (var body in _Data)
            {
                if (body != null && body.IsTracked)
                {
                    // Obtener la posición de la mano izquierda
                    Windows.Kinect.Joint handTipJoint = body.Joints[JointType.HandTipLeft];
                    Windows.Kinect.Joint thumbJoint = body.Joints[JointType.ThumbLeft];

                    // Calcular la distancia entre HandTip y Thumb
                    float distancia = Vector3.Distance(
                        new Vector3(handTipJoint.Position.X, handTipJoint.Position.Y, -handTipJoint.Position.Z),
                        new Vector3(thumbJoint.Position.X, thumbJoint.Position.Y, -thumbJoint.Position.Z)
                    );

                    // Definir un umbral para considerar la mano como cerrada
                    float umbral = 0.05f; // Ajusta este valor según tus necesidades

                    // Si la distancia es menor o igual al umbral, considerar la mano como cerrada
                    if (distancia <= umbral)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        // Si no se encuentra una mano izquierda rastreada o no hay datos, devolver false
        return false;
    }

}