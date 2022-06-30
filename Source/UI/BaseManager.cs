﻿using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using KerbalKonstructs.Utilities;
using System;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    class BaseManager : KKWindow
    {
        private static BaseManager _instance = null;
        internal static BaseManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BaseManager();

                }
                return _instance;
            }
        }


        public static Rect BaseManagerRect = new Rect(250, 60, 185, 610);

        public Texture tTitleIcon = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/titlebaricon", false);
        public Texture tSmallClose = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/littleclose", false);
        public Texture tStatusLaunchsite = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/setaslaunchsite", false);
        public Texture tSetLaunchsite = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/setaslaunchsite", false);
        public Texture tOpenedLaunchsite = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/openedlaunchsite", false);
        public Texture tClosedLaunchsite = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/closedlaunchsite", false);
        public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep", false);
        public Texture tMakeFavourite = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/makefavourite", false);
        public Texture tVerticalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/verticalsep", false);
        public Texture tFaveTemp = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/makefavourite", false);
        public Texture tIsFave = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/isFavourite", false);
        public Texture tFoldOut = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldin", false);
        public Texture tFoldIn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);
        public Texture tFolded = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);


        public static KKLaunchSite selectedSite = null;

        GUIStyle Yellowtext;
        GUIStyle TextAreaNoBorder;
        GUIStyle KKWindow;
        GUIStyle BoxNoBorder;
        GUIStyle SmallButton;
        GUIStyle LabelWhite;
        GUIStyle KKWindowTitle;
        GUIStyle LabelInfo;
        GUIStyle DeadButton;
        GUIStyle DeadButtonRed;

        Vector2 descriptionScrollPosition;
        Vector2 logScrollPosition;


        public float rangekm = 0;


        //public Boolean isOpen = false;
        public Boolean isFavourite = false;
        public Boolean displayStats = true;
        public Boolean displayLog = false;
        public Boolean foldedIn = false;
        public Boolean doneFold = false;
        //public Boolean isLaunch = false;

        private bool layoutIsInitialized = false;


        public override void Draw()
        {
            drawBaseManager();
        }

        public void drawBaseManager()
        {

            KKWindow = new GUIStyle(GUI.skin.window);
            KKWindow.padding = new RectOffset(3, 3, 5, 5);

            if (foldedIn)
            {
                if (!doneFold)
                    BaseManagerRect = new Rect(BaseManagerRect.xMin, BaseManagerRect.yMin, BaseManagerRect.width, BaseManagerRect.height - 255);

                doneFold = true;
            }

            if (!foldedIn)
            {
                if (doneFold)
                    BaseManagerRect = new Rect(BaseManagerRect.xMin, BaseManagerRect.yMin, BaseManagerRect.width, BaseManagerRect.height + 255);

                doneFold = false;
            }

            BaseManagerRect = GUI.Window(0xC00B8B7, BaseManagerRect, drawBaseManagerWindow, "", KKWindow);

            if (BaseManagerRect.Contains(Event.current.mousePosition))
            {
                InputLockManager.SetControlLock(ControlTypes.EDITOR_LOCK, "KKEditorLock");
            }
            else
            {
                InputLockManager.RemoveControlLock("KKEditorLock");
            }
        }

        public void drawBaseManagerWindow(int windowID)
        {

            if (!layoutIsInitialized)
            {
                InitializeLayout();
                layoutIsInitialized = true;
            }

            string sButtonName = "";
            sButtonName = selectedSite.LaunchSiteName;
            if (selectedSite.LaunchSiteName == "Runway") sButtonName = "KSC Runway";
            if (selectedSite.LaunchSiteName == "LaunchPad") sButtonName = "KSC LaunchPad";

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                GUILayout.Button("-KK-", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUILayout.Button("Base Manager", DeadButton, GUILayout.Height(21));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (HighLogic.LoadedScene != GameScenes.EDITOR)
                {
                    if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(21)))
                    {
                        InputLockManager.RemoveControlLock("KKEditorLock");
                        selectedSite = null;
                        this.Close();
                        return;
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            if (selectedSite.LaunchSiteName == "Runway")
                GUILayout.Box("KSC Runway", Yellowtext);
            else
                if (selectedSite.LaunchSiteName == "LaunchPad")
                GUILayout.Box("KSC LaunchPad", Yellowtext);
            else
                GUILayout.Box("" + selectedSite.LaunchSiteName, Yellowtext);

            if (!foldedIn)
            {
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(2);
                    GUILayout.Box(tVerticalSep, BoxNoBorder, GUILayout.Width(4), GUILayout.Height(135));
                    GUILayout.FlexibleSpace();
                    GUILayout.Box(selectedSite.logo, BoxNoBorder, GUILayout.Height(135), GUILayout.Width(135));
                    GUILayout.FlexibleSpace();
                    GUILayout.Box(tVerticalSep, BoxNoBorder, GUILayout.Width(4), GUILayout.Height(135));
                    GUILayout.Space(2);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(3);

                descriptionScrollPosition = GUILayout.BeginScrollView(descriptionScrollPosition, GUILayout.Height(120));
                {
                    GUI.enabled = false;
                    GUILayout.Label(selectedSite.LaunchSiteDescription, LabelWhite);
                    GUI.enabled = true;
                }
                GUILayout.EndScrollView();
            }

            GUILayout.Space(1);

            isFavourite = (selectedSite.favouriteSite == "Yes");

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = (!displayStats);
                if (GUILayout.Button("Stats", GUILayout.Height(23)))
                {
                    displayLog = false;
                    displayStats = true;
                }
                GUI.enabled = true;

                GUI.enabled = (!displayLog);
                if (GUILayout.Button("Log", GUILayout.Height(23)))
                {
                    displayLog = true;
                    displayStats = false;
                }
                GUI.enabled = true;

                if (isFavourite)
                    tFaveTemp = tIsFave;
                else
                    tFaveTemp = tMakeFavourite;

                if (GUILayout.Button(tFaveTemp, GUILayout.Height(23), GUILayout.Width(23)))
                {
                    if (isFavourite)
                        selectedSite.favouriteSite = "No";
                    else
                        selectedSite.favouriteSite = "Yes";
                }

                if (foldedIn) tFolded = tFoldOut;
                if (!foldedIn) tFolded = tFoldIn;

                if (GUILayout.Button(tFolded, GUILayout.Height(23), GUILayout.Width(23)))
                {
                    if (foldedIn) foldedIn = false;
                    else
                        foldedIn = true;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);

            if (displayStats)
            {
                GUILayout.Label("Altitude: " + selectedSite.refAlt.ToString("#0.0") + " m", LabelInfo);
                GUILayout.Label("Longitude: " + selectedSite.refLon.ToString("#0.000"), LabelInfo);
                GUILayout.Label("Latitude: " + selectedSite.refLat.ToString("#0.000"), LabelInfo);
                GUILayout.Space(3);
                GUILayout.Label("Max Length: " + ((selectedSite.LaunchSiteLength == 0f) ? "unlimited " : selectedSite.LaunchSiteLength.ToString("#0" + " m")), LabelInfo);
                GUILayout.Label("Max Width: " + ((selectedSite.LaunchSiteWidth == 0f) ? "unlimited " : selectedSite.LaunchSiteWidth.ToString("#0" + " m")), LabelInfo);
                GUILayout.Label("Max Height: " + ((selectedSite.LaunchSiteHeight == 0f) ? "unlimited " : selectedSite.LaunchSiteHeight.ToString("#0" + " m")), LabelInfo);
                GUILayout.Label("Max Mass: " + ((selectedSite.MaxCraftMass == 0f) ? "unlimited " : selectedSite.MaxCraftMass.ToString("#0" + " t")), LabelInfo);
                GUILayout.Label("Max Parts: " + ((selectedSite.MaxCraftParts == 0) ? "unlimited " : selectedSite.MaxCraftParts.ToString("#0")), LabelInfo);

                GUILayout.FlexibleSpace();
            }


            if (displayLog)
            {
                logScrollPosition = GUILayout.BeginScrollView(logScrollPosition, GUILayout.Height(120));
                {
                    Char csep = '|';
                    string[] sLogEntries = selectedSite.MissionLog.Split(csep);
                    foreach (string sEntry in sLogEntries)
                    {
                        GUILayout.Label(sEntry, LabelInfo);
                    }
                }
                GUILayout.EndScrollView();

                GUILayout.FlexibleSpace();
            }

            GUI.enabled = !selectedSite.isOpen;

            if (!KerbalKonstructs.instance.disableRemoteBaseOpening)
            {
                if (GUILayout.Button("Open Base for \n" + selectedSite.OpenCost + " funds", GUILayout.Height(38)))
                {
                    if (CareerUtils.isCareerGame && selectedSite.OpenCost > Funding.Instance.Funds)
                    {
                        MiscUtils.HUDMessage("Insufficient funds to open this base!", 10, 3);
                    }
                    else
                    {
                        LaunchSiteManager.OpenLaunchSite(selectedSite);
                        if (CareerUtils.isCareerGame)
                        {
                            Funding.Instance.AddFunds(-selectedSite.OpenCost, TransactionReasons.Cheating);
                        }
                    }
                }
            }

            GUI.enabled = true;
            GUI.enabled = selectedSite.isOpen;


            if (GUILayout.Button("Close Base for \n" + selectedSite.CloseValue + " funds", GUILayout.Height(38)))
            {
                LaunchSiteManager.CloseLaunchSite(selectedSite);
                if (CareerUtils.isCareerGame)
                {
                    Funding.Instance.AddFunds(selectedSite.CloseValue, TransactionReasons.Cheating);
                }
            }

            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                GUILayout.BeginHorizontal();
                {
                    if (selectedSite.LaunchSiteName == EditorLogic.fetch.launchSiteName)
                    {
                        tStatusLaunchsite = tSetLaunchsite;
                    }
                    else
                        if (selectedSite.isOpen)
                    {
                        tStatusLaunchsite = tOpenedLaunchsite;
                    }
                    else
                    {
                        tStatusLaunchsite = tClosedLaunchsite;
                    }

                    GUILayout.Label(tStatusLaunchsite, GUILayout.Height(32), GUILayout.Width(32));

                    GUI.enabled = (selectedSite.isOpen) && !(selectedSite.LaunchSiteName == EditorLogic.fetch.launchSiteName);
                    if (GUILayout.Button("Set as \nLaunchsite", GUILayout.Height(38)))
                    {
                        LaunchSiteManager.setLaunchSite(selectedSite);
                        string smessage = sButtonName + " has been set as the launchsite";
                        MiscUtils.HUDMessage(smessage, 10, 0);
                    }
                    GUI.enabled = true;

                }
                GUILayout.EndHorizontal();
            }



            if (HighLogic.LoadedScene != GameScenes.EDITOR)
            {
                GUILayout.BeginHorizontal();
                {
                    if (selectedSite.wayPoint == null)
                    {
                        if (GUILayout.Button("Create Waypoint", GUILayout.Height(30)))
                        {
                            MapIconSelector.CreateWPForLaunchSite(selectedSite);
                        }
                    }
                    else
                    {
                        if (FinePrint.WaypointManager.FindWaypoint(selectedSite.wayPoint.navigationId) == null)
                        {
                            selectedSite.wayPoint = null;
                            GUI.enabled = false;
                        }
                        if (GUILayout.Button("Delete Waypoint", GUILayout.Height(30)))
                        {
                            FinePrint.WaypointManager.RemoveWaypoint(selectedSite.wayPoint);
                            selectedSite.wayPoint = null;
                        }
                        GUI.enabled = true;
                    }

                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(3);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
            GUILayout.Space(1);

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }




        private void InitializeLayout()
        {
            DeadButton = new GUIStyle(GUI.skin.button);
            DeadButton.normal.background = null;
            DeadButton.hover.background = null;
            DeadButton.active.background = null;
            DeadButton.focused.background = null;
            DeadButton.normal.textColor = Color.white;
            DeadButton.hover.textColor = Color.white;
            DeadButton.active.textColor = Color.white;
            DeadButton.focused.textColor = Color.white;
            DeadButton.fontSize = 14;
            DeadButton.fontStyle = FontStyle.Bold;

            DeadButtonRed = new GUIStyle(GUI.skin.button);
            DeadButtonRed.normal.background = null;
            DeadButtonRed.hover.background = null;
            DeadButtonRed.active.background = null;
            DeadButtonRed.focused.background = null;
            DeadButtonRed.normal.textColor = Color.red;
            DeadButtonRed.hover.textColor = Color.yellow;
            DeadButtonRed.active.textColor = Color.red;
            DeadButtonRed.focused.textColor = Color.red;
            DeadButtonRed.fontSize = 12;
            DeadButtonRed.fontStyle = FontStyle.Bold;

            Yellowtext = new GUIStyle(GUI.skin.box);
            Yellowtext.normal.textColor = Color.yellow;
            Yellowtext.normal.background = null;

            TextAreaNoBorder = new GUIStyle(GUI.skin.textArea);
            TextAreaNoBorder.normal.background = null;
            TextAreaNoBorder.normal.textColor = Color.white;
            TextAreaNoBorder.fontSize = 12;
            TextAreaNoBorder.padding.left = 1;
            TextAreaNoBorder.padding.right = 1;
            TextAreaNoBorder.padding.top = 4;

            BoxNoBorder = new GUIStyle(GUI.skin.box);
            BoxNoBorder.normal.background = null;
            BoxNoBorder.normal.textColor = Color.white;

            LabelWhite = new GUIStyle(GUI.skin.label);
            LabelWhite.normal.background = null;
            LabelWhite.normal.textColor = Color.white;
            LabelWhite.fontSize = 12;
            LabelWhite.padding.left = 1;
            LabelWhite.padding.right = 1;
            LabelWhite.padding.top = 4;

            LabelInfo = new GUIStyle(GUI.skin.label);
            LabelInfo.normal.background = null;
            LabelInfo.normal.textColor = Color.white;
            LabelInfo.fontSize = 13;
            LabelInfo.fontStyle = FontStyle.Bold;
            LabelInfo.padding.left = 3;
            LabelInfo.padding.top = 0;
            LabelInfo.padding.bottom = 0;

            KKWindowTitle = new GUIStyle(GUI.skin.box);
            KKWindowTitle.normal.background = null;
            KKWindowTitle.normal.textColor = Color.white;
            KKWindowTitle.fontSize = 14;
            KKWindowTitle.fontStyle = FontStyle.Bold;

            SmallButton = new GUIStyle(GUI.skin.button);
            SmallButton.normal.textColor = Color.red;
            SmallButton.hover.textColor = Color.white;
            SmallButton.padding.top = 1;
            SmallButton.padding.left = 1;
            SmallButton.padding.right = 1;
            SmallButton.padding.bottom = 4;
            SmallButton.normal.background = null;
            SmallButton.hover.background = null;
            SmallButton.fontSize = 12;
        }


        public static KKLaunchSite getSelectedSite()
        {
            KKLaunchSite thisSite = selectedSite;
            return thisSite;
        }

        public static void setSelectedSite(KKLaunchSite soSite)
        {
            selectedSite = soSite;
        }
    }
}
