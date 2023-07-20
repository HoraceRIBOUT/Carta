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
	     animation,
	     changeface,
	     changevisual,
	     choice,
	     unlockpaper,
	     zonechange,
	}
 
	[System.Serializable]
	public class Step
	{
		[HideInInspector()]
		public int index;
		public bool alreadyRead = false;
		public string title { get { return "Step " + index; } }
		[Sirenix.OdinInspector.Title("$title")]
#if UNITY_EDITOR
		[Sirenix.OdinInspector.GUIColor("GetEnumColor")]
#endif
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
		[Sirenix.OdinInspector.ShowIf("type", stepType.animation)]
		public Step_Animation animation_Data;
		[Sirenix.OdinInspector.ShowIf("type", stepType.changeface)]
		public Step_ChangeFace changeface_Data;
		[Sirenix.OdinInspector.ShowIf("type", stepType.changevisual)]
		public Step_ChangeVisual changevisual_Data;
		[Sirenix.OdinInspector.ShowIf("type", stepType.choice)]
		public Step_Choice choice_Data;
		[Sirenix.OdinInspector.ShowIf("type", stepType.unlockpaper)]
		public Step_UnlockPaper unlockpaper_Data;
		[Sirenix.OdinInspector.ShowIf("type", stepType.zonechange)]
		public Step_ZoneChange zonechange_Data;
		
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
				case stepType.animation:
				return animation_Data;
				case stepType.changeface:
				return changeface_Data;
				case stepType.changevisual:
				return changevisual_Data;
				case stepType.choice:
				return choice_Data;
				case stepType.unlockpaper:
				return unlockpaper_Data;
				case stepType.zonechange:
				return zonechange_Data;
				default:
				Debug.LogError(type + " not implemented in Dialog.cs(class Step.Step() )");
				return null;
			}
		}
		
#if UNITY_EDITOR
		public Color GetEnumColor()
		{
			Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
			return Color.HSVToRGB((int)type * (1f / System.Enum.GetValues(typeof(stepType)).Length), 0.2f, 1);
		}
#endif
		public static Step SetUpStepFromLine(string[] lineSplit)
		{
			stepType stepType = CreateCSV.GetStepTypeFromLine(lineSplit[3]);
			Step newStep = new Step();
			newStep.type = stepType;
			
			//We add the correct step depending on the line we read
			switch (stepType)
			{
				case stepType.dialog:                newStep.dialog_Data			= new Step_Dialog(lineSplit);                          break;
				case stepType.camera:                newStep.camera_Data			= new Step_Camera(lineSplit);                          break;
				case stepType.additem:                newStep.additem_Data			= new Step_AddItem(lineSplit);                          break;
				case stepType.remitem:                newStep.remitem_Data			= new Step_RemItem(lineSplit);                          break;
				case stepType.sfx:                newStep.sfx_Data			= new Step_SFX(lineSplit);                          break;
				case stepType.music:                newStep.music_Data			= new Step_Music(lineSplit);                          break;
				case stepType.iteminteractivity:                newStep.iteminteractivity_Data			= new Step_ItemInteractivity(lineSplit);                          break;
				case stepType.dialogredirection:                newStep.dialogredirection_Data			= new Step_DialogRedirection(lineSplit);                          break;
				case stepType.setdefaultdialog:                newStep.setdefaultdialog_Data			= new Step_SetDefaultDialog(lineSplit);                          break;
				case stepType.setnextdialog:                newStep.setnextdialog_Data			= new Step_SetNextDialog(lineSplit);                          break;
				case stepType.animation:                newStep.animation_Data			= new Step_Animation(lineSplit);                          break;
				case stepType.changeface:                newStep.changeface_Data			= new Step_ChangeFace(lineSplit);                          break;
				case stepType.changevisual:                newStep.changevisual_Data			= new Step_ChangeVisual(lineSplit);                          break;
				case stepType.choice:                newStep.choice_Data			= new Step_Choice(lineSplit);                          break;
				case stepType.unlockpaper:                newStep.unlockpaper_Data			= new Step_UnlockPaper(lineSplit);                          break;
				case stepType.zonechange:                newStep.zonechange_Data			= new Step_ZoneChange(lineSplit);                          break;
				default: 		 newStep.dialog_Data = new Step_Dialog(lineSplit); 		 break;
			}
			return newStep;
		}
	}
}
