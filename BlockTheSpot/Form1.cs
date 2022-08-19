using System;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Management.Automation;
using System.Threading;
using System.ComponentModel;
using ThreadState = System.Threading.ThreadState;
using System.Configuration;

namespace BlockTheSpot
{
    public partial class BlockTheSpot : Form
    {
        private static string SpotifyDir { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Spotify";
        private static string UpdateFolderDir { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Spotify\\Update";

        public BlockTheSpot()
        {
            InitializeComponent();
        }

        private async void BlockTheSpot_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.AdminRightsNeeded.Equals(false))
                await CheckRequirements();
            else
            {
                if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                {
                    MessageBox.Show(this, "Permisos de Administrador requeridos. BlockTheSpot se cerrará.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
                PatchButtonMethod();
            }
        }

        private Task CheckRequirements()
        {
            if (Process.GetProcesses().Count(process => process.ProcessName == Process.GetCurrentProcess().ProcessName) > 1)
            {
                MessageBox.Show("BlockTheSpot no responde.", "KCI", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }

            else if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Reinicia la aplicación sin permisos de administrador.", "KCI", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }

            else if (Directory.Exists($@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\WindowsApps"))
                if (Directory.EnumerateDirectories($@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\WindowsApps").Any(n => n.IndexOf("SpotifyAB.SpotifyMusic", StringComparison.OrdinalIgnoreCase) > 0))
                {
                    MessageBox.Show(this, "Microsoft Store Spotify no es compatible con esta aplicación. Desinstala esta versión y reinicia BlockTheSpot." + Environment.NewLine + Environment.NewLine + "Para realizar la desinstalación, busca la aplicación en el menú de inicio, haz clic derecho en ella y selecciona [Desinstalar].", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Application.Exit();
                }

            else if (File.Exists($@"{SpotifyDir}\Spotify.exe"))
                {
                    if (!DowngradeRequired() && File.Exists($@"{SpotifyDir}\netutils.dll")) SpotifyPictureBox.Image = Properties.Resources.AddsOffImage;
                }
                else
                {
                    PatchButton.Text = "Instalar Spotify y Bloquear anuncios";
                    ResetButton.Enabled = false;
                }

            return Task.CompletedTask;
        }

        private bool DowngradeRequired()
        {
            if (File.Exists($@"{SpotifyDir}\Spotify.exe"))
            {
                Version actualSpotifyVersion = new Version(FileVersionInfo.GetVersionInfo(SpotifyDir + "\\Spotify.exe").FileVersion.ToString());
                if (actualSpotifyVersion.CompareTo(new Version("1.1.4.197")) > 0) return true; else return false;
            }
            else return true;
        }

        #region Buttons
        private void PatchButton_Click(object sender, EventArgs e) => PatchButtonMethod();
        private void ResetButton_Click(object sender, EventArgs e) => ResetButtonMethod();
        private void WarningButton_Click(object sender, EventArgs e) => MessageBox.Show(this, "Spotify suspenderá indefinidamente las cuentas de usuario que utilicen cualquier forma de ad-block (como BlockTheSpot) al violar los términos y condiciones de uso." + Environment.NewLine + "Medida implementada el 01/03/2019." + Environment.NewLine + Environment.NewLine + "Al utilizar BlockTheSpot asumes toda responsabilidad subyacente.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        private void ByButton_Click(object sender, EventArgs e) => Process.Start("https://www.youtube.com/channel/UCc-AA6VaZh81DYYCrSAMS5w?");

        private async void PatchButtonMethod() //Make Update portable (change permissions)
        {
            if (!File.Exists($@"{SpotifyDir}\Spotify.exe")) //If not installed, user has to install it manually if does not want a portable installation
            {
                DialogResult warningMessage = MessageBox.Show(this, "Spotify no instalado. Es recomendable pre-instalar Spotify manualmente, de lo contrario se instalará de forma portable en los directorios AppData y LocalAppData." + Environment.NewLine +"¿Deseas continuar de todas formas?", "BlockTheSpot", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (warningMessage == DialogResult.No)
                {
                    Process.Start("https://www.spotify.com/es/download/"); //Maybe download version 1.1.4.197
                    return;
                }
            }

            CentralPictureBox.Visible = true; label0.Visible = false;

            foreach (Process process in Process.GetProcessesByName("Spotify")) { process.Kill(); }

            try
            {
                if (Properties.Settings.Default.AdminRightsNeeded) goto CONTINUE;
                await DownloadRequirements();
                await SpotifyDowngrade();
                CONTINUE:
                await DisableAutoUpdate();
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, $"Error: {exception}", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                label4.Visible = label3.Visible = label2.Visible = label1.Visible = false;
                CentralPictureBox.Visible = false;
                label0.Visible = true;
                return;
            }

            PowerShell.Create().AddScript(@"$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut($env:USERPROFILE + '\Desktop\Spotify.lnk'); $S.TargetPath = $env:APPDATA + '\Spotify\Spotify.exe'; $S.Save()").Invoke();
            
            Terminate(true, "¡Anuncios bloqueados con éxito!");
        }

        private async void ResetButtonMethod()
        {
            CentralPictureBox.Visible = true; label0.Text = "Restableciendo Spotify";
            foreach (Process process in Process.GetProcessesByName("Spotify")) { process.Kill(); }
            await Task.Delay(1000);

            try
            {
                File.Delete($@"{SpotifyDir}\netutils.dll");
                FileSecurity(UpdateFolderDir, FileSystemRights.FullControl, AccessControlType.Allow, true);
                Directory.Delete(UpdateFolderDir, true);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show($"No ha sido posible acceder a las rutas: \"{SpotifyDir}\\netutils.dll\" \"{UpdateFolderDir}\\\"." + Environment.NewLine + "Finaliza Spotify desde el administrador de tareas, y comprueba que tienes privilegios suficientes para eliminar los directorios mencionados.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CentralPictureBox.Visible = false;
                label0.Text = "Selecciona la opción deseada";
                return;
            }
            catch (DirectoryNotFoundException) { }

            Terminate(false, "¡Anuncios restablecidos con éxito!" + Environment.NewLine + Environment.NewLine + @"Recuerda actualizar Spotify desde Configuración \ Acerca de Spotify (al final del todo).");
        }
        #endregion

        private async Task DownloadRequirements()
        {
            label1.Visible = true;
            await Task.Delay(1000);
            try
            {
                using (WebClient client = new WebClient()) { client.DownloadFile("http://upgrade.spotify.com/upgrade/client/win32-x86/spotify_installer-1.1.4.197.g92d52c4f-13.exe", $"{Path.GetTempPath()}spotify_installer-1.1.4.197.g92d52c4f-13.exe"); }
                using (WebClient client = new WebClient()) { client.DownloadFile("https://raw.githubusercontent.com/master131/BlockTheSpot/master/netutils.dll", $"{Path.GetTempPath()}netutils.dll"); }
            }
            catch (WebException)
            {
                throw new Exception("No ha sido posible realizar la descarga." + Environment.NewLine + "Finaliza Spotify desde el administrador de tareas, y comprueba tu conexión a internet.");
            }
        }

        private async Task SpotifyDowngrade()
        {
            label2.Visible = true;
            await Task.Delay(1000);

            if (DowngradeRequired())
            {
                //Improve catch exception & Wait to finish. Maybe download folders directly
                try { PowerShell.Create().AddScript("$ErrorActionPreference = \"Stop\";" + Path.GetTempPath() + ".\\spotify_installer-1.1.4.197.g92d52c4f-13.exe /extract \"" + SpotifyDir + "\"").Invoke(); }
                catch (Exception)
                {
                    File.Copy($"{Path.GetTempPath()}spotify_installer-1.1.4.197.g92d52c4f-13.exe", AppDomain.CurrentDomain.BaseDirectory + "spotify_installer-1.1.4.197.g92d52c4f-13.exe", true);
                    MessageBox.Show("No ha sido posible instalar Spotify v1.1.4.197." + Environment.NewLine + "Deberás realizar la instalación manualmente utilizando el instalador que encontrarás en el directorio de BlockTheSpot." + Environment.NewLine + "Mientras tanto, BlockTheSpot se mantendrá a la espera.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                while (DowngradeRequired()) await Task.Delay(1000);
            }

            label3.Visible = true;
            await Task.Delay(1000);

            try
            {
                File.Delete($@"{SpotifyDir}\SpotifyMigrator.exe");
                File.Delete($@"{SpotifyDir}\SpotifyStartupTask.exe");
                File.Copy($"{Path.GetTempPath()}netutils.dll", $@"{SpotifyDir}\netutils.dll", true);
            }
            catch (UnauthorizedAccessException)
            {
                throw new Exception($"No ha sido posible acceder a la ruta: \"{SpotifyDir}\\\"." + Environment.NewLine + Environment.NewLine + "Finaliza Spotify desde el administrador de tareas, y comprueba que tienes privilegios suficientes para modificar los directorios mencionados.");
            }
            catch (DirectoryNotFoundException) { }

            File.Delete($"{Path.GetTempPath()}spotify_installer-1.1.4.197.g92d52c4f-13.exe");

            File.Delete($"{Path.GetTempPath()}netutils.dll");
        }

        private async Task DisableAutoUpdate()
        {
            //Restart app with admin rights

            if (Properties.Settings.Default.AdminRightsNeeded.Equals(false))
            {
                ProcessStartInfo restartApp = new ProcessStartInfo(AppDomain.CurrentDomain.BaseDirectory); //Check
                restartApp.UseShellExecute = true;
                restartApp.Verb = "runas";

                try
                {
                    Process.Start(restartApp);
                }
                catch (Win32Exception exception)
                {
                    if (exception.NativeErrorCode == 1223) //The operation was canceled by the user.
                    {
                        MessageBox.Show(this, "Permisos de administrador no otorgados. BlockTheSpot se cerrará.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0); //?
                    }
                }
                finally
                {
                    Properties.Settings.Default.AdminRightsNeeded = true;
                }
            }

            label4.Visible = true;
            await Task.Delay(1000);

            if (Directory.Exists(UpdateFolderDir))
            {
                FileSecurity(UpdateFolderDir, FileSystemRights.FullControl, AccessControlType.Allow, true);
                Directory.Delete(UpdateFolderDir, true);
            }
            Directory.CreateDirectory(UpdateFolderDir);
            FileSecurity(UpdateFolderDir, FileSystemRights.FullControl, AccessControlType.Deny, true);
        }

        private void Terminate(bool patch, string messageBoxText)
        {
            if(patch) SpotifyPictureBox.Image = Properties.Resources.AddsOffImage; else SpotifyPictureBox.Image = Properties.Resources.AddsOnImage;

            label4.Visible = label3.Visible = label2.Visible = label1.Visible = false;
            label0.Text = "Todo listo"; label0.Visible = true;

            CentralPictureBox.Image = Properties.Resources.DoneImage; CentralPictureBox.Visible = true;

            MessageBox.Show(messageBoxText, "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Information);

            try { Process.Start($@"{SpotifyDir}\Spotify.exe"); } catch { }

            Application.Exit();
        }

        private void FileSecurity(string dirPath, FileSystemRights rights, AccessControlType controlType, bool addRule)
        {
            DirectorySecurity fSecurity = Directory.GetAccessControl(dirPath);
            fSecurity.SetAccessRuleProtection(false, false);
            AuthorizationRuleCollection rules = fSecurity.GetAccessRules(true, true, typeof(NTAccount));
            foreach (FileSystemAccessRule rule in rules) fSecurity.RemoveAccessRule(rule);

            if (addRule) fSecurity.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, rights, controlType));
            if (!addRule) fSecurity.RemoveAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, rights, controlType));

            Directory.SetAccessControl(dirPath, fSecurity);
        }

        //private bool TheadIsAlive()
        //{
        //    try
        //    {
        //        if (thread.ThreadState.Equals(ThreadState.Running) || thread.ThreadState.Equals(ThreadState.Stopped)) return true;
        //    }
        //    catch (NullReferenceException) { }
        //    return false;
        //}

        private void BlockTheSpot_FormClosing(object sender, FormClosingEventArgs close) //Check if thread is running without a variable and pause running thread
        {
            if (close.CloseReason == CloseReason.UserClosing)
            {
                DialogResult exitMessage = MessageBox.Show(this, "¿Deseas cerrar la aplicación?", "BlockTheSpot", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (exitMessage == DialogResult.No) close.Cancel = true;
            }
            //Properties.Settings.Default.Reset();
        }
    }
}
