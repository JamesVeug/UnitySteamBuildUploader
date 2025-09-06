using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Wireframe
{
    public class UploadTaskReport
    {
        public class StepResult
        {
            public class Log
            {
                public enum LogType
                {
                    Info,
                    Warning,
                    Error,
                    Exception
                }
                
                public LogType Type { get; private set; }
                public string Message { get; private set; }

                public Log(LogType type, string message)
                {
                    Type = type;
                    Message = message;
                }
            }
            
            public bool Successful { get; private set; } = true;
            public float PercentComplete { get; private set; } = 0f;
            public string FailReason { get; private set; } = "";
            public List<Log> Logs { get; private set; } = new List<Log>();

            private object m_lock = new object();
            private readonly UploadTaskReport m_report;

            public StepResult(UploadTaskReport report)
            {
                m_report = report;
            }
            
            public void AddLog(string log)
            {
                lock (m_lock)
                {
                    Logs.Add(new Log(Log.LogType.Info, log));
                }

                if (m_report.m_invokeDebugLogs)
                {
                    string info = "[INFO] " + log;
                    Debug.Log(info);
                }
            }
            
            public void AddWarning(string warning)
            {
                lock (m_lock)
                {
                    Logs.Add(new Log(Log.LogType.Warning, warning));
                }

                if (m_report.m_invokeDebugLogs)
                {
                    string warn = "[WARNING] " + warning;
                    Debug.LogWarning(warn);
                }
            }
            
            public void AddError(string error)
            {
                lock (m_lock)
                {
                    Logs.Add(new Log(Log.LogType.Error, error));
                }

                if (m_report.m_invokeDebugLogs)
                {
                    string err = "[ERROR] " + error;
                    Debug.LogError(err);
                }
            }

            public void AddException(Exception e)
            {
                lock (m_lock)
                {
                    Logs.Add(new Log(Log.LogType.Exception, e.Message + "\n" + e.StackTrace));
                }

                if (m_report.m_invokeDebugLogs)
                {
                    Debug.LogException(e);
                }
            }
            
            public void SetFailed(string reason)
            {
                lock (m_lock)
                {
                    Successful = false;
                    FailReason = reason;
                    Logs.Add(new Log(Log.LogType.Error, "[FAILED] " + reason));
                    m_report.Successful = false;
                }
                
                if (m_report.m_invokeDebugLogs)
                {
                    string err = "[FAILED] " + reason;
                    Debug.LogError(err);
                }
            }
            
            public void SetPercentComplete(float percent)
            {
                lock (m_lock)
                {
                    PercentComplete = Mathf.Clamp01(percent);
                }
            }
        }

        public TimeSpan Duration => EndTime - StartTime;
        public bool Successful { get; private set; } = true;
        
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public string GUID { get; private set; }
        
        private readonly bool m_invokeDebugLogs;
        private AUploadTask_Step.StepProcess m_process;

        public UploadTaskReport(string guid, bool invokeDebugLogs = true)
        {
            GUID = guid;
            m_invokeDebugLogs = invokeDebugLogs;
            StartTime = DateTime.UtcNow;
        }
        
        public Dictionary<AUploadTask_Step.StepType, Dictionary<AUploadTask_Step.StepProcess, List<StepResult>>> StepResults { get; set; } = 
            new Dictionary<AUploadTask_Step.StepType, Dictionary<AUploadTask_Step.StepProcess, List<StepResult>>>();

        public StepResult NewReport(AUploadTask_Step.StepType type)
        {
            if (!StepResults.ContainsKey(type))
            {
                StepResults[type] = new Dictionary<AUploadTask_Step.StepProcess, List<StepResult>>();
            }
            
            if (!StepResults[type].ContainsKey(m_process))
            {
                StepResults[type][m_process] = new List<StepResult>();
            }

            var stepResult = new StepResult(this);
            StepResults[type][m_process].Add(stepResult);
            return stepResult;
        }
        
        public StepResult[] NewReports(AUploadTask_Step.StepType type, int count)
        {
            if (!StepResults.ContainsKey(type))
            {
                StepResults[type] = new Dictionary<AUploadTask_Step.StepProcess, List<StepResult>>();
            }
            
            if (!StepResults[type].ContainsKey(m_process))
            {
                StepResults[type][m_process] = new List<StepResult>();
            }

            var stepResults = new StepResult[count];
            for (int i = 0; i < count; i++)
            {
                var stepResult = new StepResult(this);
                StepResults[type][m_process].Add(stepResult);
                stepResults[i] = stepResult;
            }

            return stepResults;
        }

        public void SetProcess(AUploadTask_Step.StepProcess process)
        {
            m_process = process;
        }

        public void Complete()
        {
            EndTime = DateTime.UtcNow;
        }

        public string GetReport(bool ignoreEmptySteps = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Upload Task Report");
            sb.AppendLine("Start Time: " + StartTime.ToLocalTime());
            sb.AppendLine("End Time: " + EndTime.ToLocalTime());
            sb.AppendLine("Duration: " + Duration);
            sb.AppendLine("Successful: " + Successful);
            
            
            StringBuilder stepTypeSb = new StringBuilder();
            foreach (AUploadTask_Step.StepType stepType in Enum.GetValues(typeof(AUploadTask_Step.StepType)))
            {
                if (sb.Length != 0)
                {
                    sb.AppendLine();
                }

                stepTypeSb.Clear();
                GetStepLogs(ignoreEmptySteps, stepType, stepTypeSb);
                if (stepTypeSb.Length > 0)
                {
                    sb.AppendLine($"== -- {stepType} -- ==");
                    sb.AppendLine(stepTypeSb.ToString());
                }
                else if (!ignoreEmptySteps)
                {
                    sb.AppendLine($"== -- {stepType} -- ==");
                    sb.AppendLine("No logs");
                }
            }

            return sb.ToString();
        }

        public void GetStepLogs(bool ignoreEmptySteps, AUploadTask_Step.StepType stepType, StringBuilder sb)
        {
            bool hasSteps = StepResults.TryGetValue(stepType, out var stepResults) && stepResults.Count > 0;
            if (ignoreEmptySteps && !hasSteps)
            {
                return;
            }
                
            if (!hasSteps) // Get Sources
            {
                return;
            }

            foreach (AUploadTask_Step.StepProcess stepProcess in Enum.GetValues(typeof(AUploadTask_Step.StepProcess)))
            {
                if (stepResults.TryGetValue(stepProcess, out var stepResult)) // Post
                {
                    for (var i = 0; i < stepResult.Count; i++)
                    {
                        var result = stepResult[i];
                        if (result.Logs.Count == 0)
                        {
                            continue;
                        }
                            
                        sb.AppendLine($"-- {stepProcess} {i + 1} --");
                        foreach (StepResult.Log log in result.Logs)
                        {
                            sb.AppendLine($"[{log.Type}] {log.Message}");
                        }
                    }
                }
            }
        }

        public IEnumerable<(AUploadTask_Step.StepType Key, string FailReason)> GetFailReasons()
        {
            foreach (var stepTypeLookup in StepResults)
            {
                foreach (var stepProcessTypeLookup in stepTypeLookup.Value)
                {
                    for (var i = 0; i < stepProcessTypeLookup.Value.Count; i++)
                    {
                        var stepResult = stepProcessTypeLookup.Value[i];
                        if (!stepResult.Successful)
                        {
                            yield return (stepTypeLookup.Key, stepResult.FailReason);
                        }
                    }
                }
            }
        }

        public float GetProgress(AUploadTask_Step.StepType stepType, AUploadTask_Step.StepProcess process)
        {
            if (!StepResults.TryGetValue(stepType, out var stepResults))
            {
                return 0f; // No progress if no results
            }

            float totalProgress = 0f;
            int count = 0;

            if (stepResults.TryGetValue(process, out var results))
            {
                foreach (var result in results)
                {
                    totalProgress += result.PercentComplete;
                    count++;
                }
            }

            return count > 0 ? totalProgress / count : 0f; // Return average progress
        }

        public int CountStepLogs(AUploadTask_Step.StepType stepType)
        {
            if (!StepResults.TryGetValue(stepType, out var stepResults))
            {
                return 0; // No logs if no results
            }
            
            int count = 0;
            foreach (var stepProcess in stepResults.Values)
            {
                foreach (var result in stepProcess)
                {
                    count += result.Logs.Count;
                }
            }
            return count; // Return total log count for the step type
        }
    }
}