@echo off
rd build
if exist build rd /s /q build
mklink /j build "D:\Steam Games\steamapps\common\Keep Talking and Nobody Explodes\mods"
rd Assets\TestHarness
if exist Assets\TestHarness rd /s /q Assets\TestHarness
mklink /j Assets\TestHarness "C:\Users\Tom\Data Dump\ktanemodkit-master\ktanemodkit-master\Assets\TestHarness"
rd Assets\Editor\Scripts
if exist Assets\Editor\Scripts rd /s /q Assets\Editor\Scripts
mklink /j Assets\Editor\Scripts "C:\Users\Tom\Data Dump\ktanemodkit-master\ktanemodkit-master\Assets\Editor\Scripts"
rd Assets\Editor\Steamworks.NET
if exist Assets\Editor\Steamworks.NET rd /s /q Assets\Editor\Steamworks.NET
mklink /j Assets\Editor\Steamworks.NET "C:\Users\Tom\Data Dump\ktanemodkit-master\ktanemodkit-master\Assets\Editor\Steamworks.NET"
rd Assets\Plugins
if exist Assets\Plugins rd /s /q Assets\Plugins
mklink /j Assets\Plugins "C:\Users\Tom\Data Dump\ktanemodkit-master\ktanemodkit-master\Assets\Plugins"
rd Assets\Shaders
if exist Assets\Shaders rd /s /q Assets\Shaders
mklink /j Assets\Shaders "C:\Users\Tom\Data Dump\ktanemodkit-master\ktanemodkit-master\Assets\Examples\Assets\Shaders"
rd Assets\KMScripts
if exist Assets\KMScripts rd /s /q Assets\KMScripts
mklink /j Assets\KMScripts "C:\Users\Tom\Data Dump\ktanemodkit-master\ktanemodkit-master\Assets\Scripts"
rd ProjectSettings
if exist ProjectSettings rd /s /q ProjectSettings
mklink /j ProjectSettings "C:\Users\Tom\Data Dump\ktanemodkit-master\ktanemodkit-master\ProjectSettings"
