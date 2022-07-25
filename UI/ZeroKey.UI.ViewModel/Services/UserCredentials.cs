using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;

namespace ZeroKey.UI.ViewModel.Services
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
            ContextType contextType;
            try // Check if we are in a domain
            {
                var IsDomain = System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain();

                // User is in domain
                contextType = ContextType.Domain;
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                // User is not in domain
                contextType = ContextType.Machine;
            }

            if (contextType == ContextType.Domain)
            {
                // return with domain name 
                using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
                {
                    // If Hostname is in Username
                    if (username.Contains(System.Net.Dns.GetHostName()))
                    {
                        var status = context.ValidateCredentials(username, password, ContextOptions.Signing);
                        return status;
                    }

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
