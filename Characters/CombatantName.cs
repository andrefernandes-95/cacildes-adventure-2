namespace AF
{
    using TMPro;
    using UnityEngine;

    public class CombatantName : MonoBehaviour
    {
        public CharacterManager character;

        public TextMeshPro textMeshPro => GetComponent<TextMeshPro>();

        void Awake()
        {
            textMeshPro.text = GetCombatantName();
        }

        string GetCombatantName()
        {
            if (character == null || character.combatant == null)
            {
                return "";
            }

            if (character.combatant.name.IsEmpty)
            {
                return "";
            }

            return character.combatant.name.GetLocalizedString();
        }

        private void FaceCamera()
        {
            if (Camera.main != null)
            {
                transform.LookAt(Camera.main.transform);
                transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
            }
        }

        private void Update()
        {
            FaceCamera();
        }
    }
}
