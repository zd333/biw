During development after UI markup changed (before production build):
1. Run powershell 'create UIDs in project.ps1' script to create UIDs for elements in markup.

After production build:
1. Copy main '*.exe' assembly and satellite '*.resources.dll' assembly to this folder.
2. Run powershell 'create localization CSV file.ps1' script to create 'resources.csv' file with string data to translate.
3. Edit 'resources.csv' to translate necessary texts.
4. Run powershell 'create localized resource DLL xx-XX.ps1' script to build localized satellite assembly (it will be placed in folder with culture name).
5. Copy culture folder to main assembly.