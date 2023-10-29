using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{

    [SerializeField]
    private Camera _myCamera;


    void Awake()
    {
        _myCamera = GetComponent<Camera>();
        //Camera.main.
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
