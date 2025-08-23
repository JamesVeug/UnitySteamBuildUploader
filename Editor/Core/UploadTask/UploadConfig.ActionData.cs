using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wireframe
{
    public partial class UploadConfig
    {
        [Wiki("PostUploadActionData", "Specify what actions you want to perform when the upload completes.")]
        public class PostUploadActionData
        {
            internal bool Collapsed { get; set; } = true;
            
            public enum UploadCompleteStatus : byte
            {
                Always = 0,
                Never = 1,
                IfSuccessful = 2,
                IfFailed = 3,
            }
            
            [Wiki("When To Execute", "Choose when you want this action to execute.")]
            public UploadCompleteStatus WhenToExecute;
            
            public AUploadAction UploadAction;
            public UIHelpers.BuildActionPopup.ActionData ActionType;
            
            public void SetupDefaults()
            {
                ActionType = new UIHelpers.BuildActionPopup.ActionData();
            }
            

            public Dictionary<string,object> Serialize()
            {
                Dictionary<string, object> data = new Dictionary<string, object>
                {
                    ["whenToExecute"] = (int)WhenToExecute,
                    ["actionType"] = ActionType?.Type?.FullName,
                    ["action"] = UploadAction?.Serialize()
                };

                return data;
            }

            public void Deserialize(Dictionary<string,object> data)
            {
                if (data == null)
                {
                    return;
                }

                WhenToExecute = data.TryGetValue("whenToExecute", out var value) ? (UploadCompleteStatus)(long)value : UploadCompleteStatus.Always;

                ActionType = new UIHelpers.BuildActionPopup.ActionData();
                if (data.TryGetValue("actionType", out var actionTypeData) && actionTypeData != null)
                {
                    var type = Type.GetType(actionTypeData as string);
                    if (UIHelpers.ActionsPopup.TryGetValueFromType(type, out ActionType))
                    {
                        if (Utils.CreateInstance(ActionType.Type, out UploadAction))
                        {
                            UploadAction.Deserialize(data["action"] as Dictionary<string, object>);
                        }
                    }
                    else
                    {
                        Debug.LogError($"Action type {ActionType} not found");
                    }
                }
            }
        }
    }
}
