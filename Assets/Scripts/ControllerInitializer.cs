using UnityEngine;

public class ControllerInitializer : MonoBehaviour
{
    [SerializeField] GameObject Oculus_Quest_Rift_S_Controller;
    [SerializeField] GameObject Oculus_Quest2_Controller;
    [SerializeField] GameObject default_Controller;

    public MeshRenderer grabRenderer;
    
    [HideInInspector] public GameObject controllerPrefab;
    public GameObject heldObject;

    
    private void Start()
    {
        switch (OVRPlugin.GetSystemHeadsetType())
        {
            case OVRPlugin.SystemHeadset.Oculus_Quest_2:
                controllerPrefab = Oculus_Quest2_Controller;
                
                break;
            case OVRPlugin.SystemHeadset.Oculus_Quest:
            case OVRPlugin.SystemHeadset.Rift_S:
                controllerPrefab = Oculus_Quest_Rift_S_Controller;

                break;
            default:
                controllerPrefab = default_Controller;

                break;
        }

        controllerPrefab.SetActive(true);
    }
}
