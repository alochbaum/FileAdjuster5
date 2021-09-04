# FileAdjuster5 Operational Manual for version 5.14.2

## **Overview**

![](RackMultipart20210904-4-1dpjyig_html_f55cfb3d774d682a.png)

File Adjuster 5 processes a source file or groups of files, into smaller output files by maximum number of lines to be contained in the output file or files. While the program copies lines and optional section of line excluding or including actions is used, and each line is filtered to remove Null characters (which are characters added to program logs during errors).

This program saves a history of all the source files and actions used with dates to an embedded database, as well as it has action presets and shortcuts to open output files in Notepad++ or the windows directory containing output files.

Just added in version 5.14: is a routine called &quot;On Air&quot; which runs an action preset on the source file(s), to an intermediate file, then scans rows in the intermediate for output file names between specific characters, which if names are found, those lines with matches are written to sub-files.

## Table of Contents

[FileAdjuster5 Operational Manual for version 5.14.2 1](#_Toc1332977047)

[Overview 1](#_Toc440378928)

[Table of Contents 2](#_Toc466789994)

[Beginning the Program Operations with the Top 3 User Interface Sections 3](#_Toc94108252)

[Loading Source Files 3](#_Toc667233104)

[Setting Output File and Output Options 4](#_Toc1472161003)

[Line Splitting Selectable Sizes and Progress Area 5](#_Toc920427939)

[Line Filter Actions and Status Reporting Window 6](#_Toc1676526108)

[Creation Actions and Adding Line Actions Rows 6](#_Toc1582298620)

[Quickly Creating Actions with &quot;Quick Action Insert&quot; Button 7](#_Toc1454299888)

[Quickly Adding Include or Exclude Rows to an Action List 7](#_Toc1932592269)

[Row Modification 7](#_Toc1677898205)

[Swap Rows in Action Group by Control Clicking Row 7](#_Toc1981269771)

[Writing Additional Rows and Handling Empty Rows 8](#_Toc1213861759)

[Action Presets and Modifying those Presets 8](#_Toc2102599533)

[Loading Presets Filter Actions and/or Modifier Settings 8](#_Toc1424111271)

[Modify Preset or \*On Air\* Settings 8](#_Toc135525276)

[Exporting Presets 9](#_Toc1951891576)

[Deleting Selected Presets 9](#_Toc1310382595)

[Importing Selected Presets 9](#_Toc1983734267)

[\*On Air\* Button Settings 9](#_Toc1312694015)

[Installation procedure 10](#_Toc2140569261)

[Recommendation to Export Presets for Older Versions of the Program 11](#_Toc2084205858)

[Uninstalling Program for Upgrading Users, and Users Wanting To Uninstall FileAdjuster5 11](#_Toc1248771889)

[Installing program 11](#_Toc1351255650)

[Appendix A – List of shortcut keys 13](#_Toc1637457151)

[Appendix B – Release Notes version 5.14.2 13](#_Toc1482467817)

## Beginning the Program Operations with the Top 3 User Interface Sections

### Loading Source Files

I find the easiest way to add source file(s) is to select the files in a windows folder and drag them to the file loading area; if you drop them over the &quot;Clear&quot; button the previous list of files will be cleared then the dragged file(s) are added to the list of files.

![](RackMultipart20210904-4-1dpjyig_html_38762b2f8844a4bf.png)

In addition to dragging files, in the file loading area there is an &quot;Add File&quot; (alt A) button which opens a dialog to select file or files, and the &quot;Clear&quot; button (alt C) which clears out the list.

The embedded database saves a record of all files or file groups which you have processed in the past. The previous files can be loaded with the previous or &quot;\&lt;-&quot; button (alt -), and it can be used many times.

![](RackMultipart20210904-4-1dpjyig_html_51e58308827c1f74.png)

If a more detailed view of saved source files is desired a history or &quot;«-H&quot; (alt H) button opens the File Histories window, which shows the first file and date that each group was saved, you can also set a date to search before, or even delete saved history before a specific date. Simply clicking on a row to select it and the OK button will load all the file(s) in that group to the source section again.

![](RackMultipart20210904-4-1dpjyig_html_1a07b674db786459.png)

### **Setting Output File and Output Options**

When you select some source files, the output file section will attempt to set an output file for you, but remember the main function of the File Adjuster Program is splitting files into smaller files, so this might be just the starting name of a numbered sequence of output files.

![](RackMultipart20210904-4-1dpjyig_html_c5c9ad8fa20c51b0.png)

The &quot;ext:&quot; textbox sets the extension used for the output files, common ones are .txt, .log, and .csv. These extensions are saved in action presets, so you could work on a source file with a few different methods, saving different groups of outputs.

Checking the &quot;Comment&quot; (alt v) checkbox will use the first word of the first row in the action section as the root name of the output file window. Note: if you reprocess without changing the first word of the first row of actions, the old output files will be overwritten.

Checking the &quot;Combine Files&quot; checkbox will create a single group of output file(s) for all the source files. Not checking this will start a new set for each source file.

Checking the &quot;Headers&quot; checkbox will add the name of the source file and a separator line of equal sign characters to the start of each change of source files, this is commonly used with the &quot;Combine Files&quot; feature to identify different origination files for the written lines.

![](RackMultipart20210904-4-1dpjyig_html_c1e65d96acd35aff.png)

The cleanup or &quot;CU&quot; (alt C) button will look at the output file text and delete all files that have the same root name, which could be many numbered files.

The increment or &quot;Inc&quot; (alt n) button is used when a change is made in Action Filters and you would like to save previous output files; it searches the root output name for the next available number in the sequence, if there is no current output file it will not change the output name.

The &quot;Log&quot; button (no quick keyboard shortcut) will open the program&#39;s log file in to Notepad++ for review.

![](RackMultipart20210904-4-1dpjyig_html_a52fe67c5e5b8f36.png)

The output to input or &quot;Out2In&quot; (alt 2) button, takes the listed output file and makes it a new source file, which is often used if a series of action presets are used on sequential files.

Finally, as shown in the image at the beginning of this segment, the start (alt S) Button starts the programs processing and the cancel (alt !) button stops File Adjuster processing before completion but keeps already created output file(s).

### **Line Splitting Selectable Sizes and Progress Area**

![](RackMultipart20210904-4-1dpjyig_html_fce83490c99a6c27.png)

There is a drop-down selector with the maximum number of lines to be written to an output file, before creating a new output file. This selector shows the number of lines and a description, taken from the embedded database&#39;s size table (and database edits can create additional options).

Below the drop-down line selector are 2 progress bars, the bottom bar shows the status indicator for the number of source files being processed (if there is more than one), and the top progress bar is for current output line percentage of maximum lines in drop-down. For Example: if the current file is being split in to 4 output files, then the percent of current output will go from 0 to 100% 3 times and the last will go from 0 to the completed size of the fourth file.

The directory or &quot;dir&quot; (alt d) button opens the current directory of the last processed output file in windows explorer.

The &quot;Notepad++&quot; (alt +) button stays ghosted until the first processing completes, it will then open the named output file in output section in the Notepad++ program in windows if it is installed. Note: if the output created more than 1 file, this button is going to open the file shown in the output window, which is usually only the first file processed.

## Line Filter Actions and Status Reporting Window

File Adjuster 5 supports a list of line filtering actions, which can have only comments for simple file dividing processing, or hundreds of line exclusions strings, inclusion by contained strings, or a range of included times. When using line inclusion matching, additional lines following match line can also be written by a series of checkable options.

<img width="880" alt="Actions" src="https://user-images.githubusercontent.com/13303715/132101483-700a655d-051a-4f9b-ac44-f641eb59d76e.png">

### Creation Actions and Adding Actions Rows

![ActionRows](https://user-images.githubusercontent.com/13303715/132101462-0fd93025-d916-4d6e-a59e-7f4a1bf76298.PNG)

The middle button in the action section &quot;New Action&quot; (alt w) opens action creation dialog, requesting you to create a starting comment row, which aids in historical recalling of the action section and saving the group of actions as a preset.

![](RackMultipart20210904-4-1dpjyig_html_1ab01617f67e1331.png)

The fourth &quot;Add Row &amp;&quot; (alt &amp;) button in the action list section opens the add row dialog window, in which you select an action type. Include and Exclude action types use the Parameter 1 field as a phrase to match in the rows being analyzed, and the Parameter 2 field for comments or advanced options. The Time Window action type uses Parameter 1 as a start time and Parameter 2 as ending time, selecting this type opens a setup dialog, allowing you to enter times and duration.

![](RackMultipart20210904-4-1dpjyig_html_7cc3306be2e5ae1c.png)

### **Quickly Creating Actions with &quot;Quick Action Insert&quot; Button**

There is a shortcut to constructing a new Action List with an Include Action. The first &quot;qAI&quot; (alt q) button uses saved the windows clipboard&#39;s text to create an Action List, the text is used as first line Comment Parameter 1 and the same text is used as the second line&#39;s Include type&#39;s Parameter 1.

In the example image below the phrase on the clipboard was: Error,&quot;Asset is missing! Unable to cache.&quot;, before the alt-q was pressed.

![](RackMultipart20210904-4-1dpjyig_html_7f510a1994885eb8.png)

### **Quickly Adding Include or Exclude Rows to an Action List**

Two buttons, the &quot;Q-I&quot; (alt I) and &quot;Q-X&quot; (alt X) adds Insert type row or an Exclude type row to the bottom of the action list using the text currently on the clipboard.

### **Row Modification**

The &quot;Edit Row&quot; (alt E) button allows you to change Action Type and the 2 Parameters of a row, while the &quot;Delete Row&quot; (alt R) buttons deletes currently selected row.

### Swap Rows in Action Group by Control Clicking Row

This was added in 5.11.3 to match the arranging of preset groups.

![](RackMultipart20210904-4-1dpjyig_html_9740ca90b8b8a0f1.png)

### **Writing Additional Rows and Handling Empty Rows**

![](RackMultipart20210904-4-1dpjyig_html_8279bed5ef56c0ba.png)

If your Action has Include or Any\_Case\_Include type events, then you can have the program write to output file the next 0 to 50 rows, by checking the &quot;lines after include&quot; (alt t) checkbox and adjusting the slider to its right to the number of additional lines.

You can also write rows after a matched include, depending on how they start with &quot;include if starting with Parameter 2&quot; checkbox, &quot;not date&quot; checkbox or &quot;not [--&quot; checkbox.

If you don&#39;t want to write empty rows, just check the &quot;no blank lines (not just includes)&quot; (alt b) checkbox.

## Action Presets and Modifying those Presets

To aid with their organization all action presets are saved in groups, in the order that the preset is added to the group. You don&#39;t have to always save presets all the time because there is also a saved history of all used actions.

### Loading Presets Filter Actions and/or Modifier Settings

When you click the load presets button you can, just load Modifier Settings or both Modifier Settings and Presets at the same time. (Modifiers are all the checkbox settings including the &quot;no blank lines&quot; checkbox.)

In addition to loading presets, when you are the preset loading screen, you can swap the order in which presets are displayed in the future. (Control left clicking on a preset and that preset&#39;s group is swapped with the group above.)

![](RackMultipart20210904-4-1dpjyig_html_2835c1b99ef5ccdd.png)

## Modify Preset or \*On Air\* Settings

Clicking on the &quot;Modify Presets&quot; Button, in the main window, (no keyboard shortcut) brings up the modify presets dialog.

![](RackMultipart20210904-4-1dpjyig_html_edfb050c5e2bade5.png)

### Exporting Presets

The &quot;Export All Presets&quot; button, opens a save dialog to save the presets in a .sqlite file in a windows directory.

### Deleting Selected Presets

One or many presets can be deleted in a new dialog that is opened by clicking on the &quot;Delete Selected Preset(s)&quot; button, except for the &quot;Welcome Message&quot; preset which is always shown at startup. (In this dialog and others used by this program, left clicking selects a row, ctrl-left clicking can add additional rows to the selection.)

Without an &quot;Are You Sure&quot; button, you might want to save an export of all your presets before deleting.

![](RackMultipart20210904-4-1dpjyig_html_2cd492371d6aa4d9.png)

### Importing Selected Presets

The &quot;Import Selected Preset(s)&quot; button opens a standard windows file selection dialog. After selecting a .sqlite file containing the presets, a new dialog opens to allow you to select to preset(s) to import.

Notes:

- The date of the preset will change to the imported date.
- Versions 5.11 and higher exports can be imported.

### **\*On Air\* Button Settings**

The settings for the &quot;\*On Air\*&quot; (alt \*) button are complex, because it uses a specific preset which processes one source file to a single modified intermediate output file, then each row of this intermediate file is scanned for possible phrases, and if those phrases are found output files are written with the title of that output phrase. This powerful sub-splitting of a large file can be useful in files such as a global system log, where you would like rows with computer names in different output files.

The program saves a history of different settings used, only the last one saved is used. The last saved settings are displayed when the change setting is opened, using the top &quot;Previous&quot; and &quot;Next&quot; buttons allows you to step through the history.

Under the history buttons are settings for the name of the Preset Used, the intermediate output file name and maximum line which can be written to that file.

Next in the dialog are the character settings for the row scanning for possible phrases to create the new sub-files. An example image is shown of sub dividing a sample row (shown in purple) with the 3 characters of pipe or &quot;|&quot;, hyphen or &quot;-&quot; and the number one or &quot;1&quot;.

![](RackMultipart20210904-4-1dpjyig_html_a0cb926deeb728e.png)

## Installation procedure

### Recommendation to Export Presets for Older Versions of the Program

Version 5.14 can import presets from full saved database or from partial exports in 5.11-5.13, so if you have the earlier program and created your own presets export them and import to this new version.

### Uninstalling Program for Upgrading Users, and Users Wanting to Uninstall FileAdjuster5

Upgrading users must uninstall old versions of the program, before installing the new.

You must run the standard windows uninstaller, under the control panel, programs and features, and double click on FileAdjuster5. This launches the windows uninstaller for this program, click OK.

![](RackMultipart20210904-4-1dpjyig_html_6c50b8b431daf54f.png)

### Installing program

Unzip the installer to a local drive, right click on &quot;setup.exe&quot; and run as administrator.

![](RackMultipart20210904-4-1dpjyig_html_c5fb7e6f6684aae7.png)

Then click &quot;Install&quot; during the security warning, as shown in the image below.

![](RackMultipart20210904-4-1dpjyig_html_80d66f3e09c0e124.png)

## Appendix A – List of shortcut keys

![](RackMultipart20210904-4-1dpjyig_html_3dcd07f4fb5c9e73.png)

## **Appendix B – Release Notes version 5.14.2**

The open source third party logging library was updated due to security concerns.
