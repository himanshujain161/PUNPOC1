/*using.Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame
{

    public class Launcher : MonoBehaviour
    {
        #region Private Serializable Fields

        #endregion

        #region Private Fields


        string gameVersion = "1";

        #endregion

        #region MonoBehaviour CallBacks


        void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }


        void Start()
        {
            Connect();
        }

        #endregion


        #region Public Methods


        public void Connect()
        {

            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {

                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        #endregion

    }

}
*/