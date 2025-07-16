using MxM;

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;


public class ButtonSearcher : MonoBehaviour
{
    [SerializeField] TwoBoneIKConstraint rightArmIKConstraint;
    [SerializeField] TwoBoneIKConstraint leftArmIKConstraint;
    [SerializeField] CapsuleCollider characterCollider;
    [SerializeField] CharacterController characterController;
    [SerializeField] MxMAnimator MxMAnimator;
    [SerializeField] Rig fistCloseRig;
    TwoBoneIKConstraint currentArmIKConstraint;
    Transform leftShoulderTransform;
    Transform rightShoulderTransform;
    Transform currentShoulderTransform;

    Vector3 startPosTargetRight;
    Vector3 startPosTargetLeft;
    Vector3 startLocPosTargetCurrent;
    Vector3 startCurrentButtonPos;

    BoxCollider colliderTrigger;
    GameObject currentButton;
    Dictionary<GameObject, float> buttonsDistDict = new Dictionary<GameObject, float>();
    bool IsInteractionProcess = false;
    InteractionPhase CurrentPhase;
    float maxDistanceInteraction = 1.0f;
    

    float timer = 0;
    float searchMoveDuration = 1f;
    float toButtonMoveDuration = 2f;
    float clickMoveDuration = 1f;
    //float backMoveDuration = 2f;
    float resetMoveDuration = 1f;

    void Start()
    {
        leftShoulderTransform = leftArmIKConstraint.data.root.transform;
        rightShoulderTransform = rightArmIKConstraint.data.root.transform;

        currentShoulderTransform = rightShoulderTransform;
        currentArmIKConstraint = rightArmIKConstraint;
        InitializeCollider();
        
        CurrentPhase = InteractionPhase.Search;
        startLocPosTargetCurrent = rightArmIKConstraint.data.target.transform.localPosition;

        
    }

    // Update is called once per frame
    void Update()
    {
        switch (CurrentPhase)
        {
            case InteractionPhase.None:
                {
                    NonePhase();
                    break;
                }
            case InteractionPhase.Search:
                {
                    SearchPhase();
                    break;
                }
            case InteractionPhase.Wait:
                {
                    WaitPhase();
                    break;
                }
            case InteractionPhase.ToButton:
                {
                    ToButtonPhase();
                    break;
                }
            case InteractionPhase.Click:
                {
                    ClickPhase();
                    break;
                }
            case InteractionPhase.Back:
                {
                    BackPhase();
                    break;
                }
            case InteractionPhase.Reset:
                {
                    ResetPhase();
                    break;
                }
        }

        if (IsInteractionProcess && Input.GetKeyDown(KeyCode.Q)) {
            CurrentPhase = InteractionPhase.Reset;
            MxMAnimator.enabled = true;
        }
        
    
    }

    void InitializeCollider()
    {
        colliderTrigger = gameObject.AddComponent<BoxCollider>();
        colliderTrigger.isTrigger = true;
        if (characterCollider != null)
        {
            colliderTrigger.center = new Vector3(0, characterCollider.height / 2, characterCollider.radius);
            colliderTrigger.size = new Vector3(characterCollider.radius * 4, characterCollider.height, characterCollider.radius * 4);
        }
        else
        {
            colliderTrigger.center = new Vector3(0, characterController.height / 2, characterController.radius);
            colliderTrigger.size = new Vector3(characterController.radius * 4, characterController.height, characterController.radius * 4);
        }
        

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("button") && IsValidToInteraction(other.gameObject))
        {
            buttonsDistDict.Add(other.gameObject, Vector3.Distance(currentShoulderTransform.position, other.gameObject.transform.position));

            IsInteractionProcess = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("button") && buttonsDistDict.ContainsKey(other.gameObject))
        {
            float distance = Vector3.Distance(currentShoulderTransform.position, other.gameObject.transform.position);
            if (distance > maxDistanceInteraction) {
                buttonsDistDict.Remove(other.gameObject);
                if (buttonsDistDict.Count == 0) { 
                    IsInteractionProcess = false;
                }
            }
            else
            {
                buttonsDistDict[other.gameObject] = distance;
                IsInteractionProcess = true;
            }
        }
    }
    bool IsValidToInteraction(GameObject button)
    {
        if (button == null) return false;
        if (Vector3.Distance(currentShoulderTransform.position, button.transform.position) > maxDistanceInteraction) return false;
        return true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("button") && buttonsDistDict.ContainsKey(other.gameObject))
        {
            buttonsDistDict.Remove(other.gameObject);

            if (buttonsDistDict.Count == 0) IsInteractionProcess = false;
        }
    }

    GameObject FindNearestButton()
    {
        GameObject result = buttonsDistDict.Keys.ElementAt(0);
        float min = buttonsDistDict[result];
        
        foreach (var bttn in buttonsDistDict.Keys)
        {
            if (buttonsDistDict[bttn] < min)
            {
                result = bttn;
            }
        }
        return result;
    }

    List<GameObject> GetSortedButtons() // слева направо
    {
        
        List<GameObject> buttons = buttonsDistDict.Keys.ToList();
        List<GameObject> buttonsSorted = new List<GameObject>();

        while (true)
        {
            if (buttons.Count > 1)
            {
                GameObject mostLeft = buttons[0];
                float leftShoulderDist = Vector3.Distance(leftShoulderTransform.position, mostLeft.transform.position);
                float rightShoulderDist = Vector3.Distance(rightShoulderTransform.position, mostLeft.transform.position);
                float max = leftShoulderDist - rightShoulderDist;
                foreach (var bttn in buttons)
                {
                    leftShoulderDist = Vector3.Distance(leftShoulderTransform.position, bttn.transform.position);
                    rightShoulderDist = Vector3.Distance(rightShoulderTransform.position, bttn.transform.position);
                    if (max < leftShoulderDist - rightShoulderDist)
                    {
                        max = leftShoulderDist - rightShoulderDist;
                        mostLeft = bttn;
                    }
                }
                buttonsSorted.Add(mostLeft);
                buttons.Remove(mostLeft);
            }
            else if (buttons.Count == 1)
            {
                buttonsSorted.Add(buttons[0]);
                buttons.Remove(buttons[0]);
                break;
            }
            else if (buttons.Count == 0) break;
            
        }
        return buttonsSorted;
    }

    GameObject FindRightButton()
    {
        List<GameObject> buttons = GetSortedButtons();
        int curIndex = buttons.IndexOf(currentButton);
        if (curIndex == buttons.Count - 1)
        {
            return currentButton;
        }
        else
        {
            return buttons[curIndex + 1];
        }
    }
    GameObject FindLeftButton()
    {
        List<GameObject> buttons = GetSortedButtons();
        int curIndex = buttons.IndexOf(currentButton);
        if (curIndex == 0)
        {
            return currentButton;
        }
        else
        {
            return buttons[curIndex - 1];
        }
        
    }
    /*
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("button"))
        {
            button = other.gameObject;
            startButtonPos = button.transform.position;
        }   
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("button") && other.gameObject == button)
        {
            IsInteractionProcess = IsValidInteraction();
        }
    }
    bool IsValidInteraction()
    {
        if (button == null) return false;
        if (Vector3.Distance(currentShoulderTransform.position, button.transform.position) > maxDistanceInteraction) return false;
        return true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("button") && button == other.gameObject)
        {
            button = null;
            IsInteractionProcess = false;
        }
    }
    */

    void NonePhase() {
        //Debug.Log("NonePhase");
        if (IsInteractionProcess && Input.GetKeyDown(KeyCode.E)) {
            CurrentPhase = InteractionPhase.Search; MxMAnimator.enabled = false;
            currentButton = FindNearestButton();
        }
    }

    void SearchPhase()
    {
        if (!IsInteractionProcess && currentButton == null) { CurrentPhase = InteractionPhase.Reset; return; }
        if (rightArmIKConstraint.weight != 1) { 
            Vector3 waitPos = currentButton.transform.position + currentButton.GetComponent<InteractionData>().waitPos;
            rightArmIKConstraint.data.target.transform.position = waitPos;
            timer += Time.deltaTime;
            float t = timer/searchMoveDuration;
            fistCloseRig.weight = Mathf.Lerp(0, 1, t);
            rightArmIKConstraint.weight = Mathf.Lerp(0, 1, t);
            if (rightArmIKConstraint.weight == 1) { rightArmIKConstraint.weight = 1; CurrentPhase = InteractionPhase.Wait; timer = 0; }
        }
        else
        {
            Vector3 waitPos = currentButton.transform.position + currentButton.GetComponent<InteractionData>().waitPos;
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / searchMoveDuration);
            currentArmIKConstraint.data.target.transform.position = Vector3.MoveTowards(currentArmIKConstraint.data.target.transform.position, waitPos, t);
            if (timer >= searchMoveDuration) { CurrentPhase = InteractionPhase.Wait; timer = 0; }
        }
    }

    void WaitPhase()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            CurrentPhase = InteractionPhase.ToButton;
        }
        else
        if (buttonsDistDict.Count > 1)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                GameObject nextButton = FindLeftButton();
                if (nextButton != currentButton)
                {
                    currentButton = nextButton;
                    CurrentPhase = InteractionPhase.Search;
                }
            }
            else
            if (Input.GetKeyDown(KeyCode.D))
            {
                GameObject nextButton = FindRightButton();
                if (nextButton != currentButton)
                {
                    currentButton = nextButton;
                    CurrentPhase = InteractionPhase.Search;
                }
            }
        }
        
    }


    void ToButtonPhase()
    {
        if (currentButton == null) { CurrentPhase = InteractionPhase.Reset; return; }
        Vector3 goalPos = currentButton.transform.position;
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / toButtonMoveDuration);
        currentArmIKConstraint.data.target.transform.position = Vector3.MoveTowards(currentArmIKConstraint.data.target.transform.position, goalPos, t);
        if (timer >= toButtonMoveDuration) { CurrentPhase = InteractionPhase.Click; timer = 0; startCurrentButtonPos = currentButton.transform.position; }
    }

    void ClickPhase()
    {
        if (currentButton == null){ CurrentPhase = InteractionPhase.Reset; return; }
        Vector3 puchPos = startCurrentButtonPos + currentButton.GetComponent<InteractionData>().pushPos;
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / clickMoveDuration);
        currentArmIKConstraint.data.target.transform.position = Vector3.MoveTowards(currentArmIKConstraint.data.target.transform.position, puchPos, t);
        currentButton.transform.position = Vector3.MoveTowards(currentButton.transform.position, puchPos, t);
        if (timer >= clickMoveDuration) { CurrentPhase = InteractionPhase.Back; timer = 0;}
        
    }
    
    

    void BackPhase()
    {
        
        if (currentButton == null) { CurrentPhase = InteractionPhase.Reset; return; }
        currentButton.GetComponent<ButtonControl>().freeToMove = true;
        
        CurrentPhase = InteractionPhase.Reset;
    }

    void ResetPhase()
    {
        //buttonsDistDict.Clear();
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / resetMoveDuration);
        fistCloseRig.weight = Mathf.Lerp(1, 0, t);
        currentArmIKConstraint.weight = Mathf.Lerp(1, 0, t);
        if (timer >= resetMoveDuration) { 
            currentArmIKConstraint.data.target.transform.localPosition = startLocPosTargetCurrent;
            MxMAnimator.enabled = true;
            CurrentPhase = InteractionPhase.None;
            timer = 0;
        }
        
    }


    enum InteractionPhase
    {
        None,
        Search,
        Wait,
        ToButton,
        Click,
        Back,
        Reset
    }
}
