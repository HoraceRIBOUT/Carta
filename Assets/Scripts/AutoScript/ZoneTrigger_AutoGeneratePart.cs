using UnityEngine;

public class ZoneTrigger_AutoGeneratePart : MonoBehaviour {
 
	private ZoneTrigger mainComponent;
	
	public void Start()
	{
	    mainComponent = this.GetComponent<ZoneTrigger>();
	}
 
	public enum ZoneTriggerType
	{
	     balcony,
	     fountain,
	     dream,
	     restaurant,
	     flowershop,
	}
 
	public void OnTriggerEnter(Collider collision)
	{
	    switch(mainComponent.myType)
	    {
	        case ZoneTriggerType.balcony :
	            mainComponent.EnterBalcony();
	        break;
	        case ZoneTriggerType.fountain :
	            mainComponent.EnterFountain();
	        break;
	        case ZoneTriggerType.dream :
	            mainComponent.EnterDream();
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
	        case ZoneTriggerType.balcony :
	            mainComponent.ExitBalcony();
	        break;
	        case ZoneTriggerType.fountain :
	            mainComponent.ExitFountain();
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
