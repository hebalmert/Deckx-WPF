<p align="center">
  <img src="docs/deckx-banner.png" alt="Deckx, todos tus proyectos a un solo clic" width="100%">
</p>

# Deckx

Widget de escritorio para Windows, al estilo de un StreamDeck: una rejilla de mosaicos que abre
soluciones de Visual Studio, programas, carpetas o paginas, organizados por categorias.

Nacio de un problema concreto: tener doce soluciones de Visual Studio y perder tiempo buscando
cual abrir. Un mosaico, un clic.

## Descargar

[**Descarga Deckx.exe**](https://github.com/hebalmert/Deckx-WPF/releases/latest) y abrelo. Es un
unico archivo, no necesita instalar nada y trae accesos de ejemplo para empezar.

La primera vez Windows mostrara un aviso de SmartScreen porque el ejecutable no esta firmado:
*Mas informacion* y luego *Ejecutar de todas formas*.

Para que arranque con Windows: menu `...` y luego **Instalar en este PC**. Se copia al arranque y
deja su acceso directo en el escritorio. Desde ese mismo menu se puede quitar.

## Que hace

- **Clic** en un mosaico: abre lo que tenga configurado. Un `.slnx` o `.sln` arranca Visual Studio,
  un `.exe` arranca el programa, una carpeta abre el explorador y una url abre el navegador.
- **Clic derecho en un mosaico**: abrir, abrir carpeta, abrir en VS Code, abrir terminal, editar, eliminar.
- **Clic derecho en una categoria**: renombrarla, quitarla conservando sus accesos, o eliminarla con todo.
- **Arrastrar el panel**: mueve el widget a donde quieras y guarda la posicion.
- **Arrastrar un mosaico** sobre otro: lo reordena. Soltarlo sobre el mosaico punteado lo manda al final.
- **Agregar**: el mosaico punteado abre el dialogo, con buscador de archivo y de carpeta,
  20 iconos, 10 colores, categoria y vista previa en vivo.
- **Cabecera**: buscar, fijar encima, colapsar y opciones (3 a 6 columnas, transparencia, traer al centro).

Con varios monitores, si el widget quedo en una pantalla que ya no miras, vuelve a abrir el acceso
directo: no lanza otra copia, trae la que ya corre a la pantalla donde tengas el raton.

## Donde viven tus accesos

`%APPDATA%\Deckx\widget.json`. Se crea solo la primera vez y sobrevive a cualquier actualizacion
del ejecutable.

El orden de carga es: tu `widget.json`, si no existe un `projects.json` junto al exe, y si tampoco
esta, la semilla que viaja incrustada en el propio ejecutable.

Todo se arma desde la interfaz, pero el archivo es texto plano por si prefieres editarlo a mano:

```json
{
  "Name": "Mi proyecto",
  "Subtitle": "Lo que hace",
  "Path": "C:\\Ruta\\A\\MiSolucion.slnx",
  "Arguments": "",
  "IconGlyph": "\uE943",
  "Color": "#378ADD",
  "Group": "Proyectos",
  "Order": 1
}
```

`Path` admite variables de entorno (`%USERPROFILE%`), rutas de archivo, carpetas y urls.
`IconGlyph` es un glifo de Segoe Fluent Icons, la fuente que ya trae Windows.

## Compilar

Necesitas el [SDK de .NET 10](https://dotnet.microsoft.com/download) y Windows 10 u 11 de 64 bits.

```
git clone https://github.com/hebalmert/Deckx-WPF.git
cd Deckx-WPF
dotnet build Deckx.slnx
```

Para generar el ejecutable de un solo archivo:

```
dotnet publish Deckx/Deckx.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o publish/portable
```

## Como esta hecho

.NET 10 y WPF, sin una sola dependencia externa: ni NuGet, ni red, ni base de datos. Todo ocurre
en tu maquina.

```
Deckx/
  Models/     ShortcutItem, WidgetConfig
  Services/   ConfigService (json), LaunchService (abrir), CatalogService (iconos y colores),
              ScreenService (monitores), InstallService (arranque de Windows)
  Views/      MainWindow (el widget), EditItemWindow (agregar y editar), TextPromptWindow
```

Sin MVVM ni frameworks a proposito: es una utilidad de escritorio, y el code-behind directo con
estilos en `App.xaml` se lee y se corrige en un solo lugar.

## Licencia

MIT. Usalo, copialo y cambialo como quieras.
