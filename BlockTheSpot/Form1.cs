using System;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Threading;
using System.Collections.Generic;

namespace BlockTheSpot
{
    public partial class BlockTheSpot : Form
    {
        private static string SpotifyDir { get; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Spotify";
        private static string SpotifyLocalDir { get; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Spotify";
        private static Uri SpotifyUri { get; } = new Uri("https://upgrade.scdn.co/upgrade/client/win32-x86/spotify_installer-1.1.89.862.g94554d24-13.exe");
        private static Uri ChromeElfUri { get; } = new Uri("https://github.com/mrpond/BlockTheSpot/releases/latest/download/chrome_elf.zip");


        public BlockTheSpot()
        {
            CheckIfAlreadyLaunched();
            InitializeComponent();
        }


        private void CheckIfAlreadyLaunched()
        {
            bool alreadyLaunched = Process.GetProcesses().Count(p => p.ProcessName.Equals(Process.GetCurrentProcess().ProcessName)) > 1;

            if (alreadyLaunched)
            {
                MessageBox.Show("Ya existe una instancia activa de BlockTheSpot.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }
        }
        //
        // Comprueba si el parcheado ya ha sido realizado y, si Spotify versión de escritorio esta instalado.
        private void BlockTheSpot_Load(object sender, EventArgs e)
        {
            if (!DowngradeRequired() && File.Exists($@"{SpotifyDir}\chrome_elf_bak.dll") && File.Exists($@"{SpotifyDir}\config.ini"))
            {
                SpotifyPictureBox.Image = Properties.Resources.AddsOffImage;
                PatchButton.Enabled = false;
            }
            else
                ResetButton.Enabled = false;

            if (!File.Exists($@"{SpotifyDir}\Spotify.exe"))
                PatchButton.Text = "Instalar Spotify y\n" + "Bloquear anuncios";
        }
        //
        // Comprueba si la versión actual de Spotify es superior a la compatible (1.1.89.862).
        // Si Spotify no está instalado, devuelve <true>.
        private bool DowngradeRequired()
        {
            bool spotifyIsInstalled = File.Exists($@"{SpotifyDir}\Spotify.exe");

            Version actualVer = (!spotifyIsInstalled) ? null :
                new Version(FileVersionInfo.GetVersionInfo(SpotifyDir + "\\Spotify.exe").FileVersion);

            return (actualVer == null || actualVer.CompareTo(new Version("1.1.89.862")) > 0) ? true : false;
        }

        private void PatchButtonMethod()
        {
            Cursor = Cursors.Default;
            WorkingPictureBox.BringToFront();
            WorkingPictureBox.Visible = true;

            try
            {
                TerminateSpotify();
                WinStorePackageRemoval();
                DowngradeClient();
                Patch();
                DisableAutoUpdate();

                // Si existe un acceso directo a Spotify en el escritorio, eliminalo.
                if (File.Exists($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\Spotify.lnk"))
                    File.Delete($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\Spotify.lnk");
            }
            catch (Exception e)
            {
                MessageBox.Show(this, e.StackTrace, "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WorkingPictureBox.Visible = false;
                return;
            }

            // Fin de la aplicación...

            SpotifyPictureBox.Image = Properties.Resources.AddsOffImage;
            WorkingPictureBox.Image = Properties.Resources.DoneImage;

            MessageBox.Show(this, "¡Todo listo! Gracias por utilizar BlockTheSpot.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Process.Start($@"{SpotifyDir}\Spotify.exe");

            Application.Exit();
        }

        private void ResetButtonMethod()
        {
            Cursor = Cursors.Default;
            WorkingPictureBox.BringToFront();
            WorkingPictureBox.Visible = true;

            try
            {
                TerminateSpotify();
                UndoPatch();
                UpdateClient();
            }
            catch (Exception e)
            {
                MessageBox.Show(this, $"Error: {e.Message}", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WorkingPictureBox.Visible = false;
                return;
            }

            SpotifyPictureBox.Image = Properties.Resources.AddsOnImage;
            WorkingPictureBox.Image = Properties.Resources.DoneImage;

            MessageBox.Show(this, "¡Spotify ha sido restablecido con éxito!\n" + "Gracias por utilizar BlockTheSpot.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Application.Exit();
        }
        //
        // *** Excepción esporadica no controlada: <Acceso denegado> ***
        private void TerminateSpotify()
        {
            foreach (Process p in Process.GetProcessesByName("Spotify"))
            {
                p.Kill();
                p.WaitForExit();
            }
        }
        //
        // Intenta desinstalar el paquete UWP <SpotifyAB.SpotifyMusic>, incompatible con BlockTheSpot.
        private void WinStorePackageRemoval()
        {
            var pkgManager = new Windows.Management.Deployment.PackageManager();
            var spotifyPkg = pkgManager.FindPackagesForUser(string.Empty).FirstOrDefault(pkg => pkg.Id.Name.Equals("SpotifyAB.SpotifyMusic"));

            // El paquete <SpotifyAB.SpotifyMusic> no esta instalado en la cuenta usuario actual.
            if (spotifyPkg == null)
                return;

            // Desinstala el paquete <SpotifyAB.SpotifyMusic> de la cuenta usuario actual.
            var removal = pkgManager.RemovePackageAsync(spotifyPkg.Id.FullName); /// RemovalOptions.RemoveForAllUsers

            // Este evento es señalado cunado la operación finaliza.
            ManualResetEvent removalEvent = new ManualResetEvent(false);

            // Define el delago con un argumento lambda.
            removal.Completed = (depProgress, status) => { removalEvent.Set(); };

            // Espera a que la operación finalice.
            removalEvent.WaitOne();

            if (removal.Status != Windows.Foundation.AsyncStatus.Completed)
                throw new Exception("La desintalación del paquete SpotifyAB.SpotifyMusic (Microsoft Store) ha fallado:\n" + removal.GetResults().ErrorText + "\n\nPara evitar conflictos, es necesario desinstalar este paquete. Prueba a ejecutar BlockTheSpot como administrador o realiza la desinstalación manualmente.");
        }

        private void DowngradeClient()
        {
            if (!DowngradeRequired())
                return;

            try
            {
                // Desbloquea el control total de la carpeta de actualizaciones para el grupo de usuarios actual y,
                // evitar así una excepción en el método siguiente <DeleteAllFilesExcept()>.
                FileSecurity($@"{SpotifyLocalDir}\Update", FileSystemRights.FullControl, AccessControlType.Allow, true);

                // Elimina todos los archivos de los directorios de Spotify,
                // exceptuando los archivos propios del perfil de usuario.
                DeleteAllFilesExcept(SpotifyDir, new List<string>() { "Users" }, new List<string> { "prefs" });
                DeleteAllFilesExcept(SpotifyLocalDir, new List<string>() { "Users", "Cache", "Cache_Data", "Code_Cache", "GPUCache" },
                    new List<string> { "LocalPrefs.json" });

                string fileName = "spotify_installer-1.1.89.862.g94554d24-13.exe";

                new WebClient().DownloadFile(SpotifyUri, Path.GetTempPath() + fileName);

                // Inicia el instalador de la última versión de Spotify compatible con BlockTheSpot (1.1.89.862).
                TopMost = false;
                Process.Start(Path.GetTempPath() + fileName).WaitForExit();
                TopMost = true;

                TerminateSpotify();
            }
            catch (WebException)
            {
                throw new Exception("No ha sido posible descargar los archivos necesarios.\n" + "Comprueba tu conexión a internet e inténtalo de nuevo.");
            }
            catch (Exception)
            {
                // No controlar.
            }


            // Comprueba si la instalación ha sido completada con éxito.
            if (DowngradeRequired())
                throw new Exception($"El downgrade de Spotify ha fallado.");
        }

        private void Patch()
        {
            try
            {
                // Descarga los archivos encargados de bloquear los anuncios de Spotify.
                string fileName = "chrome_elf.zip";
                new WebClient().DownloadFile(ChromeElfUri, Path.GetTempPath() + fileName);

                // Elimina el directorio destino de descompresión, si existiera.
                if (Directory.Exists(Path.GetTempPath() + "chrome_elf\\"))
                    Directory.Delete(Path.GetTempPath() + "chrome_elf\\", true);

                // Extrae los archivos <chrome_elf.dll> y <config.ini> del archivo zip descargado.
                System.IO.Compression.ZipFile.ExtractToDirectory(Path.GetTempPath() + fileName, Path.GetTempPath() + "chrome_elf\\");

                // Introduce los archivos en el directorio principal de Spotify y crea un respaldo del archivo <chrome_elf.dll>.
                // * Debe existir previamente el archivo <chrome_elf.dll> para poder llevar a cabo el parche.
                File.Replace($@"{Path.GetTempPath()}chrome_elf\chrome_elf.dll", $@"{SpotifyDir}\chrome_elf.dll", $@"{SpotifyDir}\chrome_elf_bak.dll");
                File.Copy($@"{Path.GetTempPath()}chrome_elf\config.ini", $@"{SpotifyDir}\config.ini", true);
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
            try
            {
                if (Directory.Exists($@"{SpotifyLocalDir}\Update"))
                {
                    // Elimina la carpeta de actualizaciones de Spotify.
                    FileSecurity($@"{SpotifyLocalDir}\Update", FileSystemRights.FullControl, AccessControlType.Allow, true);
                    Directory.Delete($@"{SpotifyLocalDir}\Update", true);
                }

                // Bloquea el control total de la carpeta de actualizaciones para el grupo de usuarios actual.
                Directory.CreateDirectory($@"{SpotifyLocalDir}\Update");
                FileSecurity($@"{SpotifyLocalDir}\Update", FileSystemRights.FullControl, AccessControlType.Deny, true);
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException($"No ha sido posible acceder a la siguiente ruta: \"{SpotifyLocalDir}\\Update\\\"." + Environment.NewLine + Environment.NewLine + "Prueba a ejecutar BlockTheSpot como Administrador.");
            }
        }

        private void UndoPatch()
        {
            try
            {
                // Desbloquea el control total de la carpeta de actualizaciones para el grupo de usuarios actual.
                FileSecurity($@"{SpotifyLocalDir}\Update", FileSystemRights.FullControl, AccessControlType.Allow, true);

                // Elimina el archivo parcheado <chrome_elf.dll>.
                if (File.Exists($@"{SpotifyDir}\chrome_elf.dll"))
                    File.Delete($@"{SpotifyDir}\chrome_elf.dll");

                // Restaura el archivo original desde <chrome_elf_bak.dll>.
                if (File.Exists($@"{SpotifyDir}\chrome_elf_bak.dll"))
                    File.Move($@"{SpotifyDir}\chrome_elf_bak.dll", $@"{SpotifyDir}\chrome_elf.dll");

                // Elimina el archivo <config.ini>. El archivo <chrome_elf.dll> se rescribirá al actualizar Spotify.
                if (File.Exists($@"{SpotifyDir}\config.ini"))
                    File.Delete($@"{SpotifyDir}\config.ini");
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException($"No ha sido posible acceder a la siguiente ruta: \"{SpotifyLocalDir}\\Update\\\"." + Environment.NewLine + Environment.NewLine + "Prueba a ejecutar BlockTheSpot como Administrador.");
            }
        }

        private void UpdateClient()
        {
            try
            {
                string fileName = "spotify_installer-update.exe";
                new WebClient().DownloadFile("https://download.scdn.co/SpotifySetup.exe", Path.GetTempPath() + fileName);

                // Inicia el instalador de la ultima versión de Spotify.
                TopMost = false;
                Process.Start(Path.GetTempPath() + fileName).WaitForExit();
                TopMost = true;
            }
            catch (WebException)
            {
                MessageBox.Show(this, "No ha sido posible actualizar Spotify a su última versión." + Environment.NewLine + "Puedes llevar a cabo facilmente esta actualización accediendo al apartado de [Configuración], [Acerca de Spotify] desde los ajustes de Spotify.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void FileSecurity(string dir, FileSystemRights rights, AccessControlType controlType, bool addRule)
        {
            if (Directory.Exists(dir))
            {
                DirectorySecurity dirSecurity = Directory.GetAccessControl(dir);

                dirSecurity.SetAccessRuleProtection(false, false);

                foreach (FileSystemAccessRule rule in dirSecurity.GetAccessRules(true, true, typeof(NTAccount)))
                    dirSecurity.RemoveAccessRule(rule);

                if (addRule)
                    dirSecurity.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, rights, controlType));
                else
                    dirSecurity.RemoveAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, rights, controlType));

                Directory.SetAccessControl(dir, dirSecurity);
            }
        }

        private void DeleteAllFilesExcept(string targetDir, List<string> dirNamesToSkip, List<string> fileNamesToSkip)
        {
            List<string> availableDirs = new List<string>() { targetDir };
            List<string> availableFiles = new List<string>();

            availableDirs.AddRange(Directory.EnumerateDirectories(targetDir, "*",
                SearchOption.AllDirectories).Where(s =>
                !dirNamesToSkip.Any(d => s.Contains("Spotify\\" + d))).ToList());

            foreach (string dir in availableDirs)
                availableFiles.AddRange(Directory.EnumerateFiles(dir).Where(s =>
                !fileNamesToSkip.Any(f => s.Contains("Spotify\\" + f))).ToList());

            foreach (string path in availableFiles)
                File.Delete(path);
        }


        private void PatchButton_Click(object sender, EventArgs args) => PatchButtonMethod();
        private void ResetButton_Click(object sender, EventArgs args) => ResetButtonMethod();
        private void BlockTheSpot_HelpButtonClicked(object sender, EventArgs args) => Process.Start("https://github.com/bitasuperactive/BlockTheSpot-C-Sharp");
        //
        // Si el usuario trata de cerrar BlockTheSpot mientras se encuentra en medio de la ejecución,
        // confirmar la solicitud de cierre.
        private void BlockTheSpot_FormClosing(object sender, FormClosingEventArgs close)
        {
            if (close.CloseReason == CloseReason.UserClosing && WorkingPictureBox.Visible)
            {
                DialogResult exitMessage = MessageBox.Show(this, "BlockTheSpot no ha terminado su trabajo, ¿Deseas cerrar la aplicación de todas formas?", "BlockTheSpot", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                
                if (exitMessage == DialogResult.No)
                    close.Cancel = true;
            }
        }
    }
}
