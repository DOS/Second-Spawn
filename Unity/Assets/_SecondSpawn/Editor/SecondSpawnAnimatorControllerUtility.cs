#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using SecondSpawn.Networking;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace SecondSpawn.EditorTools
{
    public static class SecondSpawnAnimatorControllerUtility
    {
        private const string CraftControllerPath =
            "Assets/ExplosiveLLC/Warrior Pack Bundle 1 FREE/Sorceress Warrior Mecanim Animation Pack/Prefabs/Crafter Animation Controller FREE.controller";

        private const string CraftStatePrefix = "Craft - ";

        [MenuItem("Second Spawn/Art/Rebuild Shared Humanoid Animator Controller")]
        public static void RebuildSharedHumanoidAnimatorController()
        {
            EnsureFolder(Path.GetDirectoryName(VisualAnimatorControllerCatalog.ProjectSharedControllerPath)?.Replace("\\", "/"));

            var baseController = AssetDatabase.LoadAssetAtPath<AnimatorController>(VisualAnimatorControllerCatalog.VendorRpgControllerPath);
            var craftController = AssetDatabase.LoadAssetAtPath<AnimatorController>(CraftControllerPath);
            if (baseController == null)
            {
                Debug.LogError($"[SecondSpawnAnimatorControllerUtility] Base animator controller not found: {VisualAnimatorControllerCatalog.VendorRpgControllerPath}");
                return;
            }

            if (craftController == null)
            {
                Debug.LogError($"[SecondSpawnAnimatorControllerUtility] Craft animator controller not found: {CraftControllerPath}");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(VisualAnimatorControllerCatalog.ProjectSharedControllerPath) != null)
            {
                File.Copy(
                    ToFullPath(VisualAnimatorControllerCatalog.VendorRpgControllerPath),
                    ToFullPath(VisualAnimatorControllerCatalog.ProjectSharedControllerPath),
                    overwrite: true);
                AssetDatabase.ImportAsset(VisualAnimatorControllerCatalog.ProjectSharedControllerPath);
            }
            else if (!AssetDatabase.CopyAsset(VisualAnimatorControllerCatalog.VendorRpgControllerPath, VisualAnimatorControllerCatalog.ProjectSharedControllerPath))
            {
                Debug.LogError($"[SecondSpawnAnimatorControllerUtility] Failed to copy base animator controller to: {VisualAnimatorControllerCatalog.ProjectSharedControllerPath}");
                return;
            }

            var sharedController = AssetDatabase.LoadAssetAtPath<AnimatorController>(VisualAnimatorControllerCatalog.ProjectSharedControllerPath);
            if (sharedController == null)
            {
                Debug.LogError($"[SecondSpawnAnimatorControllerUtility] Failed to create shared animator controller: {VisualAnimatorControllerCatalog.ProjectSharedControllerPath}");
                return;
            }

            MergeCraftParameters(sharedController, craftController);
            MergeCraftStatesIntoBaseLayer(sharedController, craftController);
            EditorUtility.SetDirty(sharedController);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SecondSpawnAnimatorControllerUtility] Rebuilt shared humanoid animator controller at {VisualAnimatorControllerCatalog.ProjectSharedControllerPath}.");
        }

        private static void MergeCraftParameters(AnimatorController target, AnimatorController source)
        {
            foreach (var parameter in source.parameters)
            {
                if (HasParameter(target, parameter.name))
                {
                    continue;
                }

                target.AddParameter(parameter.name, parameter.type);
            }
        }

        private static void MergeCraftStatesIntoBaseLayer(AnimatorController target, AnimatorController source)
        {
            if (target.layers.Length == 0 || source.layers.Length == 0)
            {
                return;
            }

            var targetMachine = target.layers[0].stateMachine;
            var sourceMachine = source.layers[0].stateMachine;
            var stateMap = new Dictionary<AnimatorState, AnimatorState>();
            CopyStates(sourceMachine, targetMachine, stateMap, "Craft", new Vector3(320f, 720f, 0f));
            CopyStateTransitions(sourceMachine, stateMap);
            CopyAnyStateTransitions(sourceMachine, targetMachine, stateMap);
        }

        private static void CopyStates(
            AnimatorStateMachine sourceMachine,
            AnimatorStateMachine targetMachine,
            Dictionary<AnimatorState, AnimatorState> stateMap,
            string pathPrefix,
            Vector3 positionOffset)
        {
            foreach (var childState in sourceMachine.states)
            {
                var sourceState = childState.state;
                if (sourceState == null)
                {
                    continue;
                }

                var stateName = $"{CraftStatePrefix}{pathPrefix} - {sourceState.name}";
                if (FindState(targetMachine, stateName) != null)
                {
                    continue;
                }

                var targetState = targetMachine.AddState(stateName, childState.position + positionOffset);
                CopyStateSettings(sourceState, targetState);
                stateMap[sourceState] = targetState;
            }

            foreach (var childMachine in sourceMachine.stateMachines)
            {
                if (childMachine.stateMachine == null)
                {
                    continue;
                }

                CopyStates(
                    childMachine.stateMachine,
                    targetMachine,
                    stateMap,
                    $"{pathPrefix} - {childMachine.stateMachine.name}",
                    childMachine.position + positionOffset);
            }
        }

        private static void CopyStateTransitions(
            AnimatorStateMachine sourceMachine,
            Dictionary<AnimatorState, AnimatorState> stateMap)
        {
            foreach (var childState in sourceMachine.states)
            {
                var sourceState = childState.state;
                if (sourceState == null || !stateMap.TryGetValue(sourceState, out var targetState))
                {
                    continue;
                }

                foreach (var sourceTransition in sourceState.transitions)
                {
                    AnimatorStateTransition targetTransition = null;
                    if (sourceTransition.isExit)
                    {
                        targetTransition = targetState.AddExitTransition();
                    }
                    else if (sourceTransition.destinationState != null &&
                             stateMap.TryGetValue(sourceTransition.destinationState, out var targetDestination))
                    {
                        targetTransition = targetState.AddTransition(targetDestination);
                    }

                    if (targetTransition != null)
                    {
                        CopyTransitionSettings(sourceTransition, targetTransition);
                    }
                }
            }

            foreach (var childMachine in sourceMachine.stateMachines)
            {
                if (childMachine.stateMachine != null)
                {
                    CopyStateTransitions(childMachine.stateMachine, stateMap);
                }
            }
        }

        private static void CopyAnyStateTransitions(
            AnimatorStateMachine sourceMachine,
            AnimatorStateMachine targetMachine,
            Dictionary<AnimatorState, AnimatorState> stateMap)
        {
            foreach (var sourceTransition in sourceMachine.anyStateTransitions)
            {
                if (sourceTransition.destinationState == null ||
                    !stateMap.TryGetValue(sourceTransition.destinationState, out var targetDestination))
                {
                    continue;
                }

                var targetTransition = targetMachine.AddAnyStateTransition(targetDestination);
                CopyTransitionSettings(sourceTransition, targetTransition);
            }

            foreach (var childMachine in sourceMachine.stateMachines)
            {
                if (childMachine.stateMachine != null)
                {
                    CopyAnyStateTransitions(childMachine.stateMachine, targetMachine, stateMap);
                }
            }
        }

        private static void CopyStateSettings(AnimatorState source, AnimatorState target)
        {
            target.motion = source.motion;
            target.speed = source.speed;
            target.mirror = source.mirror;
            target.cycleOffset = source.cycleOffset;
            target.iKOnFeet = source.iKOnFeet;
            target.writeDefaultValues = source.writeDefaultValues;
            target.tag = source.tag;
        }

        private static void CopyTransitionSettings(AnimatorStateTransition source, AnimatorStateTransition target)
        {
            target.duration = source.duration;
            target.exitTime = source.exitTime;
            target.hasExitTime = source.hasExitTime;
            target.hasFixedDuration = source.hasFixedDuration;
            target.offset = source.offset;
            target.interruptionSource = source.interruptionSource;
            target.orderedInterruption = source.orderedInterruption;
            target.canTransitionToSelf = source.canTransitionToSelf;
            target.mute = source.mute;
            target.solo = source.solo;

            foreach (var condition in source.conditions)
            {
                target.AddCondition(condition.mode, condition.threshold, condition.parameter);
            }
        }

        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
        {
            foreach (var childState in stateMachine.states)
            {
                if (childState.state != null && childState.state.name == stateName)
                {
                    return childState.state;
                }
            }

            return null;
        }

        private static bool HasParameter(AnimatorController controller, string parameterName)
        {
            foreach (var parameter in controller.parameters)
            {
                if (parameter.name == parameterName)
                {
                    return true;
                }
            }

            return false;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureFolder(parent);
            }

            var folderName = Path.GetFileName(folderPath);
            var parentFolder = string.IsNullOrWhiteSpace(parent) ? "Assets" : parent;
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(parentFolder, folderName);
            }
        }

        private static string ToFullPath(string assetPath)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.Combine(projectRoot, assetPath);
        }
    }
}
#endif
