<img src="https://github.com/bitasuperactive/BlockTheSpot-C-Sharp/blob/master/doc/logo.png">

[Descargas y Notas de Publicación](https://github.com/bitasuperactive/BlockTheSpot-C-Sharp/releases) | Basado en *[BlockTheSpot by @mrpond](https://github.com/mrpond/BlockTheSpot)*


## Descripción
Con un solo clic, BlockTheSpot (**BTS** en adelante) te permite disfrutar de Spotify sin restricciones ni anuncios.

:white_check_mark: Funcional (23/08/2022)


## Características
- Solo Windows (requerido [.NET Framework 4.7.2](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net472-web-installer) o superior).
- Bloquea todos los anuncios (banners, vídeos y audios).
- Desbloquea la función "saltar" para cualquier canción.
- Mantiene las funcionalidades de lista de amigos, vídeo vertical y rádio.

:no_entry: BTS no es compatible con la versión Microsoft Store de Spotify. Pero no te preocupes, BTS se encarga de ello.


## :heavy_check_mark: Bloquear anuncios (e instalar Spotify)
- `Desinstala` automáticamente la versión Microsoft Store de Spotify, la cual es incompatible con la metodología de parcheo de BTS.
- `Instala` o realiza un `downgrade` a la versión [_1.1.89.862.g94554d24-13_](https://upgrade.scdn.co/upgrade/client/win32-x86/spotify_installer-1.1.89.862.g94554d24-13.exe) de Spotify automáticamente, en la cual se explotará la vulnerabilidad que permite a BTS bloquear sus anuncios.
- `Inyecta` los archivos [_chrome_elf.dll_](https://github.com/mrpond/BlockTheSpot/releases) y [_config.ini_](https://github.com/mrpond/BlockTheSpot/releases), encargados de llevar a cabo el bloqueo de anuncios, en el directorio principal de Spotify.
- `Bloquea` las actualizaciones automáticas restringiéndo los permisos de acceso a la carpeta "_%LocalAppData%\Spotify\Update_".


## :heavy_check_mark: Restaurar Spotify
- `Restablece` las actualizaciones automáticas.
- `Deshace` la inyección eliminándo los archivos `chrome_elf.dll` y `config.ini`, si los hubiera.
- `Instala` automáticamente la última versión de Spotify.


## Advertencia de uso
**Al utilizar BlockTheSpot asumes toda responsabilidad subyacente.**

[01/03/2019] *Spotify suspenderá indefinidamente las cuentas de usuario que utilicen cualquier forma de ad-block al violar los términos y condiciones de uso.*
