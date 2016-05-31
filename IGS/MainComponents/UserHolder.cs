using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointAndControl.MainComponents
{
    public class UserHolder
    {
        private List<User> users { get; set; }
        public UserHolder()
        {
            users = new List<User>();
        }

        /// <summary>
        ///     Creates the user, who will be identified by his wlan adress 
        ///     and adds the user to the user-list.
        ///     <param name="wlanAdr"> Used to identify and to add a user</param>
        /// </summary>
        public bool AddUser(String wlanAdr)
        {
            foreach (User u in users)
            {
                if (u.WlanAdr == wlanAdr)
                {
                    //return Properties.Resources.UserExists;
                    return false;
                }
            }

            User createdUser = new User(wlanAdr);
            users.Add(createdUser);
            Console.WriteLine("User added");
            return true;
            //return Properties.Resources.UserAdded;
        }

        public string AddUser(String wlanAdr, out bool success)
        {
            foreach (User u in users)
            {
                if (u.WlanAdr == wlanAdr)
                {
                    success = false;
                    return String.Format(Properties.Resources.UserExists, wlanAdr);
                }
            }

            User createdUser = new User(wlanAdr);
            users.Add(createdUser);
            Console.WriteLine("User added");
            success = true;
            return String.Format(Properties.Resources.UserAdded, wlanAdr);
        }



        /// <summary>
        ///     The user with the wlan adress wlanAdr will be connected with a bodyID.
        ///     <param name="wlanAdr">Used to identify the user.</param>
        ///     <param name="bodyID">The ID of the Body which will be connected to the user specified 
        ///     by the wLan Adress.</param>
        ///     <returns>If the process was successful</returns>
        /// </summary>
        public bool SetTrackedSkeleton(String wlanAdr, int bodyID)
        {
            foreach (User u in users)
            {
                if (u.WlanAdr == wlanAdr)
                {
                    u.SkeletonId = bodyID;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Deletes a user identified by his wlan adress 
        ///     from the user-list 
        ///     <param name="wlanAdr">
        ///         Used to identify the user which will be deleted
        ///     </param>
        ///     <returns>If the process was successful</returns>
        /// </summary>
        public bool DelUser(String wlanAdr)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].WlanAdr == wlanAdr)
                {
                    users.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Returns the user by the id of his/her associated body.
        ///     <param name="bodyId">
        ///         Used to return the user with the specified bodyId
        ///     </param>
        ///     <returns>Returns the userobject, if no user is associated with the bodyID, null will be returned</returns>
        /// </summary>
        public User GetUserBySkeleton(int bodyId)
        {
            foreach (User u in users)
            {
                if (u.SkeletonId == bodyId)
                {
                    return u;
                }
            }
            return null;
        }

        /// <summary>
        ///     Returns the user through its wlan adress.
        ///     <param name="wlanAdr">
        ///         Is used to identify the user and retrun the userobject
        ///     </param>
        ///     <returns>Returns the userobject. If no user with the wlanAdr exists NULL will be returned</returns>
        /// </summary>
        public User GetUserByIp(String wlanAdr)
        {
            foreach (User u in users)
            {
                if (u.WlanAdr == wlanAdr)
                {
                    return u;
                }
            }
            return null;
        }

        /// <summary>
        ///     Deletes the associated bodyID from the through ID implicated user.
        ///     Löscht die zugewiesene ID des Skeletts von dem, durch die ID implizierten, User.
        ///     <param name="id">Wird benutzt um das Gerät zu indentifizieren.</param>
        ///     <returns>Löschen erfolgreich</returns>
        /// </summary>
        public bool DelTrackedSkeleton(int id)
        {
            foreach (User u in users)
            {
                if (u.SkeletonId == id)
                {
                    u.SkeletonId = -1;
                    return true;
                }
            }
            return false;
        }
    }
}
