using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;

namespace SSBQuests
{
    [RequireComponent(typeof(SplineContainer), typeof(SplineInstantiate))]
    public class TutorialPath : Singleton<TutorialPath>
    {
        [field: Header("Settings"), SerializeField]
        public QuestObject CurrentQuestObject { get; set; }
        [field: SerializeField]
        public List<QuestObject> QuestObjects { get; set; } = new();

        private int _currentStepId;
        private Coroutine _drawPathCoroutine;

        [Header("Spline Settings")]
        [SerializeField] private SplineContainer _spline;
        [SerializeField] private SplineInstantiate _splineInstantiate;

        protected override void Awake() => _splineInstantiate.enabled = false;
        public void StartPath()
        {
            if (_drawPathCoroutine != null) StopCoroutine(_drawPathCoroutine);
            StartCoroutine(CheckTargetsCoroutine());
            _drawPathCoroutine = StartCoroutine(DrawNewPathCoroutine());
        }
        public void EndPath()
        {
            _spline.Spline.Clear();
            CurrentQuestObject = null;
            StopCoroutine(DrawNewPathCoroutine());
        }
        public void EndPath(QuestObject target)
        {
            _spline.Spline.Clear();
            QuestObjects.Remove(target);
            CurrentQuestObject = null;
            if (QuestObjects.Count == 0)
                StopCoroutine(DrawNewPathCoroutine());
        }
        public void SetPath(QuestObject target)
        {
            QuestObjects.Add(target);
            StartPath();
        }
        private Transform CheckClosestTarget()
        {
            Transform closestTarget = null;
            float lastDistance = 0;
            for (int i = 0; i < QuestObjects.Count; i++)
            {
                var questObject = QuestObjects[i];
                if (!questObject.gameObject.activeInHierarchy) continue;
                var currentDistance = Vector3.Distance(questObject.transform.transform.position, GameReferences.PlayerTransform.position);

                if (i == 0 || currentDistance < lastDistance &&
                    (QuestManager.Instance._CurrentQuest.HasSteps && HasAnyActiveObjective(questObject) ||
                     !QuestManager.Instance._CurrentQuest.HasSteps))
                {
                    lastDistance = currentDistance;
                    closestTarget = questObject.transform;
                    CurrentQuestObject = questObject;
                }
            }
            return closestTarget;
        }
        private bool HasAnyActiveObjective(QuestObject questObject, ObjectiveType? targetObjective = null)
        {
            for (int i = 0; i < questObject._Quests.Length; i++)
            {
                Quest quest = questObject._Quests[i].Quest;
                for (int j = 0; j < quest.Objectives.Length; j++)
                {
                    var questObjectObjective = questObject._QuestObjectives[j].Objective;

                    if (quest.Objectives[j].ObjectiveType == (targetObjective ?? questObjectObjective) &&
                        quest.IsObjectiveActive(quest.Objectives[j]))
                    {
                        return questObject.transform;
                    }
                }
            }
            return false;
        }
        private IEnumerator CheckTargetsCoroutine()
        {
            var TargetTransform = CheckClosestTarget();
            while (TargetTransform != null)
            {
                TargetTransform = CheckClosestTarget();
                yield return new WaitForSeconds(3f);
            }
        }
        private IEnumerator DrawNewPathCoroutine()
        {
            if (!CurrentQuestObject) yield break;
            var TargetTransform = CurrentQuestObject.transform;
            _splineInstantiate.enabled = true;
            NavMeshPath newPath = new NavMeshPath();

            while (TargetTransform != null)
            {
                if (NavMesh.CalculatePath(GameReferences.PlayerTransform.position,
                    TargetTransform.position, NavMesh.AllAreas, newPath))
                {
                    _spline.Spline.Clear();
                    for (int i = 0; i < newPath.corners.Length; i++)
                    {
                        // Apply the offset
                        BezierKnot bezier = new();
                        bezier.Position = newPath.corners[i] + new Vector3(0, 0.01f, 0);
                        bezier.Rotation = Quaternion.identity;
                        _spline.Spline.Add(bezier);
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
