using UnityEngine;

public class Dialog_AutoGeneratePart : MonoBehaviour {
 
}
 
 
namespace Step
{
	public enum stepType
	{
	     dialog,
	     camera,
	     additem,
	     remitem,
	     sfx,
	     music,
	}
 
	[System.Serializable]
	public class Step
	{
		[Sirenix.OdinInspector.GUIColor("GetEnumColor")]
		public stepType type;
		[Sirenix.OdinInspector.ShowIf("type", stepType.dialog)]
		public Step_Dialog dialog_Data;
		[Sirenix.OdinInspector.ShowIf("type", stepType.camera)]
		public Step_Camera camera_Data;
		[Sirenix.OdinInspector.ShowIf("type", stepType.additem)]
		public Step_AddItem additem_Data;
		[Sirenix.OdinInspector.ShowIf("type", stepType.remitem)]
		public Step_RemItem remitem_Data;
		[Sirenix.OdinInspector.ShowIf("type", stepType.sfx)]
		public Step_SFX sfx_Data;
		[Sirenix.OdinInspector.ShowIf("type", stepType.music)]
		public Step_Music music_Data;
		
		public Step_father GetData()
		{
			switch (type)
			{
				case stepType.dialog:
				return dialog_Data;
				case stepType.camera:
				return camera_Data;
				case stepType.additem:
				return additem_Data;
				case stepType.remitem:
				return remitem_Data;
				case stepType.sfx:
				return sfx_Data;
				case stepType.music:
				return music_Data;
				default:
				Debug.LogError(type + " not implemented in Dialog.cs(class Step.Step() )");
				return null;
			}
		}


		public Color GetEnumColor()
		{
			Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
			return Color.HSVToRGB((int)type * (1f / System.Enum.GetValues(typeof(stepType)).Length), 0.2f, 1);
		}
	}
}
