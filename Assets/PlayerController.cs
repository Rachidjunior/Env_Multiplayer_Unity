using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

// Contrôleur de joueur avec gestion de la caméra et des déplacements
public class PlayerController : NetworkTransform
{
    // Décalage vertical de la caméra (hauteur des yeux)
    public Vector3 cameraPositionOffset = new Vector3 (0, 1.6f, 0) ;

    // Rotation initiale de la caméra
    public Quaternion cameraOrientationOffset = new Quaternion () ;

    // Référence au Transform de la caméra
    protected Transform cameraTransform ;

    // Référence à la caméra principale
    protected Camera theCamera ;

    

    // Appelé lors de l'apparition du joueur sur le réseau
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        CatchCamera();
    }

    // Attache la caméra au joueur local uniquement
    public void CatchCamera () {
        Debug.Log("CatchCamera appelé - IsOwner: " + IsOwner + ", IsLocalPlayer: " + IsLocalPlayer) ;
        
        // Vérifier si cet objet appartient au joueur local
        if (!IsOwner) {
            Debug.Log("Pas le propriétaire, caméra non attachée");
            return;
        }

        Debug.Log("Propriétaire détecté, attachement de la caméra en cours") ;

        // Récupérer la caméra dans les enfants du prefab
        theCamera = GetComponentInChildren<Camera>(true);
        
        if (theCamera == null) { 
            Debug.LogError("Caméra introuvable dans le prefab Player !"); 
            return; 
        }

        Debug.Log("Caméra trouvée: " + theCamera.gameObject.name + ", état actif: " + theCamera.gameObject.activeSelf);

        // Activer le GameObject de la caméra et ses parents
        theCamera.gameObject.SetActive(true);
        
        // Activer la caméra pour le joueur local uniquement
        theCamera.enabled = true;

        Debug.Log("Caméra activée. Active: " + theCamera.gameObject.activeSelf + ", Camera.enabled: " + theCamera.enabled);

        cameraTransform = theCamera.transform ;

        // Attacher la caméra au rig de navigation
        cameraTransform.SetParent (transform) ;

        // Positionner la caméra avec les offsets définis
        cameraTransform.localPosition = cameraPositionOffset ;
        cameraTransform.localRotation = cameraOrientationOffset ;

        Debug.Log("Configuration de la caméra terminée");
    }

    // Gestion des déplacements à chaque frame
    private void Update()
    {
        // Exécuter uniquement pour le joueur local avec autorité
        if (!IsSpawned || !HasAuthority)
            return;

        // Récupération des inputs clavier (A/D pour rotation, W/S pour avancer/reculer)
        float x = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);
        float z = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);
        
        // Application de la vitesse et du temps delta
        x *= Time.deltaTime * 150.0f;
        z *= Time.deltaTime * 3.0f;

        // Rotation sur l'axe Y (gauche/droite)
        transform.Rotate(0, x, 0);
        
        // Translation locale sur l'axe Z (avant/arrière)
        transform.Translate(0, 0, z);
    }


    

    
}
