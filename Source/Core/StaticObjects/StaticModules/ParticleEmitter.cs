using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using System.IO;

namespace KerbalKonstructs
{
    public class ParticleEmitter : StaticModule
    {
        public string smokeEmittersNames = "";

        public string smokeName = "KKPadSmoke1";

        private List<string> emitterTransforms = new List<string>();
        private string[] seperators = new string[] { ",", ";" };

        public void Start()
        {
            //Log.Normal("PadSmoke Start");
            var tmpList = smokeEmittersNames.Split( seperators, StringSplitOptions.RemoveEmptyEntries );

            foreach( string value in tmpList )
            {
                emitterTransforms.Add( value.Trim() );
            }

            //Transform receiverTransform = gameObject.transform.FindRecursive(smokeReceiverName);
            // Collider receiverCollider = receiverTransform.gameObject.GetComponent<Collider>();

            KKPadFX2 padfx = gameObject.AddOrGetComponent<KKPadFX2>();
            padfx.Setup( emitterTransforms, gameObject, smokeName );
            //if (receiverCollider != null)
            //{
            //    receiverCollider.tag = "LaunchpadFX";
            //    //Log.Normal("Collider Tag: " + receiverCollider.tag);
            //    receiverCollider.gameObject.layer = 15;
            //}
            //else
            //{
            //    Log.Warning("PadFX: Collider not found " + smokeReceiverName);
            //}
        }
    }

    public class KKPadFX2 : MonoBehaviour
    {
        [SerializeField]
        protected ParticleSystem[] ps;
        [SerializeField]
        protected Material smokeParticleMaterial;
        //[Range( 0, 1 )]
        //[SerializeField]
        //protected float fxScale;

        internal static bool isInitialized = false;

        private static List<string> stockNames = new List<string> { "PadSmokeLvl2", "PadSmokeLvl3" };

        //private FieldInfo totalFXField; // not used anymore since we're not driving the effect via the LaunchPadFX

        internal struct KKEmitter
        {
            internal List<ParticleSystem> particleSystems;
            internal List<KSPParticleEmitter> kspEmitters;
        }

        internal static Dictionary<string, KKEmitter> particleSystems = new Dictionary<string, KKEmitter>();
        KSPParticleEmitter[] kspPS;

        /// <summary>
        /// Attaches the LaunchPadFX module and 
        /// </summary>
        /// <param name="emitterTransformNames"></param>
        /// <param name="baseObject"></param>
        public void Setup( List<string> emitterTransformNames, GameObject baseObject, string smokeName )
        {
            InitializeParticleSystems();
            List<ParticleSystem> unityEmitters = new List<ParticleSystem>();
            List<KSPParticleEmitter> kspEmitters = new List<KSPParticleEmitter>();

            //totalFXField = typeof( LaunchPadFX ).GetField( "totalFX", BindingFlags.NonPublic | BindingFlags.Instance );

            foreach( string emName in emitterTransformNames )
            {
                foreach( Transform emTransform in baseObject.transform.FindAllRecursive( emName ) )
                {
                    if( particleSystems.ContainsKey( smokeName ) )
                    {
                        if( particleSystems[smokeName].particleSystems != null )
                        {
                            foreach( ParticleSystem pSystem in particleSystems[smokeName].particleSystems )
                            {
                                //Log.Normal("adding PSystem: " + pSystem.name);
                                ParticleSystem emPsystem = Instantiate( pSystem, emTransform.position, emTransform.rotation, emTransform );
                                emPsystem.gameObject.SetActive( true );
                                unityEmitters.Add( emPsystem );
                                FloatingOrigin.RegisterParticleSystem( emPsystem );
                            }
                        }
                        if( particleSystems[smokeName].kspEmitters != null )
                        {
                            foreach( KSPParticleEmitter pSystem in particleSystems[smokeName].kspEmitters )
                            {
                                //Log.Normal("adding PSystem: " + pSystem.name);
                                KSPParticleEmitter emPsystem = Instantiate( pSystem, emTransform.position, emTransform.rotation, emTransform );
                                emPsystem.gameObject.SetActive( true );
                                kspEmitters.Add( emPsystem );
                            }
                        }


                    }
                    else
                    {
                        Log.UserError( "Cannot find a LaunchPad Smoke with name: " + smokeName );
                    }
                }
            }
            // assign the emitters to the underlying component
            ps = unityEmitters.ToArray();
            kspPS = kspEmitters.ToArray();
        }


        public void LateUpdate()
        {
            foreach( ParticleSystem pSystem in ps )
            {
                ParticleSystem.MainModule main = pSystem.main;
                ParticleSystem.EmissionModule psEmissionModule = pSystem.emission;
                    if( !pSystem.isPlaying )
                    {
                        pSystem.Play();
                    }
                    if( !psEmissionModule.enabled )
                    {
                        psEmissionModule.enabled = true;
                    }
                    main.startColor = main.startColor.color.SetAlpha( 1 );
            }

            foreach( KSPParticleEmitter kspSystem in kspPS )
            {
                    kspSystem.emit = true;
                    kspSystem.doesAnimateColor = false;
                    kspSystem.color = kspSystem.color.SetAlpha( 1 );
                
            }
            // reset the emissions

            //totalFXField.SetValue( this, 0f );
        }



        //static functions below

        /// <summary>
        /// Load the assets into memory
        /// </summary>
        internal static void InitializeParticleSystems()
        {
            if( !isInitialized )
            {
                GetSquadParticleSystem();
                CustomSmoke();
                isInitialized = true;
            }
        }



        internal static void GetSquadParticleSystem()
        {
            ParticleSystem pSystem;

            foreach( string psName in stockNames )
            {
                Log.Normal( "searching for: " + psName );
                pSystem = Resources.FindObjectsOfTypeAll<ParticleSystem>().Where( ps => ps.name == psName ).FirstOrDefault();

                if( pSystem != null )
                {
                    Log.Normal( "found: " + psName );


                    KKEmitter emitter = new KKEmitter { particleSystems = new List<ParticleSystem> { pSystem } };

                    particleSystems.Add( psName, emitter );
                }
            }
        }


        internal static void CustomSmoke()
        {
            foreach( UrlDir.UrlConfig smokeConfig in GameDatabase.Instance.GetConfigs( "KKLaunchPadSmoke" ) )
            {
                ConfigNode smokeNode = smokeConfig.config;
                string path = Path.GetDirectoryName( Path.GetDirectoryName( smokeConfig.url ) ).Replace( "\\", "/" );

                if( !smokeNode.HasValue( "Name" ) )
                {
                    continue;
                }

                string name = smokeNode.GetValue( "Name" );
                if( particleSystems.ContainsKey( name ) )
                {
                    continue;
                }

                float intensity = 1;
                if( smokeNode.HasValue( "Intensity" ) )
                {
                    intensity = float.Parse( smokeNode.GetValue( "Intensity" ) );
                }

                List<ParticleSystem> psEmitters = new List<ParticleSystem>();
                List<KSPParticleEmitter> kspEmitters = new List<KSPParticleEmitter>();

                foreach( string meshname in smokeNode.GetValues( "EmitterPath" ) )
                {
                    GameObject modelObject = GameDatabase.Instance.GetModelPrefab( path + "/" + meshname );

                    if( modelObject == null )
                    {
                        modelObject = GameDatabase.Instance.GetModelPrefab( meshname );
                    }

                    if( modelObject == null )
                    {
                        Log.UserError( "could not load smoke asset: " + name + " :  " + meshname );
                        Log.UserError( "last error from file: " + smokeConfig.url );
                        continue;
                    }

                    ParticleSystem ps = null;

                    KSPParticleEmitter kspEmitter = modelObject.GetComponent<KSPParticleEmitter>();
                    if( kspEmitter != null )
                    {
                        kspEmitters.Add( kspEmitter );
                    }
                    else
                    {
                        ps = modelObject.GetComponent<ParticleSystem>();
                    }

                    if( ps == null )
                    {
                        continue;
                    }

                    var main = ps.main;

                    main.loop = true;
                    main.playOnAwake = false;
                    if( kspEmitter == null )
                    {
                        psEmitters.Add( ps );
                    }
                }

                KKEmitter kKEmitter = new KKEmitter()
                {
                    kspEmitters = kspEmitters,
                    particleSystems = psEmitters
                };
                particleSystems.Add( name, kKEmitter );

            }
        }
    }
}