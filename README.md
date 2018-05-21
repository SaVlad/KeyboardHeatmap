# KeyboardHeatmap
Count keyboard presses and see which keys are used the most.  
Very small:
* Executable size (~66K)
* Recorded data file (<1K)
* CPU and RAM usage (~0%, <10M)  

Single .exe file, but can start with Windows.  
Closing window hides application to tray.  
To close application completely press Quit button in window or in tray Right-click menu.

## Start with Windows
This option works unless you move executable file to another location.  
When you do, enable this option again and registry path will be updated.

## Recorded data
Recorded data stored in %APPDATA%\keyboard_heatmap.data  
File header is 4 bytes: 0x4B484D44 (KHMD)  
The rest is compressed using Deflate.

## Export/Import
Save recorded data to .csv file and use it for all of your Python&etc. needs.  
Unless you horribly break something, import should work fine.  
Columns are:
* **Code** - Internal (Virtual) key code.
* **Name** - Human readable key description.
* **Presses** - Key presses.

## HTML
Export recorded data to good-looking, adaptive HTML page without Javascript.

## Rehook
If application does not count presses or asks to rehook, this button does it.  
It removes active mouse and keyboard hooks and applies it again.  
You can also press **Alt+Pause** to immediately disable hooks, in case you need to do so.

## Known problems
* **No icon**: Could not come up with anything good. Might be added later.
* **Alt** keys count weird: System does not notify hooked applications of Alt key press, so it only works *sometimes*.
* **MMB, XB1, XB2** buttons does not count: Failed to find a way to count these.
* **HTML page gradient in IE** looks weird: Couldn't care less about this one.
