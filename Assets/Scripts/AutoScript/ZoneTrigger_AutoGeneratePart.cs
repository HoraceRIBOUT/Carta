using UnityEngine;

public class ZoneTrigger_AutoGeneratePart : MonoBehaviour {
 
	private ZoneTrigger mainComponent;
	
	public void Start()
	{
	    mainComponent = this.GetComponent<ZoneTrigger>();
	}
 
	public enum ZoneTriggerType
	{
	     dialog,
	     events,
	     dialogandevents,
	     dream,
	     restaurant,
	     flowershop,
	}
 
	public void OnTriggerEnter(Collider collision)
	{
	    switch(mainComponent.myType)
	    {
	        case ZoneTriggerType.dialog :
	            mainComponent.EnterDialog();
	        break;
	        case ZoneTriggerType.events :
	            mainComponent.EnterEvents();
	        break;
	        case ZoneTriggerType.dialogandevents :
	            mainComponent.EnterDialogAndEvents();
	        break;
	        case ZoneTriggerType.dream :
	            mainComponent.EnterKnowledgeCheckZone();
	        break;
	        case ZoneTriggerType.flowershop :
	            mainComponent.EnterFlowerShop();
	        break;
	    }
	}
 
	public void OnTriggerExit(Collider collision)
	{
	    switch(mainComponent.myType)
	    {
	        case ZoneTriggerType.dialog :
	            mainComponent.ExitDialog();
	        break;
	        case ZoneTriggerType.events :
	            mainComponent.ExitEvents();
	        break;
	        case ZoneTriggerType.dialogandevents :
	            mainComponent.ExitDialogAndEvents();
	        break;
	        case ZoneTriggerType.restaurant :
	            mainComponent.ExitRestaurant();
	        break;
	        case ZoneTriggerType.flowershop :
	            mainComponent.ExitFlowerShop();
	        break;
	    }
	}
 
   
   
}
