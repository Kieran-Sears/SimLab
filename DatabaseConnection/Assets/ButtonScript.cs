using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonScript : MonoBehaviour {

    EventTrigger eventTrigger = null;

    public void Start() {
        eventTrigger = gameObject.GetComponent<EventTrigger>();
        AddEventTrigger(OnPointerEnter, EventTriggerType.PointerEnter);
    }

    private void AddEventTrigger(UnityAction action, EventTriggerType triggerType) {

        // Create a new TriggerEvent and add a listener
        EventTrigger.TriggerEvent trigger = new EventTrigger.TriggerEvent();
        trigger.AddListener((eventData) => action()); // you can capture and pass the event data to the listener

        // Create and initialise EventTrigger.Entry using the created TriggerEvent
        EventTrigger.Entry entry = new EventTrigger.Entry() { callback = trigger, eventID = triggerType };

        // Add the EventTrigger.Entry to delegates list on the EventTrigger
        eventTrigger.triggers.Add(entry);
    }

    public void OnPointerEnter() {
        print("Pointer Entered");
    }

}
