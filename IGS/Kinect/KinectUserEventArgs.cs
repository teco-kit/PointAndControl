using System;

namespace IGS.Server.Kinect
{
    /// <summary>
    ///     Dieses Event wird getriggert wenn sich ein getracktes Skelett das Sichtfeld der Kinect vollständig verlassen hat.
    ///     @author Sven Ochs
    /// </summary>
    public class KinectUserEventArgs : EventArgs
    {
        /// <summary>
        ///     Konstruktor für das Event
        ///     <param name="skeletonId">Die ID des Skelettes, das den Raum vollständig verlassen hat</param>
        /// </summary>
        public KinectUserEventArgs(int skeletonId)
        {
            SkeletonId = skeletonId;
        }

        /// <summary>
        ///     Die ID des Skelettes, das den Raum vollständig verlassen hat.
        ///     Mit der "set"-Methode kann die ID des Skelettes gesetzt werden.
        ///     Mit der "get"-Methode kann die ID des Skelettes zurückgegeben werden.
        ///     <returns>Gibt ID des Skelettes zurück</returns>
        /// </summary>
        public int SkeletonId { get; set; }
    }
}