using ZeroKey.UI.ViewModel.Services;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using ZeroKey.Service.Common;
using ZeroKey.UI.ViewModel.ViewModels;
using Newtonsoft.Json;
using ZeroKey.GUI.Common;
using System.Diagnostics;

namespace ZeroKey.GUI.Pages.Steps
{
    /// <summary>
    /// Interaction logic for PlaceRingStepView.xaml
    /// </summary>
    public partial class PlaceRingStepView
    {
        private readonly ITokenService _tokenService;
        private readonly IDialogService _dialogService;
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        
        public PlaceRingStepView()
        {
            InitializeComponent();
        }

        public async Task InitializeAsync()
        {
        labelstart:
            var token = await _tokenService.GetNewTokenAsync(CancellationTokenSource.Token);

            if (CancellationTokenSource.IsCancellationRequested)
                return;

            if (string.IsNullOrEmpty(token))
            {
                _dialogService.ShowWarningDialog("Bad token. Please place the another NFC Ring");

                await InitializeAsync();

                return;
            }

            TokenHandler.NewRingViewModel.Token = token;

            //here we can integrate the token check if that token already exists

            bool duplicateTag = false;
            try
            {
#warning Hard coded config file
                string appPath = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName;
                string servicePath = Directory.GetParent(appPath).FullName + @"\Service\Service";
                if (File.Exists(servicePath + @"\Application.config"))
                {
                    string sc = File.ReadAllText(servicePath + @"\Application.config");
                    Config applicationConfiguration = JsonConvert.DeserializeObject<Config>(sc);
                    string hashedToken = Crypto.Hash(token);
                    foreach (var item in applicationConfiguration.Users)
                    {
                        foreach (var t in item.Tokens)
                        {
                            string dht = Crypto.Hash(hashedToken + item.Salt);
                            if (dht == t.Key)
                            {
                                duplicateTag = true;
                            }
                            else
                            {
                                duplicateTag = false;
                            }
                        }
                    }
                }
                else
                    duplicateTag = false;
            }
            catch
            {

            }
            if (duplicateTag)
            {
                _dialogService.ShowWarningDialog("Duplicate Tag. This tag is already registered");
                goto labelstart;
            }
            else
            {
                Debug.WriteLine("OK");
            }
        }
    }
}
