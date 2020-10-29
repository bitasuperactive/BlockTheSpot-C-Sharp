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
using Microsoft.Win32;

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

        private void BlockTheSpot_Load(object sender, EventArgs e)
        {
            CheckRequirements();
        }

        private void CheckRequirements()
        {
            if (Process.GetProcesses().Count(process => process.ProcessName == Process.GetCurrentProcess().ProcessName) > 1)
            {
                MessageBox.Show("BlockTheSpot no responde.", "KCI", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }
            else if (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\PackageRepository\Extensions\windows.protocol\spotify") != null)
            {
                MessageBox.Show(this, "La versión Microsoft Store de Spotify no es compatible con esta aplicación." + Environment.NewLine + "Desinstala Spotify y reinicia BlockTheSpot.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }
            else if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Por favor inicia BlockTheSpot sin privilegios de Administrador.", "KCI", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }
            else if (!DowngradeRequired() && File.Exists($@"{SpotifyDir}\netutils.dll"))
            {
                SpotifyPictureBox.Image = Properties.Resources.AddsOffImage;
                PatchButton.Enabled = false;
            }
            else
                ResetButton.Enabled = false;

            if (!File.Exists($@"{SpotifyDir}\Spotify.exe")) PatchButton.Text = "Instalar Spotify y" + Environment.NewLine + "Bloquear anuncios";
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
        private void BlockTheSpot_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e) => Process.Start("https://github.com/bitasuperactive/BlockTheSpot-C-Sharp");

        private void PatchButtonMethod()
        {
            this.Cursor = Cursors.Default;
            WorkingPictureBox.BringToFront();
            WorkingPictureBox.Visible = true;

            TerminateSpotify();

            try
            {
                SpotifyDowngrade();
                InjectNatutils();
                DisableAutoUpdate();
                PowerShell.Create().AddScript(@"$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut($env:USERPROFILE + '\Desktop\Spotify.lnk'); $S.TargetPath = $env:APPDATA + '\Spotify\Spotify.exe'; $S.Save()").Invoke();
                Finish(true, "¡Todo listo! Gracias por utilizar BlockTheSpot.");
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, $"Error: {exception}", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WorkingPictureBox.Visible = false;
                return;
            }
        }

        private void ResetButtonMethod()
        {
            this.Cursor = Cursors.Default;
            WorkingPictureBox.BringToFront();
            WorkingPictureBox.Visible = true;

            TerminateSpotify();

            try
            {
                ClearSpotify();
                UpdateSpotify();
                Finish(false, "¡Spotify ha sido restablecido con éxito!" + Environment.NewLine + "Gracias por utilizar BlockTheSpot.");
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, $"Error: {exception}", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WorkingPictureBox.Visible = false;
                return;
            }
        }
        #endregion

        private void TerminateSpotify()
        {
            foreach (Process process in Process.GetProcessesByName("Spotify")) { process.Kill(); }
            while (Process.GetProcessesByName("Spotify").Length > 0) Thread.Sleep(100);
        }

        #region Patch Methods
        private void SpotifyDowngrade()
        {
            if (DowngradeRequired())
            {
                try
                {
                    using (WebClient client = new WebClient()) { client.DownloadFile("http://upgrade.spotify.com/upgrade/client/win32-x86/spotify_installer-1.1.4.197.g92d52c4f-13.exe", $"{Path.GetTempPath()}spotify_installer-1.1.4.197.g92d52c4f-13.exe"); }
                }
                catch (WebException)
                {
                    throw new Exception("No ha sido posible descargar los archivos necesarios." + Environment.NewLine + "Comprueba tu conexión a internet e inténtalo de nuevo.");
                }

                if (File.Exists($@"{SpotifyDir}\Spotify.exe"))
                {
                    PowerShell.Create().AddScript($"cmd /C \"`\"{Path.GetTempPath()}spotify_installer-1.1.4.197.g92d52c4f-13.exe`\" /extract \"\"{SpotifyDir}\"").Invoke();
                }

                if (DowngradeRequired())
                {
                    Process.Start($"{Path.GetTempPath()}spotify_installer-1.1.4.197.g92d52c4f-13.exe").WaitForExit();

                    try { File.Delete($"{Path.GetTempPath()}spotify_installer-1.1.4.197.g92d52c4f-13.exe"); } catch (Exception) { }  // Conflict

                    TerminateSpotify();
                }
            }
        }

        private void InjectNatutils()
        {
            try
            {
                using (WebClient client = new WebClient()) { client.DownloadFile("https://raw.githubusercontent.com/master131/BlockTheSpot/master/netutils.dll", $"{Path.GetTempPath()}netutils.dll"); }

                if (File.Exists($"{Path.GetTempPath()}netutils.dll"))
                {
                    File.Copy($"{Path.GetTempPath()}netutils.dll", $@"{SpotifyDir}\netutils.dll", true);
                    File.Delete($"{Path.GetTempPath()}netutils.dll");
                }

                if (File.Exists($@"{SpotifyDir}\SpotifyMigrator.exe"))
                    File.Delete($@"{SpotifyDir}\SpotifyMigrator.exe");

                if (File.Exists($@"{SpotifyDir}\SpotifyStartupTask.exe"))
                    File.Delete($@"{SpotifyDir}\SpotifyStartupTask.exe");
            }
            catch (WebException)
            {
                throw new WebException("No ha sido posible descargar los archivos necesarios." + Environment.NewLine + "Comprueba tu conexión a internet e inténtalo de nuevo.");
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException($"No ha sido posible acceder a la siguiente ruta: \"{SpotifyDir}\\\"." + Environment.NewLine + Environment.NewLine + "Prueba a ejecutar BlockTheSpot como Administrador.");
            }
        }

        private void DisableAutoUpdate()
        {
            if (Directory.Exists(UpdateFolderDir))
            {
                FileSecurity(UpdateFolderDir, FileSystemRights.FullControl, AccessControlType.Allow, true);
                Directory.Delete(UpdateFolderDir, true);
            }
            Directory.CreateDirectory(UpdateFolderDir);
            FileSecurity(UpdateFolderDir, FileSystemRights.FullControl, AccessControlType.Deny, true);
        }
        #endregion

        #region Reset Methods
        private void ClearSpotify()
        {
            try
            {
                FileSecurity(UpdateFolderDir, FileSystemRights.FullControl, AccessControlType.Allow, true);

                if (Directory.Exists(UpdateFolderDir))
                    Directory.Delete(UpdateFolderDir, true);

                if (File.Exists($@"{SpotifyDir}\netutils.dll"))
                    File.Delete($@"{SpotifyDir}\netutils.dll");
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException($"Los permisos de acceso a los directorios, mencionados a continuación, han sido restringidos utilizando privilegios de Administrador, eliminelos y vuelva a intentarlo: \"{SpotifyDir}\\netutils.dll\" \"{UpdateFolderDir}\\\"");
            }
        }

        private void UpdateSpotify()
        {
            try
            {
                using (WebClient client = new WebClient()) { client.DownloadFile("https://download.scdn.co/SpotifySetup.exe", $"{Path.GetTempPath()}spotify_installer-update.exe"); }

                Process.Start($"{Path.GetTempPath()}spotify_installer-update.exe").WaitForExit();

                try { File.Delete($"{Path.GetTempPath()}spotify_installer-update.exe"); } catch (Exception) { }  // Conflict
            }
            catch (WebException)
            {
                MessageBox.Show(this, "No ha sido posible actualizar Spotify a su última versión." + Environment.NewLine + "Puede llevar a cabo facilmente esta actualización accediendo al apartado de [Configuración], [Acerca de Spotify] directamente desde los ajustes de Spotify.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion

        private void Finish(bool patch, string message)
        {
            if (patch)
                SpotifyPictureBox.Image = Properties.Resources.AddsOffImage;
            else
                SpotifyPictureBox.Image = Properties.Resources.AddsOnImage;

            WorkingPictureBox.Image = Properties.Resources.DoneImage;

            this.TopMost = true; MessageBox.Show(this, message, "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (patch)
                Process.Start($@"{SpotifyDir}\Spotify.exe");

            Application.Exit();
        }

        private void FileSecurity(string dirPath, FileSystemRights rights, AccessControlType controlType, bool addRule)
        {
            if (Directory.Exists(dirPath))
            {
                DirectorySecurity fSecurity = Directory.GetAccessControl(dirPath);
                fSecurity.SetAccessRuleProtection(false, false);
                AuthorizationRuleCollection rules = fSecurity.GetAccessRules(true, true, typeof(NTAccount));
                foreach (FileSystemAccessRule rule in rules) fSecurity.RemoveAccessRule(rule);

                if (addRule)
                    fSecurity.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, rights, controlType));
                else
                    fSecurity.RemoveAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, rights, controlType));

                Directory.SetAccessControl(dirPath, fSecurity);
            }
        }

        private void BlockTheSpot_FormClosing(object sender, FormClosingEventArgs close)
        {
            if (close.CloseReason == CloseReason.UserClosing && WorkingPictureBox.Visible)
            {
                DialogResult exitMessage = MessageBox.Show(this, "BlockTheSpot no ha terminado su trabajo, ¿deseas cerrar la aplicación de todas formas?", "BlockTheSpot", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (exitMessage == DialogResult.No)
                    close.Cancel = true;
                // Pause threads?
            }
        }
    }
}
