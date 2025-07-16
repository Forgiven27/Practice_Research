using System;
using UnityEngine;

public class ButtonControl : MonoBehaviour
{
    [SerializeField]ButtonCommand command;
    Vector3 startLocPos;
    float speed;
    [NonSerialized]public bool freeToMove = false;
    float timer = 0;
    float duration = 1f;
    float waitTimer = 0;
    float waitDuration = 1f;
    bool toDo = true;
    bool touch = false;
    bool reverse = false;
    private void Start()
    {
        startLocPos = transform.localPosition;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("trigger"))
        {
            
            if (toDo)
            {
                touch = true;
                if(command!=null)command.Execute();
                print("TRIGGERBUTTON");
                toDo = false;
            }
            
        }
    }

    private void Update()
    {
        
        if (!touch) return;
        
        if (waitTimer < waitDuration && !reverse) waitTimer += Time.deltaTime;
        else reverse = true;

        if (freeToMove && reverse && timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, startLocPos, t);
            if (timer >= duration) {
                touch = false;
                reverse = false;
                timer = 0;
                waitTimer = 0;
                toDo = true;
            }
        }
        
    }

    
}
