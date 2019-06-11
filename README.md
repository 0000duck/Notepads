# Notepads App [Preview v0.4.0.0]

Download Count: [![Github All Releases](https://img.shields.io/github/downloads/jasonstein/Notepads/total.svg)]()


## What is Notepads and why do I care?

I have waited long enough for a modern windows 10 notepad app to come before I have to do it myself. Don’t get me wrong, Notepad++, Atom, VS Code and Sublime are good editors. I have used most of them and I will continue use them in the future. However, they are either too heavy or looks old. I really need a win32 notepad.exe replacement that feels light, simple and clean to help me get things done as quickly as possible and use it as a turnaround text editor to quickly modify config files or write some notes. Most importantly, it has to be fast and beautiful. 

So here comes the “Notepads” (s stands for Sets).

* A modern, stylish text editor that you will love.
* Feels like win32 notepad.exe but looks better.
* Help you get your work done as quickly as possible.

Now, before I publish it and make it open source. Let me give you guys a sneak peak, a preview version of what is coming:

******* This is a preview release of Notepads for testing purposes only. *******

Notes: Please do not share it with others. Design and features are not final, same for the App icon.

## Things are not working or not implemented in this preview version:

* Print, Text Replace.
* Settings and any customization related.
* You won't be able to save files to system folders due to UWP restriction (I will talk more about this later).

## Known issues (v0.3.0.0):

* Only works on Windows 10 1809 and 1903 for now. Please check your Windows 10 version before installation.
* If you drag a file into Notepads, file picker will be shown when saving due to UWP restriction.
* Editor view resizing glitch is caused by system control and won't fix for now.
* Save as window does not show files, only folders because I only added txt and md registration for now. I will add more file types later.

## Author’s Notes:

The preview is intended to collect feedback, report bugs and glitches. As well as helping me come up with a list of items that you want me to put into settings (like what part of the UI you want to customize etc. Let me know if you have any. For more info and issue reporting, please use Github issues.

## Downloads:

Please head over to [release](https://github.com/JasonStein/Notepads/releases) section to download latest build.

## Roadmap:

### Phase 0 (3 weeks - 1 month):

* Have all basic features implemented including find and replace, print and settings.
* Fix active bugs and glitches as much as possible.
* Launch it on Microsoft Store and setup release pipeline on GitHub.
* Create a landing website for Notepads -> www.NotepadsApp.com
* Find or design a good app icon (Simple, modern, stylish and yet easy to recognize) and all file icons that will be supported.

### Phase 1 (2-3 months):

* Make it rock solid and complete. Add most wanted features but keep it simple. 

### Phase 2 (By end of 2019):

* I will visit .Net core 3.0, Windows Apps, Xaml Island to see if I can port it over to get rid of UWP limitations and restrictions.

## Disclaimer:

To be 100% transparent, Notepads is not and will never collect user information in terms of user privacy. I might use analytics tools to collect usage data like how many times it has been downloaded or been used but that’s it. I will not track your IP or listen your typings or read any of your files and send it over to me, or third parties.

On the other hand, you might noticed that I work for Microsoft. However, Notepads is just my personal side project and I do it for fun and for good. I do not work for Windows team, nor do I work for any Microsoft’s UX/App team. I am not expert on creating Windows apps either. I learned how to code UWP as soon as I started this project which is like only 2-3 weeks back (Today’s date: 06/01/2019). So don’t put too much hope on me or treat it as a project sponsored by Microsoft.

## Stay tuned:

* [Reddit Post](https://www.reddit.com/r/Windows10/comments/btx5qs/my_design_implementation_of_modern_fluent_notepad/)
* [Slack Workspace](https://join.slack.com/t/notepadsworkspace/shared_invite/enQtNjU0NTgyNjYxMTU4LTVhZmJjMGMzNDEzY2Q1ZDFjOTgxMjlhZTk3MzNlNWE3NWEyZjUzMjFmZTA0ZTY3YTgzYzg3N2JjNWQxMGUxYzM)
