using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField]
    private Transform _myTransform;

    [SerializeField]
    private float _speed;


    void Start()
    {
        _myTransform = GetComponentInParent<Transform>();
    }

    
    void Update()
    {
        Movement();
    }

    private void Movement()
    {
        if (Input.GetKey(KeyCode.W))
        {
            _myTransform.position = 
                new Vector3(_myTransform.position.x, _myTransform.position.y + (_speed * Time.deltaTime), 0);
        }
        else if(Input.GetKey(KeyCode.S))
        {
            _myTransform.position = 
                new Vector3(_myTransform.position.x, _myTransform.position.y - (_speed * Time.deltaTime), 0);
        }

        if (Input.GetKey(KeyCode.A))
        {
            _myTransform.position = 
                new Vector3(_myTransform.position.x - (_speed * Time.deltaTime), _myTransform.position.y, 0);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            _myTransform.position = 
                new Vector3(_myTransform.position.x + (_speed * Time.deltaTime), _myTransform.position.y, 0);
        }
    }
}
