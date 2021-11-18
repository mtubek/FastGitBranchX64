using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace FastGitBranchX64
{
    [Command(PackageIds.CreateBranch)]
    internal sealed class CreateBranch : BaseCommand<CreateBranch>
    {
        Guid id = new("e6afbe38-c42e-402c-866f-b6a612be4245");
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
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
            #endregion



            var gitBranchUC = new FastGitBranchControl();
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

            if(gitBranchUC.CreateBranch)
            {
                var solutionPath = await GetSolutionPathAsync();

                string gitCommand = "git";
                string gitArgumentStart = $@"-C {solutionPath}";

                await VS.StatusBar.StartAnimationAsync(StatusAnimation.Sync);
                await VS.StatusBar.ShowMessageAsync("Creating branch...");
                if (gitBranchUC.Checkout)
                {
                    RunGitCommand(gitCommand, $@"{gitArgumentStart} checkout -b {gitBranchUC.BranchName}");
                }
                else
                {
                    RunGitCommand(gitCommand, $@"{gitArgumentStart} branch {gitBranchUC.BranchName}");
                }
                    
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
            if(solucja !=null && solucja.FullPath!=null)
            {
                var solucjaExp = solucja.FullPath.Split(new char[] { '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
                var solutionPath = string.Join("\\", solucjaExp, 0, solucjaExp.Length - 1);
                return solutionPath;
            }
            
            return string.Empty;
        }

        async void WriteToOutputWindow(string input)
        {
            var outputwindow = await VS.Services.GetOutputWindowAsync();
            await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.RunAsync(async delegate {
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
