using UnityEngine;
using UnityModManagerNet;
namespace CMS2018CompanionBridge
{
    public static class BridgeMenu
    {
        public static void Draw(UnityModManager.ModEntry entry){GUILayout.Label("Workshop Companion · Live Bridge");GUILayout.Label("Read-only updates every second. Does not save or change your game.");GUILayout.Label("Status: "+Entry.Status);GUILayout.Label("Output: "+Entry.Folder);if(Entry.Error!=null)GUILayout.Label(Entry.Error);}
    }
}
