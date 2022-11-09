using UnityEngine;

public class ControllerInitializer : MonoBehaviour
{
    [SerializeField] GameObject Oculus_Quest_Rift_S_Controller;
    [SerializeField] GameObject Oculus_Quest2_Controller;
    [SerializeField] GameObject default_Controller;
    
    [HideInInspector] public GameObject controllerObject;

    [HideInInspector] public GameObject HeldObject;

    private void Start()
    {
        switch (OVRPlugin.GetSystemHeadsetType())
        {
            case OVRPlugin.SystemHeadset.Oculus_Quest_2:
                controllerObject = Oculus_Quest2_Controller;
                
                break;
            case OVRPlugin.SystemHeadset.Oculus_Quest:
            case OVRPlugin.SystemHeadset.Rift_S:
                controllerObject = Oculus_Quest_Rift_S_Controller;

                break;
            default:
                controllerObject = default_Controller;

                break;
        }

        controllerObject.SetActive(true);
    }
}
