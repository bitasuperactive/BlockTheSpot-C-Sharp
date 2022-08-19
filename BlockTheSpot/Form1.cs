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

namespace BlockTheSpot
{
    public partial class BlockTheSpot : Form
    {
        public BlockTheSpot()
        {
            InitializeComponent();
            CheckRequirements();
            CentralPictureBox.BringToFront();
        }

        private void CheckRequirements()
        {
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Permisos de Administrador requeridos.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Environment.Exit(1);
            }
            if (Directory.Exists($@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\WindowsApps"))
                if (Directory.EnumerateDirectories($@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\WindowsApps").Any(f => f.IndexOf("SpotifyAB.SpotifyMusic", StringComparison.OrdinalIgnoreCase) > 0))
                {
                    MessageBox.Show("Microsoft Store Spotify no es compatible con esta aplicación. Desinstala esta versión y reinicia BlockTheSpot." + Environment.NewLine + Environment.NewLine + "Para realizar la desinstalación, busca la aplicación en el menú de inicio, haz clic derecho en ella y selecciona \"desinstalar\".", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Environment.Exit(1);
                }
            if (File.Exists($@"{spotifyDir}\Spotify.exe")) spotifyInstalled = true; else WarningButton.Text = " Spotify no instalado.";
            if (CheckPatch()) { SpotifyPictureBox.Image = Properties.Resources.AddsOffImage; byButton.Location = new Point(199, 5); PatchButton.Iconimage = Properties.Resources.PatchDoneImage; PatchButton.Text = "             Parchear otra vez"; }
        }

        private bool CheckSpotifyVersion()
        {
            //If higher than needed return true in order to install a downgrade.
            if (spotifyInstalled)
            {
                Version actualSpotifyVersion = new Version(FileVersionInfo.GetVersionInfo(spotifyDir + "\\Spotify.exe").FileVersion.ToString());
                if (actualSpotifyVersion.CompareTo(neededSpotifyVersion) > 0) return true; else return false;
            } else return true;
        }

        private async void PatchButtonMethod()
        {
            byButton.Visible = false; CentralPictureBox.Image = Properties.Resources.WorkingImage; CentralPictureBox.Visible = true;
            foreach (Process process in Process.GetProcessesByName("Spotify")) { process.Kill(); }
            //if (Process.GetProcessesByName(software).Length > 0) RunHidden("taskkill", $"/F /IM {software}.exe");
            try
            {
                await DownloadRequirements();
                await SpotifyDowngrade();
                await DisableAutoUpdate();
            }
            catch
            {
                label4.Visible = label3.Visible = label2.Visible = label1.Visible = false; label0.Visible = true;
                CentralPictureBox.Visible = false;
                return;
            }
            if (!CheckPatch())
            {
                label4.Visible = label3.Visible = label2.Visible = label1.Visible = false; label0.Text = "Proceso no completado."; label0.Visible = true;
                MessageBox.Show("Parche no completado. Reinicia BlockTheSpot e intentalo de nuevo.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            Shortcut();
            Terminate(true, "¡Parche completado!", "¡Parche completado con éxito!");
        }

        private async void ResetButtonMethod()
        {
            byButton.Visible = false; CentralPictureBox.Image = Properties.Resources.WorkingImage; CentralPictureBox.Visible = true;
            label0.Text = "Reseteando parcheo";
            foreach (Process process in Process.GetProcessesByName("Spotify")) { process.Kill(); }
            await Task.Delay(3000);
            try { ResetMethod(); }
            catch
            {
                label0.Text = "Selecciona la opción deseada.";
                CentralPictureBox.Visible = false;
                return;
            }
            Terminate(false, "¡Reset completado!", "¡Reset completado con éxito!" + Environment.NewLine + Environment.NewLine + "Recuerda actualizar Spotify desde \"Configuración\" / \"Acerca de Spotify\" (al final del todo).");
        }

        private void WarningButtonMethod()
        {
            if (WarningButton.Text == " Spotify no instalado.")
                MessageBox.Show("Es recomendable pre-instalar Spotify antes de parchear. En caso contrario, se instalará de forma portable en los directorios AppData y LocalAppData.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            if (WarningButton.Text == "Spotify bloqueará cuentas con ad-block.")
                MessageBox.Show("Spotify bloqueará permanentemente las cuentas de usuario que utilicen cualquier forma de ad-block (como BlockTheSpot) al violar los términos y condiciones de uso." + Environment.NewLine + "Medida implementada el 01/03/2019." + Environment.NewLine + Environment.NewLine + "Al utilizar BlockTheSpot asumes toda responsabilidad subyacente.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private async Task DownloadRequirements()
        {
            label0.Visible = false;
            label1.Visible = true;
            await Task.Delay(1000);
            try
            {
                using (WebClient client = new WebClient()) { client.DownloadFile("http://upgrade.spotify.com/upgrade/client/win32-x86/spotify_installer-1.1.4.197.g92d52c4f-13.exe", $"{tempPath}spotify_installer-1.1.4.197.g92d52c4f-13.exe"); }
                using (WebClient client = new WebClient()) { client.DownloadFile("https://raw.githubusercontent.com/master131/BlockTheSpot/master/netutils.dll", $"{tempPath}netutils.dll"); }
            }
            catch (WebException exception)
            {
                MessageBox.Show("No ha sido posible acceder al servidor." + Environment.NewLine + "Finaliza Spotify desde el administrador de tareas, y comprueba tu conexión a internet.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw exception;
            }
        }

        private async Task SpotifyDowngrade()
        {
            label2.Visible = true;
            await Task.Delay(1000);
            if (CheckSpotifyVersion())
            {
                RunHidden("powershell", "try { " + tempPath + "spotify_installer-1.1.4.197.g92d52c4f-13.exe /extract \"" + spotifyDir + "\" } catch { Get-Process -Name BlockTheSpot | Stop-Process } exit");
                //$sh = New-Object -ComObject \"Wscript.Shell\"; $intButton = $sh.Popup('No ha sido posible instalar Spotify v1.1.4.197. Realiza la instalación manualmente accediendo al siguiente enlace: tiny.cc/spotify114197',0,'BlockTheSpot',0+16);
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
            catch (Exception) { }
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

        private void Shortcut() { if (!spotifyInstalled) RunHidden("powershell", @"$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut($env:USERPROFILE + '\Desktop\Spotify.lnk'); $S.TargetPath = $env:APPDATA + '\Spotify\Spotify.exe'; $S.Save()"); }


        private void ResetMethod()
        {
            try { File.Delete($@"{spotifyDir}\netutils.dll"); }
            catch (UnauthorizedAccessException exception)
            {
                MessageBox.Show($"No ha sido posible acceder al archivo: \"{spotifyDir}\\netutils.dll\"." + Environment.NewLine + "Finaliza Spotify desde el administrador de tareas, y comprueba que tienes privilegios suficientes para eliminar el archivo.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw exception;
            }
            catch (DirectoryNotFoundException) { }

            if (Directory.Exists(updateFolderDir))
            {
                FileSecurity(updateFolderDir, FileSystemRights.FullControl, AccessControlType.Allow, true);
                Directory.Delete(updateFolderDir, true);
            }
        }

        private bool CheckPatch()
        {
            bool spotifyDowngraded = !CheckSpotifyVersion();
            bool natutils = File.Exists($@"{spotifyDir}\netutils.dll");
            bool spotifyMigrator = !File.Exists($@"{spotifyDir}\SpotifyMigrator.exe");
            bool spotifyStartupTask = !File.Exists($@"{spotifyDir}\SpotifyStartupTask.exe");
            bool autoUpdateRestricted = false;

            if (Directory.Exists(updateFolderDir))
            {
                DirectorySecurity fSecurity = Directory.GetAccessControl(updateFolderDir, AccessControlSections.All);
                AuthorizationRuleCollection rules = fSecurity.GetAccessRules(true, true, typeof(NTAccount));
                foreach (FileSystemAccessRule rule in rules)
                    if (rule.IdentityReference.Value.Equals(UserName, StringComparison.CurrentCultureIgnoreCase))
                        if ((rule.FileSystemRights & FileSystemRights.FullControl) > 0 && rule.AccessControlType == AccessControlType.Deny) autoUpdateRestricted = true;
            }

            if (spotifyDowngraded && natutils && spotifyMigrator && spotifyStartupTask && autoUpdateRestricted) return true;
            else return false;

        }

        private void Terminate(bool patching, string label0Text, string messageBoxText)
        {
            if (patching) SpotifyPictureBox.Image = Properties.Resources.AddsOffImage; else SpotifyPictureBox.Image = Properties.Resources.AddsOnImage;
            label4.Visible = label3.Visible = label2.Visible = label1.Visible = false; label0.Text = label0Text; label0.Visible = true;
            CentralPictureBox.Image = Properties.Resources.DoneImage; CentralPictureBox.Visible = true;
            MessageBox.Show(messageBoxText, "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Information);
            try { Process.Start($@"{spotifyDir}\Spotify.exe"); } catch(Exception) { }
            Application.Exit();
        }

        private void RunHidden(string software, string commands)
        {
            Process extract = new Process();
            extract.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            extract.StartInfo.FileName = software;
            extract.StartInfo.Arguments = commands;
            extract.Start();
            extract.WaitForExit();
        }

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
                MessageBox.Show($"No ha sido posible acceder a la ruta: \"{updateFolderDir}\\\"." + Environment.NewLine + "Finaliza Spotity desde el adminisitrador de tareas, y reinicia BlockTheSpot." + Environment.NewLine + Environment.NewLine + "Si continúas con este problema, accede a la ruta anteriormente mencionada." + Environment.NewLine + "En caso de estar parcheando deberás eliminar el directorio \"Update\" y volver a crearlo." + Environment.NewLine + "Haz clic derecho sobre la carpeta y dirigete a propiedades, abre la pestaña de seguridad, pulsa en editar... y selecciona tu nombre de usuario; por último, marca la casilla \"denegar control total\", aplicar y listo." + Environment.NewLine + "En caso de estar reseteando, desmarca todas las casillas que denieguen tu acceso, aplicar y, por último, elimina el directorio.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw exception;
            }
        }

        private void PatchButton_Click(object sender, EventArgs e) => PatchButtonMethod();
        private void ResetButton_Click(object sender, EventArgs e) => ResetButtonMethod();
        private void WarningButton_Click(object sender, EventArgs e) => WarningButtonMethod();
        private void byButton_Click(object sender, EventArgs e) => Process.Start("https://github.com/master131/BlockTheSpot");
        private static string updateFolderDir { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Spotify\\Update";
        private static string spotifyDir { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Spotify";
        public static string UserName { get; set; } = WindowsIdentity.GetCurrent().Name;
        private static string tempPath { get; set; } = Path.GetTempPath();
        private static Version neededSpotifyVersion { get; set; } = new Version("1.1.4.197");
        private bool spotifyInstalled { get; set; }
    }
}
