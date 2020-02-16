using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VR_Prototyping.Scripts.Demo_Scripts
{
    public class RecruitmentProjectSelection : SelectableObject
    {
        [BoxGroup] public Transform table;
        [BoxGroup] public Transform building;
        [BoxGroup] public Transform target;
        [BoxGroup] public Transform dome;
        
        protected override void InitialisePostSetup()
        {
            selectEnd.AddListener(SelectBuilding);
        }

        private void SelectBuilding()
        {
            building.parent = null;
            table.DOMove(new Vector3(0, -1, 0), .7f);
            building.DOMove(target.transform.position, 2.5f);
            building.DOScale(target.transform.localScale, 3f);
            dome.DOScale(new Vector3(150, 150, 150), 3);
        }
    }
}