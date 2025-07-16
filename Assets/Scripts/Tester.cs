using UnityEngine;

public class Tester : MonoBehaviour
{
    [SerializeField]TagHandle m_TagHandle;
    private void OnCollisionEnter (Collision other)
    {
        Debug.Log("OnCollisionEnter");
    }

    private void OnCollisionStay(Collision collision)
    {
        Debug.Log("OnCollisionStay");
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("OnCollisionExit");
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter");
    }
    private void OnTriggerStay(Collider other)
    {
        Debug.Log("OnTriggerStay");   
    }
    

    private void OnTriggerExit(Collider other)
    {
        
        Debug.Log("OnTriggerExit");
        
    }
}
