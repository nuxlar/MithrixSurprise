using BepInEx;
using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace MithrixSurprise
{
    [BepInPlugin("com.zorp.MithrixSurprise", "MithrixSurprise", "1.0.1")]
    
    public class MithrixSurprise : BaseUnityPlugin
    {
        private readonly System.Random rng = new();
        public ConfigEntry<double> probability;
        public static ConfigFile RoRConfig { get; set; }
        public void Awake()
        {
            Log.Init(Logger);
            RoRConfig = new ConfigFile(Paths.ConfigPath + "\\MithrixSurprise.cfg", true);
            probability = RoRConfig.Bind("General", "Probability", 0.005, "Chance that ye ol Mitchell spawns on purchase interaction. Default 0.5%");
            On.RoR2.PurchaseInteraction.OnInteractionBegin += OnInteractionBegin;
        }

        private void OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (self.CanBeAffordedByInteractor(activator))
            {
                if (rng.NextDouble() < probability.Value)
                    SpawnTheBoi(activator.GetComponent<CharacterBody>());
                orig(self, activator);
            }
            else
                orig(self, activator);
        }

        private void SpawnTheBoi(CharacterBody activatorBody)
        {
            SpawnCard mitchell = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/CharacterSpawnCards/cscBrother");

            Transform transform = activatorBody.coreTransform;
            DirectorCore.MonsterSpawnDistance inputDistance = DirectorCore.MonsterSpawnDistance.Far;
            DirectorPlacementRule placementRule = new DirectorPlacementRule()
            {
                spawnOnTarget = transform,
                placementMode = DirectorPlacementRule.PlacementMode.NearestNode
            };
            DirectorCore.GetMonsterSpawnDistance(inputDistance, out placementRule.minDistance, out placementRule.maxDistance);
            DirectorSpawnRequest directorSpawnRequest = new(mitchell, placementRule, RoR2Application.rng);
            directorSpawnRequest.teamIndexOverride = new TeamIndex?(TeamIndex.Monster);
            GameObject spawnedInstance = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
            if ((bool) spawnedInstance)
                NetworkServer.Spawn(spawnedInstance);
        }
    }
}
