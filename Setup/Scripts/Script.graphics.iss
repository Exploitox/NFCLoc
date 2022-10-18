; ///////////////////////////////////////////////////////////
; // Graphical Installer for Inno Setup                    //
; // Version 6.2.1 (Zinka)                                 //
; // Copyright (c) 2011 - 2022 unSigned, s. r. o.          //
; // http://www.graphical-installer.com                    //
; // customers(at)unsigned.sk                              //
; // All Rights Reserved.                                  //
; ///////////////////////////////////////////////////////////
 
; *********************************************
; * This file contains setting for graphical  *
; * interface. Modify them freerly.           *
; ********************************************* 
 
; Script generated with Graphical Installer Wizard.
 
; UI file for Graphical Installer
#define public GraphicalInstallerUI "GraphicalInstallerUI"
 
; Texts colors
#define public TextsColor    "$000000"
#define public HeadersColor  "$000000"
#define public InversedColor "$000000"
 
; Alternative design - Texts colors
;#define public TextsColor    "$000000"
;#define public HeadersColor  "$FFFFFF"
;#define public InversedColor "$000000"

; Buttons colors
#define public ButtonNormalColor   "$FFFFFF"
#define public ButtonFocusedColor  "$FFFFFF"
#define public ButtonPressedColor  "$FFFFFF"
#define public ButtonDisabledColor "$FFFFFF"
 
; Images - all files must be in the same directory as this .iss file!
#define public TopPicture    "Setup Background-top.png"    ; 690x416 px
#define public InnerPicture  "Setup Background-inner.png"  ; 413x237 px
#define public BottomPicture "Setup Background-bottom.png" ; 690x83 px
#define public ButtonPicture "Setup Buttons.png" ; 80x136 px

; Alternative design
;#define public TopPicture    "Setup Background2-top.png"    ; 690x416 px
;#define public InnerPicture  "Setup Background2-inner.png"  ; 413x237 px
;#define public BottomPicture "Setup Background2-bottom.png" ; 690x83 px
;#define public ButtonPicture "Setup Buttons.png" ; 80x136 px

 
; File with core functions and procedures
#include "compiler:Graphical Installer\GraphicalInstaller_functions.iss"
  
[Files]
; Pictures with skin 
Source: {#TopPicture};    Flags: dontcopy;
Source: {#InnerPicture};  Flags: dontcopy;
Source: {#BottomPicture}; Flags: dontcopy;
Source: {#ButtonPicture}; Flags: dontcopy;
; DLLs
Source: compiler:Graphical Installer\botva2.dll;       Flags: dontcopy;
 
; End of file (EOF)
