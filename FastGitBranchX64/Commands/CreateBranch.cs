using LibGit2Sharp;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Commands = LibGit2Sharp.Commands;

namespace FastGitBranchX64
{
    [Command(PackageIds.CreateBranch)]
    internal sealed class CreateBranch : BaseCommand<CreateBranch>
    {
        private Guid id = new("e6afbe38-c42e-402c-866f-b6a612be4245");
        private Repository gitRepository;

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var solutionPath = await GetSolutionPathAsync();
            gitRepository = new Repository(solutionPath);

            #region definiowanie outputu

            var outputwindow = await VS.Services.GetOutputWindowAsync();
            await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if (outputwindow.GetPane(id, out var vsOutputWindowPane) != 0)
                {
                    outputwindow.CreatePane(id, "Fast Branch", 1, 0);
                }
            });

            #endregion definiowanie outputu

            var gitBranchUC = new FastGitBranchControl(gitRepository);
            System.Windows.Window window = new System.Windows.Window
            {
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                SizeToContent = SizeToContent.Height,
                Width = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Title = "Fast Branch Create",
                Content = gitBranchUC,
            };
            window.ShowDialog();

            if (gitBranchUC.CreateBranch)
            {
                string gitCommand = "git";
                string gitArgumentStart = $@"-C {solutionPath}";

                await VS.StatusBar.StartAnimationAsync(StatusAnimation.Sync);
                await VS.StatusBar.ShowMessageAsync($"Creating branch {gitBranchUC.BranchName}");
                WriteToOutputWindow($"Creating branch {gitBranchUC.BranchName}");
                var branch = gitRepository.CreateBranch(gitBranchUC.BranchName);
                if (gitBranchUC.Checkout)
                {
                    WriteToOutputWindow($"Checkout branch {gitBranchUC.BranchName}");
                    Commands.Checkout(gitRepository, branch);
                }
                WriteToOutputWindow($"Current HEAD => {gitRepository.Head.Reference.TargetIdentifier}");

                await VS.StatusBar.EndAnimationAsync(StatusAnimation.Sync);

                await VS.StatusBar.ClearAsync();
            }
        }

        private void RunGitCommand(string gitCommand, string gitArgument)
        {
            var process = new Process();
            process.StartInfo.FileName = gitCommand;
            process.StartInfo.Arguments = gitArgument;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.OutputDataReceived += (s, e) => WriteToOutputWindow(e.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }

        private async Task<string> GetSolutionPathAsync()
        {
            var solucja = await VS.Solutions.GetCurrentSolutionAsync();
            if (solucja != null && solucja.FullPath != null)
            {
                var solucjaExp = solucja.FullPath.Split(new char[] { '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
                var solutionPath = string.Join("\\", solucjaExp, 0, solucjaExp.Length - 1);
                return solutionPath;
            }

            return string.Empty;
        }

        private async void WriteToOutputWindow(string input)
        {
            var outputwindow = await VS.Services.GetOutputWindowAsync();
            await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                outputwindow.GetPane(id, out IVsOutputWindowPane pane);
                if (input != null)
                {
                    pane.OutputString($"{input}{Environment.NewLine}");
                }
            });
        }
    }
}