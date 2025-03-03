﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace EasyMobile.Editor
{
    // Partial editor class for Notification module.
    public partial class EM_SettingsEditor
    {
        const string NotificationModuleLabel = "NOTIFICATIONS";
        const string NotificationModuleIntro = "Notifications module simplifies the implementation of local and remote (push) notifications in your game.";
        const string OneSignalImportInstruction = "OneSignal plugin is required. Please download and import it.";
        const string OneSignalAvailMsg = "OneSignal plugin is imported and ready to use.";
        const string FirebaseImportInstruction = "Firebase plugin is required. Please download and import it.";
        const string FirebaseAvailMsg = "Firebase plugin is imported and ready to use.";
        const string NotificationManualInitInstruction = "You can initialize manually from script by calling Notifications.Init() method.";
        const string NotificationAndroidResourcesIntro = "Use the Android Notification Icon Generator to prepare notification icons. Default small icon should be named " + NotificationContent.DEFAULT_ANDROID_SMALL_ICON +
                                                         ", while default large icon should be " + NotificationContent.DEFAULT_ANDROID_LARGE_ICON + ". You can optionally add" +
                                                         " custom notification sounds to the resulted folder. Finally import the folder so the resources can be used in your app.";
        const string NotificationCategoryGroupIntro = "You can optionally create groups to organize your notification categories. " +
                                                      "On Android these groups are visible in the Settings app.";
        const string NotificationCategoryGroupInvalidWarning = "Groups with empty name or ID will be ignored.";
        const string NotificationCategoryIntro = "Categories (also called channels on Android) provide a unified system to manage and customize notifications. " +
                                                 "All notifications posted to a category share the same customization defined by that category.";
        const string NotificationDefaultCategoryIntro = "Your app must have at least one category. You can modify the default category but not remove it.";
        const string NotificationUserCategoriesIntro = "You can optionally create custom categories to customize various types of notifications in your app.";
        const string NotificationConstantGenerationIntro = "Generate the static class " + EM_Constants.RootNameSpace + "." + EM_Constants.NotificationsConstantsClassName + " that contains the constants of user category IDs " +
                                                           "that can be used when scheduling notifications." +
                                                           " Remember to regenerate if you make changes to these IDs.";

        // URLs.
        const string NotificationAndroidIconGeneratorUrl = "https://romannurik.github.io/AndroidAssetStudio/icons-notification.html";

        // NotificationCategoryGroup fields.
        const string NotificationCategoryGroup_Id = "id";
        const string NotificationCategoryGroup_Name = "name";

        // NotificationCategory fields.
        const string NotificationCategory_Id = "id";
        const string NotificationCategory_GroupId = "groupId";
        const string NotificationCategory_Name = "name";
        const string NotificationCategory_Description = "description";
        const string NotificationCategory_Importance = "importance";
        const string NotificationCategory_EnableBadge = "enableBadge";
        const string NotificationCategory_Lights = "lights";
        const string NotificationCategory_LightColor = "lightColor";
        const string NotificationCategory_Vibration = "vibration";
        const string NotificationCategory_VibrationPattern = "vibrationPattern";
        const string NotificationCategory_LockScreenVisibility = "lockScreenVisibility";
        const string NotificationCategory_Sound = "sound";
        const string NotificationCategory_SoundName = "soundName";
        const string NotificationCategory_ActionButtons = "actionButtons";

        // Notification Category constraints.
        const int NotificationCategory_MaxVibrationPatternLength = 13;
        const int NotificationCategory_MaxCustomButtons = 4;

        // Private variables.
        static bool notificationIsCategoryGroupsFoldout = false;
        static bool notificationIsUserCategoriesFoldout = false;
        static string notificationActionButtonsTooltip = null;
        static string notificationVibrationPatternTooltip = null;
        static string[] notificationCatGroupIDs;
        static string notificationSelectedAndroidResFolder = null;
        static string notificationCreateAndroidResCurrentStep;

        void NotificationModuleGUI()
        {
            EditorGUILayout.BeginVertical(EM_GUIStyleManager.GetCustomStyle("Module Box"));

            EditorGUI.BeginChangeCheck();
            isNotificationModuleEnable.boolValue = EM_EditorGUI.ModuleToggle(isNotificationModuleEnable.boolValue, NotificationModuleLabel);
            if (EditorGUI.EndChangeCheck())
            {
                // Update the main prefab according to the toggle state.
                GameObject prefab = EM_EditorUtil.GetMainPrefab();

                if (!isNotificationModuleEnable.boolValue)
                {
                    EM_PluginManager.DisableNotificationModule(prefab);
                }
                else
                {
                    EM_PluginManager.EnableNotificationModule(prefab);
                }
            }

            // Now draw the GUI.
            if (!isNotificationModuleEnable.boolValue)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(NotificationModuleIntro, MessageType.Info);
            }
            else
            {
                // Initialization setup
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("AUTO-INIT CONFIG", EditorStyles.boldLabel);
                NotificationProperties.autoInit.property.boolValue = EditorGUILayout.Toggle(NotificationProperties.autoInit.content, NotificationProperties.autoInit.property.boolValue);

                // Auto init delay
                EditorGUI.BeginDisabledGroup(!NotificationProperties.autoInit.property.boolValue);
                NotificationProperties.autoInitDelay.property.floatValue = EditorGUILayout.FloatField(NotificationProperties.autoInitDelay.content, NotificationProperties.autoInitDelay.property.floatValue);
                EditorGUI.EndDisabledGroup();

                // Init tip
                if (!NotificationProperties.autoInit.property.boolValue)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox(NotificationManualInitInstruction, MessageType.Info);
                }

                //--------------------------------------------------------------
                // Uncomment to expose the iOSAuthOptions setting.
                //--------------------------------------------------------------
                /*
                // iOS authorization options
                EditorGUILayout.PropertyField(NotificationProperties.iosAuthOptions.property, NotificationProperties.iosAuthOptions.content);
                */

                // Remote notification setup 
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("REMOTE NOTIFICATION SETUP", EditorStyles.boldLabel);

                // Push Notification Service
                EditorGUILayout.PropertyField(NotificationProperties.pushNotificationService.property, NotificationProperties.pushNotificationService.content);
               
                // If using OneSignal...
                if (NotificationProperties.pushNotificationService.property.enumValueIndex == (int)PushNotificationProvider.OneSignal)
                {
                    #if !EM_ONESIGNAL
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox(OneSignalImportInstruction, MessageType.Error);
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Download OneSignal Plugin", GUILayout.Height(EM_GUIStyleManager.buttonHeight)))
                    {
                        EM_ExternalPluginManager.DownloadOneSignalPlugin();
                    }
                    #else
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox(OneSignalAvailMsg, MessageType.Info);
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Download OneSignal Plugin", GUILayout.Height(EM_GUIStyleManager.buttonHeight)))
                    {
                        EM_ExternalPluginManager.DownloadOneSignalPlugin();
                    }

                    // OneSignal setup
                    EditorGUILayout.Space();
                    NotificationProperties.oneSignalAppId.property.stringValue = EditorGUILayout.TextField(NotificationProperties.oneSignalAppId.content, NotificationProperties.oneSignalAppId.property.stringValue);
                    #endif
                }

                if (NotificationProperties.pushNotificationService.property.enumValueIndex == (int)PushNotificationProvider.Firebase)
                {
                    #if !EM_FIR_MESSAGING
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox(FirebaseImportInstruction, MessageType.Error);
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Download Firebase Plugin", GUILayout.Height(EM_GUIStyleManager.buttonHeight)))
                    {
                        EM_ExternalPluginManager.DownloadFirebasePlugin();
                    }
                    #else
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox(FirebaseAvailMsg, MessageType.Info);
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Download Firebase Plugin", GUILayout.Height(EM_GUIStyleManager.buttonHeight)))
                    {
                        EM_ExternalPluginManager.DownloadFirebasePlugin();
                    }

                    // Firebase setup
                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Default Firebase Topics");

                    if (GUILayout.Button("+", GUILayout.MaxWidth(EM_GUIStyleManager.smallButtonWidth)))
                    {
                        var p = NotificationProperties.firebaseTopics.property;
                        p.arraySize++;
                        p.GetArrayElementAtIndex(p.arraySize - 1).stringValue = string.Empty;
                    }

                    if (GUILayout.Button("-", GUILayout.MaxWidth(EM_GUIStyleManager.smallButtonWidth)) && NotificationProperties.firebaseTopics.property.arraySize > 0)
                    {
                        NotificationProperties.firebaseTopics.property.arraySize--;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel++;
                    for (int i = 0; i < NotificationProperties.firebaseTopics.property.arraySize; i++)
                    {
                        EditorGUILayout.PropertyField(NotificationProperties.firebaseTopics.property.GetArrayElementAtIndex(i));
                    }
                    EditorGUI.indentLevel--;
                    #endif
                }

                // Android Resources Setup
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("ANDROID NOTIFICATION RESOURCES", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(NotificationAndroidResourcesIntro, MessageType.None);
                EditorGUILayout.Space();

                if (GUILayout.Button("Open Android Notification Icon Generator", GUILayout.Height(EM_GUIStyleManager.buttonHeight)))
                {
                    Application.OpenURL(NotificationAndroidIconGeneratorUrl);
                }

                if (GUILayout.Button("Import Res Folder", GUILayout.Height(EM_GUIStyleManager.buttonHeight)))
                {
                    EditorApplication.delayCall += ImportAndroidNotificationResFolder;
                }

                // Category groups
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("CATEGORY GROUPS", EditorStyles.boldLabel);
                DrawNotificationCategoryGroupsArray(NotificationProperties.categoryGroups, ref notificationIsCategoryGroupsFoldout);

                // Update the list of category group IDs.
                notificationCatGroupIDs = BuildListOfNotificationCategoryGroupIDs();

                // Categories
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("CATEGORIES", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(NotificationCategoryIntro, MessageType.Info);

                // Draw the default category
                EditorGUILayout.LabelField("Default Category", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(NotificationDefaultCategoryIntro, MessageType.None);
                DrawNotificationCategory(NotificationProperties.defaultCategory.property);

                if (string.IsNullOrEmpty(EM_Settings.Notifications.DefaultCategory.name) ||
                    string.IsNullOrEmpty(EM_Settings.Notifications.DefaultCategory.id))
                {
                    EditorGUILayout.HelpBox("Default category must have non-empty name and ID.", MessageType.Warning);
                }
                else
                {
                    foreach (var category in EM_Settings.Notifications.UserCategories)
                    {
                        if (!string.IsNullOrEmpty(category.id) && category.id.Equals(EM_Settings.Notifications.DefaultCategory.id))
                        {
                            EditorGUILayout.HelpBox("Default category cannot share the same ID " + category.id + " with another user category.", MessageType.Warning);
                            break;
                        }
                    }
                }

                // Draw user categories
                EditorGUILayout.LabelField("User Categories", EditorStyles.boldLabel);
                DrawNotificationCategoriesArray(NotificationProperties.userCategories, ref notificationIsUserCategoriesFoldout);

                // Constant generation.
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("CONSTANTS GENERATION", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(NotificationConstantGenerationIntro, MessageType.None);

                EditorGUILayout.Space();
                if (GUILayout.Button("Generate Constants Class", GUILayout.Height(EM_GUIStyleManager.buttonHeight)))
                {
                    GenerateNotificationConstants();
                }
            }

            EditorGUILayout.EndVertical();
        }

        //----------------------------------------------------------------
        // Draw Category Groups
        //----------------------------------------------------------------

        void DrawNotificationCategoryGroupsArray(EMProperty prop, ref bool isFoldout)
        {
            if (prop.property.arraySize <= 0)
            {
                EditorGUILayout.HelpBox(NotificationCategoryGroupIntro, MessageType.Info);
            }
            else
            { 
                EditorGUILayout.HelpBox(NotificationCategoryGroupInvalidWarning, MessageType.None);
                EditorGUI.indentLevel++;
                isFoldout = EditorGUILayout.Foldout(isFoldout, prop.property.arraySize + " " + prop.content.text);
                EditorGUI.indentLevel--;

                if (isFoldout)
                {
                    // Draw the array of category groups.    
                    DrawArrayProperty(prop.property, DrawNotificationCategoryGroup);

                    // Detect duplicate names.
                    string duplicateId = EM_EditorUtil.FindDuplicateFieldInArrayProperty(prop.property, NotificationCategoryGroup_Id);
                    if (!string.IsNullOrEmpty(duplicateId))
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox("Found duplicate category group ID of \"" + duplicateId + "\".", MessageType.Warning);
                    }
                }
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Add New Category Group", GUILayout.Height(EM_GUIStyleManager.buttonHeight)))
            {
                // Add new category group.
                AddNewNotificationCategoryGroup(prop.property);

                // Open the foldout if it's closed.
                isFoldout = true;
            }
        }

        void DrawNotificationCategoryGroup(SerializedProperty property)
        {
            SerializedProperty name = property.FindPropertyRelative(NotificationCategoryGroup_Name);
            SerializedProperty id = property.FindPropertyRelative(NotificationCategoryGroup_Id);

            EditorGUILayout.BeginVertical(EM_GUIStyleManager.GetCustomStyle("Item Box"));
            EditorGUILayout.LabelField(string.IsNullOrEmpty(name.stringValue) ? "New Category Group" : name.stringValue, EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(name);
            EditorGUILayout.PropertyField(id);
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        void AddNewNotificationCategoryGroup(SerializedProperty arrayProp)
        {
            if (arrayProp.isArray)
            {
                arrayProp.InsertArrayElementAtIndex(arrayProp.arraySize);
                var newItem = arrayProp.GetArrayElementAtIndex(arrayProp.arraySize - 1);

                // Reset the fields of newly added element or it will take the values of the preceding one.
                newItem.FindPropertyRelative(NotificationCategoryGroup_Id).stringValue = string.Empty;
                newItem.FindPropertyRelative(NotificationCategoryGroup_Name).stringValue = string.Empty;
            }
        }

        //----------------------------------------------------------------
        // Draw Categories
        //----------------------------------------------------------------
        void DrawNotificationCategoriesArray(EMProperty prop, ref bool isFoldout)
        {
            if (prop.property.arraySize <= 0)
            {
                EditorGUILayout.HelpBox(NotificationUserCategoriesIntro, MessageType.None);
            }
            else
            { 
                EditorGUI.indentLevel++;
                isFoldout = EditorGUILayout.Foldout(isFoldout, prop.property.arraySize + " " + prop.content.text);
                EditorGUI.indentLevel--;

                if (isFoldout)
                {
                    // Draw the array of category groups.    
                    DrawArrayProperty(prop.property, DrawNotificationCategory);

                    // Detect duplicate category IDs.
                    string duplicateId = EM_EditorUtil.FindDuplicateFieldInArrayProperty(prop.property, NotificationCategory_Id);
                    if (!string.IsNullOrEmpty(duplicateId))
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox("Found duplicate category ID of \"" + duplicateId + "\".", MessageType.Warning);
                    }
                }
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Add New Category", GUILayout.Height(EM_GUIStyleManager.buttonHeight)))
            {
                // Add new category group.
                AddNewNotificationCategory(prop.property);

                // Open the foldout if it's closed.
                isFoldout = true;
            }
        }

        void DrawNotificationCategory(SerializedProperty prop)
        {
            var id = prop.FindPropertyRelative(NotificationCategory_Id);
            var groupId = prop.FindPropertyRelative(NotificationCategory_GroupId);
            var name = prop.FindPropertyRelative(NotificationCategory_Name);
            var description = prop.FindPropertyRelative(NotificationCategory_Description);
            var importance = prop.FindPropertyRelative(NotificationCategory_Importance);
            var enableBadge = prop.FindPropertyRelative(NotificationCategory_EnableBadge);
            var lights = prop.FindPropertyRelative(NotificationCategory_Lights);
            var lightColor = prop.FindPropertyRelative(NotificationCategory_LightColor);
            var vibration = prop.FindPropertyRelative(NotificationCategory_Vibration);
            var vibrationPattern = prop.FindPropertyRelative(NotificationCategory_VibrationPattern);
            var lockScreenVisibility = prop.FindPropertyRelative(NotificationCategory_LockScreenVisibility);
            var sound = prop.FindPropertyRelative(NotificationCategory_Sound);
            var soundName = prop.FindPropertyRelative(NotificationCategory_SoundName);
            var actionButtons = prop.FindPropertyRelative(NotificationCategory_ActionButtons);

            EditorGUILayout.BeginVertical(EM_GUIStyleManager.GetCustomStyle("Item Box"));

            EditorGUILayout.LabelField(string.IsNullOrEmpty(name.stringValue) ? "New Category" : name.stringValue, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(name);
            EditorGUILayout.PropertyField(id);
            EditorGUILayout.PropertyField(description);

            // Dispaly group IDs as a drop down.
            DrawNotificationCategoryGroupId(groupId);

            EditorGUILayout.PropertyField(enableBadge);
            EditorGUILayout.PropertyField(importance);
            EditorGUILayout.PropertyField(lights);

            if (lights.enumValueIndex == (int)NotificationCategory.LightOptions.Custom)
                EditorGUILayout.PropertyField(lightColor);

            EditorGUILayout.PropertyField(vibration);

            if (vibration.enumValueIndex == (int)NotificationCategory.VibrationOptions.Custom)
            {
                DrawNotificationVibrationPattern(vibrationPattern);
            }

            EditorGUILayout.PropertyField(lockScreenVisibility);
            EditorGUILayout.PropertyField(sound);

            if (sound.enumValueIndex == (int)NotificationCategory.SoundOptions.Custom)
            {
                EditorGUILayout.PropertyField(soundName);
            }

            // Action buttons.
            DrawNotificationActionButtonsArray(actionButtons);
            EditorGUILayout.EndVertical();
        }

        void AddNewNotificationCategory(SerializedProperty arrayProp)
        {
            if (arrayProp.isArray)
            {
                arrayProp.InsertArrayElementAtIndex(arrayProp.arraySize);
                var newItem = arrayProp.GetArrayElementAtIndex(arrayProp.arraySize - 1);

                // Reset the fields of newly added element or it will take the values of the preceding one.
                newItem.FindPropertyRelative(NotificationCategory_Id).stringValue = string.Empty;
                newItem.FindPropertyRelative(NotificationCategory_GroupId).stringValue = string.Empty;
                newItem.FindPropertyRelative(NotificationCategory_Name).stringValue = string.Empty;
                newItem.FindPropertyRelative(NotificationCategory_Description).stringValue = string.Empty;
                newItem.FindPropertyRelative(NotificationCategory_Importance).enumValueIndex = (int)NotificationCategory.Importance.Default;
                newItem.FindPropertyRelative(NotificationCategory_EnableBadge).boolValue = true;
                newItem.FindPropertyRelative(NotificationCategory_Lights).enumValueIndex = (int)NotificationCategory.LightOptions.Default;
                newItem.FindPropertyRelative(NotificationCategory_LightColor).colorValue = Color.white;
                newItem.FindPropertyRelative(NotificationCategory_Vibration).enumValueIndex = (int)NotificationCategory.VibrationOptions.Default;
                newItem.FindPropertyRelative(NotificationCategory_VibrationPattern).ClearArray();
                newItem.FindPropertyRelative(NotificationCategory_LockScreenVisibility).enumValueIndex = (int)NotificationCategory.LockScreenVisibilityOptions.Public;
                newItem.FindPropertyRelative(NotificationCategory_Sound).enumValueIndex = (int)NotificationCategory.SoundOptions.Default;
                newItem.FindPropertyRelative(NotificationCategory_SoundName).stringValue = string.Empty;
                newItem.FindPropertyRelative(NotificationCategory_ActionButtons).ClearArray();
            }
        }

        void DrawNotificationCategoryGroupId(SerializedProperty groupId)
        {
            var groupIdLabel = new GUIContent(
                                   groupId.displayName, 
                                   EM_EditorUtil.GetFieldTooltip(typeof(NotificationCategory), NotificationCategory_GroupId)
                               );

            groupId.stringValue = DrawListAsPopup(
                groupIdLabel, 
                notificationCatGroupIDs, 
                groupId.stringValue
            );

            if (groupId.stringValue.Equals(EM_Constants.NoneSymbol))
                groupId.stringValue = string.Empty;
        }

        void DrawNotificationActionButtonsArray(SerializedProperty actionButtons)
        {
            EditorGUILayout.BeginHorizontal();

            if (notificationActionButtonsTooltip == null)
                notificationActionButtonsTooltip = EM_EditorUtil.GetFieldTooltip(typeof(NotificationCategory), NotificationCategory_ActionButtons);

            EditorGUILayout.LabelField(new GUIContent(actionButtons.displayName, notificationActionButtonsTooltip));
            EditorGUI.BeginChangeCheck();
            actionButtons.arraySize = EditorGUILayout.IntField(actionButtons.arraySize, GUILayout.Width(50));
            if (EditorGUI.EndChangeCheck())
            {
                // Won't allow values larger than the maxminum number of buttons allowed.
                actionButtons.arraySize = Mathf.Clamp(actionButtons.arraySize, 0, NotificationCategory_MaxCustomButtons);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            for (int i = 0; i < actionButtons.arraySize; i++)
            {
                EditorGUILayout.PropertyField(actionButtons.GetArrayElementAtIndex(i));
            }
            EditorGUI.indentLevel--;
        }

        void DrawNotificationVibrationPattern(SerializedProperty vibrationPattern)
        {
            EditorGUILayout.BeginHorizontal();

            if (notificationVibrationPatternTooltip == null)
                notificationVibrationPatternTooltip = EM_EditorUtil.GetFieldTooltip(typeof(NotificationCategory), NotificationCategory_VibrationPattern);

            EditorGUILayout.LabelField(new GUIContent(vibrationPattern.displayName, notificationVibrationPatternTooltip));
            EditorGUI.BeginChangeCheck();
            vibrationPattern.arraySize = EditorGUILayout.IntField(vibrationPattern.arraySize, GUILayout.Width(50));
            if (EditorGUI.EndChangeCheck())
            {
                // Put a limit on the array size, who wants long patterns? :)
                vibrationPattern.arraySize = Mathf.Clamp(vibrationPattern.arraySize, 0, NotificationCategory_MaxVibrationPatternLength);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            for (int i = 0; i < vibrationPattern.arraySize; i++)
            {
                EditorGUILayout.PropertyField(vibrationPattern.GetArrayElementAtIndex(i));
            }
            EditorGUI.indentLevel--;
        }

        string[] BuildListOfNotificationCategoryGroupIDs()
        {
            var retList = new List<string>{ EM_Constants.NoneSymbol };
            var property = NotificationProperties.categoryGroups.property;

            if (property != null && property.isArray)
            {
                var list = new List<string>();

                for (int i = 0; i < property.arraySize; i++)
                {
                    var group = property.GetArrayElementAtIndex(i);
                    string id = group.FindPropertyRelative(NotificationCategoryGroup_Id).stringValue;

                    if (!string.IsNullOrEmpty(id))
                        list.Add(id);
                }

                // Attach the [None] value to the top and return the sorted list.
                list.Sort();
                retList.AddRange(list);
            }

            return retList.ToArray();
        }

        // Build a sorted <name, id> dictionary of all category groups with a non-empty name.
        SortedDictionary<string, string> BuildNotificationCategoryGroupsDict()
        {
            var property = NotificationProperties.categoryGroups.property;
            var dict = new SortedDictionary<string, string>();

            for (int i = 0; i < property.arraySize; i++)
            {
                var group = property.GetArrayElementAtIndex(i);
                string name = group.FindPropertyRelative(NotificationCategoryGroup_Name).stringValue;
                string id = group.FindPropertyRelative(NotificationCategoryGroup_Id).stringValue;

                if (!string.IsNullOrEmpty(name) && !dict.ContainsKey(name))
                    dict.Add(name, id);
            }
                
            return dict;
        }

        string[] BuildListOfNotificationCategoryGroupsName(IDictionary<string, string> categoryGroupsDict)
        {
            if (categoryGroupsDict == null)
                return new string[]{ EM_Constants.NoneSymbol };
            
            var list = new string[categoryGroupsDict.Count + 1];

            // Add "None" as first item.
            list[0] = EM_Constants.NoneSymbol;

            // Copy keys from the dict.
            categoryGroupsDict.Keys.CopyTo(list, 1);

            return list;
        }

        string GetNotificationCategoryNameFromId(IDictionary<string, string> dict, string id)
        {
            string name = string.Empty;

            if (string.IsNullOrEmpty(id))
                name = EM_Constants.NoneSymbol;
            else if (dict != null)
                name = EM_EditorUtil.GetKeyForValue(dict, id);

            return name;
        }

        // Generate a static class containing constants of category IDs.
        void GenerateNotificationConstants()
        {           
            // First create a hashtable containing all the names to be stored as constants.
            SerializedProperty userCategoriesProp = NotificationProperties.userCategories.property;

            // First check if there're duplicate IDs.
            string duplicateID = EM_EditorUtil.FindDuplicateFieldInArrayProperty(userCategoriesProp, NotificationCategory_Id);
            if (!string.IsNullOrEmpty(duplicateID))
            {
                EM_EditorUtil.Alert("Error: Duplicate IDs", "Found duplicate category ID of \"" + duplicateID + "\".");
                return;
            }

            // Proceed with adding resource keys.
            Hashtable resourceKeys = new Hashtable();

            // Add the category IDs.
            for (int i = 0; i < userCategoriesProp.arraySize; i++)
            {
                SerializedProperty element = userCategoriesProp.GetArrayElementAtIndex(i);
                string id = element.FindPropertyRelative(NotificationCategory_Id).stringValue;

                // Ignore all items with an empty ID.
                if (!string.IsNullOrEmpty(id))
                {
                    string key = "UserCategory_" + id;
                    resourceKeys.Add(key, id);
                }
            }

            if (resourceKeys.Count > 0)
            {
                // Now build the class.
                EM_EditorUtil.GenerateConstantsClass(
                    EM_Constants.GeneratedFolder,
                    EM_Constants.RootNameSpace + "." + EM_Constants.NotificationsConstantsClassName,
                    resourceKeys,
                    true
                );
            }
            else
            {
                EM_EditorUtil.Alert("Constants Class Generation", "Please fill in required information for all products.");
            }
        }

        //----------------------------------------------------------------
        // Importing Android Notification Res Folder
        //----------------------------------------------------------------
        void ImportAndroidNotificationResFolder()
        {
            string selectedFolder = EditorUtility.OpenFolderPanel(null, notificationSelectedAndroidResFolder, null);

            if (!string.IsNullOrEmpty(selectedFolder))
            {
                // Some folder was selected.
                notificationSelectedAndroidResFolder = selectedFolder;

                // Build Android library from the selected folder.
                if (EM_EditorUtil.DisplayDialog(
                        "Building Android Resources",
                        "Please make sure the selected folder is correct before proceeding.\n" + notificationSelectedAndroidResFolder,
                        "Go Ahead",
                        "Cancel"))
                {
                    // Prepare the lib config.
                    var config = new EM_AndroidLibBuilder.AndroidLibConfig();
                    config.packageName = EM_Constants.AndroidNativeNotificationPackageName;
                    config.targetLibFolderName = EM_Constants.NotificationAndroidResFolderName;
                    config.targetContentFolderName = "res";

                    EM_AndroidLibBuilder.BuildAndroidLibFromFolder(
                        notificationSelectedAndroidResFolder,
                        config,
                        OnAndroidNotificationResBuildProgress,
                        OnAndroidNotificationResBuildNewStep,
                        OnAndroidNotificationResBuildComplete
                    );
                }
            }
        }

        void OnAndroidNotificationResBuildProgress(float progress)
        {
            // Display progress bar.
            EditorUtility.DisplayProgressBar(
                "Generating Android Notification Resources",
                notificationCreateAndroidResCurrentStep,
                progress
            );

        }

        void OnAndroidNotificationResBuildNewStep(string step)
        {
            notificationCreateAndroidResCurrentStep = step;
        }

        void OnAndroidNotificationResBuildComplete()
        {
            EditorUtility.ClearProgressBar();
            EM_EditorUtil.Alert(
                "Android Resources Imported",
                "Android notification resources have been imported successfully!"
            );
        }
    }
}