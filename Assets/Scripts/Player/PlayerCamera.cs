using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{

    [SerializeField]
    private Camera _myCamera;


    void Awake()
    {
        _myCamera = GetComponent<Camera>();
        _myCamera.fieldOfView = 50f;
        _myCamera.nearClipPlane = 0.1f;
        _myCamera.farClipPlane = 100f;
        UniversalAdditionalCameraData universalAdditionalCameraData = 
            _myCamera.GetComponent<UniversalAdditionalCameraData>();
        universalAdditionalCameraData.renderType = CameraRenderType.Base;
        universalAdditionalCameraData.antialiasing = AntialiasingMode.None;
        universalAdditionalCameraData.renderShadows = false;
        universalAdditionalCameraData.renderPostProcessing = false;
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
