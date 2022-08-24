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
        private static string RepositoryUrl = "https://github.com/bitasuperactive/BlockTheSpot-C-Sharp";
        private static string SpotifyDir { get; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Spotify";
        private static string SpotifyLocalDir { get; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Spotify";
        private static Uri SpotifyUri { get; } = new Uri("https://upgrade.scdn.co/upgrade/client/win32-x86/spotify_installer-1.1.89.862.g94554d24-13.exe");
        private static Uri ChromeElfUri { get; } = new Uri("https://github.com/mrpond/BlockTheSpot/releases/latest/download/chrome_elf.zip");


        public BlockTheSpot()
        {
            CheckIfAlreadyLaunched();
            InitializeComponent();
        }


        //
        // Comprueba si BlockTheSpot se encuentra ya en ejecución.
        private void CheckIfAlreadyLaunched()
        {
            if (Process.GetProcesses().Count(p => p.ProcessName.Equals(Process.GetCurrentProcess().ProcessName)) > 1)
            {
                MessageBox.Show("Ya existe una instancia activa de BlockTheSpot.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }
        }
        //
        // Comprueba si el parcheado ya ha sido realizado y, si Spotify versión de escritorio esta instalado.
        private void BlockTheSpot_Load(object sender, EventArgs e)
        {
            if (!DowngradeRequired() &&
                File.Exists($@"{SpotifyDir}\chrome_elf_bak.dll") &&
                File.Exists($@"{SpotifyDir}\config.ini"))
            {
                SpotifyPictureBox.Image = Properties.Resources.AddsOffImage;
                PatchButton.Enabled = false;
            }
            else
            {
                if (!File.Exists($@"{SpotifyDir}\Spotify.exe"))
                    PatchButton.Text = "Instalar Spotify y\n" + "Bloquear anuncios";

                ResetButton.Enabled = false;
            }
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
        //
        // Método accionado por el evento <PatchButton_Click>.
        private void PatchButtonMethod()
        {
            Cursor = Cursors.Default;
            WorkingPictureBox.BringToFront();
            WorkingPictureBox.Visible = true;

            try
            {
                TerminateSpotify();
                UwpPackageRemoval();
                DowngradeClient();
                Patch();
                DisableAutoUpdate();

                // Si existe un acceso directo a Spotify en el escritorio, eliminalo.
                if (File.Exists($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\Spotify.lnk"))
                    File.Delete($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\Spotify.lnk");
            }
            catch (Exception ex)
            {
                AskUserToGenerateLogs(ref ex);

                WorkingPictureBox.Visible = false;
                return;
            }

            // Fin de la aplicación...

            SpotifyPictureBox.Image = Properties.Resources.AddsOffImage;
            WorkingPictureBox.Image = Properties.Resources.DoneImage;

            MessageBox.Show(this, "¡Todo listo! Gracias por utilizar BlockTheSpot.", "BlockTheSpot",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            Process.Start($@"{SpotifyDir}\Spotify.exe");

            Application.Exit();
        }
        //
        // Método accionado por el evento <ResetButton_Click>.
        private void ResetButtonMethod()
        {
            Cursor = Cursors.Default;
            WorkingPictureBox.BringToFront();
            WorkingPictureBox.Visible = true;

            try
            {
                TerminateSpotify();
                UndoPatch();
                EnableAutoUpdates();
                UpdateClient();
            }
            catch (Exception ex)
            {
                AskUserToGenerateLogs(ref ex);

                WorkingPictureBox.Visible = false;
                return;
            }

            // Fin de la aplicación...

            SpotifyPictureBox.Image = Properties.Resources.AddsOnImage;
            WorkingPictureBox.Image = Properties.Resources.DoneImage;

            MessageBox.Show(this, "¡Spotify ha sido restablecido con éxito!\n" + "Gracias por utilizar BlockTheSpot.",
                "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Application.Exit();
        }
        //
        // Cierra todas las instancias de Spotify.
        private void TerminateSpotify()
        {
            foreach (Process p in Process.GetProcessesByName("Spotify"))
                p.Kill();
            foreach (Process p in Process.GetProcessesByName("Spotify"))
                p.WaitForExit();
        }
        //
        // Desinstala el paquete UWP <SpotifyAB.SpotifyMusic>, si lo hubiera, dado que es incompatible con la metodología de parcheo de BTS.
        private void UwpPackageRemoval()
        {
            try
            {
                var pkgManager = new Windows.Management.Deployment.PackageManager();
                var spotifyPkg = pkgManager.FindPackagesForUser(string.Empty).FirstOrDefault(
                    pkg => pkg.Id.Name.Equals("SpotifyAB.SpotifyMusic"));

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
                    throw new UnauthorizedAccessException(
                        "La desintalación del paquete SpotifyAB.SpotifyMusic (Microsoft Store) ha fallado:\n" +
                        removal.GetResults().ErrorText + "\n\n +" +
                        "Prueba a ejecutar BlockTheSpot como administrador o realiza la desinstalación manualmente.");
            }
            catch (Exception ex) // AccessDeniedException
            {
                // removal.GetResults().ErrorText
                throw new UnauthorizedAccessException(
                    "La desintalación del paquete SpotifyAB.SpotifyMusic (Microsoft Store) ha fallado:\n" +
                    "Prueba a ejecutar BlockTheSpot como administrador o realiza la desinstalación manualmente.", ex);
            }
        }
        //
        // Instala o realiza un downgrade a la versión 1.1.89.862.g94554d24-13 de Spotify,
        // en la cual se explotará la vulnerabilidad que permite a BTS bloquear sus anuncios.
        private void DowngradeClient()
        {
            if (!DowngradeRequired())
                return;

            string spotifyInstallerName = "spotify_installer-1.1.89.862.g94554d24-13.exe";

            try
            {
                // Desbloquea el control total de la carpeta de actualizaciones para el grupo de usuarios actual y,
                // evitar así una excepción en el método siguiente <DeleteAllFilesExcept()>.
                FileSecurity(AccessControlType.Allow);

                // Elimina todos los archivos de los directorios de Spotify,
                // exceptuando los archivos propios del perfil de usuario.
                ClearSpotifyDirs(SpotifyDir, new List<string>() { "Users" }, new List<string> { "prefs" });
                ClearSpotifyDirs(SpotifyLocalDir, new List<string>() { "Users", "Cache", "Cache_Data", "Code_Cache", "GPUCache" },
                    new List<string> { "LocalPrefs.json" });

                new WebClient().DownloadFile(SpotifyUri, Path.GetTempPath() + spotifyInstallerName);

                // Inicia el instalador de la última versión de Spotify compatible con BlockTheSpot (1.1.89.862).
                TopMost = false;
                Process.Start(Path.GetTempPath() + spotifyInstallerName).WaitForExit();
                TopMost = true;

                TerminateSpotify();
            }
            catch (WebException ex)
            {
                throw new WebException($"No ha sido posible realizar una instalación limpia de <{spotifyInstallerName}>.\n" +
                    "Comprueba tu conexión a internet e inténtalo de nuevo.", ex);
            }
            catch (Exception)
            {
                // El programa puede continuar con el resto de excepciones.
            }

            // Comprueba si la instalación ha sido completada con éxito.
            if (DowngradeRequired())
                throw new Exception($"El downgrade de Spotify ha fallado.");
        }
        //
        // Inyecta los archivos chrome_elf.dll y config.ini, encargados de llevar a cabo el bloqueo de anuncios, en el directorio principal de Spotify.
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
                throw new WebException("No ha sido posible descargar los archivos necesarios para llevar a cabo el parcheo.\n" + 
                    "Comprueba tu conexión a internet e inténtalo de nuevo.");
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException($"No ha sido posible acceder a la siguiente ruta: \"{SpotifyDir}\\\".\n" +
                    "Prueba a ejecutar BlockTheSpot como administrador.");
            }
        }
        //
        // Bloquea las actualizaciones automáticas restringiéndo todos los permisos de acceso a la carpeta "%LocalAppData%\Spotify\Update".
        private void DisableAutoUpdate()
        {
            try
            {
                string dir = $@"{SpotifyLocalDir}\Update";

                // Elimina la carpeta de actualizaciones de Spotify.
                if (Directory.Exists(dir))
                {
                    FileSecurity(AccessControlType.Allow);
                    Directory.Delete(dir, true);
                }

                // Bloquea el control total de la carpeta de actualizaciones para el grupo de usuarios actual.
                Directory.CreateDirectory(dir);
                FileSecurity(AccessControlType.Deny);
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException($"No ha sido posible acceder a la siguiente ruta: \"{SpotifyLocalDir}\\Update\\\".\n" +
                    "Prueba a ejecutar BlockTheSpot como administrador.");
            }
        }
        //
        // Elimina los archivos chrome_elf.dll y config.ini, además de restablecer el primero a su versión original.
        private void UndoPatch()
        {
            try
            {
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
                throw new UnauthorizedAccessException($"No ha sido posible acceder a la siguiente ruta: \"{SpotifyDir}\\\".\n" +
                    "Prueba a ejecutar BlockTheSpot como administrador.");
            }
        }
        //
        // Restablece los permisos de acceso obre el directorio "%LocalAppData%\Spotify\Update".
        private void EnableAutoUpdates()
        {
            try
            {
                string dir = $@"{SpotifyLocalDir}\Update";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                // Desbloquea el control total de la carpeta de actualizaciones para el grupo de usuarios actual.
                FileSecurity(AccessControlType.Allow);
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException($"No ha sido posible acceder a la siguiente ruta: \"{SpotifyLocalDir}\\Update\\\".\n" +
                    "Prueba a ejecutar BlockTheSpot como administrador.");
            }
        }
        //
        // Descarga e inicia el instalador de la última versión de Spotify.
        private void UpdateClient()
        {
            try
            {
                // Descarga la última versión de Spotify.
                string fileName = "spotify_installer-update.exe";
                new WebClient().DownloadFile("https://download.scdn.co/SpotifySetup.exe", Path.GetTempPath() + fileName);

                // Inicia el instalador.
                TopMost = false;
                Process.Start(Path.GetTempPath() + fileName).WaitForExit();
                TopMost = true;

                // Comprueba si la instalación se ha realizado correctamente.
                if (!DowngradeRequired())
                    throw new FileNotFoundException();
            }
            catch (WebException)
            {
                MessageBox.Show(this, "No ha sido posible descargar la última versión de Spotify.", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(this, "No ha sido posible finalizar la instalación la última versión de Spotify.", "BlockTheSpot",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        //
        // Maneja el control de acceso para el directorio "Update" de Spotify,
        // permitiéndo o negando los permisos de escritura y eliminación.
        private void FileSecurity(AccessControlType controlType)
        {
            string dir = $@"{SpotifyLocalDir}\Update";

            DirectorySecurity dirSecurity = Directory.GetAccessControl(dir);

            dirSecurity.SetAccessRuleProtection(false, false);

            foreach (FileSystemAccessRule rule in dirSecurity.GetAccessRules(true, true, typeof(NTAccount)))
                dirSecurity.RemoveAccessRule(rule);

            dirSecurity.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, FileSystemRights.Write | FileSystemRights.Delete, controlType));

            Directory.SetAccessControl(dir, dirSecurity);
        }
        //
        // Elimina todos los archivos relativos a la versión de escritorio de Spotify, si la hubiera instalada,
        // a excepción de los archivos propios de la configuración y caché del usuario,
        // permitiendo realizar una instalación limpia de Spotify sin perder la información del usuario.
        private void ClearSpotifyDirs(string targetDir, List<string> dirNamesToSkip, List<string> fileNamesToSkip)
        {
            List<string> availableDirs = new List<string>() { targetDir };
            List<string> availableFiles = new List<string>();

            availableDirs.AddRange(Directory.EnumerateDirectories(targetDir, "*", SearchOption.AllDirectories).Where(
                d => !dirNamesToSkip.Any(x => d.Contains("Spotify\\" + x))).ToList());

            foreach (string dir in availableDirs)
                availableFiles.AddRange(Directory.EnumerateFiles(dir).Where(
                    d => !fileNamesToSkip.Any(f => d.Contains("Spotify\\" + f))).ToList());

            foreach (string file in availableFiles)
                File.Delete(file);
        }
        //
        // Informa al usuario del mensaje de la excepción generada y le consulta
        // si desea generar "logs" y subirlos al repositorio en GitHub.
        private void AskUserToGenerateLogs(ref Exception ex)
        {
            // Consulta al usuario si desea subir estos logs al repositorio de GitHub para investigar el error.
            DialogResult dialogResult = MessageBox.Show(this, ex.Message + "\n\n" +
                "Si el problema persiste por favor, crea un [Issue] en el respositorio de BlockTheSpot en GitHub, " +
                "subiendo los logs <bts_log.txt> que se generarán en la carpeta %Temp% de tu equipo, así podré investigarlo.\n\n" +
                "Pulsa en [Sí] para abrir el repositorio y carpeta indicados.", "BlockTheSpot",
                MessageBoxButtons.YesNo, MessageBoxIcon.Error);

            if (dialogResult == DialogResult.No)
                return;

            // Guarda el StackTarce completo en un archivo de texto.
            string stackTraces = $"{ex.Message}\n{ex.StackTrace}";
            while (ex.InnerException != null)
            {
                stackTraces += $"\n{ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
                ex = ex.InnerException;
            }
            File.WriteAllText(Path.GetTempPath() + "bts_log.txt", stackTraces);

            // Abre el repositorio en GitHub y la carpeta de archivos temporales.
            Process.Start(RepositoryUrl + @"/issues/new");
            Process.Start(Path.GetTempPath());

            Application.Exit();
        }


        private void PatchButton_Click(object sender, EventArgs args) => PatchButtonMethod();
        private void ResetButton_Click(object sender, EventArgs args) => ResetButtonMethod();
        private void BlockTheSpot_HelpButtonClicked(object sender, EventArgs args) => Process.Start(RepositoryUrl);
        private void BlockTheSpot_FormClosing(object sender, FormClosingEventArgs close)
        {
            // Si el usuario trata de cerrar BlockTheSpot mientras se encuentra en medio de la ejecución,
            // confirmar la solicitud de cierre.
            if (close.CloseReason == CloseReason.UserClosing && WorkingPictureBox.Visible)
            {
                DialogResult exitMessage = MessageBox.Show(this, "BlockTheSpot no ha terminado su trabajo, ¿Deseas cerrar la aplicación de todas formas?", "BlockTheSpot", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                
                if (exitMessage == DialogResult.No)
                    close.Cancel = true;
            }
        }
    }
}