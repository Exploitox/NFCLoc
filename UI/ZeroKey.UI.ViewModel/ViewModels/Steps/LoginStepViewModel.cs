﻿using System;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using ZeroKey.UI.ViewModel.Services;

namespace ZeroKey.UI.ViewModel.ViewModels
{
    public class LoginStepViewModel : BaseStepViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly IUserCredentials _userCredentials;
        private string _userName;
        private bool _isError;

        public override int Index => 3;

        public override Func<Task<bool>> NextAction => Save;

        public bool IsError
        {
            get { return _isError; }
            set { Set(ref _isError, value); }
        }

        public string UserName
        {
            get { return _userName; }
            set { Set(ref _userName, value); }
        }

        public SecureString Password { get; set; }

        public LoginStepViewModel(IDialogService dialogService, IUserCredentials userCredentials)
        {
            _dialogService = dialogService;
            _userCredentials = userCredentials;

            UserName = _userCredentials.GetName();
        }

        private Task<bool> Save()
        {
            var password = ConvertToUnsecureString(Password);

            NewRingViewModel.Login = UserName;
            NewRingViewModel.Password = password;

            if (!Validate())
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        private bool Validate()
        {
            if (string.IsNullOrEmpty(NewRingViewModel.Login))
            {
                _dialogService.ShowErrorDialog((string)Application.Current.FindResource("no_username"));

                return false;
            }

            var isValidCredentials = _userCredentials.IsValidCredentials(NewRingViewModel.Login, NewRingViewModel.Password);

            if (string.IsNullOrEmpty(NewRingViewModel.Password) && !isValidCredentials)
            {
                _dialogService.ShowErrorDialog((string)Application.Current.FindResource("no_password"));

                return false;
            }

            if (!isValidCredentials)
            {
                _dialogService.ShowErrorDialog((string)Application.Current.FindResource("invalid_credentials"));

                return false;
            }

            return true;
        }

        private string ConvertToUnsecureString(SecureString securePassword)
        {
            if (securePassword == null)
                return string.Empty;

            var unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return System.Runtime.InteropServices.Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}