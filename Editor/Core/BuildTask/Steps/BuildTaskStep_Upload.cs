using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wireframe
{
    public class BuildTaskStep_Upload : ABuildTask_Step
    {
        public BuildTaskStep_Upload(StringFormatter.Context context) : base(context)
        {
            
        }

        public override StepType Type => StepType.Upload;
        
        public override async Task<bool> Run(BuildTask buildTask, BuildTaskReport report)
        {
            List<BuildConfig> buildConfigs = buildTask.BuildConfigs;

            var uploads = new List<Tuple<List<BuildConfig.DestinationData>, Task<bool>>>();
            for (int i = 0; i < buildConfigs.Count; i++)
            {
                if (!buildConfigs[i].Enabled)
                {
                    continue;
                }
                List<BuildConfig.DestinationData> destinations = buildConfigs[i].Destinations.Where(a=>a.Enabled).ToList();

                Task<bool> upload = Upload(buildTask, i, destinations, report);
                uploads.Add(new Tuple<List<BuildConfig.DestinationData>, Task<bool>>(destinations, upload));
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

        private async Task<bool> Upload(BuildTask buildTask, int configIndex,
            List<BuildConfig.DestinationData> destinations, BuildTaskReport report)
        {
            int uploadID = ProgressUtils.Start(Type.ToString(), $"Uploading Config {configIndex + 1}");

            List<Task<bool>> uploadTasks = new List<Task<bool>>();
            BuildTaskReport.StepResult[] reports = report.NewReports(Type, destinations.Count);
            for (var i = 0; i < destinations.Count; i++)
            {
                var destinationData = destinations[i];
                BuildTaskReport.StepResult result = reports[i];
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
                    ABuildDestination destination = destinations[j].Destination;
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

        private async Task<bool> UploadDestinationWrapper(ABuildDestination destinationDataDestination, BuildTaskReport.StepResult result, StringFormatter.Context ctx)
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

        public override async Task<bool> PostRunResult(BuildTask buildTask, BuildTaskReport report)
        {
            bool success = true;
            foreach (BuildConfig config in buildTask.BuildConfigs)
            {
                if (!config.Enabled)
                {
                    continue;
                }

                BuildTaskReport.StepResult[] reports = report.NewReports(Type, config.Destinations.Count);
                for (var i = 0; i < config.Destinations.Count; i++)
                {
                    var destination = config.Destinations[i];
                    var result = reports[i];
                    if (!destination.Enabled)
                    {
                        continue;
                    }

                    ABuildDestination buildDestination = destination.Destination;
                    try
                    {
                        success &= await buildDestination.PostUpload(result);
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