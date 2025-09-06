﻿using System;
using System.Collections.Generic;
using System.IO;
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
                    m_report.Successful = false;
                    // Note: We add a log entry in UploadTaskReport.GetReport()
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
        public string Name { get; private set; }
        
        private readonly bool m_invokeDebugLogs;
        private AUploadTask_Step.StepProcess m_process;

        public UploadTaskReport(string guid, string name, bool invokeDebugLogs = true)
        {
            GUID = guid;
            Name = name;
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
            sb.AppendLine("GUID: " + GUID);
            sb.AppendLine("Name: " + Name);
            sb.AppendLine("Start Time: " + StartTime + " Local: " + StartTime.ToLocalTime()); // UTC
            sb.AppendLine("End Time: " + EndTime + " Local: " + EndTime.ToLocalTime()); // UTC
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
                        StepResult result = stepResult[i];
                        if (result.Logs.Count == 0)
                        {
                            continue;
                        }

                        sb.AppendLine($"-- {stepProcess} {i + 1} --");
                        if (!string.IsNullOrEmpty(result.FailReason))
                        {
                            sb.AppendLine($"[FAILED] {result.FailReason}");
                        }
                            
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

        public static UploadTaskReport FromFilePath(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length == 0 || !lines[0].StartsWith("Upload Task Report", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            int endOfMetaDataIndex = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i].Trim()))
                {
                    endOfMetaDataIndex = i;
                    break;
                }
            }

            if (endOfMetaDataIndex == -1 || lines[0] != "Upload Task Report")
            {
                // Malformed file
                return null;
            }
            
            // Read meta data
            // Upload Task Report
            // GUID: gowsehgoewhn
            // Start Time: 4/09/2025 10:27:57 p.m.
            // End Time: 4/09/2025 10:28:35 p.m.
            // Duration: 00:00:37.4792449
            // Successful: True
            UploadTaskReport report = new UploadTaskReport("", "", false);
            for (int i = 1; i < endOfMetaDataIndex; i++)
            {
                string line = lines[i];
                if (line.StartsWith("GUID", StringComparison.OrdinalIgnoreCase))
                {
                    report.GUID = line.Substring("GUID:".Length).Trim();
                }
                else if (line.StartsWith("Name", StringComparison.OrdinalIgnoreCase))
                {
                    report.Name = line.Substring("Name:".Length).Trim();
                }
                else if (line.StartsWith("Start Time", StringComparison.OrdinalIgnoreCase))
                {
                    // Start Time: 4/09/2025 10:27:57 p.m. Local: 4/09/2025 8:27:57 a.m.
                    int localIndex = line.IndexOf("Local:", StringComparison.OrdinalIgnoreCase);
                    if (localIndex != -1)
                    {
                        line = line.Substring(0, localIndex).Trim();
                    }
                    if (DateTime.TryParse(line.Substring("Start Time:".Length).Trim(), out DateTime startTime))
                    {
                        report.StartTime = startTime;
                    }
                }
                else if (line.StartsWith("End Time", StringComparison.OrdinalIgnoreCase))
                {
                    // End Time: 4/09/2025 10:28:35 p.m. Local: 4/09/2025 8:28:35 a.m.
                    int localIndex = line.IndexOf("Local:", StringComparison.OrdinalIgnoreCase);
                    if (localIndex != -1)
                    {
                        line = line.Substring(0, localIndex).Trim();
                    }
                    if (DateTime.TryParse(line.Substring("End Time:".Length).Trim(), out DateTime endTime))
                    {
                        report.EndTime = endTime;
                    }
                }
                else if (line.StartsWith("Duration", StringComparison.OrdinalIgnoreCase))
                {
                    if (TimeSpan.TryParse(line.Substring("Duration:".Length).Trim(), out TimeSpan duration))
                    {
                        report.EndTime = report.StartTime + duration;
                    }
                }
                else if (line.StartsWith("Successful", StringComparison.OrdinalIgnoreCase))
                {
                    if (bool.TryParse(line.Substring("Successful:".Length).Trim(), out bool successful))
                    {
                        report.Successful = successful;
                    }
                }
            }

            if (string.IsNullOrEmpty(report.GUID) || string.IsNullOrEmpty(report.Name))
            {
                return null;
            }
            
            
            // Load Steps - each step is -- Intra X --
            Dictionary<AUploadTask_Step.StepType, Dictionary<AUploadTask_Step.StepProcess, List<StepResult>>> results = report.StepResults;
            AUploadTask_Step.StepType currentStepType = AUploadTask_Step.StepType.GetSources;
            AUploadTask_Step.StepProcess currentProcess = AUploadTask_Step.StepProcess.Intra;
            StepResult currentStepResult = null;

            for (int i = endOfMetaDataIndex + 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.StartsWith("== -- ") && line.EndsWith(" -- =="))
                {
                    string stepTypeStr = line.Substring(6, line.Length - 11).Trim();
                    if (Enum.TryParse(stepTypeStr, out AUploadTask_Step.StepType stepType))
                    {
                        currentStepType = stepType;
                        currentProcess = AUploadTask_Step.StepProcess.Intra; // Reset process to Intra
                        
                        if (!results.ContainsKey(currentStepType))
                        {
                            results[currentStepType] = new Dictionary<AUploadTask_Step.StepProcess, List<StepResult>>();
                        }
                        
                        if (!results[currentStepType].ContainsKey(currentProcess))
                        {
                            results[currentStepType][currentProcess] = new List<StepResult>();
                        }
                        
                        currentStepResult = new StepResult(report);
                    }
                    else
                    {
                        Debug.LogError("[UploadTaskReport] Failed to parse step type from report: " + stepTypeStr);
                    }
                }
                else if (line.StartsWith("-- ") && line.EndsWith(" --"))
                {
                    string processStr = line.Substring(3, line.Length - 6).Trim();
                    string[] parts = processStr.Split(' ');
                    if (parts.Length >= 1 && Enum.TryParse(parts[0], out AUploadTask_Step.StepProcess process))
                    {
                        currentProcess = process;
                        currentStepResult = new StepResult(report);
                        if (!results.ContainsKey(currentStepType))
                        {
                            results[currentStepType] = new Dictionary<AUploadTask_Step.StepProcess, List<StepResult>>();
                        }

                        if (!results[currentStepType].ContainsKey(currentProcess))
                        {
                            results[currentStepType][currentProcess] = new List<StepResult>();
                        }
                        
                        results[currentStepType][currentProcess].Add(currentStepResult);
                    }
                }
                else
                {
                    // Every line starts with [Info]/[Warning]/[Error]/[Exception]
                    if (line.StartsWith("[Info]", StringComparison.OrdinalIgnoreCase))
                    {
                        currentStepResult.AddLog(line.Substring("[Info]".Length));
                    }
                    else if (line.StartsWith("[Warning]", StringComparison.OrdinalIgnoreCase))
                    {
                        currentStepResult.AddWarning(line.Substring("[Warning]".Length));
                    }
                    else if (line.StartsWith("[Error]", StringComparison.OrdinalIgnoreCase))
                    {
                        currentStepResult.AddError(line.Substring("[Error]".Length));
                    }
                    else if (line.StartsWith("[Exception]", StringComparison.OrdinalIgnoreCase))
                    {
                        currentStepResult.AddException(new Exception(line.Substring("[Exception]".Length)));
                    }
                    else if (line.StartsWith("[Failed]", StringComparison.OrdinalIgnoreCase))
                    {
                        currentStepResult.SetFailed(line);
                    }
                    else if (line.StartsWith("No Logs", StringComparison.OrdinalIgnoreCase))
                    {
                        // Do nothing
                    }
                    else
                    {
                        currentStepResult.AddLog(line);
                    }
                }
            }
            
            return report;
        }
    }
}