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
using System.Threading.Tasks;

namespace BlockTheSpot
{
    public partial class BlockTheSpot : Form
    {
        private static string RepositoryUrl = "https://github.com/bitasuperactive/BlockTheSpot-C-Sharp";
        private static Uri SpotifyUri { get; } = new Uri("https://download.scdn.co/SpotifySetup.exe");
        private static Uri SpotifyDowngradedUri { get; } = new Uri("https://upgrade.scdn.co/upgrade/client/win32-x86/spotify_installer-1.1.89.862.g94554d24-13.exe");
        private static Uri ChromeElfUri { get; } = new Uri("https://github.com/mrpond/BlockTheSpot/releases/latest/download/chrome_elf.zip");
        private static string SpotifyDir { get; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Spotify";
        private static string SpotifyLocalDir { get; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Spotify";


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
                spotifyPictureBox.Image = Properties.Resources.AddsOffImage;
                patchButton.Enabled = false;
                resetButton.Enabled = true;
            }
            else
            {
                if (!File.Exists($@"{SpotifyDir}\Spotify.exe"))
                    patchButton.Text = "Instalar Spotify y\n" + "Bloquear anuncios";

                patchButton.Enabled = true;
                resetButton.Enabled = false;
            }

            outputLabel.Visible = false;
            progressBar.Visible = false;
            progressLabel.Visible = false;
        }

        //
        // Método accionado por el evento <PatchButton_Click>.
        private async Task PatchButtonMethod()
        {
            Cursor = Cursors.Default;
            patchButton.Enabled = false;
            resetButton.Enabled = false;
            outputLabel.Visible = true;

            try
            {
                await TerminateSpotifyAsync();
                UwpPackageRemoval();
                await DowngradeClientAsync();
                await Patch();
                DisableAutoUpdate();

                // Si existe un acceso directo a Spotify en el escritorio, eliminalo.
                if (File.Exists($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\Spotify.lnk"))
                    File.Delete($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\Spotify.lnk");
            }
            catch (Exception ex)
            {
                AskUserToGenerateLogs(ref ex);

                // Habilita los botones correspondientes.
                BlockTheSpot_Load(this, EventArgs.Empty);
                return;
            }

            // Fin de la aplicación...

            outputLabel.Text = "Todo listo.";
            spotifyPictureBox.Image = Properties.Resources.AddsOffImage;

            MessageBox.Show(this, "¡Todo listo! Gracias por utilizar BlockTheSpot.", "BlockTheSpot",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            Process.Start($@"{SpotifyDir}\Spotify.exe");

            Application.Exit();
        }
        //
        // Desinstala el paquete UWP <SpotifyAB.SpotifyMusic>, si lo hubiera, dado que es incompatible con la metodología de parcheo de BTS.
        private void UwpPackageRemoval()
        {
            outputLabel.Text = "Desinstalando Microsoft Store Spotify ...";

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
        private async Task DowngradeClientAsync()
        {
            if (!DowngradeRequired())
                return;

            outputLabel.Text = "Limpiando directorios ...";

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

                await DownloadAsync(SpotifyDowngradedUri, spotifyInstallerName);

                outputLabel.Text = "Instalando downgrade ...";

                // Inicia el instalador de la última versión de Spotify compatible con BlockTheSpot (1.1.89.862).
                this.TopMost = false;
                await Task.Run(() => Process.Start(Path.GetTempPath() + spotifyInstallerName).WaitForExit());
                this.TopMost = true;

                await TerminateSpotifyAsync();
            }
            catch (WebException ex)
            {
                throw new WebException($"No ha sido posible realizar una instalación limpia de <{spotifyInstallerName}>.\n" +
                    "Comprueba tu conexión a internet e inténtalo de nuevo.", ex);
            }

            // Comprueba si la instalación ha sido completada con éxito.
            if (DowngradeRequired())
                throw new FileNotFoundException($"El downgrade de Spotify ha fallado, intentalo de nuevo.");
        }
        //
        // Inyecta los archivos chrome_elf.dll y config.ini, encargados de llevar a cabo el bloqueo de anuncios, en el directorio principal de Spotify.
        private async Task Patch()
        {
            outputLabel.Text = "Parcheando ...";

            try
            {
                string fileName = "chrome_elf.zip";

                // Descarga los archivos encargados de bloquear los anuncios de Spotify.
                await DownloadAsync(ChromeElfUri, fileName);

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
            catch (WebException ex)
            {
                throw new WebException("No ha sido posible descargar los archivos necesarios para llevar a cabo el parcheo.\n" +
                    "Comprueba tu conexión a internet e inténtalo de nuevo.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException($"No ha sido posible acceder a la siguiente ruta: \"{SpotifyDir}\\\".\n" +
                    "Prueba a ejecutar BlockTheSpot como administrador.", ex);
            }
        }
        //
        // Bloquea las actualizaciones automáticas restringiéndo todos los permisos de acceso a la carpeta "%LocalAppData%\Spotify\Update".
        private void DisableAutoUpdate()
        {
            outputLabel.Text = "Bloqueando actualizaciones automáticas ...";

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
                FileSecurity(AccessControlType.Deny);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException($"No ha sido posible acceder a la siguiente ruta: \"{SpotifyLocalDir}\\Update\\\".\n" +
                    "Prueba a ejecutar BlockTheSpot como administrador.", ex);
            }
        }

        //
        // Método accionado por el evento <ResetButton_Click>.
        private async Task ResetButtonMethod()
        {
            Cursor = Cursors.Default;
            patchButton.Enabled = false;
            resetButton.Enabled = false;
            outputLabel.Visible = true;

            try
            {
                await TerminateSpotifyAsync();
                UndoPatch();
                EnableAutoUpdates();
                await UpdateClientAsync();
            }
            catch (Exception ex)
            {
                AskUserToGenerateLogs(ref ex);

                // Habilita los botones correspondientes.
                BlockTheSpot_Load(this, EventArgs.Empty);
                return;
            }

            // Fin de la aplicación...

            outputLabel.Text = "Todo listo.";
            spotifyPictureBox.Image = Properties.Resources.AddsOnImage;

            MessageBox.Show(this, "¡Spotify ha sido restablecido con éxito!\n" + "Gracias por utilizar BlockTheSpot.",
                "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Application.Exit();
        }
        //
        // Elimina los archivos chrome_elf.dll y config.ini, además de restablecer el primero a su versión original.
        private void UndoPatch()
        {
            outputLabel.Text = "Deshaciendo parcheo ...";

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
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException($"No ha sido posible acceder a la siguiente ruta: \"{SpotifyDir}\\\".\n" +
                    "Prueba a ejecutar BlockTheSpot como administrador.", ex);
            }
        }
        //
        // Restablece los permisos de acceso obre el directorio "%LocalAppData%\Spotify\Update".
        private void EnableAutoUpdates()
        {
            outputLabel.Text = "Desbloqueando actualizaciones automáticas ...";

            try
            {
                // Desbloquea el control total de la carpeta de actualizaciones para el grupo de usuarios actual.
                FileSecurity(AccessControlType.Allow);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException($"No ha sido posible acceder a la siguiente ruta: \"{SpotifyLocalDir}\\Update\\\".\n" +
                    "Prueba a ejecutar BlockTheSpot como administrador.", ex);
            }
        }
        //
        // Descarga e inicia el instalador de la última versión de Spotify.
        private async Task UpdateClientAsync()
        {
            try
            {
                string fileName = "spotify_installer-update.exe";

                // Descarga la última versión de Spotify.
                await DownloadAsync(SpotifyUri, fileName);

                outputLabel.Text = "Instalando actualización ...";

                // Inicia el instalador.
                this.TopMost = false;
                await Task.Run(() => Process.Start(Path.GetTempPath() + fileName).WaitForExit());
                this.TopMost = true;

                // Comprueba si la instalación se ha realizado correctamente.
                if (!DowngradeRequired())
                    throw new FileNotFoundException();
            }
            catch (WebException)
            {
                MessageBox.Show(this, "No ha sido posible descargar la última versión de Spotify.\n" +
                    "Realiza la instalación manualmente", "BlockTheSpot", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(this, "No ha sido posible finalizar la instalación la última versión de Spotify.\n" +
                    "Realiza la instalación manualmente.", "BlockTheSpot",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        //
        // Cierra todas las instancias de Spotify.
        private async Task TerminateSpotifyAsync()
        {
            outputLabel.Text = "Finalizando Spotify ...";

            foreach (Process p in Process.GetProcessesByName("Spotify"))
                p.Kill();

            while (Process.GetProcessesByName("Spotify").Length > 0)
                await Task.Run(() => Thread.Sleep(100));
        }
        //
        // Comprueba si la versión actual de Spotify es superior a la compatible (1.1.89.862).
        // Si Spotify no está instalado, devuelve <true>.
        private bool DowngradeRequired()
        {
            bool spotifyIsInstalled = File.Exists($@"{SpotifyDir}\Spotify.exe");
            bool chromeElfExists = File.Exists($@"{SpotifyDir}\chrome_elf.dll"); // Archivo necesario para el parcheo.

            Version actualVer = (!spotifyIsInstalled) ? null :
                new Version(FileVersionInfo.GetVersionInfo(SpotifyDir + "\\Spotify.exe").FileVersion);

            return (!spotifyIsInstalled ||
                !chromeElfExists ||
                actualVer.CompareTo(new Version("1.1.89.862")) > 0) ? true : false;
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
        // Realiza una descarga asincrónica mostrando el progreso en una barra de progreso.
        private async Task DownloadAsync(Uri uri, string fileName)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadProgressChanged += (sender, e) => progressBar_UpdateProgress(e.ProgressPercentage, e.BytesReceived == e.TotalBytesToReceive);

            this.outputLabel.Text = $"Descargando {fileName} ...";
            this.toolTip.SetToolTip(this.outputLabel, this.outputLabel.Text);

            // Realiza una descarga asincrónica del <Uri> pasado por parámetro.
            await webClient.DownloadFileTaskAsync(uri, Path.GetTempPath() + fileName);

            // Libera los recursos utilizados tras finalizar la descarga.
            webClient.Dispose();
        }
        //
        // Maneja el control de acceso para el directorio "Update" de Spotify,
        // permitiéndo o negando los permisos de escritura y eliminación.
        private void FileSecurity(AccessControlType controlType)
        {
            string dir = $@"{SpotifyLocalDir}\Update";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            DirectorySecurity dirSecurity = Directory.GetAccessControl(dir);

            dirSecurity.SetAccessRuleProtection(false, false);

            foreach (FileSystemAccessRule rule in dirSecurity.GetAccessRules(true, true, typeof(NTAccount)))
                dirSecurity.RemoveAccessRule(rule);

            dirSecurity.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, FileSystemRights.Write | FileSystemRights.Delete, controlType));

            Directory.SetAccessControl(dir, dirSecurity);
        }
        //
        // Informa al usuario del mensaje de la excepción generada y le consulta
        // si desea generar "logs" y subirlos al repositorio en GitHub.
        private void AskUserToGenerateLogs(ref Exception ex)
        {
            // Consulta al usuario si desea subir estos logs al repositorio de GitHub para investigar el error.
            DialogResult dialogResult = MessageBox.Show(this, ex.Message + "\n\n" +
                "Si el problema persiste por favor, crea un [ Issue ] en el respositorio de BlockTheSpot en GitHub, " +
                "subiendo los logs <blockthespot_log.txt> que se generarán en la carpeta %Temp% de tu equipo, así podré investigarlo.\n\n" +
                "Pulsa en [ Sí ] para abrir el repositorio y carpeta indicados.", "BlockTheSpot",
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
            File.WriteAllText(Path.GetTempPath() + "blockthespot_log.txt", stackTraces);

            // Abre el repositorio en GitHub y la carpeta de archivos temporales.
            Process.Start(RepositoryUrl + @"/issues/new");
            Process.Start(Path.GetTempPath());

            Application.Exit();
        }


        private async void patchButton_Click(object sender, EventArgs args) => await PatchButtonMethod();
        private async void resetButton_Click(object sender, EventArgs args) => await ResetButtonMethod();
        private void progressBar_UpdateProgress(int progress, bool completed)
        {
            this.progressBar.Visible = completed ? false : true;
            this.progressLabel.Visible = completed ? false : true;
            this.progressBar.Value = progress;
            this.progressLabel.Text = progress + "%";

            if (completed)
                this.toolTip.SetToolTip(this.outputLabel, "");
        }
        private void BlockTheSpot_HelpButtonClicked(object sender, EventArgs args) => Process.Start(RepositoryUrl);
        private void BlockTheSpot_FormClosing(object sender, FormClosingEventArgs close)
        {
            // Si el usuario trata de cerrar BlockTheSpot mientras se encuentra en medio de la ejecución,
            // confirmar la solicitud de cierre.
            if (close.CloseReason == CloseReason.UserClosing && patchButton.Enabled == false && resetButton.Enabled == false)
            {
                DialogResult exitMessage = MessageBox.Show(this, "BlockTheSpot no ha terminado su trabajo, ¿Deseas cerrar la aplicación de todas formas?", "BlockTheSpot", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                
                if (exitMessage == DialogResult.No)
                    close.Cancel = true;
            }
        }
    }
}