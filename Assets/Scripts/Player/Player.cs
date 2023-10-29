using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SortingGroup))]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Player : MonoBehaviour
{

    [SerializeField]
    private SortingGroup _mySortingGroup;


    void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer(LayerUtils.LAYER_Player);
        
        _mySortingGroup = gameObject.GetComponent<SortingGroup>();
        _mySortingGroup.sortingLayerName = LayerUtils.LAYER_Player;
        _mySortingGroup.sortingOrder = 0;
    }

    void Start()
    {
        GameManager.Instance.AddNewPlayerConnected(this);
    }

    void Update()
    {
        
    }

}
