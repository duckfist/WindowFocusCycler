# WindowFocusCycler
Listens to keystrokes while running in the background to automatically bring different windows into focus in sequence.

## Requirements
- .NET Framework 4.6.1. Sorry I'm so lazy, perhaps .NET Core when I have more time

## Usage
- The left panel ("Select Process") will list all running processes that have a Window Title. The number by each is the process ID.
- The right panel ("Process Switch List") will list all processes that you want to cycle focus between when keypress events are detected.
- Click the right arrow button to move a process into the switch list. Click the left arrow to remove it.
- Click the "Add Key" button and then press a keyboard key to add it to the list of keys to listen to.
- Check the "Enable Process Switching" checkbox to start listening. This is the main workflow of the application. When any of your configured keys are pressed, the first window in the Process Switch List will get focus. When a key is pressed again, the next window will focus. When the end of the list is reached, it repeats.

## Other Notes
- Click "Test: Bring Selected Process to Foreground" to... bring the selected process to the foreground. This is useful for testing which process is which when there are multiple processes of the same name. You can use this to help add windows in a sequence of your choosing into the Process Switch List.
- Click "Refresh Processes" to refresh the "Select Process" panel on the left, in case you launch your other applications after launching WindowFocusCycler. *This will also clear the Process Switch List on the right!*
- Click "Reset Keys" to clear out the list of keys to listen to.

## Ideas for future updates
- Allow each key to have its own list of processes to cycle through
- Be able to enable/disable/delete certain keys, instead of just the "Reset Keys" button which clears the entire list
- Figure out how to detect other keystrokes like DEL, INS, HOME, etc.
- Save profile(s)
- Better UX lol

## Changelog
### v0.0.1
- First commit.
