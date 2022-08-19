using System;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Drawing;
using System.Management.Automation;

namespace BlockTheSpot
{
    public partial class BlockTheSpot : Form // Version 2 //
    {
        public BlockTheSpot()
        {
            InitializeComponent();
            InitializeCode();
        }

        private void InitializeCode()
        {
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Permisos de Administrador requeridos.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Environment.Exit(1);
            }
            if (Directory.Exists($@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\WindowsApps"))
                if (Directory.EnumerateDirectories($@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\WindowsApps").Any(n => n.IndexOf("SpotifyAB.SpotifyMusic", StringComparison.OrdinalIgnoreCase) > 0))
                {
                    MessageBox.Show("Microsoft Store Spotify no es compatible con esta aplicación. Desinstala esta versión y reinicia BlockTheSpot." + Environment.NewLine + Environment.NewLine + "Para realizar la desinstalación, busca la aplicación en el menú de inicio, haz clic derecho en ella y selecciona \"desinstalar\".", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Environment.Exit(1);
                }
            if (File.Exists($@"{spotifyDir}\Spotify.exe")) spotifyPreInstalled = true; else WarningButton.Text = " Spotify no instalado.";
            if (!CheckSpotifyVersion() && File.Exists($@"{spotifyDir}\netutils.dll")) { SpotifyPictureBox.Image = Properties.Resources.AddsOffImage; byButton.Location = new Point(199, 5); PatchButton.Text = "Parchear otra vez"; }

            CentralPictureBox.BringToFront(); CentralPictureBox.Image = Properties.Resources.WorkingImage;
        }

        private bool CheckSpotifyVersion()
        {
            if (File.Exists($@"{spotifyDir}\Spotify.exe"))
            {
                Version actualSpotifyVersion = new Version(FileVersionInfo.GetVersionInfo(spotifyDir + "\\Spotify.exe").FileVersion.ToString());
                if (actualSpotifyVersion.CompareTo(neededSpotifyVersion) > 0) return true; else return false;
            } else return true;
        }

        private async void PatchButtonMethod()
        {
            byButton.Visible = false; CentralPictureBox.Visible = true; label0.Visible = false;
            foreach (Process process in Process.GetProcessesByName("Spotify")) { process.Kill(); }
            try
            {
                await DownloadRequirements();
                await SpotifyDowngrade();
                await DisableAutoUpdate();
            }
            catch(Exception)
            {
                label4.Visible = label3.Visible = label2.Visible = label1.Visible = false;
                CentralPictureBox.Visible = false; byButton.Visible = true;
                label0.Visible = true;
                return;
            }
            if (!spotifyPreInstalled) { PowerShell.Create().AddScript(@"$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut($env:USERPROFILE + '\Desktop\Spotify.lnk'); $S.TargetPath = $env:APPDATA + '\Spotify\Spotify.exe'; $S.Save()").Invoke(); }
            Terminate(true, "¡Parche completado!", "¡Parche completado con éxito!");
        }

        private async void ResetButtonMethod()
        {
            byButton.Visible = false; CentralPictureBox.Visible = true; label0.Text = "Reseteando parcheo";
            foreach (Process process in Process.GetProcessesByName("Spotify")) { process.Kill(); }
            await Task.Delay(3000);
            try { File.Delete($@"{spotifyDir}\netutils.dll"); }
            catch (UnauthorizedAccessException)
            {

                MessageBox.Show($"No ha sido posible acceder a las rutas: \"{spotifyDir}\\netutils.dll\" y \"{updateFolderDir}\\\"." + Environment.NewLine + "Finaliza Spotify desde el administrador de tareas, y comprueba que tienes privilegios suficientes para eliminar los directorios mencionados.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CentralPictureBox.Visible = false; byButton.Visible = true;
                label0.Text = "Selecciona la opción deseada.";
                return;
            }
            catch (DirectoryNotFoundException) { }
            try
            {
                FileSecurity(updateFolderDir, FileSystemRights.FullControl, AccessControlType.Allow, true);
                Directory.Delete(updateFolderDir, true);
            }
            catch (UnauthorizedAccessException)
            {
                CentralPictureBox.Visible = false; byButton.Visible = true;
                label0.Text = "Selecciona la opción deseada.";
                return;
            }
            catch (DirectoryNotFoundException) { }
            Terminate(false, "¡Reset completado!", "¡Reset completado con éxito!" + Environment.NewLine + Environment.NewLine + "Recuerda actualizar Spotify desde \"Configuración\" / \"Acerca de Spotify\" (al final del todo).");
        }

        private void WarningButtonMethod()
        {
            if (WarningButton.Text == " Spotify no instalado.")
                MessageBox.Show("Es recomendable pre-instalar Spotify antes de parchear. En caso contrario, se instalará de forma portable en los directorios AppData y LocalAppData.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            else
                MessageBox.Show("Spotify bloqueará permanentemente las cuentas de usuario que utilicen cualquier forma de ad-block (como BlockTheSpot) al violar los términos y condiciones de uso." + Environment.NewLine + "Medida implementada el 01/03/2019." + Environment.NewLine + Environment.NewLine + "Al utilizar BlockTheSpot asumes toda responsabilidad subyacente.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private async Task DownloadRequirements()
        {
            label1.Visible = true;
            await Task.Delay(1000);
            try
            {
                using (WebClient client = new WebClient()) { client.DownloadFile("http://upgrade.spotify.com/upgrade/client/win32-x86/spotify_installer-1.1.4.197.g92d52c4f-13.exe", $"{tempPath}spotify_installer-1.1.4.197.g92d52c4f-13.exe"); }
                using (WebClient client = new WebClient()) { client.DownloadFile("https://raw.githubusercontent.com/master131/BlockTheSpot/master/netutils.dll", $"{tempPath}netutils.dll"); }
            }
            catch (WebException exception)
            {
                MessageBox.Show("No ha sido posible realizar la descarga." + Environment.NewLine + "Finaliza Spotify desde el administrador de tareas, y comprueba tu conexión a internet.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw exception;
            }
        }

        private async Task SpotifyDowngrade()
        {
            label2.Visible = true;
            await Task.Delay(1000);
            if (CheckSpotifyVersion())
            {
                PowerShell powerShell = PowerShell.Create(); powerShell.AddScript(tempPath + "spotify_installer-1.1.4.197.g92d52c4f-13.exe /extract \"" + spotifyDir + "\"").Invoke();
                //RunHidden("powershell", "try { " + tempPath + "spotify_installer-1.1.4.197.g92d52c4f-13.exe /extract \"" + spotifyDir + "\" } catch { Get-Process -Name BlockTheSpot | Stop-Process } exit");
                if (powerShell.Streams.Error.Count > 0)
                {
                    File.Copy($"{tempPath}spotify_installer-1.1.4.197.g92d52c4f-13.exe", AppDomain.CurrentDomain.BaseDirectory + "spotify_installer-1.1.4.197.g92d52c4f-13.exe", true);
                    MessageBox.Show("No ha sido posible realizar la instalación." + Environment.NewLine + "Instala Spotify v1.1.4.197 manualmente utilizando el instalador que encontrarás en el directorio de BlockTheSpot, y reinicia el parcheo.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw new FileNotFoundException();
                }
                while (CheckSpotifyVersion()) await Task.Delay(1000);
            }

            label3.Visible = true;
            await Task.Delay(1000);
            try
            {
                File.Delete($@"{spotifyDir}\SpotifyMigrator.exe");
                File.Delete($@"{spotifyDir}\SpotifyStartupTask.exe");
                File.Copy($"{tempPath}netutils.dll", $@"{spotifyDir}\netutils.dll", true);
                File.Delete($"{tempPath}spotify_installer-1.1.4.197.g92d52c4f-13.exe");
                File.Delete($"{tempPath}netutils.dll");
            }
            catch (UnauthorizedAccessException exception)
            {
                MessageBox.Show($"No ha sido posible acceder a las rutas: \"{spotifyDir}\\\" y \"{tempPath}\"." + Environment.NewLine + Environment.NewLine + "Finaliza Spotify desde el administrador de tareas, y comprueba que tienes privilegios suficientes para modificar los directorios mencionados.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw exception;
            }
            catch (DirectoryNotFoundException) { }
        }

        private async Task DisableAutoUpdate()
        {
            label4.Visible = true;
            await Task.Delay(1000);
            if (Directory.Exists(updateFolderDir))
            {
                FileSecurity(updateFolderDir, FileSystemRights.FullControl, AccessControlType.Allow, true);
                Directory.Delete(updateFolderDir, true);
            }
            Directory.CreateDirectory(updateFolderDir);
            FileSecurity(updateFolderDir, FileSystemRights.FullControl, AccessControlType.Deny, true);
        }

        private void Terminate(bool patch, string label0Text, string messageBoxText)
        {
            if(patch) SpotifyPictureBox.Image = Properties.Resources.AddsOffImage; else SpotifyPictureBox.Image = Properties.Resources.AddsOnImage;
            label4.Visible = label3.Visible = label2.Visible = label1.Visible = false; label0.Text = label0Text; label0.Visible = true;
            CentralPictureBox.Image = Properties.Resources.DoneImage; CentralPictureBox.Visible = true;
            MessageBox.Show(messageBoxText, "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Information);
            try { Process.Start($@"{spotifyDir}\Spotify.exe"); } catch(Exception) { }
            Application.Exit();
        }

        //private void RunHidden(string software, string commands)
        //{
        //    Process extract = new Process();
        //    extract.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        //    extract.StartInfo.FileName = software;
        //    extract.StartInfo.Arguments = commands;
        //    extract.Start();
        //    extract.WaitForExit();
        //}

        private void FileSecurity(string dirPath, FileSystemRights rights, AccessControlType controlType, bool addRule)
        {
            try
            {
                DirectorySecurity fSecurity = Directory.GetAccessControl(dirPath);
                fSecurity.SetAccessRuleProtection(false, false);
                AuthorizationRuleCollection rules = fSecurity.GetAccessRules(true, true, typeof(NTAccount));
                foreach (FileSystemAccessRule rule in rules) fSecurity.RemoveAccessRule(rule);

                if (addRule) fSecurity.AddAccessRule(new FileSystemAccessRule(UserName, rights, controlType));
                if (!addRule) fSecurity.RemoveAccessRule(new FileSystemAccessRule(UserName, rights, controlType));

                Directory.SetAccessControl(dirPath, fSecurity);
            }
            catch (UnauthorizedAccessException exception)
            {
                MessageBox.Show($"No ha sido posible acceder a la ruta: \"{dirPath}\\\"." + Environment.NewLine + Environment.NewLine + "Finaliza Spotify desde el administrador de tareas, y comprueba que tienes privilegios suficientes para modificar el directorio mencionado.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw exception;
            }
        }

        private void PatchButton_Click(object sender, EventArgs e) => PatchButtonMethod();
        private void ResetButton_Click(object sender, EventArgs e) => ResetButtonMethod();
        private void WarningButton_Click(object sender, EventArgs e) => WarningButtonMethod();
        private void byButton_Click(object sender, EventArgs e) => Process.Start("https://github.com/master131/BlockTheSpot");
        public static string UserName { get; set; } = WindowsIdentity.GetCurrent().Name;
        //private string appDir { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
        private static string tempPath { get; set; } = Path.GetTempPath();
        private static string spotifyDir { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Spotify";
        private static string updateFolderDir { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Spotify\\Update";
        private static Version neededSpotifyVersion { get; set; } = new Version("1.1.4.197");
        private bool spotifyPreInstalled { get; set; }
    }
}
