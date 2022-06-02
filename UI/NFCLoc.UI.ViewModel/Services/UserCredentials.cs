using NFCLoc.Service.Common;
using System.DirectoryServices.AccountManagement;

namespace NFCLoc.UI.ViewModel.Services
{
    public class UserCredentials : IUserCredentials
    {
        public int MaxTokensCount => 100;

        public string GetName()
        {
            return System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        }

        public bool IsValidCredentials(string username, string password)
        {
            var contextType = Crypto.IsDomainJoined() // check if we're on a domain
                ? ContextType.Domain
                : ContextType.Machine;


            if (contextType == ContextType.Domain)
            {
                // return with domain name 
                using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
                {
                    // Try with SSL
                    try
                    {
                        var status = context.ValidateCredentials(username, password, ContextOptions.SecureSocketLayer);
                        return status;
                    }
                    catch
                    {
                        // SSL not working. Try with SimpleBind.. 
                        var status = context.ValidateCredentials(username, password, ContextOptions.SimpleBind);
                        return status;
                    }
                }                
            }

            using (PrincipalContext context = new PrincipalContext(contextType))
            {
                return context.ValidateCredentials(username, password);
            }
        }
    }
}
