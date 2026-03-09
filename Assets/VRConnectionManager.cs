using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class VRConnectionManager : MonoBehaviour
{
    // Valeurs par défaut pour la connexion VR
    // (pas de clavier disponible sur casque, donc on fixe des valeurs)
    [SerializeField] private string _defaultProfileName = "VRPlayer";
    [SerializeField] private string _defaultSessionName = "SharedWorld";
    private int _maxPlayers = 10;
    private ConnectionState _state = ConnectionState.Disconnected;
    private ISession _session;
    private NetworkManager m_NetworkManager;

    private enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
    }

    private async void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
        m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        m_NetworkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
        await UnityServices.InitializeAsync();
    }

    private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
    {
        if (m_NetworkManager.LocalClient.IsSessionOwner)
        {
            Debug.Log($"Client-{m_NetworkManager.LocalClientId} is the session owner!");
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (m_NetworkManager.LocalClientId == clientId)
        {
            Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
        }
    }

    /// <summary>
    /// Méthode principale : rejoint le monde partagé avec un nom
    /// d'utilisateur et de session passés en paramètres.
    /// C'est CETTE méthode que l'objet interactif va déclencher.
    /// </summary>
    public void JoinSharedWorld(string profileName, string sessionName)
    {
        if (_state == ConnectionState.Disconnected)
        {
            _ = CreateOrJoinSessionAsync(profileName, sessionName);
        }
        else
        {
            Debug.LogWarning("JoinSharedWorld appelé mais déjà connecté ou en cours de connexion.");
        }
    }

    /// <summary>
    /// Surcharge sans paramètre : utilise les valeurs par défaut
    /// configurées dans l'Inspector. Pratique pour l'appel
    /// depuis un événement XR (qui ne peut pas passer de paramètres).
    /// </summary>
    public void JoinSharedWorld()
    {
        JoinSharedWorld(_defaultProfileName, _defaultSessionName);
    }

    private void OnDestroy()
    {
        _session?.LeaveAsync();
    }

    private async Task CreateOrJoinSessionAsync(string profileName, string sessionName)
    {
        _state = ConnectionState.Connecting;
        Debug.Log($"Tentative de connexion : profil='{profileName}', session='{sessionName}'");

        try
        {
            AuthenticationService.Instance.SwitchProfile(profileName);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var options = new SessionOptions()
            {
                Name = sessionName,
                MaxPlayers = _maxPlayers
            }.WithDistributedAuthorityNetwork();

            _session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionName, options);

            _state = ConnectionState.Connected;
            Debug.Log($"Connecté avec succès à la session '{sessionName}' en tant que '{profileName}'");
        }
        catch (Exception e)
        {
            _state = ConnectionState.Disconnected;
            Debug.LogException(e);
        }
    }
}