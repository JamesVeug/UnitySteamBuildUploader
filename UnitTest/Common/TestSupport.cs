using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Wireframe.UnitTest
{
    public static class TestSupport
    {
        public static UploadTaskReport.StepResult Report()
        {
            return new UploadTaskReport.StepResult(new UploadTaskReport("unit-test", "UnitTest", false));
        }

        public static string RequiredEnvironment(string name)
        {
            string value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                string message = "Configure " + name + " to run this integration test.";
                if (Environment.GetEnvironmentVariable("BUILDUPLOADER_TEST_REQUIRE_CONFIGURATION") == "1")
                    Assert.Fail(message);
                Assert.Ignore(message);
            }
            return value;
        }

        public static string OutputDirectory(string name)
        {
            string root = Path.GetFullPath(Path.Combine("TestResults", "BuildUploader", name, Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(root);
            return root;
        }

        public static UploadTask FileUploadTask(string name, string sourceDirectory, AUploadDestination destination)
        {
            var config = new UploadConfig();
            config.AddSource(new FolderSource(sourceDirectory));
            config.AddDestination(destination);
            var task = new UploadTask(name, config);
            task.SetBuildDescription("Build Uploader automated dry run");
            return task;
        }

        public static string FixtureDirectory(string name, string contents = "Build Uploader integration fixture\n")
        {
            string directory = OutputDirectory(name);
            File.WriteAllText(Path.Combine(directory, "index.html"), contents);
            return directory;
        }

        public static IEnumerator Run(UploadTask task)
        {
            Task operation = task.StartAsync(false);
            yield return Await(operation);
            Assert.IsTrue(task.IsComplete);
            Assert.IsTrue(task.IsSuccessful, task.Report == null ? "UploadTask did not create a report." : task.Report.GetReport());
        }

        // UnityTest coroutines work with the Test Framework versions shipped for Unity 2019 onward.
        public static IEnumerator Await(Task task)
        {
            while (!task.IsCompleted) yield return null;
            task.GetAwaiter().GetResult();
        }

        public static string Quote(string argument)
        {
            return "\"" + argument.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }
    }
}
