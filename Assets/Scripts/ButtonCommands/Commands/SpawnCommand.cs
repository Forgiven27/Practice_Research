using NUnit.Framework;
using UnityEngine;

public class SpawnCommand : ButtonCommand
{
    [SerializeField] GameObject obj;


    override public void Execute()
    {
        GameObject gameObject = Instantiate(obj, transform);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.grey;
        
        Gizmos.DrawCube(transform.position, new Vector3(.2f, .2f, .2f));
    }
}
