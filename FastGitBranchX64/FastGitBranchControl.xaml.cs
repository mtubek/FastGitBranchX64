using LibGit2Sharp;
using Newtonsoft.Json;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FastGitBranchX64
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class FastGitBranchControl : UserControl
    {
        public string BranchName { get; set; }
        public bool Checkout { get; set; }
        public bool CreateBranch { get; set; }

        private string _firstPartString;
        private string _secondPartString;
        private string _branchName;
        private System.Windows.Threading.DispatcherTimer timer;
        private Repository gitRepository;

        public FastGitBranchControl(Repository gitRepository)
        {
            InitializeComponent();
            var FirstPart = General.Instance.FirstPart.ToList();
            var SecondPart = General.Instance.SecondPart.ToList();
            firstPart.ItemsSource = FirstPart;
            firstPart.SelectedIndex = 0;
            secondPart.ItemsSource = SecondPart;
            secondPart.SelectedIndex = 0;
            textBoxBranchName.Focus();
            this.gitRepository = gitRepository;
            if(General.Instance.ClickUpEnable)
            {
                stackPanelClickUp.Visibility = Visibility.Visible;
            }
        }

        private void buttonCreateBranch_Click(object sender, RoutedEventArgs e)
        {
            CreateGitBranch();
        }

        private void CreateGitBranch()
        {
            if (CheckIfBranchExists())
            {
                labelError.Content = "This branch alredy exists";
                labelError.Visibility = Visibility.Visible;
                textBoxBranchName.Focus();
                return;
            }
            else if (!ChechIfBranchNameValid())
            {
                labelError.Content = "Branch name not valid";
                labelError.Visibility = Visibility.Visible;
                return;
            }
            BranchName = _branchName;
            Checkout = checkBoxCheckout.IsChecked.GetValueOrDefault();
            CreateBranch = true;
            Window.GetWindow(this).DialogResult = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).DialogResult = false;
        }

        private void firstPart_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _firstPartString = firstPart.SelectedItem?.ToString();
            UpdateBranchNamePreview();
        }

        private void secondPart_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _secondPartString = secondPart.SelectedItem?.ToString();
            if (_secondPartString == null)
            {
                _secondPartString = secondPart.Text;
            }
            UpdateBranchNamePreview();
        }

        private void UpdateBranchNamePreview()
        {
            var sb = new StringBuilder();
            if (_firstPartString != null && _firstPartString != "")
            {
                sb.Append($"{_firstPartString}/");
            }
            if (_secondPartString != null && _secondPartString != "")
            {
                sb.Append($"{_secondPartString}/");
            }
            sb.Append(textBoxBranchName.Text.Replace(" ", "-"));
            _branchName = sb.ToString();
            branchNamePreview.Text = $"Branch name preview: {_branchName}";
            if (CheckIfBranchExists())
            {
                labelError.Content = "This branch alredy exists";
                labelError.Visibility = Visibility.Visible;
                return;
            }
            else if(!ChechIfBranchNameValid())
            {
                labelError.Content = "Branch name not valid";
                labelError.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                labelError.Visibility = Visibility.Collapsed;
            }
        }

        private void textBoxBranchName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CreateGitBranch();
            }
            if (timer != null)
                timer.Stop();

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += (s, args) =>
            {
                UpdateBranchNamePreview();
                timer.Stop();
            };
            timer.Interval = TimeSpan.FromMilliseconds(300);
            timer.Start();
        }

        private void firstPart_KeyUp(object sender, KeyEventArgs e)
        {
            if (firstPart.SelectedItem == null)
            {
                _firstPartString = firstPart.Text;
                UpdateBranchNamePreview();
            }
        }

        private void secondPart_KeyUp(object sender, KeyEventArgs e)
        {
            if (secondPart.SelectedItem == null)
            {
                _secondPartString = secondPart.Text;
                UpdateBranchNamePreview();
            }
        }

        private bool CheckIfBranchExists()
        {
            if (gitRepository == null || gitRepository.Branches.Any(b => b.FriendlyName == _branchName))
            {
                return true;
            }
            return false;
        }

        private bool ChechIfBranchNameValid()
        {
            if (gitRepository != null)
            {
                for (int i = 0; i < _branchName.Length; i++)
                {
                    if (refname_disposition[(int)_branchName[i]] == 4)
                    {
                        return false;
                    }
                    if (refname_disposition[(int)_branchName[i]] == 2 && _branchName[i-1]=='.')
                    {
                        return false;
                    }
                    if (refname_disposition[(int)_branchName[i]] == 3 && _branchName[i - 1] == '@')
                    {
                        return false;
                    }
                    if (refname_disposition[(int)_branchName[i]] == 5)
                    {
                        return false;
                    }
                }
                if (_branchName == "")
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        /*
         * How to handle various characters in refnames:
         * 0: An acceptable character for refs
         * 1: End-of-component
         * 2: ., look for a preceding . to reject .. in refs
         * 3: {, look for a preceding @ to reject @{ in refs
         * 4: A bad character: ASCII control characters, and
         *    ":", "?", "[", "\", "^", "~", SP, or TAB
         * 5: *, reject unless REFNAME_REFSPEC_PATTERN is set
         */

        private static int[] refname_disposition = new int[]{
            1, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
            4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
            4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 2, 1,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 4, 0, 4, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 4, 4
        };

        private void buttonClickUpGetTitle_Click(object sender, RoutedEventArgs e)
        {
            GetBranchNameFromClickUpTask();
        }

        public string RemoveDiacritics(string s)
        {
            string normalizedString = null;
            StringBuilder stringBuilder = new StringBuilder();
            normalizedString = s.Normalize(NormalizationForm.FormD);
            int i = 0;
            char c = '\0';

            for (i = 0; i <= normalizedString.Length - 1; i++)
            {
                c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().ToLower();
        }

        public static T Deserialize<T>(byte[] byteArray) where T : new()
        {
            T obj = new T();
            try
            {
                JsonSerializer serializer = new JsonSerializer();
                using (StreamReader stream = new StreamReader(new MemoryStream(byteArray)))
                {
                    obj = (T)serializer.Deserialize(new JsonTextReader(stream), typeof(T));
                }
            }
            catch (Exception ex)
            {
                return default(T);
            }
            return obj;
        }

        private void textBoxClickUpId_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetBranchNameFromClickUpTask();
            }
        }
        async void GetBranchNameFromClickUpTask()
        {
            var baseAddress = new Uri("https://api.clickup.com/api/v2/");
            var taskId = textBoxClickUpId.Text.Replace("#", "");
            if (taskId.Contains("/"))
            {
                var taskIdDecompose = taskId.Split('/');
                taskId = taskIdDecompose.LastOrDefault();
            }
            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", General.Instance.ClickUpPersonalToken);


                using (var response = await httpClient.GetAsync($"task/{taskId}/"))
                {
                    var clickupTask = Deserialize<BO.ClickUP.Task>(await response.Content.ReadAsByteArrayAsync());
                    if (clickupTask.id != null)
                    {
                        var prefix = General.Instance.ClickUpIdAsPrefix ? $"#{clickupTask.id}-" : "";
                        var name = $"{prefix}{RemoveDiacritics(clickupTask.name)}";
                        textBoxBranchName.Text = $"{name.Replace(' ', '-')}";
                        textBoxBranchName.Focus();
                        UpdateBranchNamePreview();
                    }
                    else
                    {
                        labelError.Content = "An error occured when trying to get Clickup Task";
                    }
                }
            }
        }
    }
}