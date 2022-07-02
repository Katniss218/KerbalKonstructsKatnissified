using KerbalKonstructs.Modules;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KerbalKonstructs.Core
{

    /// <summary>
    /// Settings read from the Instance Config file
    /// </summary>
    internal class CFGSetting : Attribute
    {

    }

    /// <summary>
    /// Settings written to the Savegame State file
    /// </summary>
    internal class CareerSetting : Attribute
    {

    }

    // We use dictionaries for the lookup of the parameter types, because they are way faster then making reflection lookups.
    // We use reflection calls to scan for the types of StaticModule and StaticObject with the attribute CFGSettings
    // We have for each cfgfile-setting a same named field in the classes, so we don't need a translation table.

    /// <summary>
    /// Uses reflection to fill in the fields of static modules and alike from the parsed config nodes.
    /// </summary>
    internal static class ConfigUtil
    {
        internal static bool initialized = false;

        internal static Dictionary<string, FieldInfo> modelFields = new Dictionary<string, FieldInfo>();
        internal static Dictionary<string, FieldInfo> instanceFields = new Dictionary<string, FieldInfo>();

        internal static Dictionary<string, FieldInfo> mapDecalFields = new Dictionary<string, FieldInfo>();
        internal static Dictionary<string, FieldInfo> mapDecalsMapFields = new Dictionary<string, FieldInfo>();

        internal static Dictionary<string, FieldInfo> groupCenterFields = new Dictionary<string, FieldInfo>();

        internal static HashSet<string> facilityTypes = new HashSet<string>();


        // global stuff
        internal static bool bodiesInitialized = false;
        private static Dictionary<string, CelestialBody> knownBodies = new Dictionary<string, CelestialBody>();

        /// <summary>
        /// Fills up the lookup tables for the parser. 
        /// </summary>
        internal static void InitTypes()
        {
            foreach( FieldInfo field in typeof( StaticModel ).GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) )
            {
                if( Attribute.IsDefined( field, typeof( CFGSetting ) ) )
                {
                    modelFields.Add( field.Name, field );
                    //Log.Normal("Parser Model:" + field.Name + ": " + field.FieldType.ToString());
                }

            }
            foreach( FieldInfo field in typeof( StaticInstance ).GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) )
            {
                if( Attribute.IsDefined( field, typeof( CFGSetting ) ) )
                {
                    instanceFields.Add( field.Name, field );
                    //Log.Normal("Parser Instance: " + field.Name + ": " + field.FieldType.ToString());
                }
            }

            foreach( FieldInfo field in typeof( MapDecalInstance ).GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) )
            {
                if( Attribute.IsDefined( field, typeof( CFGSetting ) ) )
                {
                    mapDecalFields.Add( field.Name, field );
                }
            }


            foreach( FieldInfo field in typeof( MapDecalsMap ).GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) )
            {
                if( Attribute.IsDefined( field, typeof( CFGSetting ) ) )
                {
                    mapDecalsMapFields.Add( field.Name, field );
                }
            }

            foreach( FieldInfo field in typeof( GroupCenter ).GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) )
            {
                if( Attribute.IsDefined( field, typeof( CFGSetting ) ) )
                {
                    groupCenterFields.Add( field.Name, field );
                }
            }

            facilityTypes = new HashSet<string>( Enum.GetNames( typeof( KKFacilityType ) ) );

            initialized = true;
        }

        // 2 methods for writing and 2 methods for reading.
        // They mirror each other, writing into fields and properties.

        // TODO - replace this with an attribute perhaps. Or look at how KSP itself fills in fields in part modules and mirror that.

        /// <summary>
        /// Writes a value from a config node into the target's FIELD.
        /// </summary>
        internal static void ReadCFGNode( object target, FieldInfo targetField, ConfigNode cfgNode )
        {
            if( !cfgNode.HasValue( targetField.Name ) )
            {
                return;
            }

            if( !string.IsNullOrEmpty( cfgNode.GetValue( targetField.Name ) ) )
            {
                switch( targetField.FieldType.ToString() )
                {
                    case "System.String":
                        targetField.SetValue( target, cfgNode.GetValue( targetField.Name ) );
                        break;

                    case "System.Int32":
                        targetField.SetValue( target, int.Parse( cfgNode.GetValue( targetField.Name ) ) );
                        break;

                    case "System.Single":
                        targetField.SetValue( target, float.Parse( cfgNode.GetValue( targetField.Name ) ) );
                        break;

                    case "System.Double":
                        targetField.SetValue( target, double.Parse( cfgNode.GetValue( targetField.Name ) ) );
                        break;

                    case "System.Boolean":
                        bool result;
                        bool.TryParse( cfgNode.GetValue( targetField.Name ), out result );
                        targetField.SetValue( target, result );
                        break;

                    case "UnityEngine.Vector3":
                        targetField.SetValue( target, ConfigNode.ParseVector3( cfgNode.GetValue( targetField.Name ) ) );
                        break;

                    case "UnityEngine.Vector3d":
                        targetField.SetValue( target, ConfigNode.ParseVector3D( cfgNode.GetValue( targetField.Name ) ) );
                        break;

                    case "UnityEngine.Color":
                        targetField.SetValue( target, ConfigNode.ParseColor( cfgNode.GetValue( targetField.Name ) ) );
                        break;

                    case "CelestialBody":
                        targetField.SetValue( target, GetCelestialBody( cfgNode.GetValue( targetField.Name ) ) );
                        break;

                    case "KerbalKonstructs.Core.SiteType":
                        SiteType value = SiteType.Any;
                        try
                        {
                            value = (SiteType)Enum.Parse( typeof( SiteType ), cfgNode.GetValue( targetField.Name ) );

                        }
                        catch
                        {
                            value = SiteType.Any;
                        }
                        targetField.SetValue( target, value );
                        break;

                    case "KerbalKonstructs.Core.LaunchSiteCategory":
                        LaunchSiteCategory category;
                        try
                        {
                            category = (LaunchSiteCategory)Enum.Parse( typeof( LaunchSiteCategory ), cfgNode.GetValue( targetField.Name ) );

                        }
                        catch
                        {
                            category = LaunchSiteCategory.Other;
                        }
                        targetField.SetValue( target, category );
                        break;
                }
            }
        }

        /// <summary>
        /// Writes a value from a config node into the target's PROPERTY.
        /// </summary>
        internal static void ReadCFGNode( object target, PropertyInfo targetProperty, ConfigNode cfgNode )
        {
            if( !cfgNode.HasValue( targetProperty.Name ) )
                return;

            if( !string.IsNullOrEmpty( cfgNode.GetValue( targetProperty.Name ) ) )
            {
                switch( targetProperty.PropertyType.ToString() )
                {
                    case "System.String":
                        targetProperty.SetValue( target, cfgNode.GetValue( targetProperty.Name ), null );
                        break;

                    case "System.Int32":
                        targetProperty.SetValue( target, int.Parse( cfgNode.GetValue( targetProperty.Name ) ), null );
                        break;

                    case "System.Single":
                        targetProperty.SetValue( target, float.Parse( cfgNode.GetValue( targetProperty.Name ) ), null );
                        break;

                    case "System.Double":
                        targetProperty.SetValue( target, double.Parse( cfgNode.GetValue( targetProperty.Name ) ), null );
                        break;

                    case "System.Boolean":
                        bool result;
                        bool.TryParse( cfgNode.GetValue( targetProperty.Name ), out result );
                        targetProperty.SetValue( target, result, null );
                        break;

                    case "UnityEngine.Vector3":
                        targetProperty.SetValue( target, ConfigNode.ParseVector3( cfgNode.GetValue( targetProperty.Name ) ), null );
                        break;

                    case "UnityEngine.Vector3d":
                        targetProperty.SetValue( target, ConfigNode.ParseVector3D( cfgNode.GetValue( targetProperty.Name ) ), null );
                        break;

                    case "CelestialBody":
                        targetProperty.SetValue( target, GetCelestialBody( cfgNode.GetValue( targetProperty.Name ) ), null );
                        break;

                    case "UnityEngine.Color":
                        targetProperty.SetValue( target, ConfigNode.ParseColor( cfgNode.GetValue( targetProperty.Name ) ), null );
                        break;

                    case "KerbalKonstructs.Core.SiteType":
                        SiteType value;
                        try
                        {
                            value = (SiteType)Enum.Parse( typeof( SiteType ), cfgNode.GetValue( targetProperty.Name ) );

                        }
                        catch
                        {
                            value = SiteType.Any;
                        }
                        targetProperty.SetValue( target, value, null );
                        break;

                    case "KerbalKonstructs.Core.LaunchSiteCategory":
                        LaunchSiteCategory category = LaunchSiteCategory.Other;
                        try
                        {
                            category = (LaunchSiteCategory)Enum.Parse( typeof( LaunchSiteCategory ), cfgNode.GetValue( targetProperty.Name ) );

                        }
                        catch
                        {
                            category = LaunchSiteCategory.Other;
                        }
                        targetProperty.SetValue( target, category, null );
                        break;
                }
            }
        }

        /// <summary>
        /// Writes a setting from an object to a confignode
        /// </summary>
        internal static void WriteToConfigNode( object source, FieldInfo field, ConfigNode cfgNode )
        {
            switch( field.FieldType.ToString() )
            {
                case "System.String":
                    cfgNode.SetValue( field.Name, (string)field.GetValue( source ), true );
                    break;
                case "System.Int32":
                    cfgNode.SetValue( field.Name, (int)field.GetValue( source ), true );
                    break;
                case "System.Single":
                    cfgNode.SetValue( field.Name, (float)field.GetValue( source ), true );
                    break;
                case "System.Double":
                    cfgNode.SetValue( field.Name, (double)field.GetValue( source ), true );
                    break;
                case "System.Boolean":
                    cfgNode.SetValue( field.Name, (bool)field.GetValue( source ), true );
                    break;
                case "UnityEngine.Vector3":
                    cfgNode.SetValue( field.Name, (Vector3)field.GetValue( source ), true );
                    break;
                case "UnityEngine.Vector3d":
                    cfgNode.SetValue( field.Name, (Vector3d)field.GetValue( source ), true );
                    break;
                case "UnityEngine.Color":
                    cfgNode.SetValue( field.Name, (Color)field.GetValue( source ), true );
                    break;
                case "CelestialBody":
                    cfgNode.SetValue( field.Name, ((CelestialBody)field.GetValue( source )).name, true );
                    break;
                case "KerbalKonstructs.Core.SiteType":
                    cfgNode.SetValue( field.Name, ((SiteType)field.GetValue( source )).ToString(), true );
                    break;
                case "KerbalKonstructs.Core.LaunchSiteCategory":
                    cfgNode.SetValue( field.Name, ((LaunchSiteCategory)field.GetValue( source )).ToString(), true );
                    break;
            }
        }

        /// <summary>
        /// Writes a property setting from an object to a confignode
        /// </summary>
        internal static void Write2CfgNode( object source, PropertyInfo property, ConfigNode cfgNode )
        {
            switch( property.PropertyType.ToString() )
            {
                case "System.String":
                    cfgNode.SetValue( property.Name, (string)property.GetValue( source, null ), true );
                    break;
                case "System.Int32":
                    cfgNode.SetValue( property.Name, (int)property.GetValue( source, null ), true );
                    break;
                case "System.Single":
                    cfgNode.SetValue( property.Name, (float)property.GetValue( source, null ), true );
                    break;
                case "System.Double":
                    cfgNode.SetValue( property.Name, (double)property.GetValue( source, null ), true );
                    break;
                case "System.Boolean":
                    cfgNode.SetValue( property.Name, (bool)property.GetValue( source, null ), true );
                    break;
                case "UnityEngine.Vector3":
                    cfgNode.SetValue( property.Name, (Vector3)property.GetValue( source, null ), true );
                    break;
                case "UnityEngine.Vector3d":
                    cfgNode.SetValue( property.Name, (Vector3d)property.GetValue( source, null ), true );
                    break;
                case "UnityEngine.Color":
                    cfgNode.SetValue( property.Name, (Color)property.GetValue( source, null ), true );
                    break;
                case "CelestialBody":
                    cfgNode.SetValue( property.Name, ((CelestialBody)property.GetValue( source, null )).name, true );
                    break;
                case "KerbalKonstructs.Core.SiteType":
                    cfgNode.SetValue( property.Name, ((SiteType)property.GetValue( source, null )).ToString(), true );
                    break;
            }
        }


        /// <summary>
        /// Fast convert of a bodyname to a CelestianBody object also Supports "Homeworld" as a key
        /// </summary>
        internal static CelestialBody GetCelestialBody( string name )
        {
            if( !bodiesInitialized )
            {
                CelestialBody[] bodies = FlightGlobals.Bodies.ToArray();
                knownBodies = new Dictionary<string, CelestialBody>();
                foreach( CelestialBody body in bodies )
                {
                    knownBodies.Add( body.name, body );
                    if( body.isHomeWorld )
                    {
                        knownBodies.Add( "HomeWorld", body );
                    }

                }
                bodiesInitialized = true;
            }
            CelestialBody returnValue;

            if( knownBodies.TryGetValue( name, out returnValue ) )
            {
                return returnValue;
            }
            else
            {
                Log.UserError( "Couldn't find body \"" + name + "\"" );
                return null;
            }
        }

        internal static void CreateNewInstanceDirIfNeeded()
        {
            if( !System.IO.Directory.Exists( KSPUtil.ApplicationRootPath + "GameData/" + KerbalKonstructs.newInstancePath ) )
            {
                System.IO.Directory.CreateDirectory( KSPUtil.ApplicationRootPath + "GameData/" + KerbalKonstructs.newInstancePath );
            }
        }
    }
}