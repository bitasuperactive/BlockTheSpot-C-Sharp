# BlockTheSpot C#
<img src="https://github.com/bitasuperactive/BlockTheSpot-C-Sharp/blob/master/doc/icon.ico" width="216"/> <img src="https://github.com/bitasuperactive/BlockTheSpot-C-Sharp/blob/master/doc/blockthespot.png" width="203"/>

**Con un solo clic, no más anuncios.**

[Descargas y Notas de Publicación](https://github.com/bitasuperactive/BlockTheSpot-C-Sharp/releases) | [YouTube](https://www.youtube.com/channel/UCc-AA6VaZh81DYYCrSAMS5w?) | Basado en *[BlockTheSpot by @master131](https://github.com/master131/BlockTheSpot)*

## Descripción
**Estado:** Funcionando en 01/11/2020 :white_check_mark:

**BlockTheSpot** (BTS en adelante) **lleva a cabo las siguientes funciones:**   
Al pulsar el botón principal, BTS descarga e instala automáticamente la última versión de Spotify (*spotify_installer-1.1.4.197.g92d52c4f-13.exe*) compatible con el archivo dll (*natutils.dll*) encargado de bloquear los banners, vídeos y audios publicitarios, archivo el cual se introduce en el directorio principal de la aplicación: *%appdata%\Spotify*.    
A continuación, BTS elimina los archivos *%appdata%\Spotify\SpotifyMigrator.exe* y *%appdata%\Spotify\SpotifyStartupTask.exe*, y restringe los permisos de acceso al directorio *%localappdata%\Spotify\Update* con el fin de evitar actualizaciones automáticas por parte de Spotify.    
Por último, al pulsar el botón secundario, instala la última versión actualizada de Spotify eliminándo toda modificación mencionada anteriormente, restableciendo así la aplicación sin dejar trazas en su sistema operativo.

**Los archivos utilizados en este proyecto proceden de las siguientes fuentes, citadas a continuación.**   
*natutils.dll*: https://raw.githubusercontent.com/master131/BlockTheSpot/master/netutils.dll        
*spotify_installer-1.1.4.197.g92d52c4f-13.exe*: https://upgrade.spotify.com/upgrade/client/win32-x86/spotify_installer-1.1.4.197.g92d52c4f-13.exe       
*spotify_installer-update*: https://download.scdn.co/SpotifySetup.exe

[Análisis antivírico realizado por VirusTotal.](https://www.virustotal.com/gui/file/11bdaf7d5faab42d251745a9b9b5a5611747bad864bd0f28a190b19cb27e6986/detection)

## Características
- Solo Windows (requerido [.NET Framework 4.5](https://www.microsoft.com/es-es/download/confirmation.aspx?id=30653) o superior).
- Bloquea todos los anuncios (banners, vídeos y audios).
- Desbloquea la función "saltar" para cualquier canción.
- Mantiene las funcionalidades de lista de amigos, vídeo vertical y rádio.
- Restablece Spotify a su estado original y actualiza automáticamente.

:warning: BlockTheSpot no es compatible con la versión Microsoft Store de Spotify.


## Advertencia de uso
**Al utilizar BlockTheSpot asumes toda responsabilidad subyacente.**    
Spotify suspenderá indefinidamente las cuentas de usuario que utilicen cualquier forma de ad-block al violar los términos y condiciones de uso. Medida implementada el 01/03/2019.

*Pero a modo de apunte, por mi experiencia personal puedo decir que, tras 2 años utilizándo este método, no he tenido problemas de ningún tipo.*
