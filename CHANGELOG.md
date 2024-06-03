# 1.0.1
- Replaced EditorCoroutine with Tasks (You can remove that dependency package now)
- Hid StreamSDK and UnityCloud credentials
- Removed Upload tab and renamed Sync tab to Upload
- Upload tab now shows `*` on the save button if something has changed.
- Added error text to the `Download and Upload` button to indicate what went wrong
- Manual upload file now goes red when file does not exist
- Changed Expand button to `>` and `\/` instead of `+` `-`
- Added more information to background tasks.
- Fixed bugs so the package works with 2020.x
- Fixed Creating new steam configs not showing in the dropdown
- Fixed Steam depot dropdown not refreshing when adding a new depot
- Fixed manual builds not deleting their cached files in cleanup
- Fixed upload configs not save/loading between sessions
- Fixed manual upload source not saving the file path

# 1.0.0
- Initial version