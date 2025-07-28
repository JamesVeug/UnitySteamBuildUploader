using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    public class UploadTaskStep_Upload : AUploadTask_Step
    {
        public UploadTaskStep_Upload(StringFormatter.Context context) : base(context)
        {
            
        }

        public override StepType Type => StepType.Upload;
        
        public override async Task<bool> Run(UploadTask uploadTask, UploadTaskReport report)
        {
            List<UploadConfig> buildConfigs = uploadTask.BuildConfigs;

            var uploads = new List<Tuple<List<UploadConfig.DestinationData>, Task<bool>>>();
            for (int i = 0; i < buildConfigs.Count; i++)
            {
                if (!buildConfigs[i].Enabled)
                {
                    continue;
                }
                List<UploadConfig.DestinationData> destinations = buildConfigs[i].Destinations.Where(a=>a.Enabled).ToList();

                Task<bool> upload = Upload(uploadTask, i, destinations, report);
                uploads.Add(new Tuple<List<UploadConfig.DestinationData>, Task<bool>>(destinations, upload));
            }
            
            bool allSuccessful = true;
            while (true)
            {
                bool done = true;
                for (int j = 0; j < uploads.Count; j++)
                {
                    var tuple = uploads[j];
                    if (!tuple.Item2.IsCompleted)
                    {
                        done = false;
                    }
                    else
                    {
                        allSuccessful &= tuple.Item2.Result;
                    }
                }

                if (done)
                {
                    break;
                }

                await Task.Yield();
            }

            return allSuccessful;
        }

        private async Task<bool> Upload(UploadTask uploadTask, int configIndex,
            List<UploadConfig.DestinationData> destinations, UploadTaskReport report)
        {
            int uploadID = ProgressUtils.Start(Type.ToString(), $"Uploading Config {configIndex + 1}");

            List<Task<bool>> uploadTasks = new List<Task<bool>>();
            UploadTaskReport.StepResult[] reports = report.NewReports(Type, destinations.Count);
            for (var i = 0; i < destinations.Count; i++)
            {
                var destinationData = destinations[i];
                UploadTaskReport.StepResult result = reports[i];
                if (!destinationData.Enabled)
                {
                    result.AddLog("Skipping upload because it's disabled");
                    continue;
                }

                Task<bool> task = UploadDestinationWrapper(destinationData.Destination, result, m_context);
                uploadTasks.Add(task);
            }

            bool allSuccessful = true;
            while (true)
            {
                bool done = true;
                float completionAmount = 0.0f;
                for (int j = 0; j < uploadTasks.Count; j++)
                {
                    Task<bool> tuple = uploadTasks[j];
                    AUploadDestination destination = destinations[j].Destination;
                    if (!tuple.IsCompleted)
                    {
                        done = false;
                        completionAmount += destination.UploadProgress();
                    }
                    else
                    {
                        allSuccessful &= tuple.Result;
                        completionAmount++;
                    }
                }

                if (done)
                {
                    break;
                }

                float progress = completionAmount / destinations.Count;
                ProgressUtils.Report(uploadID, progress, "Waiting for all to be uploaded...");
                await Task.Yield();
            }
            
            
            ProgressUtils.Remove(uploadID);
            return allSuccessful;
        }

        private async Task<bool> UploadDestinationWrapper(AUploadDestination destinationDataDestination, UploadTaskReport.StepResult result, StringFormatter.Context ctx)
        {
            try
            {
                return await destinationDataDestination.Upload(result, ctx);
            }
            catch (Exception e)
            {
                result.AddException(e);
                result.SetFailed("Upload failed: " + e.Message);
                return false;
            }
        }

        public override async Task<bool> PostRunResult(UploadTask uploadTask, UploadTaskReport report)
        {
            bool success = true;
            foreach (UploadConfig config in uploadTask.BuildConfigs)
            {
                if (!config.Enabled)
                {
                    continue;
                }

                UploadTaskReport.StepResult[] reports = report.NewReports(Type, config.Destinations.Count);
                for (var i = 0; i < config.Destinations.Count; i++)
                {
                    var destination = config.Destinations[i];
                    var result = reports[i];
                    if (!destination.Enabled)
                    {
                        continue;
                    }

                    AUploadDestination uploadDestination = destination.Destination;
                    try
                    {
                        success &= await uploadDestination.PostUpload(result);
                    }
                    catch (Exception e)
                    {
                        result.AddException(e);
                        result.SetFailed("Post upload failed: " + e.Message);
                        success = false;
                    }
                }
            }
            return success;
        }
    }
}