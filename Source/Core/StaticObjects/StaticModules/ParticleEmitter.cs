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
        public string emitterName = "";

        public string smokeName = "KKPadSmoke1";

        public float emitterScale = 1.0f;

        public void Start()
        {
            KKPadFX2 padfx = gameObject.AddOrGetComponent<KKPadFX2>();
            padfx.Setup( emitterName, gameObject, smokeName, emitterScale );
        }
    }

    public class KKPadFX2 : MonoBehaviour
    {
        internal struct KKEmitter // TODO - this looks very dumb. Replace with a tuple?
        {
            internal List<ParticleSystem> particleSystems;
            internal List<KSPParticleEmitter> kspEmitters;
        }

        [SerializeField]
        protected ParticleSystem[] ps;

        KSPParticleEmitter[] kspPS;

        private static List<string> stockNames = new List<string>() { "PadSmokeLvl2", "PadSmokeLvl3" };

        /// <summary>
        /// Some weird ass static dictionary holding an array of particle systems for each transform name.
        /// </summary>
        internal static Dictionary<string, KKEmitter> particleSystems = new Dictionary<string, KKEmitter>();


        public void Setup( string emitterName, GameObject baseObject, string smokeName, float emitterScale )
        {
            InitializeParticleSystems();

            List<ParticleSystem> unityEmitters = new List<ParticleSystem>();
            List<KSPParticleEmitter> kspEmitters = new List<KSPParticleEmitter>();

            Transform[] smokeTransforms = baseObject.transform.FindAllRecursive( emitterName );

            foreach( Transform smokeTransform in smokeTransforms )
            {
                if( particleSystems.ContainsKey( smokeName ) )
                {
                    if( particleSystems[smokeName].particleSystems != null )
                    {
                        foreach( ParticleSystem particleSystemPrefab in particleSystems[smokeName].particleSystems )
                        {
                            ParticleSystem newPS = Instantiate( particleSystemPrefab, smokeTransform.position, smokeTransform.rotation, smokeTransform );
                            newPS.gameObject.SetActive( true );
                            newPS.transform.localScale = new Vector3( emitterScale, emitterScale, emitterScale );

                            unityEmitters.Add( newPS );
                            FloatingOrigin.RegisterParticleSystem( newPS );
                        }
                    }
                    if( particleSystems[smokeName].kspEmitters != null )
                    {
                        foreach( KSPParticleEmitter kspParticlePrefab in particleSystems[smokeName].kspEmitters )
                        {
                            KSPParticleEmitter newKSPPE = Instantiate( kspParticlePrefab, smokeTransform.position, smokeTransform.rotation, smokeTransform );
                            newKSPPE.gameObject.SetActive( true );
                            newKSPPE.transform.localScale = new Vector3( emitterScale, emitterScale, emitterScale );

                            kspEmitters.Add( newKSPPE );
                        }
                    }
                }
                else
                {
                    Log.UserError( "Cannot find a LaunchPad Smoke with name: " + smokeName );
                }
            }

            // assign the emitters to the underlying component
            ps = unityEmitters.ToArray();
            kspPS = kspEmitters.ToArray();
        }

        public void LateUpdate()
        {
            // TODO - Entire method is probably unnecessary.
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
        }

        // #####################################################
        // Static functions below
        // #####################################################

        internal static bool isInitialized = false;

        /// <summary>
        /// Even though this is static, it's getting called every time an object is instantiated. And then it checks if any object was already instantiated (so it only runs once).
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

        static void GetSquadParticleSystem()
        {
            ParticleSystem pSystem;

            foreach( string psName in stockNames )
            {
                Log.Normal( "searching for: " + psName );
                pSystem = Resources.FindObjectsOfTypeAll<ParticleSystem>().Where( ps => ps.name == psName ).FirstOrDefault();

                if( pSystem != null )
                {
                    Log.Normal( "found: " + psName );

                    KKEmitter emitter = new KKEmitter()
                    {
                        particleSystems = new List<ParticleSystem>()
                        {
                            pSystem
                        }
                    };

                    particleSystems.Add( psName, emitter );
                }
            }
        }

        static void CustomSmoke()
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