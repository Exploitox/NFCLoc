using System.IO;
using System.Windows;
using Microsoft.Win32;
using ZeroKey.UI.ViewModel;
using ZeroKey.UI.ViewModel.Services;

namespace ZeroKey.UI.View.Services
{
    public class DialogService : IDialogService
    {
        private const int MaxImageSizeMb = 4;

        public bool ShowQuestionDialog(string message)
        {
            return ShowMessageDialog(message, (string)Application.Current.FindResource("question_header"), MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        public bool ShowErrorDialog(string message)
        {
            return ShowMessageDialog(message, (string)Application.Current.FindResource("error_header"), MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public bool ShowWarningDialog(string message)
        {
            return ShowMessageDialog(message, (string)Application.Current.FindResource("warning_header"), MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private bool ShowMessageDialog(string message, string caption, MessageBoxButton buttons, MessageBoxImage image)
        {
            var dialogResult = MessageBox.Show(Application.Current.MainWindow, message, caption, buttons, image);

            return dialogResult == MessageBoxResult.OK || dialogResult == MessageBoxResult.Yes;
        }

        public bool ShowImageDialog(out ImageData imageData)
        {
            imageData = new ImageData();
            var ownerWindow = Application.Current.MainWindow;

            var fileDialog = new OpenFileDialog
            {
                Filter = $"Image files (*.bmp, *.jpg, *.jpeg, *.png) | *.bmp; *.jpg; *.jpeg; *.png"
            };

            if (fileDialog.ShowDialog(ownerWindow) != true)
                return false;

            var fileName = fileDialog.FileName;

            if (!File.Exists(fileName))
            {
                ShowErrorDialog((string)Application.Current.FindResource("file_not_found"));

                return false;
            }

            var sizeMb = (double)new FileInfo(fileName).Length / 1024 / 1024;
            if (sizeMb > MaxImageSizeMb)
            {
                ShowWarningDialog($"{(string)Application.Current.FindResource("file_too_big")} {MaxImageSizeMb} MB.");

                return false;
            }

            imageData.ImageBytes = File.ReadAllBytes(fileName);
            imageData.ImageName = Path.GetFileName(fileName);

            return true;
        }
    }
}
