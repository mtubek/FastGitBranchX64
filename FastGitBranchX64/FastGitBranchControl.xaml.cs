﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        public FastGitBranchControl()
        {
            InitializeComponent();
            var FirstPart = General.Instance.FirstPart.ToList();
            var SecondPart = General.Instance.SecondPart.ToList();
            firstPart.ItemsSource = FirstPart;
            firstPart.SelectedIndex = 0;
            secondPart.ItemsSource = SecondPart;
            secondPart.SelectedIndex = 0;
        }

        private void buttonCreateBranch_Click(object sender, RoutedEventArgs e)
        {
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
            if(_secondPartString==null)
            {
                _secondPartString = secondPart.Text;
            }
            UpdateBranchNamePreview();
        }

        private void UpdateBranchNamePreview()
        {
            var sb = new StringBuilder();
            if(_firstPartString!=null && _firstPartString!="")
            {
                sb.Append($"{_firstPartString}/");
            }
            if (_secondPartString != null && _secondPartString != "")
            {
                sb.Append($"{_secondPartString}/");
            }
            sb.Append(textBoxBranchName.Text.Replace(" ", "-"));
            _branchName = sb.ToString();
            branchNamePreview.Content = $"Branch name preview: {_branchName}";
                
        }

        private void textBoxBranchName_KeyDown(object sender, KeyEventArgs e)
        {
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
    }
}
