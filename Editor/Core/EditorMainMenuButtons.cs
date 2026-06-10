#if UNITY_6000_3_OR_NEWER
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using Wireframe;

namespace MainMenuButtons
{
	public static class EditorMainMenuButtons
	{
		[MainToolbarElement("Build Uploader/Quick Upload Dropdown", defaultDockPosition = MainToolbarDockPosition.Right)]
		static IEnumerable<MainToolbarElement> CreateUploadButtonsDropdown()
		{
			yield return new MainToolbarDropdown(
				new MainToolbarContent("Upload", Utils.WindowIcon, "Pick a profile to begin uploading"),
				rect =>
				{
					var menu = new GenericMenu();
					List<UploadProfileMeta> list = UploadProfileMeta.LoadFromProjectSettings();
					for (int i = 0; i < list.Count; i++)
					{
						UploadProfileMeta meta = list[i];
						menu.AddItem(new GUIContent(meta.ProfileName), false,
							() =>
							{								
								QuickUploadPopup.ShowWindow(meta);
							}
						);
					}

					menu.DropDown(rect);
				});
		}
	}
}
#endif