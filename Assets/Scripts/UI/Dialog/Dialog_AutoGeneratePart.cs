using UnityEngine;

public class Dialog_AutoGeneratePart {
 
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
	     iteminteractivity,
	     dialogredirection,
	     setdefaultdialog,
	     setnextdialog,
	     changeface,
	     choice,
	}
 
	[System.Serializable]
	public class Step
	{
		[HideInInspector()]
		public int index;
		public bool alreadyRead = false;
		public string title { get { return "Step " + index; } }
		[Sirenix.OdinInspector.Title("$title")]
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
		[Sirenix.OdinInspector.ShowIf("type", stepType.iteminteractivity)]
		public Step_ItemInteractivity iteminteractivity_Data;
		[Sirenix.OdinInspector.ShowIf("type", stepType.dialogredirection)]
		public Step_DialogRedirection dialogredirection_Data;
		[Sirenix.OdinInspector.ShowIf("type", stepType.setdefaultdialog)]
		public Step_SetDefaultDialog setdefaultdialog_Data;
		[Sirenix.OdinInspector.ShowIf("type", stepType.setnextdialog)]
		public Step_SetNextDialog setnextdialog_Data;
		[Sirenix.OdinInspector.ShowIf("type", stepType.changeface)]
		public Step_ChangeFace changeface_Data;
		[Sirenix.OdinInspector.ShowIf("type", stepType.choice)]
		public Step_Choice choice_Data;
		
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
				case stepType.iteminteractivity:
				return iteminteractivity_Data;
				case stepType.dialogredirection:
				return dialogredirection_Data;
				case stepType.setdefaultdialog:
				return setdefaultdialog_Data;
				case stepType.setnextdialog:
				return setnextdialog_Data;
				case stepType.changeface:
				return changeface_Data;
				case stepType.choice:
				return choice_Data;
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
