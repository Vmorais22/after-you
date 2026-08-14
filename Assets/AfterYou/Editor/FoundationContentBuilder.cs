using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AfterYou.CameraSystem;
using AfterYou.Characters;
using AfterYou.Core;
using AfterYou.Dialogue;
using AfterYou.Dreams;
using AfterYou.Events;
using AfterYou.Game;
using AfterYou.Narrative;
using AfterYou.Player;
using AfterYou.Portals;
using AfterYou.Quests;
using AfterYou.Relationships;
using AfterYou.Save;
using AfterYou.TimeSystem;
using AfterYou.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace AfterYou.Editor
{
    internal static class FoundationContentBuilder
    {
        private const string Root = "Assets/AfterYou";
        private const string ScenePath = Root + "/Scenes/MainStreet.unity";
        private const string SystemsPrefabPath = Root + "/Prefabs/Systems/GameSystems.prefab";
        private const string PlayerPrefabPath = Root + "/Prefabs/Characters/Player.prefab";
        private const string NpcPrefabPath = Root + "/Prefabs/Characters/NPC.prefab";
        private const string PortalPrefabPath = Root + "/Prefabs/World/Portal.prefab";

        [InitializeOnLoadMethod]
        private static void ScheduleInitialBuild()
        {
            EditorApplication.delayCall += BuildIfMissing;
        }

        [MenuItem("Tools/After You/Rebuild Foundation Content")]
        private static void Rebuild()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Build(true);
            }
        }

        private static void BuildIfMissing()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += BuildIfMissing;
                return;
            }

            var complete =
                AssetDatabase.LoadAssetAtPath<GameObject>(SystemsPrefabPath) != null &&
                AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath) != null &&
                AssetDatabase.LoadAssetAtPath<GameObject>(NpcPrefabPath) != null &&
                AssetDatabase.LoadAssetAtPath<GameObject>(PortalPrefabPath) != null &&
                AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null;

            if (!complete)
            {
                if (HasDirtyLoadedScene())
                {
                    Debug.LogWarning(
                        "After You foundation generation is waiting because a loaded scene has unsaved changes. " +
                        "Save it, then use Tools > After You > Rebuild Foundation Content.");
                    return;
                }

                Build(false);
            }
        }

        private static bool HasDirtyLoadedScene()
        {
            for (var index = 0; index < SceneManager.sceneCount; index++)
            {
                if (SceneManager.GetSceneAt(index).isDirty)
                {
                    return true;
                }
            }

            return false;
        }

        private static void Build(bool overwrite)
        {
            EnsureFolders();

            var assets = CreateDataAssets(overwrite);
            var sprites = CreatePlaceholderSprites(overwrite);
            CreatePrefabs(assets, sprites, overwrite);
            CreateMainStreet(assets, sprites);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("After You foundation content is ready.");
        }

        private static FoundationAssets CreateDataAssets(bool overwrite)
        {
            var result = new FoundationAssets
            {
                TimeConfiguration = GetOrCreate<TimeConfigSO>(
                    Root + "/Data/Configuration/TimeConfiguration.asset",
                    overwrite),
                TimeChanged = GetOrCreate<TimeChangedChannelSO>(
                    Root + "/Data/Events/TimeChanged.asset",
                    overwrite),
                DayEnded = GetOrCreate<DayEndedChannelSO>(
                    Root + "/Data/Events/DayEnded.asset",
                    overwrite),
                NarrativeCompleted = GetOrCreate<NarrativeEventChannelSO>(
                    Root + "/Data/Events/NarrativeEventCompleted.asset",
                    overwrite),
                DialogueStarted = GetOrCreate<StringEventChannelSO>(
                    Root + "/Data/Events/DialogueStarted.asset",
                    overwrite),
                DialogueEnded = GetOrCreate<StringEventChannelSO>(
                    Root + "/Data/Events/DialogueEnded.asset",
                    overwrite)
            };

            SetBackingField(result.TimeConfiguration, "TotalDays", 7);
            SetBackingField(result.TimeConfiguration, "StartingSegment", DaySegment.Morning);
            SetBackingField(result.TimeConfiguration, "MorningHour", 8);
            SetBackingField(result.TimeConfiguration, "AfternoonHour", 14);
            SetBackingField(result.TimeConfiguration, "NightHour", 20);

            result.PortalDefinitions = new List<PortalDefinitionSO>();
            for (var index = 1; index <= 9; index++)
            {
                var portal = GetOrCreate<PortalDefinitionSO>(
                    $"{Root}/Data/Portals/Building{index:00}.asset",
                    overwrite);
                SetBackingField(portal, "Id", $"building-{index:00}");
                SetBackingField(portal, "DisplayName", $"Building {index:00}");
                SetBackingField(portal, "RequiresInteraction", false);
                SetBackingField(portal, "ArrivalOffset", new Vector2(0f, -1.15f));
                result.PortalDefinitions.Add(portal);
            }

            result.Characters = new List<CharacterDefinitionSO>();
            var characterNames = new[] { "Alex", "Mara", "Noah" };
            for (var index = 0; index < characterNames.Length; index++)
            {
                var routine = GetOrCreate<RoutineScheduleSO>(
                    $"{Root}/Data/Characters/{characterNames[index]}Routine.asset",
                    overwrite);
                SetPrivateField(routine, "slots", CreateRoutine(index));

                var character = GetOrCreate<CharacterDefinitionSO>(
                    $"{Root}/Data/Characters/{characterNames[index]}.asset",
                    overwrite);
                SetBackingField(character, "Id", characterNames[index].ToLowerInvariant());
                SetBackingField(character, "DisplayName", characterNames[index]);
                SetBackingField(character, "Description", "Placeholder character data.");
                SetBackingField(character, "Routine", routine);
                result.Characters.Add(character);
            }

            var dialogue = GetOrCreate<DialogueDefinitionSO>(
                Root + "/Data/Dialogue/FoundationExample.asset",
                overwrite);
            SetBackingField(dialogue, "Id", "foundation-example");
            SetBackingField(dialogue, "Lines", new List<DialogueLine>
            {
                new() { SpeakerId = "mara", Text = "Placeholder dialogue.", EmotionId = "neutral" }
            });

            var narrativeEvent = GetOrCreate<NarrativeEventSO>(
                Root + "/Data/Narrative/FoundationExample.asset",
                overwrite);
            SetBackingField(narrativeEvent, "Id", "foundation-example");
            SetBackingField(narrativeEvent, "DesignerNotes", "Architecture example only; not final story.");
            SetBackingField(narrativeEvent, "Dialogue", dialogue);
            SetBackingField(narrativeEvent, "Requirements", new List<NarrativeRequirement>
            {
                new() { Kind = RequirementKind.DayAtLeast, IntValue = 1 }
            });
            SetBackingField(narrativeEvent, "Consequences", new List<NarrativeConsequence>
            {
                new() { Kind = ConsequenceKind.SetFlag, Key = "foundation_event_seen", BoolValue = true }
            });

            MarkDirty(result);
            EditorUtility.SetDirty(dialogue);
            EditorUtility.SetDirty(narrativeEvent);
            return result;
        }

        private static List<RoutineSlot> CreateRoutine(int characterIndex)
        {
            var slots = new List<RoutineSlot>();
            for (var day = 1; day <= 7; day++)
            {
                slots.Add(new RoutineSlot
                {
                    Day = day,
                    Segment = DaySegment.Morning,
                    LocationId = $"street-anchor-{characterIndex + 1:00}",
                    ActivityId = "morning_walk"
                });
                slots.Add(new RoutineSlot
                {
                    Day = day,
                    Segment = DaySegment.Afternoon,
                    LocationId = $"building-{characterIndex + 2:00}-interior",
                    ActivityId = "afternoon_routine"
                });
                slots.Add(new RoutineSlot
                {
                    Day = day,
                    Segment = DaySegment.Night,
                    LocationId = $"street-anchor-{characterIndex + 4:00}",
                    ActivityId = "evening_routine"
                });
            }

            return slots;
        }

        private static PlaceholderSprites CreatePlaceholderSprites(bool overwrite)
        {
            return new PlaceholderSprites
            {
                White = GetOrCreateSprite(Root + "/Art/Placeholders/WhiteSquare.asset", Color.white, overwrite),
                Player = GetOrCreateSprite(
                    Root + "/Art/Placeholders/PlayerBlue.asset",
                    new Color(0.1f, 0.45f, 1f),
                    overwrite),
                Npc = GetOrCreateSprite(
                    Root + "/Art/Placeholders/NpcYellow.asset",
                    new Color(1f, 0.82f, 0.08f),
                    overwrite),
                Portal = GetOrCreateSprite(
                    Root + "/Art/Placeholders/PortalCyan.asset",
                    new Color(0.2f, 0.95f, 1f, 0.55f),
                    overwrite)
            };
        }

        private static void CreatePrefabs(
            FoundationAssets assets,
            PlaceholderSprites sprites,
            bool overwrite)
        {
            if (overwrite || AssetDatabase.LoadAssetAtPath<GameObject>(SystemsPrefabPath) == null)
            {
                var root = new GameObject("GameSystems");
                root.AddComponent<GameBootstrapper>();
                root.AddComponent<GameManager>();
                Configure(root.AddComponent<TimeManager>(),
                    ("configuration", assets.TimeConfiguration),
                    ("timeChanged", assets.TimeChanged),
                    ("dayEnded", assets.DayEnded));
                root.AddComponent<StoryManager>();
                Configure(root.AddComponent<CharacterManager>(), ("timeChanged", assets.TimeChanged));
                Configure(root.AddComponent<DialogueManager>(),
                    ("dialogueStarted", assets.DialogueStarted),
                    ("dialogueEnded", assets.DialogueEnded));
                root.AddComponent<QuestManager>();
                root.AddComponent<RelationshipManager>();
                root.AddComponent<PortalManager>();
                Configure(root.AddComponent<EventManager>(), ("eventCompleted", assets.NarrativeCompleted));
                Configure(root.AddComponent<DreamManager>(), ("dayEnded", assets.DayEnded));
                Configure(root.AddComponent<SaveManager>(), ("dayEnded", assets.DayEnded));
                SavePrefab(root, SystemsPrefabPath);
            }

            if (overwrite || AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath) == null)
            {
                var player = CreateVisual("Player", sprites.Player, "Characters", new Vector2(1f, 1f));
                player.tag = "Player";
                player.layer = LayerMask.NameToLayer("Player");
                var body = player.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                body.freezeRotation = true;
                player.AddComponent<BoxCollider2D>();
                player.AddComponent<PlayerController>();
                player.AddComponent<PortalTraveller>();
                SavePrefab(player, PlayerPrefabPath);
            }

            if (overwrite || AssetDatabase.LoadAssetAtPath<GameObject>(NpcPrefabPath) == null)
            {
                var npc = CreateVisual("NPC", sprites.Npc, "Characters", new Vector2(1f, 1f));
                npc.tag = "NPC";
                npc.layer = LayerMask.NameToLayer("NPC");
                npc.AddComponent<BoxCollider2D>();
                npc.AddComponent<NpcController>();
                SavePrefab(npc, NpcPrefabPath);
            }

            if (overwrite || AssetDatabase.LoadAssetAtPath<GameObject>(PortalPrefabPath) == null)
            {
                var portal = CreateVisual("Portal", sprites.Portal, "Effects", new Vector2(1.3f, 2.2f));
                portal.layer = LayerMask.NameToLayer("Portal");
                var collider = portal.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                portal.AddComponent<PortalEndpoint>();
                SavePrefab(portal, PortalPrefabPath);
            }
        }

        private static void CreateMainStreet(FoundationAssets assets, PlaceholderSprites sprites)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainStreet";

            var sceneRoot = new GameObject("MainStreet");
            sceneRoot.AddComponent<MainStreetMarker>();

            var systemsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SystemsPrefabPath);
            PrefabUtility.InstantiatePrefab(systemsPrefab, scene);

            CreateTilemapRoots(sceneRoot.transform);
            CreateStreetGeometry(sceneRoot.transform, sprites.White);

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            player.transform.position = new Vector3(-45f, 0f, 0f);

            CreateCamera(sceneRoot.transform, player.transform);
            CreateBuildingsAndPortals(sceneRoot.transform, assets, sprites);
            CreateNpcs(scene, assets.Characters);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }

        private static void CreateTilemapRoots(Transform parent)
        {
            var grid = new GameObject("Tilemaps", typeof(Grid));
            grid.transform.SetParent(parent);

            CreateTilemap("StreetTilemap", grid.transform, "Environment");
            CreateTilemap("InteriorTilemap", grid.transform, "Environment");
            CreateTilemap("ForegroundTilemap", grid.transform, "Foreground");
        }

        private static void CreateTilemap(string name, Transform parent, string sortingLayer)
        {
            var tilemapObject = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
            tilemapObject.transform.SetParent(parent);
            tilemapObject.GetComponent<TilemapRenderer>().sortingLayerName = sortingLayer;
        }

        private static void CreateStreetGeometry(Transform parent, Sprite sprite)
        {
            var background = CreateVisual(
                "Sky Placeholder",
                sprite,
                "Background",
                new Vector2(100f, 30f),
                new Color(0.16f, 0.2f, 0.32f));
            background.transform.SetParent(parent);
            background.transform.position = new Vector3(0f, 8f, 5f);

            var street = CreateVisual(
                "Street",
                sprite,
                "Environment",
                new Vector2(100f, 3f),
                new Color(0.18f, 0.18f, 0.22f));
            street.transform.SetParent(parent);
            street.transform.position = new Vector3(0f, -1.5f, 0f);
            street.layer = LayerMask.NameToLayer("Environment");
            street.AddComponent<BoxCollider2D>();
        }

        private static void CreateBuildingsAndPortals(
            Transform parent,
            FoundationAssets assets,
            PlaceholderSprites sprites)
        {
            var facadeColors = new[]
            {
                new Color(0.55f, 0.24f, 0.25f),
                new Color(0.24f, 0.42f, 0.57f),
                new Color(0.58f, 0.46f, 0.22f)
            };

            for (var index = 0; index < 9; index++)
            {
                var number = index + 1;
                var x = -40f + index * 10f;
                var facade = CreateVisual(
                    $"Building {number:00}",
                    sprites.White,
                    "Environment",
                    new Vector2(8f, 8f),
                    facadeColors[index % facadeColors.Length]);
                facade.transform.SetParent(parent);
                facade.transform.position = new Vector3(x, 3.1f, 0f);

                CreateWindows(facade.transform, sprites.White);
                CreateAnchor(
                    $"street-anchor-{number:00}",
                    new Vector3(x + 2.5f, 0f, 0f),
                    parent);

                var interior = CreateVisual(
                    $"Building {number:00} Interior",
                    sprites.White,
                    "Environment",
                    new Vector2(8f, 6f),
                    facadeColors[index % facadeColors.Length] * 0.65f);
                interior.transform.SetParent(parent);
                interior.transform.position = new Vector3(x, -18f, 0f);
                interior.layer = LayerMask.NameToLayer("Interior");

                CreateAnchor(
                    $"building-{number:00}-interior",
                    new Vector3(x + 2f, -20f, 0f),
                    parent);

                CreatePortalPair(
                    number,
                    x,
                    assets.PortalDefinitions[index],
                    parent);
            }
        }

        private static void CreateWindows(Transform facade, Sprite sprite)
        {
            for (var row = 0; row < 2; row++)
            {
                for (var column = 0; column < 3; column++)
                {
                    var window = CreateVisual(
                        $"Window {row + 1}-{column + 1}",
                        sprite,
                        "Foreground",
                        new Vector2(0.8f, 1.1f),
                        new Color(0.95f, 0.78f, 0.35f));
                    window.transform.SetParent(facade);
                    window.transform.localPosition = new Vector3(-2.2f + column * 2.2f, 0.8f + row * 2.2f, -0.1f);
                }
            }
        }

        private static void CreatePortalPair(
            int number,
            float x,
            PortalDefinitionSO definition,
            Transform parent)
        {
            var portalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PortalPrefabPath);

            var exterior = (GameObject)PrefabUtility.InstantiatePrefab(portalPrefab);
            exterior.name = $"Building {number:00} Exterior Portal";
            exterior.transform.SetParent(parent);
            exterior.transform.position = new Vector3(x, 0.05f, -0.2f);
            Configure(exterior.GetComponent<PortalEndpoint>(),
                ("<Definition>k__BackingField", definition),
                ("<EndpointId>k__BackingField", $"building-{number:00}-exterior"),
                ("<DestinationEndpointId>k__BackingField", $"building-{number:00}-interior"));

            var interior = (GameObject)PrefabUtility.InstantiatePrefab(portalPrefab);
            interior.name = $"Building {number:00} Interior Portal";
            interior.transform.SetParent(parent);
            interior.transform.position = new Vector3(x, -20.2f, -0.2f);
            Configure(interior.GetComponent<PortalEndpoint>(),
                ("<Definition>k__BackingField", definition),
                ("<EndpointId>k__BackingField", $"building-{number:00}-interior"),
                ("<DestinationEndpointId>k__BackingField", $"building-{number:00}-exterior"));
        }

        private static void CreateNpcs(Scene scene, IReadOnlyList<CharacterDefinitionSO> characters)
        {
            var npcPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NpcPrefabPath);
            for (var index = 0; index < characters.Count; index++)
            {
                var npc = (GameObject)PrefabUtility.InstantiatePrefab(npcPrefab, scene);
                npc.name = characters[index].DisplayName;
                npc.transform.position = new Vector3(-24f + index * 12f, 0f, 0f);
                Configure(
                    npc.GetComponent<NpcController>(),
                    ("<Definition>k__BackingField", characters[index]));
            }
        }

        private static void CreateCamera(Transform parent, Transform target)
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(parent);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(target.position.x, 2f, -10f);

            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.625f;
            camera.backgroundColor = new Color(0.08f, 0.1f, 0.16f);

            AddComponentIfAvailable(cameraObject, "UnityEngine.U2D.PixelPerfectCamera");
            var brain = AddComponentIfAvailable(cameraObject, "Unity.Cinemachine.CinemachineBrain");

            var virtualCamera = new GameObject("MainStreet Cinemachine Camera");
            virtualCamera.transform.SetParent(parent);
            virtualCamera.transform.position = cameraObject.transform.position;
            var component = AddComponentIfAvailable(
                virtualCamera,
                "Unity.Cinemachine.CinemachineCamera");
            var positionComposer = AddComponentIfAvailable(
                virtualCamera,
                "Unity.Cinemachine.CinemachinePositionComposer");

            if (brain == null ||
                component == null ||
                positionComposer == null ||
                !TryAssignTrackingTarget(component, target))
            {
                if (brain is Behaviour brainBehaviour)
                {
                    brainBehaviour.enabled = false;
                }

                if (component is Behaviour cameraBehaviour)
                {
                    cameraBehaviour.enabled = false;
                }

                var follow = cameraObject.AddComponent<CameraFollow2D>();
                follow.SetTarget(target);
            }
        }

        private static Component AddComponentIfAvailable(GameObject target, string fullTypeName)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(fullTypeName))
                .FirstOrDefault(candidate => candidate != null);
            return type != null && typeof(Component).IsAssignableFrom(type)
                ? target.AddComponent(type)
                : null;
        }

        private static bool TryAssignTrackingTarget(Component component, Transform target)
        {
            var serializedObject = new SerializedObject(component);
            var iterator = serializedObject.GetIterator();

            while (iterator.Next(true))
            {
                if (iterator.propertyType == SerializedPropertyType.ObjectReference &&
                    (iterator.name.Contains("TrackingTarget") ||
                     iterator.name.Contains("Follow")))
                {
                    iterator.objectReferenceValue = target;
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    return true;
                }
            }

            return false;
        }

        private static void CreateAnchor(string id, Vector3 position, Transform parent)
        {
            var anchor = new GameObject($"Anchor {id}");
            anchor.transform.SetParent(parent);
            anchor.transform.position = position;
            Configure(anchor.AddComponent<LocationAnchor>(), ("<Id>k__BackingField", id));
        }

        private static GameObject CreateVisual(
            string name,
            Sprite sprite,
            string sortingLayer,
            Vector2 scale,
            Color? color = null)
        {
            var gameObject = new GameObject(name);
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color ?? Color.white;
            renderer.sortingLayerName = sortingLayer;
            gameObject.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            return gameObject;
        }

        private static void SavePrefab(GameObject instance, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
        }

        private static T GetOrCreate<T>(string path, bool overwrite) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null && !overwrite)
            {
                return existing;
            }

            if (existing != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = System.IO.Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static Sprite GetOrCreateSprite(string path, Color color, bool overwrite)
        {
            var existing = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
            if (existing != null && !overwrite)
            {
                return existing;
            }

            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false)
            {
                name = System.IO.Path.GetFileNameWithoutExtension(path),
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = Enumerable.Repeat(color, 16 * 16).ToArray();
            texture.SetPixels(pixels);
            texture.Apply();
            AssetDatabase.CreateAsset(texture, path);

            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 16f, 16f),
                new Vector2(0.5f, 0.5f),
                16f);
            sprite.name = texture.name + " Sprite";
            AssetDatabase.AddObjectToAsset(sprite, texture);
            EditorUtility.SetDirty(texture);
            return sprite;
        }

        private static void Configure(Object target, params (string field, Object value)[] values)
        {
            var serializedObject = new SerializedObject(target);
            foreach (var (field, value) in values)
            {
                var property = serializedObject.FindProperty(field);
                if (property != null)
                {
                    property.objectReferenceValue = value;
                }
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Configure(Object target, params (string field, string value)[] values)
        {
            var serializedObject = new SerializedObject(target);
            foreach (var (field, value) in values)
            {
                var property = serializedObject.FindProperty(field);
                if (property != null)
                {
                    property.stringValue = value;
                }
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Configure(Object target, params (string field, object value)[] values)
        {
            var serializedObject = new SerializedObject(target);
            foreach (var (field, value) in values)
            {
                var property = serializedObject.FindProperty(field);
                if (property == null)
                {
                    continue;
                }

                if (value is Object unityObject)
                {
                    property.objectReferenceValue = unityObject;
                }
                else if (value is string text)
                {
                    property.stringValue = text;
                }
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetBackingField<T>(Object target, string propertyName, T value)
        {
            SetPrivateField(target, $"<{propertyName}>k__BackingField", value);
        }

        private static void SetPrivateField<T>(Object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(target.GetType().Name, fieldName);
            }

            field.SetValue(target, value);
            EditorUtility.SetDirty(target);
        }

        private static void MarkDirty(FoundationAssets assets)
        {
            EditorUtility.SetDirty(assets.TimeConfiguration);
            EditorUtility.SetDirty(assets.TimeChanged);
            EditorUtility.SetDirty(assets.DayEnded);
            EditorUtility.SetDirty(assets.NarrativeCompleted);
            EditorUtility.SetDirty(assets.DialogueStarted);
            EditorUtility.SetDirty(assets.DialogueEnded);
            foreach (var portal in assets.PortalDefinitions)
            {
                EditorUtility.SetDirty(portal);
            }

            foreach (var character in assets.Characters)
            {
                EditorUtility.SetDirty(character);
                if (character.Routine != null)
                {
                    EditorUtility.SetDirty(character.Routine);
                }
            }
        }

        private static void EnsureFolders()
        {
            var paths = new[]
            {
                Root + "/Art",
                Root + "/Art/Placeholders",
                Root + "/Data",
                Root + "/Data/Characters",
                Root + "/Data/Configuration",
                Root + "/Data/Dialogue",
                Root + "/Data/Events",
                Root + "/Data/Narrative",
                Root + "/Data/Portals",
                Root + "/Data/Quests",
                Root + "/Prefabs",
                Root + "/Prefabs/Characters",
                Root + "/Prefabs/Systems",
                Root + "/Prefabs/World",
                Root + "/Scenes"
            };

            foreach (var path in paths)
            {
                EnsureFolder(path);
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            var name = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private sealed class FoundationAssets
        {
            public TimeConfigSO TimeConfiguration;
            public TimeChangedChannelSO TimeChanged;
            public DayEndedChannelSO DayEnded;
            public NarrativeEventChannelSO NarrativeCompleted;
            public StringEventChannelSO DialogueStarted;
            public StringEventChannelSO DialogueEnded;
            public List<PortalDefinitionSO> PortalDefinitions;
            public List<CharacterDefinitionSO> Characters;
        }

        private sealed class PlaceholderSprites
        {
            public Sprite White;
            public Sprite Player;
            public Sprite Npc;
            public Sprite Portal;
        }
    }
}
