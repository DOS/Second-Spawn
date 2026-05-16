using System.Collections;
using SecondSpawn.Networking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SecondSpawn.AI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SecondSpawnGatewayClient))]
    public sealed class PrototypeNPCChatClient : MonoBehaviour
    {
        [SerializeField] private string _npcId = "prototype-guide";
        [SerializeField, TextArea] private string _prototypeMessage =
            "What should this body remember while I am offline?";
        [SerializeField] private Key _talkKey = Key.O;
        [SerializeField] private Key _voiceKey = Key.V;

        private SecondSpawnGatewayClient _gateway;

        private void Awake()
        {
            _gateway = GetComponent<SecondSpawnGatewayClient>();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard[_talkKey].wasPressedThisFrame)
            {
                StartCoroutine(SendPrototypeChat());
            }

            if (keyboard[_voiceKey].wasPressedThisFrame)
            {
                StartCoroutine(CheckVoiceSession());
            }
        }

        private IEnumerator SendPrototypeChat()
        {
            yield return _gateway.Chat(new NpcChatRequestDto
            {
                npc_id = _npcId,
                message = _prototypeMessage
            }, response =>
            {
                Debug.Log($"[PrototypeNPCChatClient] {response.npc_id}: {response.text}");
                PresentSpeech(response.text);
                PlayTalkAnimation();
            }, Debug.LogWarning);
        }

        private IEnumerator CheckVoiceSession()
        {
            yield return _gateway.GetVoiceSession(response =>
            {
                Debug.Log($"[PrototypeNPCChatClient] Voice provider={response.provider}, available={response.voice_available}, reason={response.reason}");
            }, Debug.LogWarning);
        }

        private static void PlayTalkAnimation()
        {
            var driver = FindAnyObjectByType<VisualAnimationIntentDriver>();
            if (driver != null)
            {
                driver.TryPlay(VisualAnimationIntent.Talk);
            }
        }

        private static void PresentSpeech(string text)
        {
            var speechBubble = FindAnyObjectByType<PrototypeSpeechBubble>();
            if (speechBubble != null)
            {
                speechBubble.Show(text);
            }

            var voiceCue = FindAnyObjectByType<PrototypeVoiceCue>();
            if (voiceCue != null)
            {
                voiceCue.PlayCue(text);
            }
        }
    }
}
