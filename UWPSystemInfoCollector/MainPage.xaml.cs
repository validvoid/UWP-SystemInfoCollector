using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources.Core;
using Windows.ApplicationModel.Store;
using Windows.ApplicationModel.UserDataAccounts;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.NetworkOperators;
using Windows.Phone.Management.Deployment;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage.Streams;
using Windows.System;
using Windows.System.Profile;
using Windows.System.UserProfile;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPSystemInfoCollector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                #region Windows.System.Profile.AnalyticsInfo

                AppendLog("Device Analytics Info", 3);
                AppendLog(() => AnalyticsInfo.DeviceForm);
                AppendLog(() => AnalyticsInfo.VersionInfo.DeviceFamily);
                AppendLog(() => AnalyticsInfo.VersionInfo.DeviceFamilyVersion);

                //Reconstruct the opaque version string
                string sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
                ulong v = ulong.Parse(sv);
                ulong v1 = (v & 0xFFFF000000000000L) >> 48;
                ulong v2 = (v & 0x0000FFFF00000000L) >> 32;
                ulong v3 = (v & 0x00000000FFFF0000L) >> 16;
                ulong v4 = (v & 0x000000000000FFFFL);

                AppendLog("Reconstructed OS Version", $"{v1}.{v2}.{v3}.{v4}");

                #endregion

                #region Windows.ApplicationModel.Resources.Core.ResourceContext

                AppendLog("------------", 4);

                AppendLog("ResourceContext QualifierValues", 3);

                foreach (var qualifierValue in ResourceContext.GetForCurrentView().QualifierValues)
                {
                    AppendLog(qualifierValue.Key, qualifierValue.Value);
                }

                #endregion

                #region Info of Deployed Packages on Windows Mobile Device

                if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                {
                    AppendLog("------------", 4);

                    AppendLog("Info of Deployed Packages on Windows Mobile Device", 3);

                    IEnumerable<Package> packages =
                        InstallationManager.FindPackagesForCurrentPublisher();
                    var package =
                        packages.Where(
                            x =>
                                x.Id.ProductId.Replace("{", "").Replace("}", "").ToLower() ==
                                Package.Current.Id.Name.ToLower());
                    var packagecount = packages.Count();

                    AppendLog($"Found {packagecount} packages:", 2);

                    if (packagecount > 0)
                    {
                        foreach (var pk in packages)
                        {
                            var id = pk.Id.ProductId;
                            var fn = pk.Id.Name;
                            AppendLog(id, fn);
                        }
                    }
                }

                #endregion

                #region Windows.ApplicationModel.Package

                AppendLog("------------", 4);

                AppendLog("ApplicationModel Package Information", 3);

                AppendLog(() => Package.Current.DisplayName);
                AppendLog(() => Package.Current.PublisherDisplayName);
                AppendLog("Logo", Package.Current.Logo.ToString());
                AppendLog(() => Package.Current.Id.FullName);
                AppendLog(() => Package.Current.Id.FamilyName);
                AppendLog(() => Package.Current.Id.Name);
                AppendLog(() => Package.Current.Id.Publisher);
                AppendLog(() => Package.Current.Id.PublisherId);
                AppendLog(() => Package.Current.Id.Architecture);
                AppendLog(() => Package.Current.Id.ResourceId);

                AppendLog("Version",
                    string.Format(
                        $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}"));

                AppendLog(() => Package.Current.Description);
                AppendLog(() => Package.Current.InstalledDate);

                AppendLog("Installed Location", Package.Current.InstalledLocation.Path);
                AppendLog(() => Package.Current.IsBundle);
                AppendLog(() => Package.Current.IsDevelopmentMode);
                AppendLog(() => Package.Current.IsFramework);
                AppendLog(() => Package.Current.IsResourcePackage);

                AppendLog(() => Package.Current.Status.Disabled);
                AppendLog(() => Package.Current.Status.DependencyIssue);
                AppendLog(() => Package.Current.Status.LicenseIssue);
                AppendLog(() => Package.Current.Status.DeploymentInProgress);
                AppendLog(() => Package.Current.Status.Modified);
                AppendLog(() => Package.Current.Status.NeedsRemediation);
                AppendLog(() => Package.Current.Status.NotAvailable);
                AppendLog(() => Package.Current.Status.PackageOffline);
                AppendLog(() => Package.Current.Status.Servicing);
                AppendLog(() => Package.Current.Status.Tampered);
                AppendLog("VerifyIsOK", Package.Current.Status.VerifyIsOK().ToString());

                if (Package.Current.Dependencies.Any())
                {
                    AppendLog($"Found {Package.Current.Dependencies.Count} dependencies:", 2);
                    foreach (var dependency in Package.Current.Dependencies)
                    {
                        AppendLog(dependency.Id.Name + " ", dependency.Id.FullName);
                    }
                }

                #endregion

                #region Windows.ApplicationModel.UserDataAccounts

                //Requires at least one of the capabilities: contacts, appointments, email
                UserDataAccountStore userDataAccountStore =
                    await UserDataAccountManager.RequestStoreAsync(UserDataAccountStoreAccessType.AllAccountsReadOnly);

                IReadOnlyList<UserDataAccount> dataAccounts = await userDataAccountStore.FindAccountsAsync();

                if (dataAccounts.Any())
                {
                    AppendLog("------------", 4);

                    AppendLog("UserDataAccount Information", 3);

                    AppendLog($"Found {dataAccounts.Count} ReadOnly accounts:", 2);

                    foreach (var userDataAccount in dataAccounts)
                    {
                        AppendLog(
                            $"Id:{userDataAccount.Id}\tUserDisplayName:{userDataAccount.UserDisplayName}\tPackageFamilyName:{userDataAccount.PackageFamilyName}\tDeviceAccountTypeId:{userDataAccount.DeviceAccountTypeId}");
                    }
                }
                
                #endregion

                #region Windows.System.User

                //Requires "User Account Information" capability and the user's permission
                IReadOnlyList<User> users = await User.FindAllAsync();

                if (users.Any())
                {
                    AppendLog("------------", 4);

                    AppendLog("User Information", 3);

                    AppendLog($"Found {users.Count} users:", 2);

                    foreach (User user in users)
                    {
                        AppendLog($"User {user.NonRoamableId}");
                        AppendLog(() => user.AuthenticationStatus);
                        AppendLog(() => user.Type);
                        AppendLog($"DisplayName: {await user.GetPropertyAsync(KnownUserProperties.DisplayName)}");
                        AppendLog($"AccountName: {await user.GetPropertyAsync(KnownUserProperties.AccountName)}");
                        AppendLog($"DomainName: {await user.GetPropertyAsync(KnownUserProperties.DomainName)}");
                        AppendLog($"FirstName: {await user.GetPropertyAsync(KnownUserProperties.FirstName)}");
                        AppendLog($"LastName: {await user.GetPropertyAsync(KnownUserProperties.LastName)}");
                        AppendLog($"PrincipalName: {await user.GetPropertyAsync(KnownUserProperties.PrincipalName)}");
                        AppendLog($"ProviderName: {await user.GetPropertyAsync(KnownUserProperties.ProviderName)}");
                        AppendLog($"GuestHost: {await user.GetPropertyAsync(KnownUserProperties.GuestHost)}");
                    }
                }

                #endregion

                #region Windows.ApplicationModel.Store.CurrentAppSimulator

                AppendLog("Simulated Store Inforamtion", 3);
                AppendLog(() => CurrentAppSimulator.AppId);
                AppendLog(() => CurrentAppSimulator.LicenseInformation.ExpirationDate);
                AppendLog(() => CurrentAppSimulator.LicenseInformation.IsActive);
                AppendLog(() => CurrentAppSimulator.LicenseInformation.IsTrial);
                AppendLog($"Found {CurrentAppSimulator.LicenseInformation.ProductLicenses.Count} ProductLicenses:", 2);
                if (CurrentAppSimulator.LicenseInformation.ProductLicenses.Any())
                {
                    foreach (var productLicense in CurrentAppSimulator.LicenseInformation.ProductLicenses)
                    {
                        AppendLog(productLicense.Key, productLicense.Value.ProductId);
                    }
                }
                AppendLog(() => CurrentAppSimulator.LinkUri);

                AppendLog("CampaignId", await CurrentAppSimulator.GetAppPurchaseCampaignIdAsync());

                #endregion

                #region Windows.System.Profile.RetailInfo

                AppendLog("------------", 4);

                AppendLog("Retail Information", 3);
                AppendLog(() => RetailInfo.IsDemoModeEnabled);

                AppendLog($"Found {RetailInfo.Properties.Count} RetailInfo properties:", 2);
                if (RetailInfo.Properties.Any())
                {
                    foreach (var property in RetailInfo.Properties)
                    {
                        AppendLog(property.Key, property.Value.ToString());
                    }
                }

                #endregion

                #region Windows.System.UserProfile.AdvertisingManager

                AppendLog("------------", 4);
                AppendLog("Advertising ID", 3);
                AppendLog(() => AdvertisingManager.AdvertisingId);

                #endregion

                #region EasClientDeviceInformation

                AppendLog("------------", 4);

                var eascdi = new EasClientDeviceInformation();
                AppendLog("EAS Client Device Information", 3);
                AppendLog(() => eascdi.Id);
                AppendLog(() => eascdi.FriendlyName);
                AppendLog(() => eascdi.OperatingSystem);
                AppendLog(() => eascdi.SystemFirmwareVersion);
                AppendLog(() => eascdi.SystemHardwareVersion);
                AppendLog(() => eascdi.SystemManufacturer);
                AppendLog(() => eascdi.SystemProductName);
                AppendLog(() => eascdi.SystemSku);

                #endregion

                #region HardwareIdentification

                AppendLog("------------", 4);

                string strDeviceUniqueId = "";
                string strSignature = "";
                string strCertificate = "";

                var token = HardwareIdentification.GetPackageSpecificToken(null);
                IBuffer hardwareId = token.Id;
                IBuffer signature = token.Signature;
                IBuffer certificate = token.Certificate;

                var byteArray = new byte[hardwareId.Length];
                var dataReader = DataReader.FromBuffer(hardwareId);
                dataReader.ReadBytes(byteArray);

                foreach (byte b in byteArray)
                {
                    string strTemp = b.ToString();
                    if (1 == strTemp.Length)
                    {
                        strTemp = "00" + strTemp;
                    }
                    else if (2 == strTemp.Length)
                    {
                        strTemp = "0" + strTemp;
                    }
                    strDeviceUniqueId += strTemp;
                }

                AppendLog(() => strDeviceUniqueId);

                if (AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
                {
                    //EAS Signature, not available on Windows Mobile 10
                    var byteSigArray = new byte[signature.Length];
                    var dataSigReader = DataReader.FromBuffer(signature);
                    dataSigReader.ReadBytes(byteSigArray);

                    foreach (byte b in byteArray)
                    {
                        string strTemp = b.ToString();
                        if (1 == strTemp.Length)
                        {
                            strTemp = "00" + strTemp;
                        }
                        else if (2 == strTemp.Length)
                        {
                            strTemp = "0" + strTemp;
                        }
                        strSignature += strTemp;
                    }

                    AppendLog(() => strSignature);

                    //EAS Certificate, not available on Windows Mobile 10
                    var byteCertArray = new byte[certificate.Length];
                    var dataCertReader = DataReader.FromBuffer(certificate);
                    dataCertReader.ReadBytes(byteCertArray);

                    foreach (byte b in byteCertArray)
                    {
                        string strTemp = b.ToString();
                        if (1 == strTemp.Length)
                        {
                            strTemp = "00" + strTemp;
                        }
                        else if (2 == strTemp.Length)
                        {
                            strTemp = "0" + strTemp;
                        }
                        strCertificate += strTemp;
                    }

                    AppendLog(() => strCertificate);
                }

                #endregion

                #region Mobile Broadband Information

                AppendLog("------------", 4);

                AppendLog("Mobile Broadband Information", 3);

                //Mobile Broadband Informationis a privileged API. It requires Microsoft Partnership capabilities.
                var allNetworkAccountIds = MobileBroadbandAccount.AvailableNetworkAccountIds;

                if (allNetworkAccountIds.Any())
                {
                    foreach (string aid in allNetworkAccountIds)
                    {
                        var myNetworkAccountObject = MobileBroadbandAccount.CreateFromNetworkAccountId(aid);

                        var myDeviceInfo = myNetworkAccountObject.CurrentDeviceInformation;
                        if (myDeviceInfo == null)
                        {
                            AppendLog($"No device info for {aid}");
                        }
                        else
                        {
                            AppendLog(() => myDeviceInfo.CellularClass);
                            AppendLog(() => myDeviceInfo.CurrentRadioState);
                            AppendLog(() => myDeviceInfo.CustomDataClass);
                            AppendLog(() => myDeviceInfo.DataClasses);
                            AppendLog(() => myDeviceInfo.DeviceId);
                            AppendLog(() => myDeviceInfo.DeviceType);
                            AppendLog(() => myDeviceInfo.FirmwareInformation);
                            AppendLog(() => myDeviceInfo.Manufacturer);
                            AppendLog(() => myDeviceInfo.MobileEquipmentId);
                            AppendLog(() => myDeviceInfo.Model);
                            AppendLog(() => myDeviceInfo.NetworkDeviceStatus);
                            AppendLog(() => myDeviceInfo.Revision);
                            AppendLog(() => myDeviceInfo.SerialNumber);
                            AppendLog(() => myDeviceInfo.SubscriberId);
                            AppendLog(() => myDeviceInfo.SimIccId);
                        }
                    }
                }
                else
                {
                    AppendLog("No valid network account.");
                }

                #endregion

                #region Devices Information

                AppendLog("------------", 4);

                AppendLog("Devices Information", 3);
                DeviceInformationCollection dic = await DeviceInformation.FindAllAsync(DeviceClass.All);
                AppendLog($"Found {dic.Count} devices (lists first 10 ones):", 2);
                if (dic.Count > 10)
                {
                    //foreach (var deviceinfo in dic)
                    //{
                    //    AppendLog($"{deviceinfo.Id}, {deviceinfo.Name}, {deviceinfo.Kind}, {deviceinfo.EnclosureLocation}, {deviceinfo.IsEnabled}");
                    //}

                    for (int i = 0; i < 10; i++)
                    {
                        AppendLog(
                            $"{dic[i].Id}, {dic[i].Name}, {dic[i].Kind}, {dic[i].EnclosureLocation}, {dic[i].IsEnabled}");
                    }
                }

                #endregion

            }
            catch (Exception exception)
            {
                AppendLog(() => exception.HResult, 1);
                AppendLog(() => exception.Message, 1);
                AppendLog(() => exception.InnerException, 1);
            }
        }

        #region AppendOutput

        private void AppendLog(int color = 0)
        {
            Paragraph para = new Paragraph();

            Run run = new Run();

            switch (color)
            {
                case 1:
                    run.Foreground = errorBrush;
                    break;
                case 2:
                    run.Foreground = itemBrush;
                    break;
                case 3:
                    run.Foreground = categroyBrush;
                    break;
                case 4:
                    run.Foreground = dividerBrsh;
                    break;
            }

            para.Inlines.Add(new LineBreak());

            rtbOutput.Blocks.Add(para);
        }
        
        private void AppendLog(string content, int color = 0, bool bold = false)
        {
            Run run = new Run();

            switch (color)
            {
                case 1:
                    run.Foreground = errorBrush;
                    break;
                case 2:
                    run.Foreground = itemBrush;
                    break;
                case 3:
                    run.Foreground = categroyBrush;
                    break;
                case 4:
                    run.Foreground = dividerBrsh;
                    break;
                default:
                    run.Foreground = dividerBrsh;
                    break;
            }

            run.Text = content;
            if (bold)
                run.FontWeight = FontWeights.Bold;

            Paragraph para = new Paragraph();
            para.Inlines.Add(run);
            para.Inlines.Add(new LineBreak());

            rtbOutput.Blocks.Add(para);
        }

        private void AppendLog(string prefix, string content)
        {
            Paragraph para = new Paragraph();
            para.Inlines.Add(new Run() { Text = prefix + ": ", Foreground = itemBrush });
            para.Inlines.Add(new Run() { Text = content });
            para.Inlines.Add(new LineBreak());

            rtbOutput.Blocks.Add(para);
        }

        private void AppendLog<T>(Expression<Func<T>> content, int color = 0)
        {
            if (content != null)
            {
                Paragraph para = new Paragraph();

                var body = content.Body as MemberExpression;
                if (body != null)
                {
                    string name = body.Member.Name;
                    var compiledLambda = content.Compile();
                    var result = compiledLambda.DynamicInvoke();

                    if (result == null)
                        result = "null";

                    switch (color)
                    {
                        case 1:
                            para.Inlines.Add(new Run() { Text = name + ": ", Foreground = errorBrush });
                            break;
                        case 2:
                            para.Inlines.Add(new Run() { Text = name + ": ", Foreground = itemBrush });
                            break;
                        case 3:
                            para.Inlines.Add(new Run() { Text = name + ": ", Foreground = categroyBrush });
                            break;
                        case 4:
                            para.Inlines.Add(new Run() { Text = name + ": ", Foreground = dividerBrsh });
                            break;
                        default:
                            para.Inlines.Add(new Run() { Text = name + ": ", Foreground = itemBrush });
                            break;
                    }

                    para.Inlines.Add(new Run() { Text = result.ToString() });
                }
                else
                {
                    para.Inlines.Add(new Run() { Text = content.ToString().Replace("() => ", "") });
                }

                para.Inlines.Add(new LineBreak());

                rtbOutput.Blocks.Add(para);

            }
        }

        private SolidColorBrush errorBrush = new SolidColorBrush(Colors.Red);
        private SolidColorBrush itemBrush = new SolidColorBrush(Colors.Green);
        private SolidColorBrush categroyBrush = new SolidColorBrush(Colors.DodgerBlue);
        private SolidColorBrush dividerBrsh = new SolidColorBrush(Colors.Gray);

        #endregion


    }
}
