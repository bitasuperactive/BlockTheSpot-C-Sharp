# Overview
<img src="https://github.com/bitasuperactive/BlockTheSpot-C-Sharp/blob/master/doc/icon.ico" width="216"/> <img src="https://github.com/bitasuperactive/BlockTheSpot-C-Sharp/blob/master/doc/blockthespot.png" width="203"/>

**Con un solo clic, no más anuncios en Spotify.**

[Downloads & Release notes](https://github.com/bitasuperactive/BlockTheSpot-C-Sharp/releases) | [YouTube](https://www.youtube.com/channel/UCc-AA6VaZh81DYYCrSAMS5w?).

## Descripción General    
**Estado:** *Working on 2020-11-01* :white_check_mark:    

*BlockTheSpot* (BTS en adelante) lleva a cabo las siguientes funciones:
Al pulsar el botón principal, BTS instala automáticamente la última versión de Spotify testada para el propósito de este repositorio (*spotify_installer-1.1.4.197.g92d52c4f-13.exe*), tras lo cual, introduce un archivo dll (natutils.dll), encargado de llevar a cabo las *características* especificadas más adelante, en el directorio principal de Spotify ubicado en la siguiente ruta: "%appdata%\Spotify". A continuación, BTS restringe los permisos de acceso al directorio "Update" localizado en la siguiente ruta: "%localappdata%\Spotify\Update", con el fin de evitar actualizaciones automáticas por parte de Spotify.    
Por último, al pulsar el botón secundario, instala la última versión actualizada de Spotify eliminándo toda modificación mencionada anteriormente, restableciendo así la aplicación sin dejar rastro en su sistema operativo.   
    
Los archivos utilizados para este propósito proceden de las fuentes citadas a continuación.
    
natutils.dll: "https://raw.githubusercontent.com/master131/BlockTheSpot/master/netutils.dll"
spotify_installer-1.1.4.197.g92d52c4f-13.exe: "http://upgrade.spotify.com/upgrade/client/win32-x86/spotify_installer-1.1.4.197.g92d52c4f-13.exe"
spotify_installer-update: "https://download.scdn.co/SpotifySetup.exe"

## Características
- Solo Windows.
- BlockTheSpot se encarga de todo, pulsa un botón y olvídate.
- Bloquea todos los anuncios (banners/vídeos/audio) de la aplicación.
- Desbloquea la función de *saltar* para cualquier canción.
- Mantiene las funcionalidades de lista de amigos, vídeo vertical y rádio.
- Restablece Spotify a su versión original.

:warning: BlockTheSpot no es compatible con la versión Microsoft Store de Spotify.

## Instalación | Desinstalación
Simplemente ejecuta BlockTheSpot y elija la opción deseada.

:information_source: Privilegios de Administrador NO requeridos.

## Credits
Based on BlockTheSpot by [@master131](https://github.com/master131/BlockTheSpot).

## Disclaimer
**Al utilizar BlockTheSpot asumes toda responsabilidad subyacente.**    
*Spotify suspenderá indefinidamente las cuentas de usuario que utilicen cualquier forma de ad-block (como BlockTheSpot) al violar los términos y condiciones de uso. Medida implementada el 01/03/2019.*
