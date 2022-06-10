﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NFCLoc.UI.ViewModel.ViewModels;

namespace NFCLoc.GUI.Pages.Steps
{
    /// <summary>
    /// Interaction logic for LoginStepView.xaml
    /// </summary>
    public partial class LoginStepView
    {
        public LoginStepView()
        {
            InitializeComponent();
        }

        #region Password

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox == null)
                return;

            PasswordTextBox.Text = passwordBox.Password;
            var viewModel = DataContext as LoginStepViewModel;


            viewModel.Password = passwordBox.SecurePassword;
        }

        private void ShowPasswordButton_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PasswordBox.Visibility = Visibility.Visible;
        }

        private void ShowPasswordButton_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PasswordBox.Visibility = Visibility.Collapsed;
        }

        #endregion
    }


}
