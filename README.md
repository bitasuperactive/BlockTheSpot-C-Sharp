# Overview
<img src="https://github.com/bitasuperactive/BlockTheSpot-C-Sharp/blob/master/doc/icon.ico" width="216"/> <img src="https://github.com/bitasuperactive/BlockTheSpot-C-Sharp/blob/master/doc/blockthespot.png" width="203"/>

**Con un solo clic, no más anuncios.**

[Descargas y Notas de Publicación](https://github.com/bitasuperactive/BlockTheSpot-C-Sharp/releases) | [YouTube](https://www.youtube.com/channel/UCc-AA6VaZh81DYYCrSAMS5w?) | Basado en BlockTheSpot by [@master131](https://github.com/master131/BlockTheSpot)

## Descripción
**Estado:** Funcionando en 01/11/2020 :white_check_mark:

**BlockTheSpot** (BTS en adelante) **lleva a cabo las siguientes funciones:**   
Al pulsar el botón principal, BTS instala automáticamente la última versión de Spotify testada para el propósito de este repositorio (*spotify_installer-1.1.4.197.g92d52c4f-13.exe*), tras lo cual, introduce un archivo dll (natutils.dll), encargado de bloquear banners/vídeos/audios publicitarios, en el directorio principal de Spotify ubicado en la siguiente ruta: *%appdata%\Spotify*. A continuación, BTS restringe los permisos de acceso al directorio *%localappdata%\Spotify\Update* con el fin de evitar actualizaciones automáticas por parte de Spotify.    
Por último, al pulsar el botón secundario, instala la última versión actualizada de Spotify eliminándo toda modificación mencionada anteriormente, restableciendo así la aplicación sin dejar rastro en su sistema operativo.

**Los archivos utilizados en este proyecto proceden de las siguientes fuentes, citadas a continuación.**   
*natutils.dll*: https://raw.githubusercontent.com/master131/BlockTheSpot/master/netutils.dll        
*spotify_installer-1.1.4.197.g92d52c4f-13.exe*: https://upgrade.spotify.com/upgrade/client/win32-x86/spotify_installer-1.1.4.197.g92d52c4f-13.exe       
*spotify_installer-update*: https://download.scdn.co/SpotifySetup.exe

[**Análisis antivírico realizado mediante VirusTotal**](https://www.virustotal.com/gui/file/db72d1346a96ca303bfc3c8c46497cfd58c42f987394679860298ef0f3e4f2a3/detection)

## Características
- Compatible únicamente con Windows.
- Bloquea todos los anuncios (banners, vídeos y audios).
- Desbloquea la función "saltar" para cualquier canción.
- Mantiene las funcionalidades de lista de amigos, vídeo vertical y rádio.
- Restablece Spotify a su estado original y actualiza automáticamente.

:warning: BlockTheSpot no es compatible con la versión Microsoft Store de Spotify.    

:information_source: Privilegios de Administrador no requeridos.

## Advertencia
**Al utilizar BlockTheSpot asumes toda responsabilidad subyacente.**    
Spotify suspenderá indefinidamente las cuentas de usuario que utilicen cualquier forma de ad-block (como BlockTheSpot) al violar los términos y condiciones de uso. Medida implementada el 01/03/2019.

*Pero a modo de apunte, por mi experiencia personal puedo decir que tras 2 años utilizándo este método, no he tenido problemas de ningún tipo.*
