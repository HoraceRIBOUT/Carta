using UnityEngine;

public class ZoneTrigger_AutoGeneratePart : MonoBehaviour {
 
	private ZoneTrigger mainComponent;
	
	public void Start()
	{
	    mainComponent = this.GetComponent<ZoneTrigger>();
	}
 
	public enum ZoneTriggerType
	{
	}
 
	public void OnTriggerEnter(Collider collision)
	{
	    switch(mainComponent.myType)
	    {
	    }
	}
 
	public void OnTriggerExit(Collider collision)
	{
	    switch(mainComponent.myType)
	    {
	    }
	}
 
   
   
}
