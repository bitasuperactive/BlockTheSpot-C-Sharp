using System;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Threading;
using Microsoft.Win32;
using System.ComponentModel;

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
                MessageBox.Show(this, "La versión Microsoft Store de Spotify no es compatible con esta aplicación." + Environment.NewLine + "Desinstale Spotify y reinicie BlockTheSpot.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }
            else if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Por favor inicie BlockTheSpot sin privilegios de Administrador.", "KCI", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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

        private void TerminateSpotify()
        {
            try
            {
                foreach (Process process in Process.GetProcessesByName("Spotify")) { process.Kill(); }
                while (Process.GetProcessesByName("Spotify").Length > 0) Thread.Sleep(100);
            }
            catch (Win32Exception)
            {
                throw new Win32Exception("No ha sido posible terminar Spotify." + Environment.NewLine + "Ciérrelo manualmente si este continúa abierto, e inténtelo de nuevo.");
            }
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

        #region Buttons
        private void PatchButton_Click(object sender, EventArgs e) => PatchButtonMethod();
        private void ResetButton_Click(object sender, EventArgs e) => ResetButtonMethod();
        private void HelpButton_Click(object sender, EventArgs e) => Process.Start("https://github.com/bitasuperactive/BlockTheSpot-C-Sharp");

        private void PatchButtonMethod()
        {
            this.Cursor = Cursors.Default;
            WorkingPictureBox.BringToFront();
            WorkingPictureBox.Visible = true;

            try
            {
                TerminateSpotify();
                SpotifyDowngrade();
                InjectNatutils();
                DisableAutoUpdate();
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, $"Error: {exception}", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WorkingPictureBox.Visible = false;
                return;
            }
            
            SpotifyPictureBox.Image = Properties.Resources.AddsOffImage;
            WorkingPictureBox.Image = Properties.Resources.DoneImage;
            this.TopMost = true; MessageBox.Show(this, "¡Todo listo! Gracias por utilizar BlockTheSpot.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Process.Start($@"{SpotifyDir}\Spotify.exe");
            Application.Exit();
        }

        private void ResetButtonMethod()
        {
            this.Cursor = Cursors.Default;
            WorkingPictureBox.BringToFront();
            WorkingPictureBox.Visible = true;

            try
            {
                TerminateSpotify();
                ClearSpotifyDir();
                UpdateSpotify();
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, $"Error: {exception}", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WorkingPictureBox.Visible = false;
                return;
            }

            SpotifyPictureBox.Image = Properties.Resources.AddsOnImage;
            WorkingPictureBox.Image = Properties.Resources.DoneImage;
            this.TopMost = true; MessageBox.Show(this, "¡Spotify ha sido restablecido con éxito!" + Environment.NewLine + "Gracias por utilizar BlockTheSpot.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.Exit();
        }
        #endregion

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
                    throw new Exception("No ha sido posible descargar los archivos necesarios." + Environment.NewLine + "Compruebe su conexión a internet e inténtelo de nuevo.");
                }

                if (File.Exists($@"{SpotifyDir}\Spotify.exe"))
                {
                    ProcessStartInfo process = new ProcessStartInfo();
                    process.FileName = "cmd.exe";
                    process.Arguments = $"/C \"\"{Path.GetTempPath()}spotify_installer-1.1.4.197.g92d52c4f-13.exe\" /extract \"{SpotifyDir}\"\"";
                    process.WindowStyle = ProcessWindowStyle.Hidden;
                    process.CreateNoWindow = true;
                    Process.Start(process).WaitForExit();
                }
                else
                {
                    Process.Start($"{Path.GetTempPath()}spotify_installer-1.1.4.197.g92d52c4f-13.exe").WaitForExit();
                    TerminateSpotify();
                }

                try { File.Delete($"{Path.GetTempPath()}spotify_installer-1.1.4.197.g92d52c4f-13.exe"); } catch (Exception) { }

                if (DowngradeRequired())
                {
                    throw new Exception("No ha sido posible realizar la instalación de Spotify versión 1.1.4.197.g92d52c4f-13." + Environment.NewLine + "Póngase en contacto con el desarrollador utilizando el botón de ayuda.");
                }
            }
        }

        private void InjectNatutils()
        {
            try
            {
                using (WebClient client = new WebClient()) { client.DownloadFile("https://raw.githubusercontent.com/master131/BlockTheSpot/master/netutils.dll", $"{Path.GetTempPath()}netutils.dll"); }
                
                File.Copy($"{Path.GetTempPath()}netutils.dll", $@"{SpotifyDir}\netutils.dll", true);

                try { File.Delete($"{Path.GetTempPath()}netutils.dll"); } catch (Exception) { }

                if (File.Exists($@"{SpotifyDir}\SpotifyMigrator.exe"))
                    File.Delete($@"{SpotifyDir}\SpotifyMigrator.exe");

                if (File.Exists($@"{SpotifyDir}\SpotifyStartupTask.exe"))
                    File.Delete($@"{SpotifyDir}\SpotifyStartupTask.exe");
            }
            catch (WebException)
            {
                throw new WebException("No ha sido posible descargar los archivos necesarios." + Environment.NewLine + "Compruebe su conexión a internet e inténtelo de nuevo.");
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException($"Los permisos de acceso a los archivos, mencionados a continuación, han sido restringidos utilizando privilegios de Administrador, eliminelos y vuelva a intentarlo: \"{SpotifyDir}\\SpotifyMigrator.exe\" \"{SpotifyDir}\\SpotifyStartupTask.exe\".");
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
        private void ClearSpotifyDir()
        {
            try
            {
                if (File.Exists($@"{SpotifyDir}\netutils.dll"))
                    File.Delete($@"{SpotifyDir}\netutils.dll");

                if (Directory.Exists(UpdateFolderDir))
                {
                    FileSecurity(UpdateFolderDir, FileSystemRights.FullControl, AccessControlType.Allow, true);
                    Directory.Delete(UpdateFolderDir, true);
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException($"Los permisos de acceso a los directorios, mencionados a continuación, han sido restringidos utilizando privilegios de Administrador, eliminelos y vuelva a intentarlo: \"{SpotifyDir}\\netutils.dll\" \"{UpdateFolderDir}\\\".");
            }
        }

        private void UpdateSpotify()
        {
            try
            {
                using (WebClient client = new WebClient()) { client.DownloadFile("https://download.scdn.co/SpotifySetup.exe", $"{Path.GetTempPath()}spotify_installer-update.exe"); }

                Process.Start($"{Path.GetTempPath()}spotify_installer-update.exe").WaitForExit();

                try { File.Delete($"{Path.GetTempPath()}spotify_installer-update.exe"); } catch (Exception) { }

                if (!DowngradeRequired()) throw new WebException();
            }
            catch (WebException)
            {
                MessageBox.Show(this, "No ha sido posible actualizar Spotify a su última versión." + Environment.NewLine + "Puede llevar a cabo facilmente esta actualización accediendo al apartado de [Configuración], [Acerca de Spotify] y [Haz clic para descargar].", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion
    }
}
