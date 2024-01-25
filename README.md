# ZeroKey

<img align="right" src="https://github.com/Wolkenhof/ZeroKey/blob/master/UI/ZeroKey.UI.View/Icon.png" width="150">

[![License](https://img.shields.io/badge/license-Apache%20License%202.0-purple)](/LICENSE)
[![.NET](https://github.com/Wolkenhof/ZeroKey/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Wolkenhof/ZeroKey/actions/workflows/dotnet.yml)

<p align="center">
   <strong>Status: No longer maintained</strong>
   <br />
   <strong>Version: </strong>2.0.0 (preview)
   <br />
   <a href="https://github.com/Wolkenhof/ZeroKey/issues">Report Bug</a>
   ·
   <a href="https://github.com/Wolkenhof/ZeroKey/blob/main/CHANGELOG.md">View Changelog</a>
  </p>
</p>
</br>

## ‼️ This Project is no longer maintained


## 🔔 About ZeroKey
ZeroKey provides NFC based login and logout functionality for the Microsoft Windows Operating System.  

## ℹ️ Prerequisites
* ~Windows 7~, 8, 8.1 or 10 64-bit Windows (It will not work on a 32-bit system).
* [.NET Framework 4.5, included with Windows 8.1 or higher](https://www.microsoft.com/en-au/download/details.aspx?id=40779)
* [.NET Desktop Runtime 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* [Visual C++ Redistributable Packages for Visual Studio 2015](https://www.microsoft.com/en-au/download/details.aspx?id=48145) and install "vc_redist.x64.exe"

## ℹ️ Installation Instructions

There are three separate parts to this, if any fails then you will need to check that you have the dependencies correctly installed and are running programs in Administrator mode where applicable.  *These installation instructions are temporary and will be replaced with a simple MSI installer in the future.*

### Building the binaries (If you have downloaded the source)

1. Open ``ZeroKey.sln`` with Visual Studio 2022.
2. Build the entire solution (Release or debug, but only x64). This creates a ``\bin\`` folder in the root Project directory.
3. For the rest of the instructions, I'll assume you built for Release. If you chose Debug instead, then replace "Release" in the following instructions with "Debug".
4. Make sure your NFC reader is connected to the PC.

### Installing the Service

1. Browse to ``\bin\Release\Service`` and Right-click "InstallService.bat" and select "Run as Administrator" and accept the UAC prompt. The last line of the command window should say the task completed successfully. 

2. Press any key to exit the command prompt window.


### Registering the credential provider

1. Copy ``\bin\Release\Credential\ZeroKeyCredentialProvider.dll`` to ``C:\Windows\System32``.

1. Run ``\bin\Release\Credential\Register.reg``. You may need to run it as Administrator. Allow the UAC prompt if it pops up. If unsuccessful, run "regedit.exe" as administrator and select "File -> Import" and browse to "Register.reg"


### Registering a token

1. Run ``\bin\Release\UI\ZeroKey.UI.View.exe``.

1. Start by selecting "Add new NFC Card" and follow the wizard steps.

1. Swipe your card when prompted.

1. Enter your password.

1. Swipe your card again to encrypt the password.


## 📖 Credits
- mclear - [Original project "Sesame / NFC Fence"](https://github.com/mclear/Sesame)

---
```Copyright (c) 2022 Wolkenhof GmbH. Author: Jonas G.```
